namespace Saspect.Test.Samples;

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