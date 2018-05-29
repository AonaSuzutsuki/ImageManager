using CommonExtentionLib.Extentions;
using ImageManagerLib.Extentions.Imager;
using ImageManagerLib.SQLite;
using System;
using System.Collections.Generic;
using System.IO;

namespace ImageManagerLib.Imager
{
    public class ImageManager : IDisposable
    {
        private IDatabase sqlite;

        #if DEBUG
        public IDatabase Sqlite { get => sqlite; } // For Test
        #endif

        /// <summary>
        /// Manage database for images.
        /// </summary>
        /// <param name="filename">Database path</param>
        public ImageManager(string filename, bool newFile = false)
        {
            if (newFile)
            {
                if (File.Exists(filename))
                    File.Delete(filename);
            }

            sqlite = new SQLiteWrapper(filename);
        }

        /// <summary>
        /// Initialize.
        /// Create default table.
        /// </summary>
        public void CreateTable()
        {
            //string dirField = "'Id'	INTEGER NOT NULL UNIQUE, 'Parent' INTEGER NOT NULL, 'Name' TEXT NOT NULL, PRIMARY KEY('Id')";
            //string imageField = "'Id' INTEGER NOT NULL UNIQUE, 'Parent' INTEGER NOT NULL, 'Filename' TEXT NOT NULL, 'Data' TEXT NOT NULL, 'Thumbnail' TEXT NOT NULL, PRIMARY KEY('Id')";
            //string ThumbField = "'Id' INTEGER NOT NULL UNIQUE, 'Data' TEXT NOT NULL, PRIMARY KEY('Id')";
            var dirField = new TableFieldList()
            {
                new TableFieldInfo("Id", TableFieldType.Integer, TableFieldAttribute.NotNull, TableFieldAttribute.Unique, TableFieldAttribute.PrimaryKey),
                new TableFieldInfo("Parent", TableFieldType.Integer, TableFieldAttribute.NotNull),
                new TableFieldInfo("Name", TableFieldType.Text, TableFieldAttribute.NotNull)
            };
            var imageField = new TableFieldList()
            {
                new TableFieldInfo("Id", TableFieldType.Integer, TableFieldAttribute.NotNull, TableFieldAttribute.Unique, TableFieldAttribute.PrimaryKey),
                new TableFieldInfo("Parent", TableFieldType.Integer, TableFieldAttribute.NotNull),
                new TableFieldInfo("Filename", TableFieldType.Text, TableFieldAttribute.NotNull),
                new TableFieldInfo("Data", TableFieldType.Text, TableFieldAttribute.NotNull),
                new TableFieldInfo("Type", TableFieldType.Text, TableFieldAttribute.NotNull)
            };
            var ThumbField = new TableFieldList()
            {
                new TableFieldInfo("Id", TableFieldType.Integer, TableFieldAttribute.NotNull, TableFieldAttribute.Unique, TableFieldAttribute.PrimaryKey),
                new TableFieldInfo("Data", TableFieldType.Text, TableFieldAttribute.NotNull)
            };

            sqlite.CreateTable("Directories", dirField);
            sqlite.CreateTable("Files", imageField);
            sqlite.CreateTable("Thumbnails", ThumbField);
        }

        /// <summary>
        /// Get directories.
        /// </summary>
        /// <param name="did">Directory Id.</param>
        /// <returns>Directories of directory id.</returns>
        public DataFileInfo[] GetDirectories(int did = 0)
        {
            var sList = sqlite.GetValues("Directories", "Parent = {0}".FormatString(did));
            var dataFileInfoList = new List<DataFileInfo>(sList.Length);
            foreach (var list in sList)
            {
                int id = list[0].ToInt();
                int parent = list[1].ToInt();
                string filename = list[2];
                var type = DataFileType.Dir;

                var dataFileInfo = new DataFileInfo(id, parent, filename, type);
                dataFileInfoList.Add(dataFileInfo);
            }

            return dataFileInfoList.ToArray();
        }

        /// <summary>
        /// Get directories.
        /// </summary>
        /// <param name="dataFileInfo">DataFileInfo of directory.</param>
        /// <returns>Directories of directory id.</returns>
        public DataFileInfo[] GetDirectories(DataFileInfo dataFileInfo)
        {
            return GetDirectories(dataFileInfo.Id);
        }

        /// <summary>
        /// Get files.
        /// </summary>
        /// <param name="did">Directory Id.</param>
        /// <returns>Files of directory id.</returns>
        public DataFileInfo[] GetFiles(int did = 0)
        {
            var sList = sqlite.GetValues("Files", "Parent = {0}".FormatString(did));
            var dataFileInfoList = new List<DataFileInfo>(sList.Length);
            foreach (var list in sList)
            {
                int id = list[0].ToInt();
                int parent = list[1].ToInt();
                string filename = list[2];
                var type = DataFileType.File;

                var dataFileInfo = new DataFileInfo(id, parent, filename, type)
                {
                    Image = Convert.FromBase64String(list[3]),
                    Thumbnail = Convert.FromBase64String(list[3])
                };

                dataFileInfoList.Add(dataFileInfo);
            }

            return dataFileInfoList.ToArray();
        }

        /// <summary>
        /// Get Files.
        /// </summary>
        /// <param name="dataFileInfo">DataFileInfo of directory.</param>
        /// <returns>Directories of directory id.</returns>
        public DataFileInfo[] GetFiles(DataFileInfo dataFileInfo)
        {
            return GetFiles(dataFileInfo.Id);
        }

        /// <summary>
        /// Create directory to database.
        /// </summary>
        /// <param name="dirName">Directory name</param>
        /// <param name="parent">Parent directory path</param>
        public void CreateDirectory(string dirName, string parent)
        {
            var pathItem = Path.PathSplitter.SplitPath(parent);
            int dirCount = sqlite.GetValues("Directories").Length + 1;

            int dirId = 0;
            foreach (var path in pathItem.ToArray())
            {
                var list = sqlite.GetValues("Directories", "Parent = {0} and Name = '{1}'".FormatString(dirId, path));
                if (list.Length > 0)
                {
                    dirId = list[0][0].ToInt();
                }
            }
            sqlite.InsertValue("Directories", dirCount.ToString(), dirId.ToString(), dirName);
        }

        /// <summary>
        /// Create image data to database.
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <param name="parent">Parent directory path</param>
        /// <param name="data">Byte array of Image data</param>
        /// <param name="thumbnail">Byte array of Thumbnail data</param>
        public void CreateImage(string fileName, string parent, byte[] data, string mimeType)
        {
            var pathItem = Path.PathSplitter.SplitPath(parent);
            int dirCount = sqlite.GetValues("Files").Length + 1;

            int dirId = 0;
            foreach (var path in pathItem.ToArray())
            {
                var list = sqlite.GetValues("Directories", "Parent = {0} and Name = '{1}'".FormatString(dirId, path));
                if (list.Length > 0)
                {
                    dirId = list[0][0].ToInt();
                }
            }

            var text = Convert.ToBase64String(data);
            
            sqlite.InsertValue("Files", dirCount.ToString(), dirId.ToString(), fileName, text, mimeType);
        }

        /// <summary>
        /// Create image data to database.
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <param name="parent">Parent directory path</param>
        /// <param name="data">Byte array of Image data</param>
        public void CreateImage(string fileName, string parent, byte[] data)
        {
            var img = data.ByteArrayToImage();
            var thumb = img.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
            byte[] thumbnail = thumb.ImageToByteArray();

            CreateImage(fileName, parent, data, "");
        }

        public void Dispose()
        {
            ((IDisposable)sqlite).Dispose();
        }
    }
}
