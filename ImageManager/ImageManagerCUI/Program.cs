﻿using CommonExtensionLib.Extensions;
using ImageManagerCUI.Parser;
using System;
using FileManagerLib.Filer;
using System.IO;
using System.Linq;
using System.Diagnostics;
using FileManagerLib.Filer.Json;
using FileManagerLib.Extensions.Path;
using System.Text;

namespace ImageManagerCUI
{
    public class Program
    {
        public static void Main(string[] args)
        {
			AbstractProgram program = new JsonProgram();

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


	public abstract class AbstractProgram
	{
		public abstract bool Parse(string cmd);

		protected virtual void Initialize()
		{
			
		}
	}

	public class JsonProgram : AbstractProgram
	{
		JsonFileManager fileManager;

		public override bool Parse(string cmd)
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
				case "save":
					fileManager.Save();
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
            var checkHash = parser.GetAttribute("hash") ?? parser.GetAttribute(1);
            var isCheckHash = checkHash == null ? true : false;

            fileManager = new JsonFileManager(dbFilename, true, isCheckHash);
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

		protected override void Initialize()
		{
			fileManager.WriteIntoResourceProgress += FileManager_WriteProgress;
			fileManager.WriteToFilesProgress += FileManager_WriteProgress;
            fileManager.VacuumProgress += FileManager_WriteProgress;
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
            var fullPath = parser.GetAttribute("name") ?? parser.GetAttribute(0);

            FileStructure[] fileStructures;
            if (fullPath.Substring(0, 1).Equals(":"))
            {
                var id = fullPath.TrimStart(':').ToInt();
                fileStructures = fileManager.GetFiles(id);
            }
            else
            {
                fileStructures = fileManager.GetFiles(fullPath);
            }
            
			foreach (var file in fileStructures)
                Console.WriteLine("{0}", file);
        }

		public void GetDirs(CmdParser parser)
		{
            var fullPath = parser.GetAttribute("name") ?? parser.GetAttribute(0);

            DirectoryStructure[] directoryStructures;
            if (fullPath.Substring(0, 1).Equals(":"))
            {
                var id = fullPath.TrimStart(':').ToInt();
                directoryStructures = fileManager.GetDirectories(id);
            }
            else
            {
                directoryStructures = fileManager.GetDirectories(fullPath);
            }

            foreach (var dir in directoryStructures)
                Console.WriteLine("{0}", dir);
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
        
		void FileManager_WriteProgress(object sender, AbstractJsonResourceManager.ReadWriteProgressEventArgs eventArgs)
		{
			if (eventArgs.IsCompleted)
				Console.WriteLine("{0}/{1} ({2}%)\t{3}".FormatString(eventArgs.CompletedNumber, eventArgs.FullNumber, eventArgs.Percentage, eventArgs.CurrentFilepath));
			else
				Console.WriteLine("Failed {0}".FormatString(eventArgs.CurrentFilepath));
		}

	}


	public class JsonDataProgram : AbstractProgram
	{
  
		JsonResourceManager fileManager;

		public override bool Parse(string cmd)
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
                case "adddata":
                    AddData(parser);
                    break;
                case "getdata":
                    GetData(parser);
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
            var checkHash = parser.GetAttribute("hash") ?? parser.GetAttribute(1);
            var isCheckHash = checkHash == null ? true : false;

            fileManager = new JsonResourceManager(dbFilename, true, isCheckHash);
            Initialize();
            Console.WriteLine("Loaded {0}.", dbFilename);
        }

        public void LoadDatabase(CmdParser parser)
        {
            var dbFilename = parser.GetAttribute("file") ?? parser.GetAttribute(0);
			fileManager = new JsonResourceManager(dbFilename, false);
            Initialize();
            Console.WriteLine("Loaded {0}.", dbFilename);
        }

        public void AddData(CmdParser parser)
        {
            var text = parser.GetAttribute("text") ?? parser.GetAttribute(0);
            var fullPath = parser.GetAttribute("name") ?? parser.GetAttribute(1);

            fileManager.WriteString(fullPath, text);
        }

        public void GetData(CmdParser parser)
        {
            var fullPath = parser.GetAttribute("name") ?? parser.GetAttribute(0);
            
            var text = fileManager.GetString(fullPath);
            Console.WriteLine(text);
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

}
