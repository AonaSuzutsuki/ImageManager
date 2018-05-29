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
            Console.WriteLine(parser);
            switch (parser.Command)
            {
                case "exit":
                    return false;
                case "make":
                    MakeDatabase(parser);
                    break;
            }
            return true;
        }

        public void MakeDatabase(CmdParser parser)
        {
            dbFilename = parser.GetAttribute("file");
            imageManager = new ImageManager(dbFilename);
        }

        public void LoadDatabase(CmdParser parser)
        {
            dbFilename = parser.GetAttribute("file");
        }
    }
}
