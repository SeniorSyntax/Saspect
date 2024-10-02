namespace Saspect.Test.Samples;

public class Sample
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