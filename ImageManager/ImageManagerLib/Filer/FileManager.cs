using CommonExtentionLib.Extentions;
using FileManagerLib.Extentions.Imager;
using FileManagerLib.SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileManagerLib.Filer
{
    public class FileManager : IDisposable
    {
        private IDatabase sqlite;

        #if DEBUG
        public IDatabase Sqlite { get => sqlite; } // For Test
        #endif

        /// <summary>
        /// Manage database for images.
        /// </summary>
        /// <param name="filename">Database path</param>
        public FileManager(string filename, bool newFile = false)
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
                new TableFieldInfo("Name", TableFieldType.Text, TableFieldAttribute.NotNull),
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


        #region Directories
        /// <summary>
        /// Create directory to database.
        /// </summary>
        /// <param name="dirName">Directory name</param>
        /// <param name="parent">Parent directory path</param>
        public (bool, string) CreateDirectory(string dirName, string parent)
        {
            var pathItem = Path.PathSplitter.SplitPath(parent);
            var dirArray = sqlite.GetValues("Directories");
            int dirCount = dirArray.Length > 0 ? dirArray[dirArray.Length - 1][0].ToInt() + 1 : 1;

            int parentRootId = GetDirectoryId(pathItem);
            var dirs = sqlite.GetValues("Directories", "Parent = {0} and Name = '{1}'".FormatString(parentRootId, dirName));
            if (dirs.Length > 0)
                return (false, "Existed {0} on {1}".FormatString(dirName, parent));
            else if (parentRootId < 0)
                return (false, "Not found {0}".FormatString(parent));

            sqlite.InsertValue("Directories", dirCount.ToString(), parentRootId.ToString(), dirName);
            return (true, string.Empty);
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
        /// Get directory Id.
        /// </summary>
        /// <param name="dirName">name of directory</param>
        /// <param name="rootId">id of root directory</param>
        /// <returns></returns>
        public int GetDirectoryId(string dirName, int rootId)
        {
            if (string.IsNullOrEmpty(dirName) && rootId == 0)
                return 0;

            int dirId = 0;
            var list = sqlite.GetValues("Directories", "Parent = {0} and Name = '{1}'".FormatString(rootId, dirName));
            if (list.Length > 0)
                dirId = list[0][0].ToInt();
            else
                dirId = -1;

            return dirId;
        }

        /// <summary>
        /// Get last directory Id.
        /// </summary>
        /// <param name="pathItem">pathitem of directory</param>
        /// <returns></returns>
        public int GetDirectoryId(Path.PathItem pathItem)
        {
            int dirId = 0;
            foreach (var path in pathItem.ToArray())
                dirId = GetDirectoryId(path, dirId);

            return dirId;
        }

        /// <summary>
        /// Delete a directory.
        /// </summary>
        /// <param name="dirName">Directory name</param>
        /// <param name="parent">Parent directory path</param>
        public void DeleteDirectory(string dirName, string parent)
        {
            var pathItem = Path.PathSplitter.SplitPath(parent);
            int dirCount = sqlite.GetValues("Directories").Length + 1;

            int rootId = GetDirectoryId(pathItem);
            int dirId = GetDirectoryId(dirName, rootId);

            sqlite.DeleteValue("Directories", "Id = {0} and Name = '{1}'".FormatString(dirId, dirName));

            sqlite.DeleteValue("Directories", "Parent = {0}".FormatString(dirId));
            sqlite.DeleteValue("Files", "Parent = {0}".FormatString(dirId));

            sqlite.Vacuum();
        }

        /// <summary>
        /// Delete a directory.
        /// </summary>
        /// <param name="id">Directory id</param>
        public void DeleteDirectory(int id)
        {
            sqlite.DeleteValue("Directories", "Id = {0}".FormatString(id));

            sqlite.DeleteValue("Directories", "Parent = {0}".FormatString(id));
            sqlite.DeleteValue("Files", "Parent = {0}".FormatString(id));

            sqlite.Vacuum();
        }
        #endregion


        #region Files
        /// <summary>
        /// Get files.
        /// </summary>
        /// <param name="did">Directory Id.</param>
        /// <returns>Files of directory id.</returns>
        public DataFileInfo[] GetFiles(int did = 0)
        {
            var sList = sqlite.GetValues("Files", "Parent = {0}".FormatString(did));
            
            return ConvertDataFileInfo(sList);
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
        /// Create image data to database.
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <param name="parent">Parent directory path</param>
        /// <param name="data">Byte array of Image data</param>
        /// <param name="thumbnail">Byte array of Thumbnail data</param>
        public (bool, string) CreateImage(string fileName, string parent, byte[] data, string mimeType)
        {
            var pathItem = Path.PathSplitter.SplitPath(parent);
            var fileArray = sqlite.GetValues("Files");
            int dirCount = fileArray.Length > 0 ? fileArray[fileArray.Length - 1][0].ToInt() + 1 : 1;

            int parentRootId = GetDirectoryId(pathItem);
            var dirs = sqlite.GetValues("Files", "Parent = {0} and Name = '{1}'".FormatString(parentRootId, fileName));
            if (dirs.Length > 0)
                return (false, "Existed {0} on {1}".FormatString(fileName, parent));
            else if (parentRootId < 0)
                return (false, "Not found {0}".FormatString(parent));

            var text = Convert.ToBase64String(data);

            sqlite.InsertValue("Files", dirCount.ToString(), parentRootId.ToString(), fileName, text, mimeType);
            return (true, string.Empty);
        }

        /// <summary>
        /// Create image data to database.
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <param name="parent">Parent directory path</param>
        /// <param name="data">Byte array of Image data</param>
        public void CreateImage(string fileName, string parent, byte[] data)
        {
            //var img = data.ByteArrayToImage();
            //var thumb = img.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
            //byte[] thumbnail = thumb.ImageToByteArray();

            CreateImage(fileName, parent, data, "");
        }

        /// <summary>
        /// Create image data to database.
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <param name="parent">Parent directory path</param>
        /// <param name="inFilepath">Filepath</param>
        public void CreateImage(string fileName, string parent, string inFilepath)
        {
            var data = ImageLoader.FromImageFile(inFilepath).Data;
            var mimetype = MimeType.MimeTypeMap.GetMimeType(inFilepath);
            //var img = data.ByteArrayToImage();
            //var thumb = img.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
            //byte[] thumbnail = thumb.ImageToByteArray();
            if (data != null)
                CreateImage(fileName, parent, data, mimetype);
        }

        public void CreateImages(string parent, string[] filePathArray)
        {
            sqlite.StartTransaction();
            foreach (var file in filePathArray.Select((v, i) => new { v, i }))
            {
                Console.WriteLine("{0}/{1}".FormatString(file.i + 1, filePathArray.Length));
                CreateImage(System.IO.Path.GetFileName(file.v), parent, file.v);
            }
            sqlite.DoCommit();
            sqlite.EndTransaction();
        }
        
        /// <summary>
        /// Delete a file.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="parent">Parent directory path</param>
        public void DeleteFile(string fileName, string parent)
        {
            var pathItem = Path.PathSplitter.SplitPath(parent);
            int dirCount = sqlite.GetValues("Directories").Length + 1;

            int rootId = GetDirectoryId(pathItem);

            sqlite.DeleteValue("Files", "Parent = {0} and Name = '{1}'".FormatString(rootId, fileName));

            sqlite.Vacuum();
        }

        /// <summary>
        /// Delete a file.
        /// </summary>
        /// <param name="id">File Id</param>
        public void DeleteFile(int id)
        {
            sqlite.DeleteValue("Files", "Id = '{0}'".FormatString(id));
            sqlite.Vacuum();
        }

        public void WriteToFile(string fileName, string parent, string outFilePath)
        {
            var pathItem = Path.PathSplitter.SplitPath(parent);
            int dirCount = sqlite.GetValues("Directories").Length + 1;

            int rootId = GetDirectoryId(pathItem);

            var values = GetFiles(rootId);
            if (values.Length > 0)
            {
                var data = values[0].Image;
                using (var fs = new FileStream(outFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fs.Write(data, 0, data.Length);
                }
            }
        }

        public void WriteToFile(int id, string outFilePath)
        {
            var values = ConvertDataFileInfo(sqlite.GetValues("Files", "Id = {0}".FormatString(id)));
            if (values.Length > 0)
            {
                var data = values[0].Image;
                using (var fs = new FileStream(outFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fs.Write(data, 0, data.Length);
                }
            }
        }
        #endregion


        #region Trace
        public string GetDirectoryPath(int id)
        {
            var pathList = new List<string>();

            int did = id;
            while (did > 0)
            {
                var item = sqlite.GetValues("Directories", "Id = {0}".FormatString(did));

                pathList.Add(item[0][2]);
                var parent = item[0][1].ToInt();
                did = parent;
            }

            var path = new StringBuilder();
            pathList.Reverse();
            foreach (var pathItem in pathList)
            {
                path.AppendFormat("/{0}", pathItem);
            }
            return path.ToString();
        }
        public string GetFilePath(int id)
        {
            var item = sqlite.GetValues("Files", "Id = {0}".FormatString(id));
            var fileName = item[0][2];
            var parent = item[0][1].ToInt();

            string dirPath = GetDirectoryPath(parent);
            return "{0}/{1}".FormatString(dirPath, fileName);
        }

        public string TraceDirs()
        {
            var dArray = sqlite.GetValues("Directories");

            var sb = new StringBuilder();
            sb.AppendFormat("Directories [\n");
            foreach (var dItem in dArray)
            {
                int id = dItem[0].ToInt();

                sb.AppendFormat("\t[\n");
                sb.AppendFormat("\t\tId:\t {0}\n", id);
                sb.AppendFormat("\t\tParent:\t {0}\n", dItem[1]);
                sb.AppendFormat("\t\tPath:\t {0}\n", GetDirectoryPath(id));
                sb.AppendFormat("\t\tName:\t {0}\n", dItem[2]);
                sb.AppendFormat("\t]\n");
            }
            sb.AppendFormat("]\n");
            
            return sb.ToString();
        }

        public string TraceFiles()
        {
            var fArray = sqlite.GetValues("Files");

            var sb = new StringBuilder();
            sb.AppendFormat("Files [\n");
            foreach (var fItem in fArray)
            {
                int id = fItem[0].ToInt();
                var data = fItem[3];
                if (data.Length > 40)
                    data = data.Substring(0, 40);

                sb.AppendFormat("\t[\n");
                sb.AppendFormat("\t\tId:\t {0}\n", id);
                sb.AppendFormat("\t\tParent:\t {0}\n", fItem[1]);
                sb.AppendFormat("\t\tPath:\t {0}\n", GetFilePath(id));
                sb.AppendFormat("\t\tName:\t {0}\n", fItem[2]);
                sb.AppendFormat("\t\tData:\t {0}\n", data);
                sb.AppendFormat("\t\tLength:\t {0}kb\n", Convert.FromBase64String(fItem[3]).Length / 1024);
                sb.AppendFormat("\t\tType:\t {0}\n", fItem[4]);
                sb.AppendFormat("\t]\n");
            }
            sb.AppendFormat("]\n");

            return sb.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(TraceDirs());
            sb.Append(TraceFiles());
            
            return sb.ToString();
        }
        #endregion


        #region Static Function
        public static DataFileInfo[] ConvertDataFileInfo(string[][] arg)
        {
            var dataFileInfoList = new List<DataFileInfo>(arg.Length);
            foreach (var list in arg)
            {
                int id = list[0].ToInt();
                int parent = list[1].ToInt();
                string filename = list[2];
                var type = DataFileType.File;

                var dataFileInfo = new DataFileInfo(id, parent, filename, type)
                {
                    Image = Convert.FromBase64String(list[3])
                };

                dataFileInfoList.Add(dataFileInfo);
            }
            return dataFileInfoList.ToArray();
        }
        #endregion


        public void Dispose()
        {
            ((IDisposable)sqlite).Dispose();
        }
    }
}
