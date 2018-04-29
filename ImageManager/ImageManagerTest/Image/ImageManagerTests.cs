using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageManagerLib.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManagerLib.Image.Tests
{
    [TestClass()]
    public class ImageManagerTests
    {
        public ImageManager CreateImagaManager()
        {
            var imageManager = new ImageManager(":memory:");

            imageManager.CreateTable();

            imageManager.CreateDirectory("dir", "/");
            imageManager.CreateDirectory("subdir", "/dir");
            imageManager.CreateDirectory("subdir2", "/dir/subdir/");
            imageManager.CreateDirectory("subdir3", "/dir");

            imageManager.CreateImage("test1.jpg", "/", new byte[] { 1, 2, 3 });
            imageManager.CreateImage("test2.jpg", "/", new byte[] { 1, 2, 3 });
            imageManager.CreateImage("test3.jpg", "/dir", new byte[] { 1, 2, 3 });

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
            string[][] excepted = {
                new string[] { "1", "0", "test1.jpg", Convert.ToBase64String(new byte[] { 1, 2, 3 }), Convert.ToBase64String(new byte[] { 1, 2, 3 }) },
                new string[] { "2", "0", "test2.jpg", Convert.ToBase64String(new byte[] { 1, 2, 3 }), Convert.ToBase64String(new byte[] { 1, 2, 3 }) },
                new string[] { "3", "1", "test3.jpg", Convert.ToBase64String(new byte[] { 1, 2, 3 }), Convert.ToBase64String(new byte[] { 1, 2, 3 }) },
            };

            var imageManager = CreateImagaManager();
            var sqlite = imageManager.Sqlite;

            var values = sqlite.GetValues("Images");
            for (int i = 0; i < values.Length; i++)
            {
                CollectionAssert.AreEqual(excepted[i], values[i]);
            }
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
                new DataFileInfo(1, 0, "test1.jpg", DataFileType.Image)
                {
                    Image = new byte[] { 1, 2, 3 },
                    Thumbnail = new byte[] { 1, 2, 3 },
                },
                new DataFileInfo(2, 0, "test2.jpg", DataFileType.Image)
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