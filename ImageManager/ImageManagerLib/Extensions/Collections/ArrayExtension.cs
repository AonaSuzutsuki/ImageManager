using System;
namespace FileManagerLib.Extensions.Collections
{
    /// <summary>
	/// Provides array related extension methods.
    /// </summary>
	public static class ArrayExtension
	{
        /// <summary>
		/// Performs the processing of the specified delegate for each element of the array.
        /// </summary>
        /// <typeparam name="T">Generics type of Array.</typeparam>
        /// <param name="array">Target array.</param>
		/// <param name="action">Delegate to execute.</param>
		public static void ForEach<T>(this T[] array, Action<T> action)
		{
			foreach (var val in array)
				action(val);
		}
	}
}
