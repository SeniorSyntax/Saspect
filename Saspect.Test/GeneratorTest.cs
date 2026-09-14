// avoid adding System usings here... yeah right.

using Autofac;
using Autofac.Builder;
using NUnit.Framework;
using Saspect.Autofac;
using Saspect.Test.Samples;
using Shouldly;
using Sinjector;

namespace Saspect.Test;

[SinjectorFixture]
public class GeneratorTest : IContainerSetup
{
	public void ContainerSetup(IExtend context)
	{
		context.AddService<AspectInterceptor>();

		context.AddServiceWithAspects<Sample>();
		context.AddService<NonAspectedSample>();
		context.AddServiceWithAspects<NestedClassSample.NestedClass>();
		context.AddServiceWithAspects<CtorSample>();
		context.AddServiceWithAspects<PrimaryCtorSample>();
		context.AddService<DependencySample>();
		context.AddService<Aspect1>();
	}

	public void RegistrationCallback(object registration)
	{
		((IRegistrationBuilder<object, ConcreteReflectionActivatorData, object>)registration).ApplyAspects();
	}

	public IComponentContext Container;

	[Test]
	public void ShouldGenerateProxy()
	{
		Container.Resolve<Sample>()
			.ShouldBeOfType<SampleAspected>();
	}

	[Test]
	public void ShouldManageNestedClass()
	{
		Container.Resolve<NestedClassSample.NestedClass>()
			.ShouldBeOfType<NestedClassSample_NestedClassAspected>();
	}

	[Test]
	public void ShouldNotGenerateProxyForNonAspected()
	{
		Container.Resolve<NonAspectedSample>()
			.ShouldBeOfType<NonAspectedSample>();
	}
	
	[Test]
	public void ShouldGenerateCtor()
	{
		Container.Resolve<CtorSample>()
			.ShouldBeOfType<CtorSampleAspected>();
	}

	[Test]
	public void ShouldGeneratePrimaryCtor()
	{
		Container.Resolve<PrimaryCtorSample>()
			.ShouldBeOfType<PrimaryCtorSampleAspected>();
	}
}