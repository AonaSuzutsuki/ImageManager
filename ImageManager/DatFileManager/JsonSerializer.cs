using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using Newtonsoft.Json;

namespace DatFileManager
{
	public static class JsonSerializer
	{
		public static string ToJson<T>(T obj)
		{
			var json = JsonConvert.SerializeObject(obj);
			return json;
		}

		public static T ToObject<T>(string json)
		{
			var obj = JsonConvert.DeserializeObject<T>(json);
			return obj;
		}
	}   
}
