using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileManagerLib.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.File.Tests
{
    [TestClass()]
    public class DirectorySearcherTests
    {
        
        [TestMethod()]
        public void GetAllDirectoriesTest()
        {
            var exp = new string[] {
                "TestData\\File",
                "TestData\\MimeType",
                "TestData\\Path",
                "TestData\\Properties",
                "TestData\\File\\Exceptions",
                "TestData\\File\\Json",
            };
            var dirs = DirectorySearcher.GetAllDirectories("TestData");
            CollectionAssert.AreEqual(exp, dirs);
        }

        [TestMethod()]
        public void GetAllFilesTest()
        {
            var exp = new string[] {
                "TestData\\FileManagerLib.csproj",
                "TestData\\packages.config",
                "TestData\\test.txt",
                "TestData\\File\\ByteLoader.cs",
                "TestData\\File\\DirectorySearcher.cs",
                "TestData\\MimeType\\MimeTypeMap.cs",
                "TestData\\Path\\PathItem.cs",
                "TestData\\Path\\PathSplitter.cs",
                "TestData\\Properties\\AssemblyInfo.cs",
                "TestData\\File\\Exceptions\\FileExistedException.cs",
                "TestData\\File\\Json\\AbstractJsonResourceManager.cs",
                "TestData\\File\\Json\\JsonFileManager.cs",
                "TestData\\File\\Json\\JsonStructureManager.cs",
                "TestData\\File\\Json\\Structures.cs",
            };
            var files = DirectorySearcher.GetAllFiles("TestData");
            CollectionAssert.AreEqual(exp, files);
        }

        [TestMethod()]
        public void CountFilesTest()
        {
            int exp = 9;
            var count = DirectorySearcher.CountFiles(new string[] { "TestData\\File", "TestData\\Path" });
            Assert.AreEqual(exp, count);
        }
    }
}