// avoid adding System usings here... yeah right.

using System;
using Autofac;
using Autofac.Builder;
using NUnit.Framework;
using Saspect.Autofac;
using SharpTestsEx;
using Sinjector;

namespace Saspect.Test;

[SinjectorFixture]
public class GeneratorTest : IContainerSetup, IContainerRegistrationSetup
{
	public void ContainerSetup(IContainerSetupContext context)
	{
		context.AddService<AspectInterceptor>();

		context.AddService<CtorSample>();
		context.AddService<DependencySample>();
		context.AddService<Aspect1Attribute.Aspect1>();
	}

	public void ContainerRegistrationSetup<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> registration) where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData
	{
		registration.ApplyAspects();
	}

	public IComponentContext Container;

	[Test]
	public void ShouldGenerateCtor()
	{
		Container.Resolve<CtorSample>().AspectedMethod();
	}
	
	// [Test]
	// public void ShouldGeneratePrimaryCtor()
	// {
	// 	Container.Resolve<PrimaryCtorSample>().AspectedMethod();
	// }

	public class CtorSample
	{
		public CtorSample(DependencySample injected)
		{
		}

		[Aspect1]
		public virtual void AspectedMethod()
		{
		}
	}
	//
	// public class PrimaryCtorSample(DependencySample injected)
	// {
	// 	[Aspect1]
	// 	public virtual void AspectedMethod()
	// 	{
	// 	}
	// }

	public class Aspect1Attribute : AspectAttribute
	{
		public Aspect1Attribute() : base(typeof(Aspect1))
		{
		}

		public class Aspect1 : IAspect
		{
			public void OnBeforeInvocation(InvocationInfo invocation)
			{
			}

			public void OnAfterInvocation(Exception exception, InvocationInfo invocation)
			{
			}
		}
	}
}

public class DependencySample
{
}
