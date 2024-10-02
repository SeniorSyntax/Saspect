using System;

namespace Saspect.Test.Samples;

public class Aspect1 : IAspect
{
	public void OnBeforeInvocation(InvocationInfo invocation)
	{
	}

	public void OnAfterInvocation(Exception exception, InvocationInfo invocation)
	{
	}
}