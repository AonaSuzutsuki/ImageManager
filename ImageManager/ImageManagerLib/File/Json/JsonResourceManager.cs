using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommonExtensionLib.Extensions;
using FileManagerLib.Dat;
using FileManagerLib.Extensions.Path;
using FileManagerLib.File.Exceptions;
using FileManagerLib.MimeType;
using FileManagerLib.CommonPath;

namespace FileManagerLib.File.Json
{
	/// <summary>
    /// Resource manager by JSON.
    /// </summary>
	public class JsonResourceManager : IDisposable
	{

		#region Constants
        /// <summary>
		/// The identifier length.
        /// </summary>
		protected const int LEN = 4;

        /// <summary>
		/// The identifier length of the json.
        /// </summary>
		protected const int JSON_LEN = 8;
		#endregion

		#region Fields
        /// <summary>
        /// The json structure manager.
        /// </summary>
		protected JsonStructureManager jsonStructureManager;
        
        /// <summary>
        /// The dat file manager.
        /// </summary>
		protected DatFileManager fManager;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the file path.
        /// </summary>
        /// <value>The file path.</value>
        public string FilePath
        {
            get;
        }
        #endregion

        #region Events
        /// <summary>
        /// Read write progress event arguments.
        /// </summary>
        public class ReadWriteProgressEventArgs : EventArgs
		{
			/// <summary>
            /// Gets the completed number.
            /// </summary>
            /// <value>The completed number.</value>
			public int CompletedNumber { get; }

            /// <summary>
            /// Gets the full number.
            /// </summary>
            /// <value>The full number.</value>
			public int FullNumber { get; }

            
			public bool IsCompleted { get; }

            /// <summary>
            /// Gets the percentage.
            /// </summary>
            /// <value>The percentage.</value>
			public double Percentage
			{
				get
				{
					if (FullNumber > 0)
						return Math.Ceiling(((double)CompletedNumber / (double)FullNumber) * 100);
					return 0;
				}
			}

            /// <summary>
            /// Gets the current filepath.
            /// </summary>
            /// <value>The current filepath.</value>
			public string CurrentFilepath { get; }

            /// <summary>
            /// Initializes a new instance of the
            /// <see cref="T:FileManagerLib.File.Json.JsonResourceManager.ReadWriteProgressEventArgs"/> class.
            /// </summary>
            /// <param name="completedNumber">Completed number.</param>
            /// <param name="fullNumber">Full number.</param>
            /// <param name="currentFilePath">Current file path.</param>
            /// <param name="isCompleted">If set to <c>true</c> is completed.</param>
			public ReadWriteProgressEventArgs(int completedNumber, int fullNumber, string currentFilePath, bool isCompleted)
			{
				CompletedNumber = completedNumber;
				FullNumber = fullNumber;
				CurrentFilepath = currentFilePath;
				IsCompleted = isCompleted;
			}
		}

        /// <summary>
        /// Read write progress event handler.
        /// </summary>
		public delegate void ReadWriteProgressEventHandler(object sender, ReadWriteProgressEventArgs eventArgs);

