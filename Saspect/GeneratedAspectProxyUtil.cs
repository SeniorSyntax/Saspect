using System;
using System.Linq;
using System.Reflection;

namespace Saspect;

public static class GeneratedAspectProxyUtil
{
	public static Type GetAspectedType(Type type)
	{
		var typeName = type.FullName;
		var aspectedTypeName = typeName.Replace("+", "_") + "Aspected";
		var aspectedAsemblyQualifiedName = type.AssemblyQualifiedName.Replace(typeName, aspectedTypeName);
		var aspectedType = Type.GetType(aspectedAsemblyQualifiedName);
		return aspectedType;
	}
	
	public static bool IsGeneratedAspectedType(Type type) =>
		type.GetCustomAttributes<GeneratedAspectProxyAttribute>().Any();

	public static bool IsGeneratedAspected(this Type type) =>
		IsGeneratedAspectedType(type);
	
	public static Type RealTypeOf(Type type)
	{
		if (IsGeneratedAspectedType(type))
			return type.BaseType;
		return type;
	}
}
