using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileManagerLib.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileManagerLib.File.Tests
{
    [TestClass()]
    public class ByteLoaderTests
    {
        [TestMethod()]
        public void FromFileTest()
        {
            var exp = new byte[] { 0xEF, 0xBB, 0xBF, 0x61, 0x62, 0x63, 0x64, 0x0D, 0x0A };
            var stream = new FileStream("TestData/test.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
            var data = ByteLoader.FromStream(stream);
            CollectionAssert.AreEqual(exp, data);
        }

        [TestMethod()]
        public void FromFileTest2()
        {
            var data = ByteLoader.FromStream(null);
            CollectionAssert.AreEqual(null, data);
        }
    }
}