using CommonExtensionLib.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace ResxMeasurement
{
    class Program
    {
        public static void Main(string[] args)
        {
            var program = new Program();
			var time = program.ImageManagerMeasure();
			Console.WriteLine("{0}s {1}ms".FormatString(program.ConvertSeconds(time), time));
			time = program.ResXMeasure();
			Console.WriteLine("{0}s {1}ms".FormatString(program.ConvertSeconds(time), time));
            Console.ReadLine();
        }

        public double ConvertSeconds(long msec)
        {
            return (double)msec / (double)1000;
        }

        public long ResXMeasure()
        {
			var fileName = "test.resx";
			if (File.Exists(fileName))
				File.Delete(fileName);
			var manager = new ResXResourceWriter(fileName);
            var files = Directory.GetFiles("TestData/Images");

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (var file in files)
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var data = new byte[fs.Length];
                    fs.Read(data, 0, data.Length);
                    manager.AddResource("/{0}".FormatString(file), data);
                }
            }
            stopWatch.Stop();
            manager.Dispose();

            return stopWatch.ElapsedMilliseconds;
        }
        public long ImageManagerMeasure()
        {
            var manager = new FileManagerLib.File.Json.JsonFileManager("test.dat", true);
            var files = Directory.GetFiles("TestData/Images");

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (var file in files)
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var data = new byte[fs.Length];
                    fs.Read(data, 0, data.Length);
					var base64 = Convert.ToBase64String(data);
					manager.WriteBytes("/{0}".FormatString(file), System.Text.Encoding.UTF8.GetBytes(base64));
                }
            }
            stopWatch.Stop();
            manager.Dispose();

            return stopWatch.ElapsedMilliseconds;
        }
    }

    public class ResxManager : IDisposable
    {
        private Stream stream;
        private ResXResourceWriter resXResourceWriter;

        public ResxManager(string filePath)
        {
            stream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            resXResourceWriter = new ResXResourceWriter(stream);
        }

        public void Dispose()
        {
            resXResourceWriter.Dispose();
            stream.Dispose();
        }
    }
}
