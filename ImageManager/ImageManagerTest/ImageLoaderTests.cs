using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageManagerLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using FileManagerLib.Filer;
using ImageManagerTest;

namespace ImageManagerLib.Tests
{
    [TestClass()]
    public class ImageLoaderTests
    {
        [TestMethod()]
        public void GetBytesTest()
        {
            var imageInfo = ImageLoader.FromImageFile(Constants.filename);
            var bytes = imageInfo.Data;
            var base64 = Convert.ToBase64String(bytes);

            string ansBase64;
            using (var sr = new StreamReader(Constants.base64path))
            {
                ansBase64 = sr.ReadToEnd();
            }

            Assert.AreEqual(base64, ansBase64);
        }

        [TestMethod()]
        public void GetBase64Test()
        {
            var imageInfo = ImageLoader.FromImageFile(Constants.filename);
            var base64 = imageInfo.Base64;

            string ansBase64;
            using (var sr = new StreamReader(Constants.base64path))
            {
                ansBase64 = sr.ReadToEnd();
            }

            Assert.AreEqual(base64, ansBase64);
        }
    }
}