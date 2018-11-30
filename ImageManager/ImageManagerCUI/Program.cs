using CommonExtensionLib.Extensions;
using ImageManagerCUI.Parser;
using System;
using FileManagerLib.File;
using System.IO;
using System.Linq;
using System.Diagnostics;
using FileManagerLib.File.Json;
using FileManagerLib.Extensions.Path;
using System.Text;
using System.Collections.Generic;

namespace ImageManagerCUI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var program = new ArgumentsProgram();
                program.Parse(args);
            }
            else
            {
                var program = new JsonProgram();
                while (true)
                {
                    Console.Write("> ");
                    string cmd = Console.ReadLine();
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
    }
   

    public class ArgumentsProgram
    {
        public void Parse(string[] args)
        {
            var parser = new EnvArgumentParser(args);
            if (parser.IsInsert())
            {
                Insert(parser);
            }
            else if (parser.IsExtract())
            {
                Extract(parser);
            }
        }

        public void Insert(EnvArgumentParser parser)
        {
            var outFilename = parser.GetOutputFilepath();
            var targetFiles = parser.GetValues();

            var fileManager = new JsonFileManager(outFilename, true);
            fileManager.WriteIntoResourceProgress += FileManager_WriteIntoResourceProgress;

            fileManager.CreateFilesOnDirectories("/", targetFiles);

            fileManager.Dispose();
        }

        public void Extract(EnvArgumentParser parser)
        {
            var outFilename = parser.GetOutputFilepath();
            var targetFile = parser.GetValue();

            var fileManager = new JsonFileManager(targetFile);
            fileManager.WriteToFilesProgress += FileManager_WriteIntoResourceProgress;
            fileManager.WriteToDir("/", outFilename);
        }

        private void FileManager_WriteIntoResourceProgress(object sender, JsonResourceManager.ReadWriteProgressEventArgs eventArgs)
        {
            Console.WriteLine("{0}/{1}", eventArgs.CompletedNumber, eventArgs.FullNumber);
        }
    }


	public class JsonProgram
	{
		JsonFileManager fileManager;

		private Dictionary<string, Tuple<Action<CmdParser>, string, string>> actionMap;
		private Dictionary<string, Tuple<Func<CmdParser, bool>, string, string>> funcMap;

		public JsonProgram()
		{
			funcMap = new Dictionary<string, Tuple<Func<CmdParser, bool>, string, string>>()
			{
				{ "exit", new Tuple<Func<CmdParser, bool>, string, string>((parser) => {
					fileManager?.Dispose();
					return false;
				}, "", "Exit this program.") },
			};
			actionMap = new Dictionary<string, Tuple<Action<CmdParser>, string, string>>()
			{
				{ "gc", new Tuple<Action<CmdParser>, string, string>(parser => GC.Collect(), "", "Run GC.Collect.") },
				{ "help", new Tuple<Action<CmdParser>, string, string>(ShowHelp, "[comamnd]", "Show the help.") },
				{ "close", new Tuple<Action<CmdParser>, string, string>(parser => fileManager?.Dispose(), "", "Close and release file manager.") },
				{ "make", new Tuple<Action<CmdParser>, string, string>(MakeDatabase, "", "Create new resource database.") },
				{ "open", new Tuple<Action<CmdParser>, string, string>(LoadDatabase, "", "Open existed resource database.") },
				{ "mkdir", new Tuple<Action<CmdParser>, string, string>(CreateDirectory, "", "Create new directory into database.") },
				{ "deldir", new Tuple<Action<CmdParser>, string, string>(DeleteDirectory, "", "") },
				{ "addfile", new Tuple<Action<CmdParser>, string, string>(AddFile, "", "") },
				{ "addfiles", new Tuple<Action<CmdParser>, string, string>(AddFiles, "", "") },
				{ "delfile", new Tuple<Action<CmdParser>, string, string>(DeleteFile, "", "") },
				{ "getfiles", new Tuple<Action<CmdParser>, string, string>(GetFiles, "", "") },
				{ "getdirs", new Tuple<Action<CmdParser>, string, string>(GetDirs, "", "") },
				{ "writetofile", new Tuple<Action<CmdParser>, string, string>(WriteTo, "", "") },
				{ "writetodir", new Tuple<Action<CmdParser>, string, string>(WriteToDir, "", "") },
				{ "vacuum", new Tuple<Action<CmdParser>, string, string>(parser => fileManager.DataVacuum(), "", "") },
				{ "save", new Tuple<Action<CmdParser>, string, string>(parser => fileManager.Save(), "", "") },
				{ "trace", new Tuple<Action<CmdParser>, string, string>(Trace, "", "") },
			};
		}

		public bool Parse(string cmd)
		{
			var parser = new CmdParser(cmd);
			var command = parser.Command;
			if (funcMap.ContainsKey(command))
				return funcMap[command].Item1(parser);
			if (actionMap.ContainsKey(command))
				actionMap[command].Item1(parser);
			return true;
		}


		public void ShowHelp(CmdParser parser)
		{
			var sb = new StringBuilder();
			var command = parser.GetAttribute("command") ?? parser.GetAttribute(0);
			if (command != null)
			{
				
			}
			else
			{
				foreach (var tuple in funcMap)
                {
					sb.AppendFormat("\t{0}\t\t{1}\n", tuple.Key, tuple.Value.Item2, tuple.Value.Item3);
                }

				foreach (var tuple in actionMap)
                {
					sb.AppendFormat("\t{0}\t\t{1}\n", tuple.Key, tuple.Value.Item2, tuple.Value.Item3);
                }
			}

			Console.WriteLine(sb);
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

		protected void Initialize()
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
        
		void FileManager_WriteProgress(object sender, JsonResourceManager.ReadWriteProgressEventArgs eventArgs)
		{
			if (eventArgs.IsCompleted)
				Console.WriteLine("{0}/{1} ({2}%)\t{3}".FormatString(eventArgs.CompletedNumber, eventArgs.FullNumber, eventArgs.Percentage, eventArgs.CurrentFilepath));
			else
				Console.WriteLine("Failed {0}".FormatString(eventArgs.CurrentFilepath));
		}

	}
}
