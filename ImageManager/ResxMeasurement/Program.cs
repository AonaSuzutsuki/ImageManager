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
            Console.WriteLine("{0}s {1}ms".FormatString(program.ConvertSeconds(program.ImageManagerMeasure()), program.ImageManagerMeasure()));
            Console.ReadLine();
        }

        public double ConvertSeconds(long msec)
        {
            return (double)msec / (double)1000;
        }

        public void ResXMeasure()
        {
            var manager = new ResxManager("test.resx");
        }

        public long ImageManagerMeasure()
        {
            var manager = new FileManagerLib.File.Json.JsonFileManager("test.dat", true);
            var files = Directory.GetFiles("TestData\\Images");

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (var file in files)
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var data = new byte[fs.Length];
                    fs.Read(data, 0, data.Length);
                    manager.WriteBytes("/{0}".FormatString(file), data);
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
