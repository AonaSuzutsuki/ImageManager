using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManager.Models
{
    public class ImageManager
    {
        private SQLiteWrapper sqlite;

        public ImageManager(string filename)
        {
            sqlite = new SQLiteWrapper(filename);
        }

        public string[] GetDirectories(string dirName)
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

        public string[] GetFiles(string dirName)
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
    }
}
