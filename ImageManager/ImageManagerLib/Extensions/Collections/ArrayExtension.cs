using System;
namespace FileManagerLib.Extensions.Collections
{
	public static class ArrayExtension
	{
		public static void ForEach<T>(this T[] array, Action<T> action)
		{
			foreach (var val in array)
				action(val);
		}
	}
}
