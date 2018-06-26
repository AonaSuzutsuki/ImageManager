using CommonExtensionLib.Extensions;
using ImageManagerCUI.Parser;
using System;
using FileManagerLib.Filer;
using System.IO;
using System.Linq;
using System.Diagnostics;
using FileManagerLib.Filer.Json;

namespace ImageManagerCUI
{
    public class Program
    {
        public static void Main(string[] args)
        {
			IProgram program;
			if (args[0].Equals("-j"))
				program = new JsonProgram();
			else
				program = new SQLiteProgram();
				

            while (true)
            {
                Console.Write("> ");
                string cmd = Console.ReadLine();
                //try
                //{
                //    if (!program.Parse(cmd))
                //        break;
                //}
                //catch (Exception e)
                //{
                //    Console.WriteLine(e.Message);
                //    Console.WriteLine(e.StackTrace);
                //}
                var sw = new Stopwatch();
                sw.Start();
                if (!program.Parse(cmd))
                    break;
                sw.Stop();
                var msec = sw.ElapsedMilliseconds;
                Console.WriteLine("{0}ms".FormatString(msec));
            }
        }
    }


	public interface IProgram
	{
		bool Parse(string cmd);
	}

	public class JsonProgram : IProgram
	{
		JsonFileManager fileManager;

		public bool Parse(string cmd)
		{
			var parser = new CmdParser(cmd);
            switch (parser.Command)
            {
				case "exit":
					fileManager?.Dispose();
                    return false;
                case "gc":
                    GC.Collect();
                    break;
                case "close":
					fileManager?.Dispose();
                    break;
                case "make":
                    MakeDatabase(parser);
                    break;
                case "open":
                    LoadDatabase(parser);
                    break;
                case "mkdir":
                    CreateDirectory(parser);
                    break;
                case "deldir":
                    //DeleteDirectory(parser);
                    break;
                case "addfile":
                    //AddFile(parser);
                    break;
                case "addfiles":
                    //AddFiles(parser);
                    break;
                case "delfile":
                    //DeleteFile(parser);
                    break;
                case "writeto":
                    //WriteTo(parser);
                    break;
                case "vacuum":
                    //imageManager.DataVacuum();
                    break;
                case "trace":
                    Trace(parser);
                    break;
            }
            return true;
		}



		public void MakeDatabase(CmdParser parser)
        {
            var dbFilename = parser.GetAttribute("file") ?? parser.GetAttribute(0);
			fileManager = new JsonFileManager(dbFilename, true);
        }

		public void LoadDatabase(CmdParser parser)
        {
            var dbFilename = parser.GetAttribute("file") ?? parser.GetAttribute(0);
			fileManager= new JsonFileManager(dbFilename, false);
        }

		public void CreateDirectory(CmdParser parser)
        {
            var fullPath = parser.GetAttribute("name") ?? parser.GetAttribute(0);

			var succ = fileManager.CreateDirectory(fullPath);
            if (succ.Item1)
                Console.WriteLine("Success to mkdir {0} on {1}.", fullPath, succ.Item2);
            else
                Console.WriteLine("Failed to mkdir: {0}.", succ.Item2);
        }

		public void Trace(CmdParser parser)
        {
            var type = parser.GetAttribute("type") ?? parser.GetAttribute(0);

            switch (type)
            {
                case "d":
					Console.WriteLine(fileManager.TraceDirs());
                    break;
                case "f":
					Console.WriteLine(fileManager.TraceFiles());
                    break;
                default:
					Console.WriteLine(fileManager);
                    break;
            }
        }
	}

	public class SQLiteProgram : IProgram
	{
		private string dbFilename;
        private IFileManager imageManager;


        public bool Parse(string cmd)
        {
            var parser = new CmdParser(cmd);
            switch (parser.Command)
            {
                case "exit":
                    Close(parser);
                    return false;
                case "gc":
                    GC.Collect();
                    break;
                case "close":
                    Close(parser);
                    break;
                case "make":
                    MakeDatabase(parser);
                    break;
                case "open":
                    LoadDatabase(parser);
                    break;
                case "mkdir":
                    CreateDirectory(parser);
                    break;
                case "deldir":
                    DeleteDirectory(parser);
                    break;
                case "addfile":
                    AddFile(parser);
                    break;
                case "addfiles":
                    AddFiles(parser);
                    break;
                case "delfile":
                    DeleteFile(parser);
                    break;
                case "writeto":
                    WriteTo(parser);
                    break;
                case "vacuum":
                    imageManager.DataVacuum();
                    break;
                case "trace":
                    Trace(parser);
                    break;
            }
            return true;
        }

