namespace Saspect.Test.Samples;

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