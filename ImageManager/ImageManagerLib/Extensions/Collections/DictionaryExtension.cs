using System;
using System.Collections.Generic;

namespace FileManagerLib.Extensions.Collections
{
    /// <summary>
    /// Dictionary関連の拡張メソッドを提供します。
    /// </summary>
	public static class DictionaryExtension
    {
        /// <summary>
        /// SortedDictionaryのValue値を配列に変換します。
        /// </summary>
        /// <typeparam name="TKey">SortedDictionaryのキーの型</typeparam>
        /// <typeparam name="TValue">SortedDictionaryの値の型</typeparam>
        /// <param name="values">変換したいSortedDictionaryのValue</param>
        /// <returns>変換された配列</returns>
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
