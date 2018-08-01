using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommonExtensionLib.Extensions;
using FileManagerLib.Dat;
using FileManagerLib.Extensions.Path;
using FileManagerLib.Filer.Exceptions;
using FileManagerLib.MimeType;
using FileManagerLib.Path;

namespace FileManagerLib.Filer.Json
{
	public abstract class AbstractJsonResourceManager : IDisposable
	{

		#region Constants
		protected const int LEN = 4;
		protected const int JSON_LEN = 8;
		#endregion

		#region Fields
		protected JsonStructureManager jsonStructureManager;
		protected DatFileManager fManager;
        #endregion

        #region Properties
        public string FilePath
        {
            get;
        }
        #endregion

        #region Events
        public class ReadWriteProgressEventArgs : EventArgs
		{
			public int CompletedNumber { get; }
			public int FullNumber { get; }
			public bool IsCompleted { get; }
			public double Percentage
			{
				get
				{
					if (FullNumber > 0)
						return Math.Ceiling(((double)CompletedNumber / (double)FullNumber) * 100);
					return 0;
				}
			}
			public string CurrentFilepath { get; }

			public ReadWriteProgressEventArgs(int completedNumber, int fullNumber, string currentFilePath, bool isCompleted)
			{
				CompletedNumber = completedNumber;
				FullNumber = fullNumber;
				CurrentFilepath = currentFilePath;
				IsCompleted = isCompleted;
			}
		}
		public delegate void ReadWriteProgressEventHandler(object sender, ReadWriteProgressEventArgs eventArgs);
		public event ReadWriteProgressEventHandler VacuumProgress;
		#endregion


		protected AbstractJsonResourceManager(string filePath, bool newFile = false, bool isCheckHash = true)
        {
            if (newFile)
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }

            fManager = new DatFileManager(filePath) { IsShiftJsonPosition = true };
            var json = newFile ? string.Empty : Encoding.UTF8.GetString(fManager.GetBytesFromEnd(JSON_LEN));
            jsonStructureManager = new JsonStructureManager(json, isCheckHash);
            fManager.IsCheckHash = jsonStructureManager.IsCheckHash;
            FilePath = filePath;
        }    


		protected (int nextId, int parentId) ResolveTermParameters(string fileName, string parent)
		{
			var pathItem = PathSplitter.SplitPath(parent);
			var fileArray = jsonStructureManager.GetFileStructures();
			int dirCount = jsonStructureManager.NextFileId;//fileArray.Length > 0 ? fileArray[fileArray.Length - 1].Id + 1 : 1;

			int parentRootId = GetDirectoryId(pathItem);
			var dirs = jsonStructureManager.GetFileStructureFromParent(parentRootId, fileName);
			//var dirs = sqlite.GetValues(TABLE_FILES, "Parent = {0} and Name = '{1}'".FormatString(parentRootId, fileName));
			if (dirs != null)
				throw new FileExistedException("Existed {0} on {1}".FormatString(fileName, parent));
			if (parentRootId < 0)
				throw new DirectoryNotFoundException("Not found {0}".FormatString(parent));

			return (dirCount, parentRootId);
		}

        #region Directory
        public void CreateDirectory(string fullPath)
		{
			var (parent, dirName) = fullPath.GetFilenameAndParent();
			var dirArray = jsonStructureManager.GetDirectoryStructures();
			int dirCount = jsonStructureManager.NextDirectoryId;//dirArray.Length > 0 ? dirArray[dirArray.Length - 1].Id + 1 : 1;

			int parentRootId = GetDirectoryId(parent);
			var dirs = jsonStructureManager.GetDirectoryStructures();

			bool isExisted = false;
			foreach (var dir in dirs)
				if (dir.Parent == parentRootId && dir.Name.Equals(dirName))
					isExisted = true;

			if (isExisted)
				throw new Exception("Existed {0} on {1}".FormatString(dirName, parent));
			if (parentRootId < 0)
				throw new DirectoryNotFoundException("Not found {0}".FormatString(parent));

			jsonStructureManager.CreateDirectory(dirCount, parentRootId, dirName);
			//return (true, parent.ToString());
		}

