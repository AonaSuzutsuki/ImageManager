﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace FileManagerLib.Extensions.Collections
{
    /// <summary>
    /// Extension functions of <see cref="IDictionary"/>.
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

        /// <summary>
        /// It checks for the existence of the element and returns the element of the key if it exists.
        /// If it does not exist, it returns the default value.
        /// </summary>
        /// <typeparam name="K">Key type.</typeparam>
        /// <typeparam name="V">Value type.</typeparam>
        /// <param name="dict">Target dictionary.</param>
        /// <param name="key">The key you want to search</param>
        /// <param name="defaultValue">Default value/</param>
        /// <returns></returns>
        public static V Get<K, V>(this Dictionary<K, V> dict, K key, V defaultValue = default)
        {
            if (dict.ContainsKey(key))
                return dict[key];
            return defaultValue;
        }


        public static void Put<K, V>(this Dictionary<K, V> dict, K key, V value)
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, value);
        }
    }
}
