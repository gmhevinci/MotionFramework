using System.Collections.Generic;

public static class System_Collections_Generic
{
	public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
	{
		return collection == null || collection.Count == 0;
	}
}