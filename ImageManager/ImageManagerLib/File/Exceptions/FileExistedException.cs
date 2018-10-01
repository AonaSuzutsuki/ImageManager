using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.File.Exceptions
{
    public class FileDirectoryExistedException : IOException
    {
        public FileDirectoryExistedException(string message) : base(message) { }
    }
}