		public void DeleteDirectory(string fullPath)
		{
			var (parent, dirName) = fullPath.GetFilenameAndParent();

			int rootId = GetDirectoryId(parent);
			int dirId = GetDirectoryId(dirName, rootId);

			jsonStructureManager.DeleteDirectory(dirId);
		}

		public void DeleteDirectory(int id)
		{
			jsonStructureManager.DeleteDirectory(id);
		}

        public bool ExistDirectory()
        {
            return false;
        }

        public int GetDirectoryId(string dirName, int rootId)
        {
            if (string.IsNullOrEmpty(dirName) && rootId == 0)
                return 0;

            int dirId = 0;
            var dArray = jsonStructureManager.GetDirectoryStructuresFromParent(rootId);
            //var list = sqlite.GetValues(TABLE_DIRECTORIES, "Parent = {0} and Name = '{1}'".FormatString(rootId, dirName));
            if (dArray != null && dArray.Length > 0)
            {
                foreach (var dir in dArray)
                {
                    if (dir.Name.Equals(dirName))
                    {
                        dirId = dir.Id;
                        break;
                    }
                    dirId = -1;
                }
            }

            return dirId;
        }
        public int GetDirectoryId(PathItem pathItem)
        {
            int dirId = 0;
            foreach (var path in pathItem.ToArray())
                dirId = GetDirectoryId(path, dirId);

            return dirId;
        }

        public DirectoryStructure[] GetDirectories(int dirId)
        {
            var dirs = jsonStructureManager.GetDirectoryStructuresFromParent(dirId);
            return dirs;
        }
        public DirectoryStructure[] GetDirectories(string fullPath)
        {
            var (parent, dirName) = fullPath.GetFilenameAndParent();

            int rootId = GetDirectoryId(parent);
            int dirId = GetDirectoryId(dirName, rootId);

            return jsonStructureManager.GetDirectoryStructuresFromParent(dirId);
        }
        #endregion

        #region File
        public void DeleteFile(string fullPath)
		{

		}
		public void DeleteFile(int id)
		{

		}

        public bool ExistFile(string fullPath)
        {
            var (parent, fileName) = fullPath.GetFilenameAndParent();
            int rootId = GetDirectoryId(parent);
            return jsonStructureManager.ExistedFile(rootId, fileName);
        }

        public FileStructure[] GetFiles(string fullPath)
        {
            var (parent, dirName) = fullPath.GetFilenameAndParent();

            int rootId = GetDirectoryId(parent);
            int dirId = GetDirectoryId(dirName, rootId);

            return jsonStructureManager.GetFileStructuresFromParent(dirId);
        }
        public FileStructure[] GetFiles(int dirId)
        {
            var files = jsonStructureManager.GetFileStructuresFromParent(dirId);
            return files;
        }
        #endregion
        
        #region Byte Read
        public byte[] GetBytes(string fullPath)
        {
            var (parent, fileName) = fullPath.GetFilenameAndParent();
            int rootId = GetDirectoryId(parent);
            if (jsonStructureManager.ExistedFile(rootId, fileName))
            {
                var file = jsonStructureManager.GetFileStructureFromParent(rootId, fileName);
                return fManager.GetBytes(file.Location, LEN);
            }
            return null;
        }

        public byte[] GetBytes(int id)
        {
            if (jsonStructureManager.ExistedFile(id))
            {
                var file = jsonStructureManager.GetFileStructure(id);
                return fManager.GetBytes(file.Location, LEN);
            }
            return null;
        }

        public string GetString(string fullPath)
        {
            var bytes = GetBytes(fullPath);
            return Encoding.UTF8.GetString(bytes);
        }
        #endregion
        
