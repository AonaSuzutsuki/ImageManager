using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileManagerLib.MimeType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.MimeType.Tests
{
    [TestClass()]
    public class MimeTypeMapTests
    {
        [TestMethod()]
        public void GetMimeTypeFromExtensionTest()
        {
            var act1 = MimeTypeMap.GetMimeTypeFromExtension(".mp3");
            var act2 = MimeTypeMap.GetMimeTypeFromExtension(".mp4");
            var act3 = MimeTypeMap.GetMimeTypeFromExtension("");

            var exp1 = "audio/mpeg";
            var exp2 = "video/mp4";
            var exp3 = "application/octet-stream";

            Assert.AreEqual(exp1, act1);
            Assert.AreEqual(exp2, act2);
            Assert.AreEqual(exp3, act3);
        }

        [TestMethod()]
        public void GetMimeTypeTest()
        {
            var act1 = MimeTypeMap.GetMimeType("test.mp3");
            var act2 = MimeTypeMap.GetMimeType("test.mp4");
            var act3 = MimeTypeMap.GetMimeType("test");

            var exp1 = "audio/mpeg";
            var exp2 = "video/mp4";
            var exp3 = "application/octet-stream";

            Assert.AreEqual(exp1, act1);
            Assert.AreEqual(exp2, act2);
            Assert.AreEqual(exp3, act3);
        }
    }
}