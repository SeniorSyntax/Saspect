using System;

namespace Saspect;

public interface IAspect
{
	void OnBeforeInvocation(InvocationInfo invocation);
	void OnAfterInvocation(Exception exception, InvocationInfo invocation);
}
