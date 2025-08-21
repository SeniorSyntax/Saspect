// avoid adding System usings here... yeah right.

using Autofac.Builder;
using NUnit.Framework;
using Saspect.Autofac;
using Saspect.Test.Samples;
using SharpTestsEx;
using Sinjector;
using Sinjector.Autofac;

namespace Saspect.Test;

[AutofacSinjectorFixture]
public class AspectsTest : IContainerSetup, IContainerRegistrationSetup
{
	public void ContainerSetup(IContainerSetupContext context)
	{
		context.AddService<AspectInterceptor>();

		context.AddService<Sample>();
		context.AddService<Aspect1>();
		context.AddService<Aspect2>();
		context.AddService<Aspect3>();
	}

	public void RegistrationCallback(object registration)
	{
		((IRegistrationBuilder<object, ConcreteReflectionActivatorData, object>)registration).ApplyAspects();
	}

	public ISinjectorTestContext Context;
	public Sample Target;
	public Aspect1 Aspect1;
	public Aspect2 Aspect2;
	public Aspect3 Aspect3;

	[Test]
	public void ShouldInvokeAspectBeforeMethod()
	{
		Target.AspectedMethod();

		Aspect1.BeforeInvoked.Should().Be.True();
	}

	[Test]
	public void ShouldInvokeAspectAfterMethod()
	{
		Target.AspectedMethod();

		Aspect1.AfterInvoked.Should().Be.True();
	}

	[Test]
	public void ShouldInvokeOriginalMethod()
	{
		Target.AspectedMethod();

		Target.Invoked.Should().Be.True();
	}

	[Test]
	public void ShouldIgnoreOtherAttributes()
	{
		Target.AttributedMethod();
	}

	[Test]
	public void ShouldResolveAspectFromAttribute()
	{
		Target.AspectedMethod();

		Aspect1.BeforeInvoked.Should().Be.True();
		Aspect1.AfterInvoked.Should().Be.True();
	}

	[Test]
	public void ShouldThrowOnAspectException()
	{
		Aspect1.BeforeFailsWith = new System.ArithmeticException();

		Assert.Throws<System.ArithmeticException>(Target.AspectedMethod);
	}

	[Test]
	public void ShouldInvokeAspectAfterMethodEvenThoughInvokationThrows()
	{
		Target.FailsWith = new System.ArithmeticException();

		Assert.Throws<System.ArithmeticException>(Target.AspectedMethod);
		Aspect1.AfterInvoked.Should().Be.True();
	}

	[Test]
	public void ShouldInvokeAspectAfterMethodWithExceptionFromInvokation()
	{
		System.Exception expected = new System.ArithmeticException();
		Target.FailsWith = expected;

		Assert.Throws<System.ArithmeticException>(Target.AspectedMethod);

		Aspect1.AfterInvokedWith.Should().Be.SameInstanceAs(expected);
	}

	[Test]
	public void ShouldInvokeAllAfterInvocationMethods()
	{
		Aspect1.AfterFailsWith = new System.ArithmeticException();

		Assert.Throws<System.ArithmeticException>(Target.AspectedMethod);
		Aspect2.AfterInvoked.Should().Be.True();
		Aspect3.AfterInvoked.Should().Be.True();
	}

	[Test]
	public void ShouldInvokeAfterInvocationForCompletedBeforeInvocationMethods()
	{
		Aspect3.BeforeFailsWith = new System.ArithmeticException();

		Assert.Throws<System.ArithmeticException>(Target.AspectedMethod);
		Aspect1.AfterInvoked.Should().Be.True();
		Aspect2.AfterInvoked.Should().Be.True();
	}

	[Test]
	public void ShouldInvokeAfterInvocationForStartedBeforeInvocationMethods()
	{
		Aspect2.BeforeFailsWith = new System.ArithmeticException();

		Assert.Throws<System.ArithmeticException>(Target.AspectedMethod);

		Aspect2.AfterInvoked.Should().Be.True();
		Aspect3.AfterInvoked.Should().Be.False();
	}

	[Test]
	public void ShouldResolveAspectServicesAfterShutdown()
	{
		Target.AspectedMethod();
		Target.Reset();
		Context.SimulateShutdown();

		Target.AspectedMethod();

		Target.Invoked.Should().Be.True();
		Aspect1.AfterInvoked.Should().Be.True();
		Aspect2.AfterInvoked.Should().Be.True();
		Aspect3.AfterInvoked.Should().Be.True();
	}

	[Test]
	public void ShouldInterceptReturnValue()
	{
		Target.Returns = "hello";

		Target.AspectedFunc();

		Aspect1.ReturnValue.Should().Be("hello");
	}

	[Test]
	public void ShouldInvokeOriginalProtectedMethod()
	{
		Target.AspectedProtectedCall();

		Target.Invoked.Should().Be.True();
	}
}