// avoid adding System usings here... yeah right.

using Autofac;
using Autofac.Builder;
using NUnit.Framework;
using Saspect.Autofac;
using Saspect.Test.Samples;
using SharpTestsEx;
using Sinjector;

namespace Saspect.Test;

[SinjectorFixture]
public class GeneratorTest : IContainerSetup, IContainerRegistrationSetup
{
	public void ContainerSetup(IContainerSetupContext context)
	{
		context.AddService<AspectInterceptor>();

		context.AddService<Sample>();
		context.AddService<NonAspectedSample>();
		context.AddService<NestedClassSample.NestedClass>();
		context.AddService<CtorSample>();
		context.AddService<PrimaryCtorSample>();
		context.AddService<DependencySample>();
		context.AddService<Aspect1>();
	}

	public void ContainerRegistrationSetup<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> registration) where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData
	{
		registration.ApplyAspects();
	}

	public IComponentContext Container;

	[Test]
	public void ShouldGenerateProxy()
	{
		Container.Resolve<Sample>()
			.Should().Be.OfType<SampleAspected>();
	}

	[Test]
	public void ShouldManageNestedClass()
	{
		Container.Resolve<NestedClassSample.NestedClass>()
			.Should().Be.OfType<NestedClassSample_NestedClassAspected>();
	}

	[Test]
	public void ShouldNotGenerateProxyForNonAspected()
	{
		Container.Resolve<NonAspectedSample>()
			.Should().Be.OfType<NonAspectedSample>();
	}
	
	[Test]
	public void ShouldGenerateCtor()
	{
		Container.Resolve<CtorSample>()
			.Should().Be.OfType<CtorSampleAspected>();
	}

	[Test]
	public void ShouldGeneratePrimaryCtor()
	{
		Container.Resolve<PrimaryCtorSample>()
			.Should().Be.OfType<PrimaryCtorSampleAspected>();
	}
}