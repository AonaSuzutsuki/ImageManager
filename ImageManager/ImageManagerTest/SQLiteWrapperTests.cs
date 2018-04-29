using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageManagerLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManagerLib.Tests
{
    [TestClass()]
    public class SQLiteWrapperTests
    {
        [TestMethod()]
        public void SQLiteWrapperTableTest()
        {
            using (var sqlite = new SQLiteWrapper(":memory:"))
            {
                sqlite.CreateTable("Test", "Id integer primary key, Name text");
                Assert.IsTrue(sqlite.TableExist("Test"));
                Assert.IsFalse(sqlite.TableExist("Test2"));

                sqlite.DeleteTable("Test");
                Assert.IsFalse(sqlite.TableExist("Test"));
                sqlite.Vacuum();
            }
        }

        [TestMethod()]
        public void ArrayToStringTest()
        {
            string[] test = { "0", "1", "2", "3" };
            string excepted = "'0', '1', '2', '3'";

            var actual = SQLiteWrapper.ArrayToString(test);
            Assert.AreEqual(excepted, actual);
        }

        [TestMethod()]
        public void GetValuesTest()
        {
            using (var sqlite = new SQLiteWrapper(":memory:"))
            {
                string[][] excepted = { new string[] { "1", "test1" }, new string[] { "2", "test2" }, };

                sqlite.CreateTable("Test", "Id integer primary key, Name text");
                sqlite.InsertValue("Test", "1", "test1");
                sqlite.InsertValue("Test", "2", "test2");
                var values = sqlite.GetValues("Test");

                for (int i = 0; i < values.Length; i++)
                {
                    CollectionAssert.AreEqual(excepted[i], values[i]);
                }
            }
        }

        [TestMethod()]
        public void GetValuesTermTest()
        {
            using (var sqlite = new SQLiteWrapper(":memory:"))
            {
                string[][] excepted = { new string[] { "1", "test1" } };

                sqlite.CreateTable("Test", "Id integer primary key, Name text");
                sqlite.InsertValue("Test", "1", "test1");
                sqlite.InsertValue("Test", "2", "test2");
                var values = sqlite.GetValues("Test", "id = 1");

                for (int i = 0; i < values.Length; i++)
                {
                    CollectionAssert.AreEqual(excepted[i], values[i]);
                }
            }
        }

        [TestMethod()]
        public void UpdateTest()
        {
            using (var sqlite = new SQLiteWrapper(":memory:"))
            {
                string[][] excepted = { new string[] { "1", "test1" } };
                string[][] excepted2 = { new string[] { "1", "test2" } };

                sqlite.CreateTable("Test", "Id integer primary key, Name text");
                sqlite.InsertValue("Test", "1", "test1");
                var values = sqlite.GetValues("Test", "id = 1");
                for (int i = 0; i < values.Length; i++)
                {
                    CollectionAssert.AreEqual(excepted[i], values[i]);
                }

                sqlite.Update("Test", "Name", "test2", "id = 1");
                values = sqlite.GetValues("Test", "id = 1");
                for (int i = 0; i < values.Length; i++)
                {
                    CollectionAssert.AreEqual(excepted2[i], values[i]);
                }
            }
        }
    }
}