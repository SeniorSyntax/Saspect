namespace Saspect.Test.Samples;

public class PrimaryCtorSample(DependencySample injected)
{
	[Aspect1]
	public virtual void AspectedMethod()
	{
	}
}