        /// <summary>
        /// Occurs when vacuum progress.
        /// </summary>
		public event ReadWriteProgressEventHandler VacuumProgress;
		#endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="T:FileManagerLib.File.Json.JsonResourceManager"/> class.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <param name="newFile">If set to <c>true</c> new file.</param>
        /// <param name="isCheckHash">If set to <c>true</c> is check hash.</param>
		public JsonResourceManager(string filePath, bool newFile = false, bool isCheckHash = true)
        {
            if (newFile)
            {
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

			fManager = new DatFileManager(filePath);
            var json = newFile ? string.Empty : Encoding.UTF8.GetString(fManager.GetBytesFromEnd(JSON_LEN));
            jsonStructureManager = new JsonStructureManager(json, isCheckHash);
            fManager.IsCheckHash = jsonStructureManager.IsCheckHash;
            FilePath = filePath;
        }    

        /// <summary>
        /// Resolves the term parameters.
        /// </summary>
        /// <returns>The term parameters.</returns>
        /// <param name="fileName">File name.</param>
        /// <param name="parent">Parent.</param>
		protected (int nextId, int parentId) ResolveTermParameters(string fileName, string parent)
		{
			var pathItem = PathSplitter.SplitPath(parent);
			var fileArray = jsonStructureManager.GetFileStructures();
			int dirCount = jsonStructureManager.NextFileId;//fileArray.Length > 0 ? fileArray[fileArray.Length - 1].Id + 1 : 1;

			int parentRootId = GetDirectoryId(pathItem);
			var dirs = jsonStructureManager.GetFileStructureFromParent(parentRootId, fileName);
			//var dirs = sqlite.GetValues(TABLE_FILES, "Parent = {0} and Name = '{1}'".FormatString(parentRootId, fileName));
			if (dirs != null)
				throw new FileDirectoryExistedException("Existed {0} on {1}".FormatString(fileName, parent));
			if (parentRootId < 0)
				throw new DirectoryNotFoundException("Not found {0}".FormatString(parent));

			return (dirCount, parentRootId);
		}

        #region Directory
        /// <summary>
        /// Creates the directory.
        /// </summary>
        /// <param name="fullPath">Full path.</param>
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
				throw new FileDirectoryExistedException("Existed {0} on {1}".FormatString(dirName, parent));
			if (parentRootId < 0)
				throw new DirectoryNotFoundException("Not found {0}".FormatString(parent));

			jsonStructureManager.CreateDirectory(dirCount, parentRootId, dirName);
			//return (true, parent.ToString());
		}

        /// <summary>
        /// Deletes the directory.
        /// </summary>
        /// <param name="fullPath">Full path.</param>
		public void DeleteDirectory(string fullPath)
		{
			var (parent, dirName) = fullPath.GetFilenameAndParent();

			int rootId = GetDirectoryId(parent);
			int dirId = GetDirectoryId(dirName, rootId);

			jsonStructureManager.DeleteDirectory(dirId);
		}

        /// <summary>
        /// Deletes the directory.
        /// </summary>
        /// <param name="id">Identifier.</param>
		public void DeleteDirectory(int id)
		{
			jsonStructureManager.DeleteDirectory(id);
		}

        /// <summary>
        /// Exists the directory.
        /// </summary>
        /// <returns><c>true</c>, if directory was existed, <c>false</c> otherwise.</returns>
        /// <param name="fullPath">Full path.</param>
        public bool ExistDirectory(string fullPath)
        {
            var (parent, fileName) = fullPath.GetFilenameAndParent();
            int rootId = GetDirectoryId(parent);
            return jsonStructureManager.ExistedDirectory(rootId, fileName);
        }

        /// <summary>
        /// Gets the directory identifier.
        /// </summary>
        /// <returns>The directory identifier.</returns>
        /// <param name="dirName">Dir name.</param>
        /// <param name="rootId">Root identifier.</param>
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

        /// <summary>
        /// Gets the directory identifier.
        /// </summary>
        /// <returns>The directory identifier.</returns>
        /// <param name="pathItem">Path item.</param>
        public int GetDirectoryId(PathItem pathItem)
        {
            int dirId = 0;
            foreach (var path in pathItem.ToArray())
                dirId = GetDirectoryId(path, dirId);

            return dirId;
        }

        /// <summary>
        /// Gets the directories.
        /// </summary>
        /// <returns>The directories.</returns>
        /// <param name="dirId">Dir identifier.</param>
        public DirectoryStructure[] GetDirectories(int dirId)
        {
            var dirs = jsonStructureManager.GetDirectoryStructuresFromParent(dirId);
            return dirs;
        }

        /// <summary>
        /// Gets the directories.
        /// </summary>
        /// <returns>The directories.</returns>
        /// <param name="fullPath">Full path.</param>
        public DirectoryStructure[] GetDirectories(string fullPath)
        {
            var (parent, dirName) = fullPath.GetFilenameAndParent();

            int rootId = GetDirectoryId(parent);
            int dirId = GetDirectoryId(dirName, rootId);

            return jsonStructureManager.GetDirectoryStructuresFromParent(dirId);
        }
        #endregion

