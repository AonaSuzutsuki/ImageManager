using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClusterableFileStream
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = CommonCoreLib.Path.PathConverter.ToAbsolutePath("c:\\str.dat", () => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            Console.WriteLine(path);
            Console.ReadLine();
        }
    }
}
