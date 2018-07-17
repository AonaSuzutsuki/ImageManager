using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClusterableFileStream
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = CommonCoreLib.Path.PathConverter.ToAbsolutePath("test.dat", () => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
			var fs = new ClusterableFileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
			fs.SplitSize = 11;

			var data = new byte[]
			{
				0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10
			};
			fs.Write(data, 0, data.Length);
			fs.Seek(0, SeekOrigin.Begin);
			var rdata = new byte[1024];
			int rc = fs.Read(rdata, 0, rdata.Length);

			fs.Dispose();
			fs.Seek(0, SeekOrigin.Begin);
        }
    }
}
