using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommonExtensionLib.Extensions;
using FileManagerLib.Dat;
using FileManagerLib.Extensions.Path;
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


		protected AbstractJsonResourceManager(string filePath, bool newFile = false, bool isCheckHash = true, Func<string, bool> fileExistAct = null)
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


		protected (int nextId, int parentId) ResolveTermParameters(string fileName, string parent)
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
				sb.AppendFormat("\t\tLocation:\t {0}\n", location);
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


		#region Vacuum
		public void DataVacuum()
		{
			DatFileManager makeDatFileManager(string f) => new DatFileManager(f) { IsCheckHash = fManager.IsCheckHash };

			var tempFilePath = "{0}.temp".FormatString(this.fManager.FilePath);
			using (var tempfManager = makeDatFileManager(tempFilePath))
			{
				jsonStructureManager.Vacuum(fManager, tempfManager, LEN, (completedNumber, fullNumber, currentFilePath) =>
				{
					VacuumProgress?.Invoke(this, new ReadWriteProgressEventArgs(completedNumber, fullNumber, currentFilePath, true));
				});
			}

			var filePath = fManager.FilePath;
			fManager = fManager.Rename(".temp");
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
