using System;
using System.Collections.Generic;

namespace Saspect;

internal static class Extensions
{
	public static void ForEach<T>(this IEnumerable<T> source, Action<T> act)
	{
		foreach (T item in source)
			act(item);
	}
}
