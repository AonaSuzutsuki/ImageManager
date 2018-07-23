using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommonExtensionLib.Extensions;
using FileManagerLib.Dat;
using FileManagerLib.Extensions.Path;
using FileManagerLib.Path;

namespace FileManagerLib.Filer.Json
{
	public class JsonFileManager : IFileManager
	{

        #region Constants
        private const int LEN = 4;
        private const int JSON_LEN = 8;
        #endregion

        #region Fields
        private JsonStructureManager jsonStructureManager;
		private DatFileManager fManager;
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
		public event ReadWriteProgressEventHandler WriteToFilesProgress;
		public event ReadWriteProgressEventHandler WriteIntoResourceProgress;
        public event ReadWriteProgressEventHandler VacuumProgress;
        #endregion

        public JsonFileManager(string filePath, bool isCheckHash, bool newFile = false, Func<string, bool> fileExistAct = null)
		{
			if (newFile)
			{
				if (File.Exists(filePath) && fileExistAct != null)
					newFile = fileExistAct.Invoke(filePath);
            }

			fManager = new DatFileManager(filePath) { IsShiftJsonPosition = true };
			var json = newFile ? string.Empty : Encoding.UTF8.GetString(fManager.GetBytesFromEnd(JSON_LEN));
			jsonStructureManager = new JsonStructureManager(json, isCheckHash);
            fManager.IsCheckHash = jsonStructureManager.IsCheckHash;
        }
        
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

		private (int nextId, int parentId) ResolveTermParameters(string fileName, string parent)
		{
			var pathItem = PathSplitter.SplitPath(parent);
            var fileArray = jsonStructureManager.GetFileStructures();
            int dirCount = jsonStructureManager.NextFileId;//fileArray.Length > 0 ? fileArray[fileArray.Length - 1].Id + 1 : 1;

            int parentRootId = GetDirectoryId(pathItem);
            var dirs = jsonStructureManager.GetFileStructureFromParent(parentRootId, fileName);
            //var dirs = sqlite.GetValues(TABLE_FILES, "Parent = {0} and Name = '{1}'".FormatString(parentRootId, fileName));
            if (dirs != null)
                throw new Exception("Existed {0} on {1}".FormatString(fileName, parent));
            if (parentRootId < 0)
                throw new DirectoryNotFoundException("Not found {0}".FormatString(parent));

			return (dirCount, parentRootId);
		}

		public void CreateFile(string fileName, string parent, byte[] data, string mimeType, string hash)
		{
			var (nextId, parentId) = ResolveTermParameters(fileName, parent);         
			var start = fManager.Write(data, LEN);         
			jsonStructureManager.CreateFile(nextId, parentId, fileName, start, mimeType, hash);
			//sqlite.InsertValue(TABLE_FILES, dirCount.ToString(), parentRootId.ToString(), fileName, start.ToString(), mimeType);
			//return (true, string.Empty);
		}
        