        #region File
        /// <summary>
        /// Deletes the resource.
        /// </summary>
        /// <param name="fullPath">Full path.</param>
        public void DeleteResource(string fullPath)
		{
            var (parent, fileName) = fullPath.GetFilenameAndParent();
            int rootId = GetDirectoryId(parent);
            var fileStructure = jsonStructureManager.GetFileStructureFromParent(rootId, fileName);

            jsonStructureManager.DeleteFile(fileStructure.Id);
        }

        /// <summary>
        /// Deletes the resource.
        /// </summary>
        /// <param name="id">Identifier.</param>
		public void DeleteResource(int id)
		{
            jsonStructureManager.DeleteFile(id);
		}

        /// <summary>
        /// Exists the resource.
        /// </summary>
        /// <returns><c>true</c>, if resource was existed, <c>false</c> otherwise.</returns>
        /// <param name="fullPath">Full path.</param>
        public bool ExistResource(string fullPath)
        {
            var (parent, fileName) = fullPath.GetFilenameAndParent();
            int rootId = GetDirectoryId(parent);
            return jsonStructureManager.ExistedFile(rootId, fileName);
        }

        /// <summary>
        /// Gets the resources.
        /// </summary>
        /// <returns>The resources.</returns>
        /// <param name="fullPath">Full path.</param>
        public FileStructure[] GetResources(string fullPath)
        {
            var (parent, dirName) = fullPath.GetFilenameAndParent();

            int rootId = GetDirectoryId(parent);
            int dirId = GetDirectoryId(dirName, rootId);

            return jsonStructureManager.GetFileStructuresFromParent(dirId);
        }

        /// <summary>
        /// Gets the resources.
        /// </summary>
        /// <returns>The resources.</returns>
        /// <param name="dirId">Dir identifier.</param>
        public FileStructure[] GetResources(int dirId)
        {
            var files = jsonStructureManager.GetFileStructuresFromParent(dirId);
            return files;
        }
        #endregion
        
        #region Byte Read
        /// <summary>
        /// Gets the bytes.
        /// </summary>
        /// <returns>The bytes.</returns>
        /// <param name="fullPath">Full path.</param>
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

        /// <summary>
		/// Reads the actual data little by little the data and executes the designated delegate every time it reads it.
        /// </summary>
        /// <param name="fullPath">Full path.</param>
        /// <param name="size">Size.</param>
        /// <param name="action">Action.</param>
        public void ActionBytes(string fullPath, int size, Action<byte[], int> action)
        {
            var (parent, fileName) = fullPath.GetFilenameAndParent();
            int rootId = GetDirectoryId(parent);
            if (jsonStructureManager.ExistedFile(rootId, fileName))
            {
                var file = jsonStructureManager.GetFileStructureFromParent(rootId, fileName);
                fManager.ActionBytes(file.Location, size, action, LEN);
            }
        }

