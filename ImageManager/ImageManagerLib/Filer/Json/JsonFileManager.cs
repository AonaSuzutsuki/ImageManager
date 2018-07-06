﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommonExtensionLib.Extensions;
using Dat;
using FileManagerLib.Extensions.Path;
using FileManagerLib.Path;

namespace FileManagerLib.Filer.Json
{
	public class JsonFileManager : IFileManager
	{

		#region Fields
		private string datName;
		private string jsonName;
		private JsonStructureManager jsonStructureManager;
		private DatFileManager fManager;
        #endregion

        public JsonFileManager(string filePath, bool newFile = false, Action<string> fileExistAct = null)
		{
			if (newFile)
			{
                if (File.Exists(filePath))
                    fileExistAct?.Invoke(filePath);
            }

			fManager = new DatFileManager(filePath) { IsShiftJsonPosition = true };
			var json = newFile ? string.Empty : Encoding.UTF8.GetString(fManager.GetBytesFromEnd());
			jsonStructureManager = new JsonStructureManager(json);
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

		public void CreateImage(string fileName, string parent, byte[] data, string mimeType)
		{
			var pathItem = Path.PathSplitter.SplitPath(parent);
			var fileArray = jsonStructureManager.GetFileStructures();
			int dirCount = jsonStructureManager.NextFileId;//fileArray.Length > 0 ? fileArray[fileArray.Length - 1].Id + 1 : 1;

			int parentRootId = GetDirectoryId(pathItem);
			var dirs = jsonStructureManager.GetFileStructureFromParent(parentRootId, fileName);
			//var dirs = sqlite.GetValues(TABLE_FILES, "Parent = {0} and Name = '{1}'".FormatString(parentRootId, fileName));
			if (dirs != null)
				throw new Exception("Existed {0} on {1}".FormatString(fileName, parent));
			if (parentRootId < 0)
				throw new DirectoryNotFoundException("Not found {0}".FormatString(parent));

			var start = fManager.Write(data);

			jsonStructureManager.CreateFile(dirCount, parentRootId, fileName, start, mimeType);
			//sqlite.InsertValue(TABLE_FILES, dirCount.ToString(), parentRootId.ToString(), fileName, start.ToString(), mimeType);
			//return (true, string.Empty);
		}

		public void CreateImage(string fileName, string parent, string inFilePath)
		{
			var data = ImageLoader.FromImageFile(inFilePath).Data;
			var mimetype = MimeType.MimeTypeMap.GetMimeType(inFilePath);
			if (data != null)
				CreateImage(fileName, parent, data, mimetype);
		}

		public void CreateImage(string fullPath, string inFilePath)
		{
			var (parent, fileName) = fullPath.GetFilenameAndParent();
			CreateImage(fileName, parent.ToString(), inFilePath);
		}

		public void CreateImages(string parent, string dirPath)
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
				Console.WriteLine("{0}/{1}\t{2}".FormatString(file.i + 1, filePathArray.Length, file.v));
				CreateImage(System.IO.Path.GetFileName(path), System.IO.Path.GetDirectoryName(internalFilePathArray[file.i]), path);
			}
		}

		private string[] ResolveAbsolutePath(string basePath, string parent, string[] dirPathArray)
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

		public void CreateTable()
		{
			return;
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

		public DataFileInfo[] GetDirectories(int did = 0)
		{
			throw new NotImplementedException();
		}

		public DataFileInfo[] GetDirectories(DataFileInfo dataFileInfo)
		{
			throw new NotImplementedException();
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

		public DataFileInfo[] GetFiles(int did = 0)
		{
			throw new NotImplementedException();
		}

		public DataFileInfo[] GetFiles(DataFileInfo dataFileInfo)
		{
			throw new NotImplementedException();
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

				var tuple = fManager.GetPartialBytes(location, 40);
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
			DatFileManager makeDatFileManager(string f) => new DatFileManager(f);

			var tempFilePath = "{0}.temp".FormatString(this.fManager.FilePath);
			using (var tempfManager = makeDatFileManager(tempFilePath))
			{
				var files = jsonStructureManager.GetFileStructures();
				foreach (var dataFileInfo in files.Select((v, i) => new { v, i }))
				{
					var id = dataFileInfo.v.Id;
					var loc = dataFileInfo.v.Location;
					var nloc = fManager.WriteToTemp(loc, tempfManager);

					jsonStructureManager.ChangeFile(id, new FileStructure
					{
						Id = dataFileInfo.v.Id,
						Parent = dataFileInfo.v.Parent,
						Name = dataFileInfo.v.Name,
						Location = nloc,
						MimeType = dataFileInfo.v.MimeType
					});
					Console.WriteLine("{0}/{1}".FormatString(dataFileInfo.i + 1, files.Length));
				}
			}

			var filePath = fManager.FilePath;
			fManager.Dispose();
			File.Delete(filePath);
			File.Move(tempFilePath, filePath);

			fManager = makeDatFileManager(filePath);
		}

		public void WriteToFile(string filePath, string outFilePath)
		{
            var (parent, fileName) = filePath.GetFilenameAndParent();
            int rootId = GetDirectoryId(parent);
            if (jsonStructureManager.ExistedFile(rootId, fileName))
            {
                var file = jsonStructureManager.GetFileStructureFromParent(rootId, fileName);
                WriteToFile(file, outFilePath);
            }
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

                foreach (var file in files)
                {
                    string path;
                    var pathItem = PathSplitter.SplitPath(GetFilePath(jsonStructureManager, file.Id));
                    if (filePath.Equals("/"))
                        path = outFilePath + pathItem.ToString();
                    else
                        path = outFilePath + pathItem.GetPathItemFrom(filePath).ToString();
                    WriteToFile(file, path);
                }
            }
        }

		public void WriteToFile(int id, string outFilePath)
		{
            var filepath = GetFilePath(jsonStructureManager, id);
            WriteToFile(filepath, outFilePath);
        }

        public void WriteToDir(int id, string outFilePath)
        {
            var filepath = GetDirectoryPath(jsonStructureManager, id);
            WriteToDir(filepath, outFilePath);
        }

        public void WriteToFile(FileStructure structure, string outFilePath)
        {
            if (structure != null)
            {
                var loc = structure.Location;
                var data = fManager.GetBytes(loc);
                using (var fs = new FileStream(outFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    fs.Write(data, 0, data.Length);
            }
        }

		public void WriteToFile(DataFileInfo[] values, string outFilePath)
		{

		}


		public void Dispose()
		{
			var json = jsonStructureManager?.ToString();
			Console.WriteLine(json);
			//jsonStructureManager?.WriteToFile(jsonName);
			fManager.WriteToEnd(Encoding.UTF8.GetBytes(json));
			fManager?.Dispose();

			fManager = null;
			jsonStructureManager = null;
		}
	}
}
