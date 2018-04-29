using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageManagerLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageManagerLib.Path;

namespace ImageManagerLib.Tests
{
    [TestClass()]
    public class PathSplitterTests
    {
        private string pathTest1 = "root/item/subitem";
        private string pathTest2 = "root\\item\\subitem\\";

        private string[] excepts = { "root", "item", "subitem" };

        [TestMethod()]
        public void GetPathTest1()
        {
            var pathItem = PathSplitter.SplitPath(pathTest1);

            Assert.AreEqual(excepts[0], pathItem.GetPath(0));
            Assert.AreEqual(string.Empty, pathItem.GetPath(-1));
            Assert.AreEqual(excepts[2], pathItem.GetLast());
            
            Assert.AreEqual(pathTest1, pathItem.ToString());

            var array = pathItem.ToArray();
            CollectionAssert.AreEqual(excepts, array);
        }

        [TestMethod()]
        public void GetPathTest2()
        {
            var pathItem = PathSplitter.SplitPath(pathTest2);

            Assert.AreEqual(excepts[0], pathItem.GetPath(0));
            Assert.AreEqual(string.Empty, pathItem.GetPath(-1));
            Assert.AreEqual(excepts[2], pathItem.GetLast());

            Assert.AreEqual(pathTest1, pathItem.ToString());

            var array = pathItem.ToArray();
            CollectionAssert.AreEqual(excepts, array);
        }
    }
}