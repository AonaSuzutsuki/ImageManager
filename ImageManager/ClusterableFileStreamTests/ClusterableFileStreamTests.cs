using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clusterable.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Clusterable.IO.Tests
{
    [TestClass()]
    public class ClusterableFileStreamTests
    {
        private byte[] exceptedData = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        public ClusterableFileStream MakeFileStream(string filename, FileMode mode, long splitSize = 0, string asmDirPath = null)
        {
            ClusterableFileStream fs;
            if (splitSize > 0)
                fs = new ClusterableFileStream(filename, mode, FileAccess.ReadWrite, FileShare.None, splitSize, asmDirPath);
            else
                fs = new ClusterableFileStream(filename, mode, FileAccess.ReadWrite, FileShare.None, asmDirPath);
            return fs;
        }

        public void WriteTestData(ClusterableFileStream fs)
        {
            fs.Write(exceptedData, 0, exceptedData.Length);
        }

        [TestMethod()]
        public void WriteTest()
        {
            var fs = MakeFileStream("test2.dat", FileMode.Create, 2);
            WriteTestData(fs);
            fs.Dispose();
        }

        [TestMethod()]
        public void ReadTest()
        {
            var fs = MakeFileStream("test2.dat", FileMode.Create);
            WriteTestData(fs);

            fs.Seek(0, SeekOrigin.Begin);
            var res1 = new byte[exceptedData.Length];
            fs.Read(res1, 0, res1.Length);

            fs.Seek(0, SeekOrigin.Begin);
            var res2 = new byte[exceptedData.Length];
            int readCount = 0;
            while (true)
            {
                var rdata = new byte[2];
                int readSize = fs.Read(rdata, 0, rdata.Length);
                if (readSize <= 0)
                    break;
                Buffer.BlockCopy(rdata, 0, res2, readCount, readSize);
                readCount += readSize;
            }
            fs.Dispose();

            CollectionAssert.AreEqual(exceptedData, res1);
            CollectionAssert.AreEqual(exceptedData, res2);
        }

        [TestMethod()]
        public void ReadTest2()
        {
            var dir = Path.GetDirectoryName(typeof(ClusterableFileStreamTests).Assembly.Location);
            var fs = MakeFileStream("AvailableTestData/test2.dat", FileMode.OpenOrCreate, -1, dir);

            fs.Seek(0, SeekOrigin.Begin);
            var res1 = new byte[exceptedData.Length];
            fs.Read(res1, 0, res1.Length);

            fs.Seek(0, SeekOrigin.Begin);
            var res2 = new byte[exceptedData.Length];
            int readCount = 0;
            while (true)
            {
                var rdata = new byte[2];
                int readSize = fs.Read(rdata, 0, rdata.Length);
                if (readSize <= 0)
                    break;
                Buffer.BlockCopy(rdata, 0, res2, readCount, readSize);
                readCount += readSize;
            }
            fs.Dispose();

            CollectionAssert.AreEqual(exceptedData, res1);
            CollectionAssert.AreEqual(exceptedData, res2);
        }
    }
}