using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageManagerLib.Imager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ImageManagerTest;

namespace ImageManagerLib.Imager.Tests
{
    [TestClass()]
    public class ImageManagerTests
    {
        private const string outfile = ":memory:";

        public ImageManager CreateImagaManager()
        {
            var imageManager = new ImageManager(outfile);

            imageManager.CreateTable();

            imageManager.CreateDirectory("dir", "/");
            imageManager.CreateDirectory("subdir", "/dir");
            imageManager.CreateDirectory("subdir2", "/dir/subdir/");
            imageManager.CreateDirectory("subdir3", "/dir");

            // For Failed
            imageManager.CreateDirectory("subdir3", "/dir");
            imageManager.CreateDirectory("subdir3", "/dir2");

            imageManager.CreateImage("test1.jpg", "/", Constants.filename);
            imageManager.CreateImage("test2.jpg", "/", Constants.filename);
            imageManager.CreateImage("test3.jpg", "/dir", Constants.filename);

            return imageManager;
        }

        [TestMethod()]
        public void CreateDirectoryTest()
        {
            string[][] excepted = {
                new string[] { "1", "0", "dir" },
                new string[] { "2", "1", "subdir" },
                new string[] { "3", "2", "subdir2" },
                new string[] { "4", "1", "subdir3" }
            };

            var imageManager = CreateImagaManager();
            var sqlite = imageManager.Sqlite;

            var values = sqlite.GetValues("Directories");
            for (int i = 0; i < values.Length; i++)
            {
                CollectionAssert.AreEqual(excepted[i], values[i]);
            }
        }

        [TestMethod()]
        public void CreateImageTest()
        {
            string ansBase64 = ReadToEnd(Constants.base64path);
            string thumbAns = ReadToEnd(Constants.thumbBase64path);

            string[][] excepted = {
                new string[] { "1", "0", "test1.jpg", ansBase64, "" },
                new string[] { "2", "0", "test2.jpg", ansBase64, "" },
                new string[] { "3", "1", "test3.jpg", ansBase64, "" },
            };

            var imageManager = CreateImagaManager();
            var sqlite = imageManager.Sqlite;

            var values = sqlite.GetValues("Files");
            for (int i = 0; i < values.Length; i++)
            {
                CollectionAssert.AreEqual(excepted[i], values[i]);
            }
        }

        public string ReadToEnd(string path)
        {
            string ans = string.Empty; 
            using (var sr = new StreamReader(path))
                ans = sr.ReadToEnd();
            return ans;
        }

        [TestMethod()]
        public void GetDirectoriesTestRoot()
        {
            DataFileInfo[] excepted = {
                new DataFileInfo(1, 0, "dir", DataFileType.Dir)
            };

            var imageManager = CreateImagaManager();
            var dataFileInfos = imageManager.GetDirectories();

            CollectionAssert.AreEqual(excepted, dataFileInfos);

            return;
        }

        [TestMethod()]
        public void GetDirectoriesTestOthers()
        {
            var root = new DataFileInfo(1, 0, "dir", DataFileType.Dir);
            DataFileInfo[] excepted = {
                new DataFileInfo(2, 1, "subdir", DataFileType.Dir),
                new DataFileInfo(4, 1, "subdir3", DataFileType.Dir)
            };

            var imageManager = CreateImagaManager();
            var dataFileInfos = imageManager.GetDirectories(root);

            CollectionAssert.AreEqual(excepted, dataFileInfos);

            return;
        }

        [TestMethod()]
        public void GetFilesTest()
        {
            var root = new DataFileInfo(0, 0, "", DataFileType.Dir);
            DataFileInfo[] excepted = {
                new DataFileInfo(1, 0, "test1.jpg", DataFileType.File)
                {
                    Image = new byte[] { 1, 2, 3 },
                    Thumbnail = new byte[] { 1, 2, 3 },
                },
                new DataFileInfo(2, 0, "test2.jpg", DataFileType.File)
                {
                    Image = new byte[] { 1, 2, 3 },
                    Thumbnail = new byte[] { 1, 2, 3 },
                }
            };

            var imageManager = CreateImagaManager();
            var dataFileInfos = imageManager.GetFiles(root);

            CollectionAssert.AreEqual(excepted, dataFileInfos);

            return;
        }
    }
}