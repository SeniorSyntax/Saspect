using System;
using Autofac;
using NUnit.Framework;
using Saspect.Autofac;

namespace Saspect.Test;

public class MyService
{
	[Log]
	public virtual void MyMethod()
	{
	}
}

public class LogAttribute() : AspectAttribute(typeof(LogAspect));

public class LogAspect : IAspect
{
	public void OnBeforeInvocation(InvocationInfo invocation)
	{
		Console.WriteLine($"{invocation.Method.Name} is invoked");
	}

	public void OnAfterInvocation(Exception exception, InvocationInfo invocation)
	{
		Console.WriteLine($"{invocation.Method.Name} exited");
	}
}

public class SampleTest
{
	[Test]
	public void ShouldLogMethodCallToConsole()
	{
		var builder = new ContainerBuilder();
		builder.RegisterType<AspectInterceptor>();
		builder.RegisterType<LogAspect>().As<IAspect>();
		builder.RegisterType<MyService>().ApplyAspects();
		var container = builder.Build();
		var target = container.Resolve<MyService>();
		
		target.MyMethod();
	}
}