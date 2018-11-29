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

            //Console.WriteLine("Write.");
            long start = Environment.WorkingSet;
            var (times, memory) = program.WriteImageManagerMeasure();
            program.WriteTimes("ImageManagerMeasure", times);
            program.ShowMemories(start, memory);
            var (times2, memory2) = program.WriteResXMeasure();
            program.WriteTimes("ResXMeasure", times2);
            program.ShowMemories(start, memory2);


            //Console.WriteLine("Read.");
            //long start = Environment.WorkingSet;
            //times = program.ReadImageManagerMeasure();
            //program.WriteTimes("ImageManagerMeasure", times);
            //times = program.ReadResXMeasure();
            //program.WriteTimes("ResXMeasure", times);
            //GC.Collect();
            //program.ShowMmeory(start, Environment.WorkingSet);
            Console.ReadLine();
        }

        public void ShowMmeory(long start, long end)
        {
            double startd = start / 1024d / 1024d;
            double endd = end / 1024d / 1024d;
            Console.WriteLine("= {0} - {1}", endd.ToString("N0"), startd.ToString("N0"));
        }

        public void ShowMemories(long start, long[] memories)
        {
            foreach (var memory in memories)
                ShowMmeory(start, memory);
        }

        public void WriteTimes(string name, long[] times)
        {
            long avg = 0;
            Console.WriteLine("{0}".FormatString(name));
            foreach (var time in times)
            {
                avg += time;
                Console.WriteLine("{0}".FormatString(time));
            }
            avg = avg / times.Length;
            Console.WriteLine("AVG: {0}ms".FormatString(avg));
        }

        public double ConvertSeconds(long msec)
        {
            return (double)msec / (double)1000;
        }

        public (long[], long[]) WriteResXMeasure()
        {
			var fileName = "test.resx";
			if (File.Exists(fileName))
				File.Delete(fileName);

			var manager = new ResXResourceWriter(fileName);
            var files = Directory.GetFiles("TestData/Zip");
            var timeList = new List<long>();
            var memory = new List<long>();

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
                GC.Collect();
                memory.Add(Environment.WorkingSet);
                timeList.Add(stopWatch.ElapsedMilliseconds);
            }
            manager.Dispose();

            return (timeList.ToArray(), memory.ToArray());
        }
        public (long[], long[]) WriteImageManagerMeasure()
        {
            var manager = new FileManagerLib.File.Json.JsonFileManager("test.dat", true, true);
            var files = Directory.GetFiles("TestData/Zip");
            var timeList = new List<long>();
            var memory = new List<long>();

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
                GC.Collect();
                memory.Add(Environment.WorkingSet);
                timeList.Add(stopWatch.ElapsedMilliseconds);
            } 
            manager.Dispose();

            return (timeList.ToArray(), memory.ToArray());
        }

        public long[] ReadResXMeasure()
        {
            var instanceStopWatch = new Stopwatch();
            instanceStopWatch.Start();
            var manager = new ResXResourceSet("TestData/Dat/large.resx");
            instanceStopWatch.Stop();
            
            var readStopWatch = new Stopwatch();
            readStopWatch.Start();
            foreach (var resource in manager)
            {
                var hash = resource.GetHashCode();
            }
            readStopWatch.Stop();

            var times = new long[2] { instanceStopWatch.ElapsedMilliseconds, readStopWatch.ElapsedMilliseconds };

            return times;
        }

        public long[] ReadImageManagerMeasure()
        {
            var instanceStopWatch = new Stopwatch();
            instanceStopWatch.Start();
            var manager = new FileManagerLib.File.Json.JsonFileManager("TestData/Dat/large.dat");
            instanceStopWatch.Stop();


            var readStopWatch = new Stopwatch();
            readStopWatch.Start();
            var dirs = manager.GetDirectories("/");
            foreach (var dir in dirs)
            {
                var files = manager.GetResources(dir.Name);
                var hash = files.GetHashCode();
            }
            readStopWatch.Stop();

            var times = new long[2] { instanceStopWatch.ElapsedMilliseconds, readStopWatch.ElapsedMilliseconds };

            return times;
        }
    }
}