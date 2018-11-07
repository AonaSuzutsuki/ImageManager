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

            Console.WriteLine("Enter to start.");
            Console.ReadLine();


            Console.WriteLine("Write.");
            var times = program.WriteImageManagerMeasure();
            program.WriteTimes("ImageManagerMeasure", times);
            times = program.WriteResXMeasure();
            program.WriteTimes("ResXMeasure", times);


            Console.WriteLine("Read.");
            Console.ReadLine();
        }

        public void WriteTimes(string name, long[] times)
        {
            long avg = 0;
            Console.WriteLine("{0}".FormatString(name));
            foreach (var time in times)
            {
                avg += time;
                Console.WriteLine("  {0}ms".FormatString(time));
            }
            avg = avg / times.Length;
            Console.WriteLine("AVG: {0}ms".FormatString(avg));
        }

        public double ConvertSeconds(long msec)
        {
            return (double)msec / (double)1000;
        }

        public long[] WriteResXMeasure()
        {
			var fileName = "test.resx";
			if (File.Exists(fileName))
				File.Delete(fileName);

			var manager = new ResXResourceWriter(fileName);
            var files = Directory.GetFiles("TestData/Zip");
            var timeList = new List<long>();

            foreach (var index in new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                foreach (var file in files)
                {
                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var data = new byte[fs.Length];
                        fs.Read(data, 0, data.Length);
                        manager.AddResource("{0}/{1}".FormatString(index, Path.GetFileName(file)), data);
                    }
                }

                stopWatch.Stop();
                timeList.Add(stopWatch.ElapsedMilliseconds);
            }
            manager.Dispose();

            return timeList.ToArray();
        }
        public long[] WriteImageManagerMeasure()
        {
            var manager = new FileManagerLib.File.Json.JsonFileManager("test.dat", true, true);
            var files = Directory.GetFiles("TestData/Zip");
            var timeList = new List<long>();
            
            foreach (var index in new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                manager.CreateDirectory("/{0}".FormatString(index));
                foreach (var file in files)
                {
                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var data = new byte[fs.Length];
                        fs.Read(data, 0, data.Length);
                        //var base64 = Encoding.UTF8.GetBytes(Convert.ToBase64String(data));
                        manager.WriteBytes("{0}/{1}".FormatString(index, Path.GetFileName(file)), data);
                    }
                }

                stopWatch.Stop();
                timeList.Add(stopWatch.ElapsedMilliseconds);
            } 
            manager.Dispose();

            return timeList.ToArray();
        }

        public long[] ReadResXMeasure()
        {
            var manager = new ResXResourceReader("TestData/Dat/large.resx");
            return null;
        }

        public long[] ReadImageManagerMeasure()
        {
            return null;
        }
    }
}
