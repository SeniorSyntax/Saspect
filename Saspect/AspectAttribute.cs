using System;

namespace Saspect;

public abstract class AspectAttribute : Attribute
{
	protected AspectAttribute(Type aspectType)
	{
		AspectType = aspectType;
	}

	public Type AspectType { get; protected set; }
}