        public void Close(CmdParser parser)
        {
            imageManager?.Dispose();
        }

        public void MakeDatabase(CmdParser parser)
        {
            dbFilename = parser.GetAttribute("file") ?? parser.GetAttribute(0);
            imageManager = new FileManager(dbFilename, true);
            imageManager.CreateTable();
        }

        public void LoadDatabase(CmdParser parser)
        {
            dbFilename = parser.GetAttribute("file") ?? parser.GetAttribute(0);
            imageManager = new FileManager(dbFilename, false);
        }

        public void CreateDirectory(CmdParser parser)
        {
            var fullPath = parser.GetAttribute("name") ?? parser.GetAttribute(0);
            //var parent = parser.GetAttribute("parent") ?? parser.GetAttribute(1) ?? "/";

            var succ = imageManager.CreateDirectory(fullPath);
            if (succ.Item1)
                Console.WriteLine("Success to mkdir {0} on {1}.", fullPath, succ.Item2);
            else
                Console.WriteLine("Failed to mkdir: {0}.", succ.Item2);
        }

        public void DeleteDirectory(CmdParser parser)
        {
            var fullPath = parser.GetAttribute("name") ?? parser.GetAttribute(0);
            //var parent = parser.GetAttribute("parent") ?? parser.GetAttribute(1) ?? "/";

            if (fullPath.Substring(0, 1).Equals(":"))
            {
                var id = fullPath.TrimStart(':').ToInt();
                imageManager.DeleteDirectory(id);
            }
            else
            {
                imageManager.DeleteDirectory(fullPath);
            }
        }

        public void AddFile(CmdParser parser)
        {
            var fullPath = parser.GetAttribute("name") ?? parser.GetAttribute(0);
            var filePath = parser.GetAttribute("file") ?? parser.GetAttribute(1);
            //var parent = parser.GetAttribute("parent") ?? parser.GetAttribute(2) ?? "/";

            imageManager.CreateImage(fullPath, filePath);
        }

        public void AddFiles(CmdParser parser)
        {
            var dirPath = parser.GetAttribute("dir") ?? parser.GetAttribute(0);
            var parent = parser.GetAttribute("parent") ?? parser.GetAttribute(1) ?? "/";

            var files = Directory.GetFiles(dirPath);
            imageManager.CreateImages(parent, files);
        }

        public void DeleteFile(CmdParser parser)
        {
            var fullPath = parser.GetAttribute("name") ?? parser.GetAttribute(0);
            //var parent = parser.GetAttribute("parent") ?? parser.GetAttribute(1) ?? "/";

            if (fullPath.Substring(0, 1).Equals(":"))
            {
                var id = fullPath.TrimStart(':').ToInt();
                imageManager.DeleteFile(id);
            }
            else
            {
                imageManager.DeleteFile(fullPath);
            }
        }

        public void WriteTo(CmdParser parser)
        {
            var fullPath = parser.GetAttribute("name") ?? parser.GetAttribute(0);
            var outFilePath = parser.GetAttribute("out") ?? parser.GetAttribute(1);
            //var parent = parser.GetAttribute("parent") ?? parser.GetAttribute(2) ?? "/";

            if (fullPath.Substring(0, 1).Equals(":"))
            {
                var id = fullPath.TrimStart(':').ToInt();
                imageManager.WriteToFile(id, outFilePath);
            }
            else
            {
                imageManager.WriteToFile(fullPath, outFilePath);
            }
        }

        public void Trace(CmdParser parser)
        {
            var type = parser.GetAttribute("type") ?? parser.GetAttribute(0);

            switch (type)
            {
                case "d":
                    Console.WriteLine(imageManager.TraceDirs());
                    break;
                case "f":
                    Console.WriteLine(imageManager.TraceFiles());
                    break;
                default:
                    Console.WriteLine(imageManager);
                    break;
            }
        }
	}
}
