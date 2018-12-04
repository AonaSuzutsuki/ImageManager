using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileManagerLib.CommonPath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.CommonPath.Tests
{
    [TestClass()]
    public class PathItemTests
    {

        private const string ExpectedPath = "/root/dir/subdir";
        
        [TestMethod()]
        public void AddPathTest()
        {
            var pathItem = new PathItem();
            pathItem.AddPath("root");
            pathItem.AddPath("dir");
            pathItem.AddPath("subdir");

            var expectPathItem = PathSplitter.SplitPath(ExpectedPath);

            var expected = "/root/dir/subdir";
            var act = pathItem.ToString();

            Assert.AreEqual(expected, act);
            Assert.AreEqual(expectPathItem, pathItem);
        }

        [TestMethod()]
        public void GetPathTest()
        {
            var pathItem = PathSplitter.SplitPath(ExpectedPath);
            var act1 = pathItem.GetPath(0);
            var act2 = pathItem.GetPath(1);
            var act3 = pathItem.GetPath(2);
            var act4 = pathItem.GetPath(3);
            var exp1 = "root";
            var exp2 = "dir";
            var exp3 = "subdir";
            string exp4 = null;

            Assert.AreEqual(exp1, act1);
            Assert.AreEqual(exp2, act2);
            Assert.AreEqual(exp3, act3);
            Assert.AreEqual(exp4, act4);
        }

        [TestMethod()]
        public void PopTest()
        {
            var pathItem = PathSplitter.SplitPath(ExpectedPath);
            string act = pathItem.Pop();
            var expected = "subdir";

            Assert.AreEqual(expected, act);
        }

        [TestMethod()]
        public void GetLastTest()
        {
            var pathItem = PathSplitter.SplitPath(ExpectedPath);
            var expected = "subdir";
            var act = pathItem.GetLast();

            Assert.AreEqual(expected, act);
        }

        [TestMethod()]
        public void GetPathItemFromTest()
        {
            var pathItem = PathSplitter.SplitPath(ExpectedPath);
            var act = pathItem.GetPathItemFrom(1);
            var expectedPathItem = PathSplitter.SplitPath("/dir/subdir");

            Assert.AreEqual(expectedPathItem, act);
        }

        [TestMethod()]
        public void GetPathItemFromTest1()
        {
            var pathItem = PathSplitter.SplitPath(ExpectedPath);
            var act = pathItem.GetPathItemFrom("root");
            var expectedPathItem = PathSplitter.SplitPath("/dir/subdir");

            Assert.AreEqual(expectedPathItem, act);
        }

        [TestMethod()]
        public void ToArrayTest()
        {
            var pathItem = PathSplitter.SplitPath(ExpectedPath);
            var expectedArray = new string[] { "root", "dir", "subdir" };
            var actArray = pathItem.ToArray();
            
            CollectionAssert.AreEqual(expectedArray, actArray);
        }

        [TestMethod()]
        public void ToArrayTest1()
        {
            var pathItem = PathSplitter.SplitPath(ExpectedPath);
            var expectedArray = new string[] { "root", "dir" };
            var actArray = pathItem.ToArray(1);

            CollectionAssert.AreEqual(expectedArray, actArray);
        }

        [TestMethod()]
        public void ToStringTest()
        {
            var pathItem = PathSplitter.SplitPath(ExpectedPath);
            var pathItemStr = pathItem.ToString();

            Assert.AreEqual(ExpectedPath, pathItemStr);
        }
    }
}