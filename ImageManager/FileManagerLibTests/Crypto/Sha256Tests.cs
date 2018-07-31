using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileManagerLib.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileManagerLib.Crypto.Tests
{
    [TestClass()]
    public class Sha256Tests
    {

        private readonly byte[] testData = Encoding.UTF8.GetBytes("Hello,World!!");
        private const string expectedHash = "899948C9C72B65EBB59A8D01EE4DB68BABCF91D55C6D93DE66637EA869A3EF1D";

        [TestMethod()]
        public void GetSha256Test()
        {
            var hash = Sha256.GetSha256(testData);
            Assert.AreEqual(expectedHash, hash);
        }

        [TestMethod()]
        public void GetSha256Test1()
        {
            string hash = "";
            using (var stream = new MemoryStream(testData))
            {
                hash = Sha256.GetSha256(stream);
            }
            Assert.AreEqual(expectedHash, hash);
        }
    }
}