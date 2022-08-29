// avoid adding System usings here... yeah right.
using Autofac.Builder;
using NUnit.Framework;
using Saspect.Autofac;
using SharpTestsEx;
using Sinjector;

namespace Saspect.Test;

[SinjectorFixture]
public class AspectsTest : IContainerSetup, IContainerRegistrationSetup
{
	public void ContainerSetup(IContainerSetupContext context)
	{
		context.AddService<AspectInterceptor>();
			
		context.AddService<AspectedService>();
		context.AddService<NonAspectedService>();
		context.AddService<Aspect1Attribute.Aspect1>();
		context.AddService<Aspect2Attribute.Aspect2>();
		context.AddService<Aspect3Attribute.Aspect3>();
	}
		
	public void ContainerRegistrationSetup<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> registration) where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData
	{
		registration.ApplyAspects();
	}

	public ISinjectorTestContext Context;
	public AspectedService Target;
	public NonAspectedService NonAspectedTarget;
	public Aspect1Attribute.Aspect1 Aspect1;
	public Aspect2Attribute.Aspect2 Aspect2;
	public Aspect3Attribute.Aspect3 Aspect3;
		
	[Test]
	public void ShouldResolveProxy()
	{
		Target.Should().Be.OfType<AspectsTest_AspectedServiceAspected>();
	}

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
		
	[Test]
	public void ShouldNotGenerateProxyForNonAspected()
	{
		NonAspectedTarget.Should().Be.OfType<NonAspectedService>();
	}

	public class NonAspectedService
	{
		public void Method()
		{
		}
	}
		
	public class AspectedService
	{
		public bool Invoked;
		public System.Exception FailsWith;
		public object Returns;

		public void Reset()
		{
			Invoked = false;
			FailsWith = null;
		}

		[An]
		public virtual void AttributedMethod()
		{
		}

		[Aspect1]
		[Aspect2]
		[Aspect3]
		public virtual void AspectedMethod()
		{
			Invoked = true;
			if (FailsWith != null)
				throw FailsWith;
		}
			
		[Aspect1]
		public virtual object AspectedFunc()
		{
			return Returns;
		}
			
		public void AspectedProtectedCall() => AspectedProtected();
		[Aspect1]
		protected virtual void AspectedProtected()
		{
			Invoked = true;
		}
	}

	public class AnAttribute : System.Attribute
	{
	}

	public class Aspect1Attribute : AspectAttribute
	{
		public Aspect1Attribute() : base(typeof(Aspect1))
		{
		}

		public class Aspect1 : FakeAspect
		{
		}
	}

	public class Aspect2Attribute : AspectAttribute
	{
		public Aspect2Attribute() : base(typeof(Aspect2))
		{
		}

		public class Aspect2 : FakeAspect
		{
		}
	}


	public class Aspect3Attribute : AspectAttribute
	{
		public Aspect3Attribute() : base(typeof(Aspect3))
		{
		}

		public class Aspect3 : FakeAspect
		{
		}
	}

	public class FakeAspect : IAspect
	{
		public int BeforeInvokedCalls;
		public bool BeforeInvoked => BeforeInvokedCalls > 0;

		public bool AfterInvoked;
		public System.Exception BeforeFailsWith;
		public System.Exception AfterFailsWith;
		public System.Exception AfterInvokedWith;

		public object ReturnValue;

		public void OnBeforeInvocation(InvocationInfo invocation)
		{
			BeforeInvokedCalls++;
			if (BeforeFailsWith != null)
				throw BeforeFailsWith;
		}

		public void OnAfterInvocation(System.Exception exception, InvocationInfo invocation)
		{
			AfterInvokedWith = exception;
			AfterInvoked = true;
			if (AfterFailsWith != null)
				throw AfterFailsWith;
			ReturnValue = invocation.ReturnValue;
		}
	}

}
