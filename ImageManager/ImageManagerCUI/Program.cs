using CommonExtensionLib.Extensions;
using ImageManagerCUI.Parser;
using System;
using FileManagerLib.Filer;
using System.IO;
using System.Linq;
using System.Diagnostics;
using FileManagerLib.Filer.Json;
using FileManagerLib.Extensions.Path;

namespace ImageManagerCUI
{
    public class Program
    {
        public static void Main(string[] args)
        {
			IProgram program = new JsonProgram();

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
				//try
				//{
                    if (!program.Parse(cmd))
                        break;
				//}
				//catch (Exception e)
				//{
				//	Console.WriteLine(e.Message);
				//}
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
                    DeleteDirectory(parser);
                    break;
                case "addfile":
                    AddFile(parser);
                    break;
                case "addfiles":
                    AddFiles(parser);
                    break;
                case "delfile":
                    //DeleteFile(parser);
                    break;
				case "getfiles":
					GetFiles(parser);
					break;
				case "getdirs":
					GetDirs(parser);
                    break;
                case "writetofile":
                    WriteTo(parser);
                    break;
                case "writetodir":
                    WriteToDir(parser);
                    break;
                case "vacuum":
					fileManager.DataVacuum();
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
			fileManager = new JsonFileManager(dbFilename, true, filePath =>
            {
                Console.WriteLine("{0} exist. Are you sure you want to delete this item? [y/n]", filePath);
                Console.Write("> ");
                var ans = Console.ReadLine();
                if (ans.Equals("y"))
				{
                    File.Delete(filePath);
					return true;
				}
				return false;
            });
			Initialize();
            Console.WriteLine("Loaded {0}.", dbFilename);
        }

		public void LoadDatabase(CmdParser parser)
        {
            var dbFilename = parser.GetAttribute("file") ?? parser.GetAttribute(0);
			fileManager= new JsonFileManager(dbFilename, false);
			Initialize();
            Console.WriteLine("Loaded {0}.", dbFilename);
        }

		public void Initialize()
		{
			fileManager.WriteIntoResourceProgress += FileManager_WriteProgress;
			fileManager.WriteToFilesProgress += FileManager_WriteProgress;
		}

		public void CreateDirectory(CmdParser parser)
        {
			var fullPath = parser.GetAttribute("name") ?? parser.GetAttribute(0);

            try
            {
				fileManager.CreateDirectory(fullPath);
                Console.WriteLine("Success to mkdir {0} on {1}.", fullPath, fullPath.GetFilenameAndParent().parent.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to mkdir: {0}.", e.Message);
            }
        }

		public void DeleteDirectory(CmdParser parser)
        {
            var fullPath = parser.GetAttribute("name") ?? parser.GetAttribute(0);
            //var parent = parser.GetAttribute("parent") ?? parser.GetAttribute(1) ?? "/";

            if (fullPath.Substring(0, 1).Equals(":"))
            {
                var id = fullPath.TrimStart(':').ToInt();
				fileManager.DeleteDirectory(id);
            }
            else
            {
				fileManager.DeleteDirectory(fullPath);
            }
        }

		public void AddFile(CmdParser parser)
		{
            var filePath = parser.GetAttribute("file") ?? parser.GetAttribute(0);
            var fullPath = parser.GetAttribute("name") ?? parser.GetAttribute(1);
            //var parent = parser.GetAttribute("parent") ?? parser.GetAttribute(2) ?? "/";

			fileManager.CreateFile(fullPath, filePath);
        }

        public void AddFiles(CmdParser parser)
        {
            var dirPath = parser.GetAttribute("dir") ?? parser.GetAttribute(0);
            var parent = parser.GetAttribute("parent") ?? parser.GetAttribute(1) ?? "/";
            
			fileManager.CreateFiles(parent, dirPath);
        }

        public void DeleteFile(CmdParser parser)
        {
            var fullPath = parser.GetAttribute("name") ?? parser.GetAttribute(0);
            //var parent = parser.GetAttribute("parent") ?? parser.GetAttribute(1) ?? "/";

            if (fullPath.Substring(0, 1).Equals(":"))
            {
                var id = fullPath.TrimStart(':').ToInt();
                fileManager.DeleteFile(id);
            }
            else
            {
                fileManager.DeleteFile(fullPath);
            }
        }

		public void GetFiles(CmdParser parser)
		{
			var did = parser.GetAttribute("id") ?? parser.GetAttribute(0);
			var files = fileManager.GetFiles(did.ToInt());
			foreach (var file in files)
			{
				Console.WriteLine("{0}", file);
			}
		}

		public void GetDirs(CmdParser parser)
		{
			var did = parser.GetAttribute("id") ?? parser.GetAttribute(0);
			var dirs = fileManager.GetDirectories(did.ToInt());
            foreach (var dir in dirs)
            {
                Console.WriteLine("{0}", dir);
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
                //fileManager.WriteToFile(id, outFilePath);
				fileManager.WriteToFile(id, outFilePath);
            }
            else
            {
				fileManager.WriteToFile(fullPath, outFilePath);
                //fileManager.WriteToFile(fullPath, outFilePath);
            }
        }

		public void WriteToDir(CmdParser parser)
		{
			var fullPath = parser.GetAttribute("name") ?? parser.GetAttribute(0);
            var outFilePath = parser.GetAttribute("out") ?? parser.GetAttribute(1);

            if (fullPath.Substring(0, 1).Equals(":"))
            {
                var id = fullPath.TrimStart(':').ToInt();
				fileManager.WriteToDir(id, outFilePath);
            }
            else
            {
				fileManager.WriteToDir(fullPath, outFilePath);
            }
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
        
		void FileManager_WriteProgress(object sender, JsonFileManager.ReadWriteProgressEventArgs eventArgs)
		{
			if (eventArgs.IsCompleted)
				Console.WriteLine("{0}/{1} ({2}%)\t{3}".FormatString(eventArgs.CompletedNumber, eventArgs.FullNumber, eventArgs.Percentage, eventArgs.CurrentFilepath));
			else
				Console.WriteLine("Failed {0}".FormatString(eventArgs.CurrentFilepath));
		}

	}
}
