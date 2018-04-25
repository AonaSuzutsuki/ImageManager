using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using ImageManager.Models.Image;

namespace ImageManager.Models.Tests
{
    [TestClass()]
    public class ImageLoaderTests
    {
        private string filename = "Images/20180415_082736812_iOS_s.jpg";
        private string base64path = "Images/20180415_082736812_iOS_s.txt";

        [TestMethod()]
        public void GetBytesTest()
        {
            var imageInfo = ImageLoader.FromImageFile(filename);
            var bytes = imageInfo.Data;
            var base64 = Convert.ToBase64String(bytes);

            string ansBase64;
            using (var sr = new StreamReader(base64path))
            {
                ansBase64 = sr.ReadToEnd();
            }

            Assert.AreEqual(base64, ansBase64);
        }

        [TestMethod()]
        public void GetBase64Test()
        {
            var imageInfo = ImageLoader.FromImageFile(filename);
            var base64 = imageInfo.Base64;

            string ansBase64;
            using (var sr = new StreamReader(base64path))
            {
                ansBase64 = sr.ReadToEnd();
            }

            Assert.AreEqual(base64, ansBase64);
        }
    }
}