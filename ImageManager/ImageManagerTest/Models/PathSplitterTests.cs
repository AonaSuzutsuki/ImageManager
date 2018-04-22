﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManager.Models.Tests
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
            var array = pathItem.ToArray();
            CollectionAssert.AreEqual(excepts, array);
        }

        [TestMethod()]
        public void GetPathTest2()
        {
            var pathItem = PathSplitter.SplitPath(pathTest2);
            var array = pathItem.ToArray();
            CollectionAssert.AreEqual(excepts, array);
        }
    }
}