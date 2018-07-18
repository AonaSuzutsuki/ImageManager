using CommonExtensionLib.Extensions;
using ImageManagerCUI.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DatFileManager
{
	class Program
	{
		public static void Main(string[] args)
		{
			// JSONにしたいDictionaryデータ
			var dList = new List<DirectoryStructure>
			{
				new DirectoryStructure { Id = 1, Name = "test", Parent = 0 },
				new DirectoryStructure { Id = 2, Name = "test2", Parent = 0 },
			};
			var fList = new List<FileStructure>
			{
				new FileStructure { Id = 1, Name = "aaa.jpg", Parent = 0, Location = 0, MimeType = ""}
			};
			var table = new TableStructure
			{
				Directory = dList.ToArray(),
				File = fList.ToArray(),
			};

			var dJson = JsonSerializer.ToJson(dList.ToArray());
			var fJson = JsonSerializer.ToJson(fList.ToArray());
			Console.WriteLine(dJson);
			Console.WriteLine(fJson);

			var dArray = JsonSerializer.ToObject<DirectoryStructure[]>(dJson);
			var fArray = JsonSerializer.ToObject<FileStructure[]>(fJson);
			new List<DirectoryStructure>(dArray).ForEach(Console.WriteLine);
			new List<FileStructure>(fArray).ForEach(Console.WriteLine);

			//var program = new Program();

			//while (true)
			//{
			//    Console.Write("> ");
			//    string cmd = Console.ReadLine();
			//    if (!program.Parse(cmd))
			//        break;
			//}
		}


		private DatFileManager fManager;

		public bool Parse(string cmd)
		{
			var parser = new CmdParser(cmd);
			switch (parser.Command)
			{
				case "exit":
					return false;
				case "open":
					Open(parser);
					break;
				case "write":
					Write(parser);
					break;
				case "get":
					Get(parser);
					break;
			}
			return true;
		}


		public void Open(CmdParser parser)
		{
			var fileName = parser.GetAttribute("file") ?? parser.GetAttribute(0);
			fManager = new DatFileManager(fileName);
		}

		public void Write(CmdParser parser)
		{
			var dirPath = parser.GetAttribute("dir") ?? parser.GetAttribute(0);

			var files = Directory.GetFiles(dirPath);
			foreach (var file in files.Select((v, i) => new { v, i }))
			{
				byte[] data;
				using (var fs = new FileStream(file.v, FileMode.Open, FileAccess.Read))
				{
					data = new byte[fs.Length];
					fs.Read(data, 0, data.Length);
					fs.Close();
				}
				fManager.Write(data);
				Console.WriteLine("{0}/{1}".FormatString(file.i + 1, files.Length));
			}
		}

		public void Get(CmdParser parser)
		{
			var start = parser.GetAttribute("start") ?? parser.GetAttribute(0);
			var istart = start.ToInt();

			var dat = fManager.GetBytes(istart);
		}
	}
}
