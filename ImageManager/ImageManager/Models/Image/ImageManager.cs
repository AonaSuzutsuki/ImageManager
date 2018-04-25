using CommonLib.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManager.Models.Image
{
    public class ImageManager
    {
        private SQLiteWrapper sqlite;
        #if DEBUG
        public SQLiteWrapper Sqlite { get => sqlite; }
        #endif

        public ImageManager(string filename)
        {
            sqlite = new SQLiteWrapper(filename);
        }

        public string[] GetDirectories(string dirName = null)
        {
            if (string.IsNullOrEmpty(dirName))
            {
                // GetRootDirs
                return null;
            }
            else
            {
                // GetDirsFromDirname
                return null;
            }
        }

        public string[] GetFiles(string dirName = null)
        {
            if (string.IsNullOrEmpty(dirName))
            {
                // GetRootFiles
                return null;
            }
            else
            {
                // GetFilesFromDirname
                return null;
            }
        }

        public void CreateDirectory(string dirName, string parent)
        {
            var pathItem = Path.PathSplitter.SplitPath(parent);
            int dirCount = sqlite.GetValues("Directories").Length + 1;

            int dirId = 0;
            foreach (var path in pathItem.ToArray())
            {
                var list = sqlite.GetValues("Directories", "Parent = {0} and Name = '{1}'".FormatString(dirId, path));
                if (list.Length > 0)
                {
                    dirId = list[0][0].ToInt();
                }
            }
            sqlite.InsertValue("Directories", dirCount.ToString(), dirId.ToString(), dirName);
        }
    }
}
