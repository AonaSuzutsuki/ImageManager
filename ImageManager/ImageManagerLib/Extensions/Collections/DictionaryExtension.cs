using System;
using System.Collections.Generic;

namespace FileManagerLib.Extensions.Collections
{
	public static class DictionaryExtension
    {
		public static (TKey, TValue) GetLast<TKey, TValue>(this Dictionary<TKey, TValue> dic)
		{
			var keys = new List<TKey>(dic.Keys);
			var values = new List<TValue>(dic.Values);
			var key = keys[keys.Count - 1];
			var val = values[values.Count - 1];
			return (key, val);
		}
        
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
