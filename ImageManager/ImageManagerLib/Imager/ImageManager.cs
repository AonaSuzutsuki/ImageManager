using CommonExtentionLib.Extentions;
using ImageManagerLib.Extentions.Imager;
using ImageManagerLib.SQLite;
using System;
using System.Collections.Generic;

namespace ImageManagerLib.Imager
{
    public enum DataFileType
    {
        Other,
        Image,
        Dir
    }

    public class DataFileInfo
    {
        public int Id { get; }

        public int Parent { get; }

        public string Filename { get; }

        public DataFileType Type { get; }

        public byte[] Image { get; set; }

        public byte[] Thumbnail { get; set; }

        public DataFileInfo(int id, int parent, string filename, DataFileType type)
        {
            Id = id;
            Parent = parent;
            Filename = filename;
            Type = type;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            else if (base.Equals(obj))
            {
                return true;
            }
            else if (obj is DataFileInfo dataFileInfo)
            {
                return dataFileInfo.Id == Id
                    && dataFileInfo.Parent == Parent
                    && Filename.Equals(dataFileInfo.Filename)
                    && dataFileInfo.Type == Type;
                    //&& dataFileInfo.Image.SequenceEqual(Image)
                    //&& dataFileInfo.Thumbnail.SequenceEqual(Thumbnail);
            }

            return false;
        }
    }

    public class ImageManager : IDisposable
    {
        private SQLiteWrapper sqlite;
        #if DEBUG
        public SQLiteWrapper Sqlite { get => sqlite; }
        #endif

        public ImageManager(string filename)
        {
            sqlite = new SQLiteWrapper(filename);
        }

        public void CreateTable()
        {
            string dirField = "'Id'	INTEGER NOT NULL UNIQUE, 'Parent' INTEGER NOT NULL, 'Name' TEXT NOT NULL, PRIMARY KEY('Id')";
            string imageField = "'Id' INTEGER NOT NULL UNIQUE, 'Parent' INTEGER NOT NULL, 'Filename' TEXT NOT NULL, 'Data' TEXT NOT NULL, 'Thumbnail' TEXT NOT NULL, PRIMARY KEY('Id')";
            string ThumbField = "'Id' INTEGER NOT NULL UNIQUE, 'Data' TEXT NOT NULL, PRIMARY KEY('Id')";
            var tList = new TableFieldList()
            {
                new TableFieldInfo("")
            };

            sqlite.CreateTable("Directories", dirField);
            sqlite.CreateTable("Images", imageField);
            sqlite.CreateTable("Thumbnails", ThumbField);
        }

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

        public DataFileInfo[] GetDirectories(DataFileInfo dataFileInfo)
        {
            return GetDirectories(dataFileInfo.Id);
        }

        public DataFileInfo[] GetFiles(int did = 0)
        {
            var sList = sqlite.GetValues("Images", "Parent = {0}".FormatString(did));
            var dataFileInfoList = new List<DataFileInfo>(sList.Length);
            foreach (var list in sList)
            {
                int id = list[0].ToInt();
                int parent = list[1].ToInt();
                string filename = list[2];
                var type = DataFileType.Image;

                var dataFileInfo = new DataFileInfo(id, parent, filename, type)
                {
                    Image = Convert.FromBase64String(list[3]),
                    Thumbnail = Convert.FromBase64String(list[3])
                };

                dataFileInfoList.Add(dataFileInfo);
            }

            return dataFileInfoList.ToArray();
        }

        public DataFileInfo[] GetFiles(DataFileInfo dataFileInfo)
        {
            return GetFiles(dataFileInfo.Id);
        }

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

        public void CreateImage(string fileName, string parent, byte[] data, byte[] thumbnail)
        {
            var pathItem = Path.PathSplitter.SplitPath(parent);
            int dirCount = sqlite.GetValues("Images").Length + 1;

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
            var thumbText = Convert.ToBase64String(thumbnail);
            
            sqlite.InsertValue("Images", dirCount.ToString(), dirId.ToString(), fileName, text, thumbText);
        }

        public void CreateImage(string fileName, string parent, byte[] data)
        {
            var img = data.ByteArrayToImage();
            var thumb = img.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
            byte[] thumbnail = thumb.ImageToByteArray();

            CreateImage(fileName, parent, data, thumbnail);
        }

        public void Dispose()
        {
            ((IDisposable)sqlite).Dispose();
        }
    }
}
