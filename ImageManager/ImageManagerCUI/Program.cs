using ImageManagerCUI.Parser;
using ImageManagerLib.Imager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                if (!program.Parse(cmd))
                    break;
            }
        }


        private string dbFilename;
        private ImageManager imageManager;


        public bool Parse(string cmd)
        {
            var parser = new CmdParser(cmd);
            //Console.WriteLine(parser);
            switch (parser.Command)
            {
                case "exit":
                    return false;
                case "make":
                    MakeDatabase(parser);
                    break;
                case "load":
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
                case "delfile":
                    DeleteFile(parser);
                    break;
                case "trace":
                    Trace(parser);
                    break;
            }
            return true;
        }

        public void MakeDatabase(CmdParser parser)
        {
            dbFilename = parser.GetAttribute("file");
            imageManager = new ImageManager(dbFilename, true);
            imageManager.CreateTable();
        }

        public void LoadDatabase(CmdParser parser)
        {
            dbFilename = parser.GetAttribute("file");
            imageManager = new ImageManager(dbFilename, false);
        }

        public void CreateDirectory(CmdParser parser)
        {
            var dirName = parser.GetAttribute("name");
            var parent = parser.GetAttribute("parent") ?? "/";

            var succ = imageManager.CreateDirectory(dirName, parent);
            if (succ.Item1)
                Console.WriteLine("Success to mkdir {0} on {1}.", dirName, parent);
            else
                Console.WriteLine("Failed to mkdir: {0}.", succ.Item2);
        }

        public void DeleteDirectory(CmdParser parser)
        {
            var dirName = parser.GetAttribute("name");
            var parent = parser.GetAttribute("parent") ?? "/";

            imageManager.DeleteDirectory(dirName, parent);
        }

        public void AddFile(CmdParser parser)
        {
            var fileName = parser.GetAttribute("name");
            var filePath = parser.GetAttribute("file");
            var parent = parser.GetAttribute("parent") ?? "/";

            imageManager.CreateImage(fileName, parent, filePath);
        }

        public void DeleteFile(CmdParser parser)
        {
            var fileName = parser.GetAttribute("name");
            var parent = parser.GetAttribute("parent") ?? "/";

            imageManager.DeleteFile(fileName, parent);
        }

        public void Trace(CmdParser parser)
        {
            Console.WriteLine(imageManager);
        }
    }
}
