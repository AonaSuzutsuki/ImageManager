using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileManagerLib.Extensions.Path;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileManagerLib.CommonPath;

namespace FileManagerLib.Extensions.Path.Tests
{
    [TestClass()]
    public class PathItemExtensionsTests
    {
        [TestMethod()]
        public void GetFilenameAndParentTest()
        {
            var expParent = new PathItem(new string[] { "opt", "test" });
            var expFilename = "data.dat";

            var (parent, fileName) = "/opt/test/data.dat".GetFilenameAndParent();

            Assert.AreEqual(expParent, parent);
            Assert.AreEqual(expFilename, fileName);
        }
    }
}