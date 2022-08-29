using System;
using System.Collections.Generic;
using System.Reflection;

namespace Saspect;

public class InvocationInfo
{
	internal InvocationInfo()
	{
	}
	
	internal Action ProceedCall;
	internal void Proceed() => ProceedCall.Invoke();

	public object[] Arguments { get; internal set; }
	public MethodInfo Method { get; internal set; }
	public object ReturnValue { get; set; }
	public Type TargetType { get; internal set; }

	public List<Exception> Exceptions = new();
	
}