        #region Trace
        public static string GetDirectoryPath(JsonStructureManager jsonStructureManager, int id)
		{
			var pathList = new List<string>();

			if (id > 0)
			{
				int did = id;
				while (did > 0)
				{
					var dir = jsonStructureManager.GetDirectoryStructure(did);

					pathList.Add(dir.Name);
					var parent = dir.Parent;
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
			else
			{
				return "/";
			}
		}
		public static string GetFilePath(JsonStructureManager jsonStructureManager, int id)
		{
			var file = jsonStructureManager.GetFileStructure(id);
			var fileName = file.Name;
			var parent = file.Parent;

			string dirPath = GetDirectoryPath(jsonStructureManager, parent);
			if (!dirPath.Equals("/"))
			    return "{0}/{1}".FormatString(dirPath, fileName);
			return "/{0}".FormatString(fileName);
		}

		public string TraceDirs()
		{
			var dArray = jsonStructureManager.GetDirectoryStructures();

			var sb = new StringBuilder();
			sb.AppendFormat("Directories [\n");
			foreach (var dItem in dArray)
			{
				int id = dItem.Id;

				sb.AppendFormat("\t[\n");
				sb.AppendFormat("\t\tId:\t {0}\n", id);
				sb.AppendFormat("\t\tParent:\t {0}\n", dItem.Parent);
				sb.AppendFormat("\t\tPath:\t {0}\n", GetDirectoryPath(jsonStructureManager, id));
				sb.AppendFormat("\t\tName:\t {0}\n", dItem.Name);
				sb.AppendFormat("\t]\n");
			}
			sb.AppendFormat("]\n");

			return sb.ToString();
		}

		public string TraceFiles()
		{
			var fArray = jsonStructureManager.GetFileStructures();

			var sb = new StringBuilder();
			sb.AppendFormat("Files [\n");
			foreach (var fItem in fArray)
			{
				var id = fItem.Id;
				var location = fItem.Location;

				var tuple = fManager.GetPartialBytes(location, 40, LEN);
				var data = tuple.Item2;
				var len = tuple.Item1;
                
                sb.AppendFormat("\t[\n");
				sb.AppendFormat("\t\tId:\t {0}\n", id);
				sb.AppendFormat("\t\tParent:\t {0}\n", fItem.Parent);
				sb.AppendFormat("\t\tPath:\t {0}\n", GetFilePath(jsonStructureManager, id));
				sb.AppendFormat("\t\tName:\t {0}\n", fItem.Name);
				sb.AppendFormat("\t\tData:\t {0}\n", Convert.ToBase64String(data));
				sb.AppendFormat("\t\tLength:\t {0}kb\n", len / 1024);
				sb.AppendFormat("\t\tLocate:\t {0}\n", location);

                if (fItem.Additional != null)
                {
                    var zip = fItem.Additional.Keys.Zip(fItem.Additional.Values, (_key, _value) => new { key = _key, value = _value });
                    foreach (var val in zip)
                    {
                        sb.AppendFormat("\t\t{0}:\t {1}\n", val.key, val.value);
                    }
                }

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


		#region Vacuum
		public void DataVacuum()
		{
			DatFileManager makeDatFileManager(string f) => new DatFileManager(f) { IsCheckHash = fManager.IsCheckHash };

			var tempFilePath = "{0}_temp".FormatString(this.fManager.FilePath);
			using (var tempfManager = makeDatFileManager(tempFilePath))
			{
				jsonStructureManager.Vacuum(fManager, tempfManager, LEN, (completedNumber, fullNumber, currentFilePath) =>
				{
					VacuumProgress?.Invoke(this, new ReadWriteProgressEventArgs(completedNumber, fullNumber, currentFilePath, true));
				});
			}

			var filePath = fManager.FilePath;
			fManager = fManager.Rename("_temp");
		}
		#endregion

		#region Save
		public void Save()
		{
			if (jsonStructureManager != null && jsonStructureManager.IsChenged)
                jsonStructureManager.WriteJson(fManager, JSON_LEN);
		}
		#endregion

		#region Dispose
		public void Dispose()
        {
			Save();
            fManager?.Dispose();

            fManager = null;
            jsonStructureManager = null;
        }
		#endregion
	}
}
