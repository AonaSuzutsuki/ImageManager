using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Clusterable.IO
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = CommonCoreLib.Path.PathConverter.ToAbsolutePath("test.dat", () => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            var fs = new ClusterableFileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
            {
                SplitSize = 3
            };

            var data = new byte[]
			{
				0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10
			};
			fs.Write(data, 0, data.Length);
			fs.Seek(2, SeekOrigin.Begin);

            while (true)
            {
                var rdata = new byte[2];
                int readSize = fs.Read(rdata, 0, rdata.Length);
                if (readSize <= 0)
                    break;
                for (int i = 0; i < readSize; i++)
                    Console.WriteLine(rdata[i]);
            }

            //var rdata = new byte[6];
            //int rc = fs.Read(rdata, 0, rdata.Length);

            fs.Dispose();
            Console.ReadLine();
        }
    }
}