		public void CreateFile(string fileName, string parent, string inFilePath)
		{
			if (!File.Exists(inFilePath))
				return;
   
			var mimeType = MimeType.MimeTypeMap.GetMimeType(inFilePath);
			using (var stream = new FileStream(inFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
                string hash = string.Empty;
                if (jsonStructureManager.IsCheckHash)
                    hash = Crypto.Sha256.GetSha256(stream);

                if (stream.Length > fManager.SplitSize)
                {
                    var (nextId, parentId) = ResolveTermParameters(fileName, parent);
                    var start = fManager.Write(stream, (writeStream) => {
                        while (true)
                        {
                            byte[] bs = new byte[fManager.SplitSize];
                            int readSize = stream.Read(bs, 0, bs.Length);
                            if (readSize == 0)
                                break;
							writeStream.Write(bs, 0, readSize);
                        }
                    }, LEN);
					jsonStructureManager.CreateFile(nextId, parentId, fileName, start, mimeType, hash);
                }
                else
                {
                    var data = ImageLoader.FromImageFile(stream).Data;
                    if (data != null)
                        CreateFile(fileName, parent, data, mimeType, hash);
                }
			}         
		}

		public void CreateFile(string fullPath, string inFilePath)
		{
			var (parent, fileName) = fullPath.GetFilenameAndParent();
			CreateFile(fileName, parent.ToString(), inFilePath);
		}

		public void CreateFiles(string parent, string dirPath)
		{
			var filePathArray = DirectorySearcher.GetAllFiles(dirPath);
			var dirPathArray = DirectorySearcher.GetAllDirectories(dirPath);

			var internalFilePathArray = ResolveAbsolutePath(dirPath, parent, filePathArray);
			var internalDirPathArray = ResolveAbsolutePath(dirPath, parent, dirPathArray);
			Array.Sort(internalDirPathArray);
			foreach (var dir in internalDirPathArray)
				CreateDirectory(dir);

			foreach (var file in filePathArray.Select((v, i) => new { v, i }))
			{
				var path = file.v;
				var par = System.IO.Path.GetDirectoryName(internalFilePathArray[file.i]);
				CreateFile(System.IO.Path.GetFileName(path), System.IO.Path.GetDirectoryName(internalFilePathArray[file.i]), path);
				WriteIntoResourceProgress?.Invoke(this, new ReadWriteProgressEventArgs(file.i + 1, filePathArray.Length, path, true));
			}
		}

		private static string[] ResolveAbsolutePath(string basePath, string parent, string[] dirPathArray)
		{
			string func(string path, string referencePath)
			{
				//var fileUri = new Uri(path);
				//var referenceUri = new Uri(referencePath);
				//return parent + referenceUri.MakeRelativeUri(fileUri).ToString();
				return parent.Remove(0, 1) + path.Replace(referencePath, "");
			}

			var list = new List<string>();
			foreach (var dir in dirPathArray)
                list.Add(func(dir, basePath));
			return list.ToArray();
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

		public void DeleteFile(string fullPath)
		{

		}

		public void DeleteFile(int id)
		{

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

		public string[] GetFiles(int dirId)
		{
			var files = jsonStructureManager.GetFileStructuresFromParent(dirId);
			var fList = new List<string>();
			foreach (var file in files)
			{
				fList.Add(file.Name);
			}
			return fList.ToArray();
		}
        
		public string[] GetDirectories(int dirId)
		{
			var dirs = jsonStructureManager.GetDirectoryStructuresFromParent(dirId);
            var dList = new List<string>();
            foreach (var dir in dirs)
            {
                dList.Add(dir.Name);
            }
            return dList.ToArray();
		}

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
			return "{0}/{1}".FormatString(dirPath, fileName);
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
		#endregion

		public void DataVacuum()
		{
			DatFileManager makeDatFileManager(string f) => new DatFileManager(f) { IsCheckHash = fManager.IsCheckHash };

			var tempFilePath = "{0}.temp".FormatString(this.fManager.FilePath);
			using (var tempfManager = makeDatFileManager(tempFilePath))
			{
                //var files = jsonStructureManager.GetFileStructures();
                //foreach (var dataFileInfo in files.Select((v, i) => new { v, i }))
                //{
                //	var id = dataFileInfo.v.Id;
                //	var loc = dataFileInfo.v.Location;
                //	var nloc = fManager.WriteToTemp(loc, tempfManager, LEN);

                //	jsonStructureManager.ChangeFile(id, new FileStructure
                //	{
                //		Id = dataFileInfo.v.Id,
                //		Parent = dataFileInfo.v.Parent,
                //		Name = dataFileInfo.v.Name,
                //		Location = nloc,
                //		MimeType = dataFileInfo.v.MimeType,
                //                    Hash = dataFileInfo.v.Hash
                //	});
                //	Console.WriteLine("{0}/{1}".FormatString(dataFileInfo.i + 1, files.Length));
                //}
                JsonStructureManager.Vacuum(jsonStructureManager, fManager, tempfManager, LEN, (completedNumber, fullNumber, currentFilePath) =>
                {
                    VacuumProgress?.Invoke(this, new ReadWriteProgressEventArgs(completedNumber, fullNumber, currentFilePath, true));
                });
			}

			var filePath = fManager.FilePath;
            //fManager.Dispose();
            //File.Delete(filePath);
            //File.Move(tempFilePath, filePath);
            fManager = fManager.Rename(".temp");

            //fManager = makeDatFileManager(filePath);
        }


		#region Writer
		public bool WriteToFile(string filePath, string outFilePath)
		{
            var (parent, fileName) = filePath.GetFilenameAndParent();
            int rootId = GetDirectoryId(parent);
            if (jsonStructureManager.ExistedFile(rootId, fileName))
            {
                var file = jsonStructureManager.GetFileStructureFromParent(rootId, fileName);
                return WriteToFile(file, outFilePath);
            }
			return false;
        }

		public void WriteToDir(string filePath, string outFilePath)
        {
            var (parent, fileName) = filePath.GetFilenameAndParent();
            int rootId = GetDirectoryId(parent);
            if (jsonStructureManager.ExistedDirectory(rootId, fileName))
            {
                var dirId = GetDirectoryId(PathSplitter.SplitPath(filePath));
                var dirs = jsonStructureManager.GetDirectoryAllStructuresFromParent(dirId);
                var files = jsonStructureManager.GetFileAllStructuresFromParent(dirId);

                if (!Directory.Exists(outFilePath))
                    Directory.CreateDirectory(outFilePath);

                foreach (var dir in dirs)
                {
                    string path;
                    var pathItem = PathSplitter.SplitPath(GetDirectoryPath(jsonStructureManager, dir.Id));
                    if (filePath.Equals("/"))
                        path = outFilePath + pathItem.ToString();
                    else
                        path = outFilePath + pathItem.GetPathItemFrom(filePath).ToString();
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }

                foreach (var item in files.Select((value, index) => new { index, value }))
                {
                    string path;
                    var pathItem = PathSplitter.SplitPath(GetFilePath(jsonStructureManager, item.value.Id));
                    if (filePath.Equals("/"))
                        path = outFilePath + pathItem.ToString();
                    else
                        path = outFilePath + pathItem.GetPathItemFrom(filePath).ToString();

					var isOk = WriteToFile(item.value, path);               
					WriteToFilesProgress?.Invoke(this, new ReadWriteProgressEventArgs(item.index + 1, files.Length, item.value.Name, isOk));
                }
            }
        }
        
		public bool WriteToFile(int id, string outFilePath)
		{
            var filepath = GetFilePath(jsonStructureManager, id);
            return WriteToFile(filepath, outFilePath);
        }

		public void WriteToDir(int id, string outFilePath)
        {
            var filepath = GetDirectoryPath(jsonStructureManager, id);
			WriteToDir(filepath, outFilePath);
        }

        public bool WriteToFile(FileStructure structure, string outFilePath)
        {
            if (structure != null)
            {
                var loc = structure.Location;
				return fManager.WriteToFile(loc, outFilePath, structure.Hash, LEN);
                //var data = fManager.GetBytes(loc);
                //using (var fs = new FileStream(outFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    //fs.Write(data, 0, data.Length);
            }
			return false;
        }
        #endregion


		public void Dispose()
		{
			if (jsonStructureManager != null && jsonStructureManager.IsChenged)
			{
				var json = jsonStructureManager?.ToString();
                Console.WriteLine(json);
                fManager?.WriteToEnd(Encoding.UTF8.GetBytes(json), JSON_LEN);
			}         
			fManager?.Dispose();

			fManager = null;
			jsonStructureManager = null;
		}
	}
}
