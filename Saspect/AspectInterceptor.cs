using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Saspect;

[DebuggerStepThrough]
public class AspectInterceptor
{
	private readonly Lazy<IEnumerable<IAspect>> _aspects;

	public AspectInterceptor(Lazy<IEnumerable<IAspect>> aspects)
	{
		_aspects = aspects;
	}

	public void Intercept(Type aspectedType, string methodName, Type[] parameterTypes, object[] arguments, Action<InvocationInfo> proceed, BindingFlags bindingFlags)
	{
		var type = aspectedType.BaseType;
		var invocation = new InvocationInfo
		{
			Method = getMethodBestMatch(type, methodName, parameterTypes, bindingFlags),
			Arguments = arguments,
			TargetType = type
		};
		invocation.ProceedCall = () => proceed.Invoke(invocation);
		intercept(invocation);
	}

	private static MethodInfo getMethodBestMatch(Type type, string methodName, Type[] parameterTypes, BindingFlags bindingFlags)
	{
		var method = type.GetMethod(methodName, bindingFlags, null, parameterTypes, null);
		if (method != null)
			return method;
		
		// var methods = type.GetMethods().Where(x => x.Name == methodName).ToArray();
		// var method2 = Type.DefaultBinder.SelectMethod(BindingFlags.Public, methods, parameterTypes, null);

		var methods = type.GetMethods().Where(x => x.Name == methodName).ToArray();
		if (methods.Length == 1)
			return methods.Single();
		methods = methods.Where(x => x.GetParameters().Length == parameterTypes.Length).ToArray();
		if (methods.Length == 1)
			return methods.Single();
		methods = methods.Where(x =>
		{
			var firstParameter = x.GetParameters().First();
			return firstParameter.ParameterType.GetGenericArguments().Length == parameterTypes.First().GetGenericArguments().Length;
		}).ToArray();
		if (methods.Length == 1)
			return methods.Single();

		throw new Exception($"{nameof(getMethodBestMatch)} cant find method {methodName} yet... please improve me!");
	}

	private void intercept(InvocationInfo invocationInfo)
	{
		var methodOnClass = invocationInfo.Method;
		var allAttributes = methodOnClass.GetCustomAttributes<AspectAttribute>(false);

		if (!allAttributes.Any())
		{
			invocationInfo.Proceed();
			return;
		}

		var aspects = allAttributes
			.Select(a => a.AspectType)
			.Select(t =>
			{
				try
				{
					return _aspects.Value.Single(t.IsInstanceOfType);
				}
				catch (InvalidOperationException e)
				{
					throw new InvalidOperationException($"{t.Name} aspect not found. Is it registered?", e);
				}
			})
			.ToArray();

		// var invocationInfo = new InvocationInfo(invocation);
		//
		runBefores(invocationInfo, aspects);

		throwAny(invocationInfo);

		invoke(invocationInfo);

		runAfters(invocationInfo, aspects);

		throwAny(invocationInfo);
	}

	private static void runBefores(InvocationInfo invocation, IEnumerable<IAspect> aspects)
	{
		var startedBefores = new List<IAspect>();

		try
		{
			aspects.ForEach(a =>
			{
				startedBefores.Add(a);
				a.OnBeforeInvocation(invocation);
			});
		}
		catch (Exception e)
		{
			invocation.Exceptions.Add(e);
			runAfters(invocation, startedBefores);
		}
	}

	private static void invoke(InvocationInfo invocation)
	{
		try
		{
			invocation.Proceed();
		}
		catch (Exception e)
		{
			invocation.Exceptions.Add(e);
		}
	}

	private static void runAfters(InvocationInfo invocation, IEnumerable<IAspect> aspects)
	{
		aspects
			.Reverse()
			.ForEach(a =>
			{
				try
				{
					a.OnAfterInvocation(invocation.Exceptions.FirstOrDefault(), invocation);
				}
				catch (Exception e)
				{
					invocation.Exceptions.Add(e);
				}
			});
	}

	private static void throwAny(InvocationInfo invocation)
	{
		if (invocation.Exceptions.Count == 1)
			ExceptionDispatchInfo.Capture(invocation.Exceptions.First()).Throw();
		if (invocation.Exceptions.Any())
			throw new AggregateException(invocation.Exceptions);
	}
}