        /// <summary>
        /// Gets the bytes.
        /// </summary>
        /// <returns>The bytes.</returns>
        /// <param name="id">Identifier.</param>
        public byte[] GetBytes(int id)
        {
            if (jsonStructureManager.ExistedFile(id))
            {
                var file = jsonStructureManager.GetFileStructure(id);
                return fManager.GetBytes(file.Location, LEN);
            }
            return null;
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="fullPath">Full path.</param>
        public string GetString(string fullPath)
        {
            var bytes = GetBytes(fullPath);
            return Encoding.UTF8.GetString(bytes);
        }
        #endregion

        #region Byte Write
        /// <summary>
        /// Writes the bytes without exception.
        /// </summary>
        /// <param name="fullPath">Full path.</param>
        /// <param name="bytes">Bytes.</param>
        public void WriteBytesWithoutException(string fullPath, byte[] bytes)
        {
            var (parent, fileName) = fullPath.GetFilenameAndParent();
            int rootId = GetDirectoryId(parent);
            if (!jsonStructureManager.ExistedFile(rootId, fileName))
            {
                var nextId = jsonStructureManager.NextFileId;
                var hash = CommonCoreLib.Crypto.Sha256.GetSha256(bytes);

                var start = fManager.Write(bytes, LEN);
                jsonStructureManager.CreateFile(nextId, rootId, fileName, start, hash);
            }
        }

        /// <summary>
        /// Writes the bytes.
        /// </summary>
        /// <param name="fullPath">Full path.</param>
        /// <param name="data">Data.</param>
        public void WriteBytes(string fullPath, byte[] data)
        {
            var (parent, fileName) = fullPath.GetFilenameAndParent();
            //var (nextId, rootId) = ResolveTermParameters(fileName, parent.ToString());
            var hash = CommonCoreLib.Crypto.Sha256.GetSha256(data);
            WriteBytes(fileName, parent.ToString(), data, hash);
            //if (!jsonStructureManager.ExistedFile(rootId, fileName))
            //{
            //    var hash = Crypto.Sha256.GetSha256(data);
            //    var start = fManager.Write(data, LEN);
            //    jsonStructureManager.CreateFile(nextId, rootId, fileName, start, hash);
            //}
        }

        /// <summary>
        /// Writes the bytes.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="data">Data.</param>
        /// <param name="hash">Hash.</param>
        /// <param name="options">Options.</param>
        public void WriteBytes(string fileName, string parent, byte[] data, string hash, Dictionary<string, string> options = null)
        {
            var (nextId, rootId) = ResolveTermParameters(fileName, parent);
            if (!jsonStructureManager.ExistedFile(rootId, fileName))
            {
                var start = fManager.Write(data, LEN);
                jsonStructureManager.CreateFile(nextId, rootId, fileName, start, hash, options);
            }
        }

        /// <summary>
        /// Writes the bytes.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="hash">Hash.</param>
        /// <param name="func">Func.</param>
        /// <param name="options">Options.</param>
        public void WriteBytes(string fileName, string parent, string hash, Func<long> func, Dictionary<string, string> options = null)
        {
            var (nextId, rootId) = ResolveTermParameters(fileName, parent);
            if (!jsonStructureManager.ExistedFile(rootId, fileName))
            {
                var start = func();
                jsonStructureManager.CreateFile(nextId, rootId, fileName, start, hash, options);
            }
        }

        /// <summary>
        /// Writes the string.
        /// </summary>
        /// <param name="fullPath">Full path.</param>
        /// <param name="text">Text.</param>
        public void WriteString(string fullPath, string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            WriteBytes(fullPath, bytes);
        }
        #endregion

        #region Trace
        /// <summary>
        /// Gets the directory path.
        /// </summary>
        /// <returns>The directory path.</returns>
        /// <param name="jsonStructureManager">Json structure manager.</param>
        /// <param name="id">Identifier.</param>
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

        /// <summary>
        /// Gets the file path.
        /// </summary>
        /// <returns>The file path.</returns>
        /// <param name="jsonStructureManager">Json structure manager.</param>
        /// <param name="id">Identifier.</param>
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

        /// <summary>
        /// Traces the dirs.
        /// </summary>
        /// <returns>The dirs.</returns>
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

        /// <summary>
        /// Traces the files.
        /// </summary>
        /// <returns>The files.</returns>
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

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:FileManagerLib.File.Json.JsonResourceManager"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:FileManagerLib.File.Json.JsonResourceManager"/>.</returns>
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append(TraceDirs());
			sb.Append(TraceFiles());

			return sb.ToString();
		}
		#endregion


		#region Vacuum
        /// <summary>
        /// Datas the vacuum.
        /// </summary>
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
        /// <summary>
        /// Save this instance.
        /// </summary>
		public void Save()
		{
			if (jsonStructureManager != null && jsonStructureManager.IsChenged)
                jsonStructureManager.WriteJson(fManager, JSON_LEN);
		}
		#endregion

		#region Dispose
        /// <summary>
        /// Releases all resource used by the <see cref="T:FileManagerLib.File.Json.JsonResourceManager"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:FileManagerLib.File.Json.JsonResourceManager"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="T:FileManagerLib.File.Json.JsonResourceManager"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:FileManagerLib.File.Json.JsonResourceManager"/> so the garbage collector can reclaim the memory
        /// that the <see cref="T:FileManagerLib.File.Json.JsonResourceManager"/> was occupying.</remarks>
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
