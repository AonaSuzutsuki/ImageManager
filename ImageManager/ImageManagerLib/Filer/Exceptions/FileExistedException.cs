using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.Filer.Exceptions
{
    public class FileExistedException : IOException
    {
        public FileExistedException(string message) : base(message) { }
    }
}
