using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileManagerLib.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.Extensions.Collections.Tests
{
    [TestClass()]
    public class DictionaryExtensionTests
    {
        [TestMethod()]
        public void ToArrayTest()
        {
            var exp = new string[] { "bbb", "aaa", "ccc" };
            var sorted = new SortedDictionary<int, string>
            {
                { 3, "aaa" },
                { 5, "ccc" },
                { 1, "bbb" }
            };
            var array = sorted.Values.ToArray();
            CollectionAssert.AreEqual(exp, array);
        }
    }
}