using System;
using System.Collections.Generic;

namespace FileManagerLib.Extensions.Collections
{
    /// <summary>
	/// Provides <c>Dictionary</c> related extension methods.
    /// </summary>
	public static class DictionaryExtension
    {
        /// <summary>
        /// Convert value of <c>SortedDictionary</c> to array.
        /// </summary>
		/// <typeparam name="TKey">Type of <c>SortedDictionary</c> key</typeparam>
		/// <typeparam name="TValue">Type of <c>SortedDictionary</c> value</typeparam>
        /// <param name="values">Target SortedDictionary.Value</param>
        /// <returns>Converted array.</returns>
		public static TValue[] ToArray<TKey, TValue>(this SortedDictionary<TKey, TValue>.ValueCollection values)
		{
			var array = new TValue[values.Count];
			int i = 0;
			foreach (var value in values)
                array[i++] = value;
			return array;
		}
    }
}
