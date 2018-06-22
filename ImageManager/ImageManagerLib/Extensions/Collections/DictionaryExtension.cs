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
    }
}
