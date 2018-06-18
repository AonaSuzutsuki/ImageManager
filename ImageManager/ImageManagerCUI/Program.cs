using CommonExtensionLib.Extensions;
using ImageManagerCUI.Parser;
using System;
using FileManagerLib.Filer;
using System.IO;
using System.Linq;

namespace ImageManagerCUI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var program = new Program();

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
                if (!program.Parse(cmd))
                    break;
            }
        }


        private string dbFilename;
        private FileManager imageManager;


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
            var dirName = parser.GetAttribute("name") ?? parser.GetAttribute(0);
            var parent = parser.GetAttribute("parent") ?? parser.GetAttribute(1) ?? "/";

            var succ = imageManager.CreateDirectory(dirName, parent);
            if (succ.Item1)
                Console.WriteLine("Success to mkdir {0} on {1}.", dirName, parent);
            else
                Console.WriteLine("Failed to mkdir: {0}.", succ.Item2);
        }

        public void DeleteDirectory(CmdParser parser)
        {
            var dirName = parser.GetAttribute("name") ?? parser.GetAttribute(0);
            var parent = parser.GetAttribute("parent") ?? parser.GetAttribute(1) ?? "/";

            if (dirName.Substring(0, 1).Equals(":"))
            {
                var id = dirName.TrimStart(':').ToInt();
                imageManager.DeleteDirectory(id);
            }
            else
            {
                imageManager.DeleteDirectory(dirName, parent);
            }
        }

        public void AddFile(CmdParser parser)
        {
            var fileName = parser.GetAttribute("name") ?? parser.GetAttribute(0);
            var filePath = parser.GetAttribute("file") ?? parser.GetAttribute(1);
            var parent = parser.GetAttribute("parent") ?? parser.GetAttribute(2) ?? "/";

            imageManager.CreateImage(fileName, parent, filePath);
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
            var fileName = parser.GetAttribute("name") ?? parser.GetAttribute(0);
            var parent = parser.GetAttribute("parent") ?? parser.GetAttribute(1) ?? "/";


            if (fileName.Substring(0, 1).Equals(":"))
            {
                var id = fileName.TrimStart(':').ToInt();
                imageManager.DeleteFile(id);
            }
            else
            {
                imageManager.DeleteFile(fileName, parent);
            }
        }

        public void WriteTo(CmdParser parser)
        {
            var fileName = parser.GetAttribute("name") ?? parser.GetAttribute(0);
            var outFilePath = parser.GetAttribute("out") ?? parser.GetAttribute(1);
            var parent = parser.GetAttribute("parent") ?? parser.GetAttribute(2) ?? "/";

            if (fileName.Substring(0, 1).Equals(":"))
            {
                var id = fileName.TrimStart(':').ToInt();
                imageManager.WriteToFile(id, outFilePath);
            }
            else
            {
                imageManager.WriteToFile(fileName, parent, outFilePath);
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
