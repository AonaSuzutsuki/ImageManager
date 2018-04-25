using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageManager.Models.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManager.Models.Image.Tests
{
    [TestClass()]
    public class ImageManagerTests
    {
        [TestMethod()]
        public void CreateDirectoryTest()
        {
            string[][] excepted = {
                new string[] { "1", "0", "dir" },
                new string[] { "2", "1", "subdir" },
                new string[] { "3", "2", "subdir2" },
                new string[] { "4", "1", "subdir3" }
            };

            var imageManager = new ImageManager(":memory:");
            var sqlite = imageManager.Sqlite;

            sqlite.CreateTable("Directories", "'Id'	INTEGER NOT NULL UNIQUE, 'Parent'	INTEGER, 'Name'	TEXT NOT NULL, PRIMARY KEY('Id')");

            imageManager.CreateDirectory("dir", "/");
            imageManager.CreateDirectory("subdir", "/dir");
            imageManager.CreateDirectory("subdir2", "/dir/subdir/");
            imageManager.CreateDirectory("subdir3", "/dir");

            var values = sqlite.GetValues("Directories");
            for (int i = 0; i < values.Length; i++)
            {
                CollectionAssert.AreEqual(excepted[i], values[i]);
            }
            return;
        }
    }
}