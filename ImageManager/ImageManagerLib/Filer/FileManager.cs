using CommonExtensionLib.Extensions;
using Dat;
using FileManagerLib.Extensions.Imager;
using FileManagerLib.Extensions.Path;
using FileManagerLib.SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileManagerLib.Filer
{
    public class FileManager : IDisposable, IFileManager
	{
        #region Constants
        private const string TABLE_DIRECTORIES = "Directories";
        private const string TABLE_FILES = "Files";
        #endregion

        #region Fields
        private DatFileManager fManager;
        private IDatabase sqlite;
        #endregion

        #if DEBUG
        public IDatabase Sqlite { get => sqlite; } // For Test
        #endif

        /// <summary>
        /// Manage database for images.
        /// </summary>
        /// <param name="filename">Database path</param>
        public FileManager(string filename, bool newFile = false)
        {
            var datName = "{0}.dat".FormatString(System.IO.Path.GetFileNameWithoutExtension(filename));

            if (newFile)
            {
                if (File.Exists(filename))
                    File.Delete(filename);
                if (File.Exists(datName))
                    File.Delete(datName);
            }

            fManager = new DatFileManager(datName);
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
                new TableFieldInfo("Location", TableFieldType.Text, TableFieldAttribute.NotNull),
                new TableFieldInfo("Type", TableFieldType.Text, TableFieldAttribute.NotNull)
            };

            sqlite.CreateTable(TABLE_DIRECTORIES, dirField);
            sqlite.CreateTable(TABLE_FILES, imageField);
        }


        #region Directories
        /// <summary>
        /// Create directory to database.
        /// </summary>
        /// <param name="dirName">Directory name</param>
        /// <param name="parent">Parent directory path</param>
        public (bool, string) CreateDirectory(string fullPath)
        {
            var (parent, dirName) = fullPath.GetFilenameAndParent();
            var dirArray = sqlite.GetValues(TABLE_DIRECTORIES);
            int dirCount = dirArray.Length > 0 ? dirArray[dirArray.Length - 1][0].ToInt() + 1 : 1;

            int parentRootId = GetDirectoryId(parent);
            var dirs = sqlite.GetValues(TABLE_DIRECTORIES, "Parent = {0} and Name = '{1}'".FormatString(parentRootId, dirName));
            if (dirs.Length > 0)
                return (false, "Existed {0} on {1}".FormatString(dirName, parent));
            else if (parentRootId < 0)
                return (false, "Not found {0}".FormatString(parent));

            sqlite.InsertValue(TABLE_DIRECTORIES, dirCount.ToString(), parentRootId.ToString(), dirName);
            return (true, parent.ToString());
        }

        /// <summary>
        /// Get directories.
        /// </summary>
        /// <param name="did">Directory Id.</param>
        /// <returns>Directories of directory id.</returns>
        public DataFileInfo[] GetDirectories(int did = 0)
        {
            var sList = sqlite.GetValues(TABLE_DIRECTORIES, "Parent = {0}".FormatString(did));
            var dataFileInfoList = new List<DataFileInfo>(sList.Length);
            foreach (var list in sList)
            {
                int id = list[0].ToInt();
                int parent = list[1].ToInt();
                string filename = list[2];
                var location = list[3].ToInt64();
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
            var list = sqlite.GetValues(TABLE_DIRECTORIES, "Parent = {0} and Name = '{1}'".FormatString(rootId, dirName));
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
        public void DeleteDirectory(string fullPath)
        {
            var (parent, dirName) = fullPath.GetFilenameAndParent();

            int rootId = GetDirectoryId(parent);
            int dirId = GetDirectoryId(dirName, rootId);

            sqlite.DeleteValue(TABLE_DIRECTORIES, "Id = {0} and Name = '{1}'".FormatString(dirId, dirName));

            sqlite.DeleteValue(TABLE_DIRECTORIES, "Parent = {0}".FormatString(dirId));
            //var prefiles = sqlite.GetValues(TABLE_FILES, "Parent = {0}".FormatString(dirId));
            //foreach (var files in prefiles)
            //{
            //    sqlite.InsertValue("Trash", files[3]);
            //}
            sqlite.DeleteValue(TABLE_FILES, "Parent = {0}".FormatString(dirId));
        }

        /// <summary>
        /// Delete a directory.
        /// </summary>
        /// <param name="id">Directory id</param>
        public void DeleteDirectory(int id)
        {
            sqlite.DeleteValue(TABLE_DIRECTORIES, "Id = {0}".FormatString(id));

            sqlite.DeleteValue(TABLE_DIRECTORIES, "Parent = {0}".FormatString(id));
            sqlite.DeleteValue(TABLE_FILES, "Parent = {0}".FormatString(id));
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
            var sList = sqlite.GetValues(TABLE_FILES, "Parent = {0}".FormatString(did));
            
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
            var fileArray = sqlite.GetValues(TABLE_FILES);
            int dirCount = fileArray.Length > 0 ? fileArray[fileArray.Length - 1][0].ToInt() + 1 : 1;

            int parentRootId = GetDirectoryId(pathItem);
            var dirs = sqlite.GetValues(TABLE_FILES, "Parent = {0} and Name = '{1}'".FormatString(parentRootId, fileName));
            if (dirs.Length > 0)
                return (false, "Existed {0} on {1}".FormatString(fileName, parent));
            else if (parentRootId < 0)
                return (false, "Not found {0}".FormatString(parent));

            var start = fManager.Write(data);

            sqlite.InsertValue(TABLE_FILES, dirCount.ToString(), parentRootId.ToString(), fileName, start.ToString(), mimeType);
            return (true, string.Empty);
        }

        /// <summary>
        /// Create image data to database.
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <param name="parent">Parent directory path</param>
        /// <param name="inFilepath">Filepath</param>
        public void CreateImage(string fileName, string parent, string inFilePath)
        {
            var data = ImageLoader.FromImageFile(inFilePath).Data;
            var mimetype = MimeType.MimeTypeMap.GetMimeType(inFilePath);
            //var img = data.ByteArrayToImage();
            //var thumb = img.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
            //byte[] thumbnail = thumb.ImageToByteArray();
            if (data != null)
                CreateImage(fileName, parent, data, mimetype);
        }

        public void CreateImage(string fullPath, string inFilePath)
        {
            var (parent, fileName) = fullPath.GetFilenameAndParent();
            CreateImage(fileName, parent.ToString(), inFilePath);
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
        public void DeleteFile(string fullPath)
        {
            var (parent, fileName) = fullPath.GetFilenameAndParent();
            int rootId = GetDirectoryId(parent);

            //var _files = sqlite.GetValues(TABLE_FILES, "Parent = {0} and Name = '{1}'".FormatString(rootId, fileName));
            //var files = _files.Length > 0 ? _files[0] : null;
            //if (files != null)
            //{
            //    sqlite.InsertValue("Trash", files);
            //}
            sqlite.DeleteValue(TABLE_FILES, "Parent = {0} and Name = '{1}'".FormatString(rootId, fileName));
        }

        /// <summary>
        /// Delete a file.
        /// </summary>
        /// <param name="id">File Id</param>
        public void DeleteFile(int id)
        {
            sqlite.DeleteValue(TABLE_FILES, "Id = '{0}'".FormatString(id));
        }

        

        public void WriteToFile(string filePath, string outFilePath)
        {
            var (parent, fileName) = filePath.GetFilenameAndParent();
            int rootId = GetDirectoryId(parent);

            var values = ConvertDataFileInfo(sqlite.GetValues(TABLE_FILES, "Parent = {0} and Name = '{1}'".FormatString(rootId, fileName)));
            WriteToFile(values, outFilePath);
        }

        public void WriteToFile(int id, string outFilePath)
        {
            var values = ConvertDataFileInfo(sqlite.GetValues(TABLE_FILES, "Id = {0}".FormatString(id)));
            WriteToFile(values, outFilePath);
        }

        public void WriteToFile(DataFileInfo[] values, string outFilePath)
        {
            if (values.Length > 0)
            {
                var loc = values[0].Location;
                var data = fManager.GetBytes(loc);
                using (var fs = new FileStream(outFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    fs.Write(data, 0, data.Length);
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
                var item = sqlite.GetValues(TABLE_DIRECTORIES, "Id = {0}".FormatString(did));

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
            var item = sqlite.GetValues(TABLE_FILES, "Id = {0}".FormatString(id));
            var dataFileInfo = ConvertDataFileInfo(item);
            var fileName = dataFileInfo[0].Filename;
            var parent = dataFileInfo[0].Parent;

            string dirPath = GetDirectoryPath(parent);
            return "{0}/{1}".FormatString(dirPath, fileName);
        }

        public string TraceDirs()
        {
            var dArray = sqlite.GetValues(TABLE_DIRECTORIES);

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
            var fArray = sqlite.GetValues(TABLE_FILES);
            var dataFileInfos = ConvertDataFileInfo(fArray);

            var sb = new StringBuilder();
            sb.AppendFormat("Files [\n");
            foreach (var fItem in dataFileInfos)
            {
                var id = fItem.Id;
                var location = fItem.Location;

                var tuple = fManager.GetPartialBytes(location, 40);
                var data = tuple.Item2;
                var len = tuple.Item1;

                sb.AppendFormat("\t[\n");
                sb.AppendFormat("\t\tId:\t {0}\n", id);
                sb.AppendFormat("\t\tParent:\t {0}\n", fItem.Parent);
                sb.AppendFormat("\t\tPath:\t {0}\n", GetFilePath(id));
                sb.AppendFormat("\t\tName:\t {0}\n", fItem.Filename);
                sb.AppendFormat("\t\tData:\t {0}\n", Convert.ToBase64String(data));
                sb.AppendFormat("\t\tLength:\t {0}kb\n", len / 1024);
                sb.AppendFormat("\t\tType:\t {0}\n", fItem.MimeType);
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

        public void DataVacuum()
        {
            DatFileManager makeDatFileManager(string f) => new DatFileManager(f);

            var tempFilePath = "{0}.temp".FormatString(this.fManager.FilePath);
            using (var tempfManager = makeDatFileManager(tempFilePath))
            {
                var files = sqlite.GetValues(TABLE_FILES);
                var dataFileInfos = ConvertDataFileInfo(files);
                foreach (var dataFileInfo in dataFileInfos.Select((v, i) => new { v, i }))
                {
                    var id = dataFileInfo.v.Id;
                    var loc = dataFileInfo.v.Location;
                    var nloc = this.fManager.WriteToTemp(loc, tempfManager);

                    sqlite.Update(TABLE_FILES, ("Location", nloc.ToString()), "Id = {0}".FormatString(id));

                    Console.WriteLine("{0}/{1}".FormatString(dataFileInfo.i + 1, dataFileInfos.Length));
                }
            }

            var filePath = this.fManager.FilePath;
            this.fManager.Dispose();
            File.Delete(filePath);
            File.Move(tempFilePath, filePath);
            var fManager = makeDatFileManager(filePath);
            sqlite.Vacuum();

            this.fManager = fManager;
        }
        #endregion


        #region Static Function
        public static DataFileInfo[] ConvertDataFileInfo(string[][] arg)
        {
            var dataFileInfoList = new List<DataFileInfo>(arg.Length);
            foreach (var list in arg)
            {
                if (list.Length >= 5)
                {
                    int id = list[0].ToInt();
                    int parent = list[1].ToInt();
                    string filename = list[2];
                    var loc = list[3].ToInt64();
                    var mime = list[4];
                    var type = DataFileType.File;

                    var dataFileInfo = new DataFileInfo(id, parent, filename, type)
                    {
                        Location = loc,
                        MimeType = mime
                    };

                    dataFileInfoList.Add(dataFileInfo);
                }
            }
            return dataFileInfoList.ToArray();
        }
        #endregion


        public void Dispose()
        {
            fManager.Dispose();
            ((IDisposable)sqlite).Dispose();
        }
    }
}
