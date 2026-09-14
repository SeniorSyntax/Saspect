// avoid adding System usings here... yeah right.

using NUnit.Framework;
using Saspect.Test.Samples;
using Shouldly;
using Sinjector;

namespace Saspect.Test;

[SinjectorFixture]
public class AspectsTest : IContainerSetup
{
	public void ContainerSetup(IExtend context)
	{
		context.AddService<AspectInterceptor>();

		context.AddServiceWithAspects<Sample>();
		context.AddService<Aspect1>();
		context.AddService<Aspect2>();
		context.AddService<Aspect3>();
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

		Aspect1.BeforeInvoked.ShouldBeTrue();
	}

	[Test]
	public void ShouldInvokeAspectAfterMethod()
	{
		Target.AspectedMethod();

		Aspect1.AfterInvoked.ShouldBeTrue();
	}

	[Test]
	public void ShouldInvokeOriginalMethod()
	{
		Target.AspectedMethod();

		Target.Invoked.ShouldBeTrue();
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

		Aspect1.BeforeInvoked.ShouldBeTrue();
		Aspect1.AfterInvoked.ShouldBeTrue();
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
		Aspect1.AfterInvoked.ShouldBeTrue();
	}

	[Test]
	public void ShouldInvokeAspectAfterMethodWithExceptionFromInvokation()
	{
		System.Exception expected = new System.ArithmeticException();
		Target.FailsWith = expected;

		Assert.Throws<System.ArithmeticException>(Target.AspectedMethod);

		Aspect1.AfterInvokedWith.ShouldBeSameAs(expected);
	}

	[Test]
	public void ShouldInvokeAllAfterInvocationMethods()
	{
		Aspect1.AfterFailsWith = new System.ArithmeticException();

		Assert.Throws<System.ArithmeticException>(Target.AspectedMethod);
		Aspect2.AfterInvoked.ShouldBeTrue();
		Aspect3.AfterInvoked.ShouldBeTrue();
	}

	[Test]
	public void ShouldInvokeAfterInvocationForCompletedBeforeInvocationMethods()
	{
		Aspect3.BeforeFailsWith = new System.ArithmeticException();

		Assert.Throws<System.ArithmeticException>(Target.AspectedMethod);
		Aspect1.AfterInvoked.ShouldBeTrue();
		Aspect2.AfterInvoked.ShouldBeTrue();
	}

	[Test]
	public void ShouldInvokeAfterInvocationForStartedBeforeInvocationMethods()
	{
		Aspect2.BeforeFailsWith = new System.ArithmeticException();

		Assert.Throws<System.ArithmeticException>(Target.AspectedMethod);

		Aspect2.AfterInvoked.ShouldBeTrue();
		Aspect3.AfterInvoked.ShouldBeFalse();
	}

	[Test]
	public void ShouldResolveAspectServicesAfterShutdown()
	{
		Target.AspectedMethod();
		Target.Reset();
		Context.SimulateShutdown();

		Target.AspectedMethod();

		Target.Invoked.ShouldBeTrue();
		Aspect1.AfterInvoked.ShouldBeTrue();
		Aspect2.AfterInvoked.ShouldBeTrue();
		Aspect3.AfterInvoked.ShouldBeTrue();
	}

	[Test]
	public void ShouldInterceptReturnValue()
	{
		Target.Returns = "hello";

		Target.AspectedFunc();

		Aspect1.ReturnValue.ShouldBe("hello");
	}

	[Test]
	public void ShouldInvokeOriginalProtectedMethod()
	{
		Target.AspectedProtectedCall();

		Target.Invoked.ShouldBeTrue();
	}
}