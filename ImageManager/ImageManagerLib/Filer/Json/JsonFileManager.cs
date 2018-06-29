﻿using System;
using System.Collections.Generic;
using System.IO;
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

		public JsonFileManager(string filePath, bool newFile = false)
		{
			var name = System.IO.Path.GetFileNameWithoutExtension(filePath);
			datName = "{0}.dat".FormatString(name);
			jsonName = "{0}.json".FormatString(name);

            if (newFile)
            {
				if (File.Exists(datName))
					File.Delete(datName);
				if (File.Exists(jsonName))
					File.Delete(jsonName);
            }

            fManager = new DatFileManager(datName);
			jsonStructureManager = new JsonStructureManager(CommonCoreLib.File.FileReader.ReadToEnd(jsonName));
		}

		public (bool, string) CreateDirectory(string fullPath)
		{
			var (parent, dirName) = fullPath.GetFilenameAndParent();
			var dirArray = jsonStructureManager.GetDirectoryStructures();
			int dirCount = dirArray.Length > 0 ? dirArray[dirArray.Length - 1].Id + 1 : 1;

            int parentRootId = GetDirectoryId(parent);
			var dirs = jsonStructureManager.GetDirectoryStructures();

			bool isExisted = false;
			foreach (var dir in dirs)
				if (dir.Parent == parentRootId && dir.Name.Equals(dirName))
					isExisted = true;
            
			if (isExisted)
                return (false, "Existed {0} on {1}".FormatString(dirName, parent));
            if (parentRootId < 0)
                return (false, "Not found {0}".FormatString(parent));

			jsonStructureManager.CreateDirectory(dirCount, parentRootId, dirName);
            return (true, parent.ToString());
		}

		public (bool, string) CreateImage(string fileName, string parent, byte[] data, string mimeType)
		{
			throw new NotImplementedException();
		}

		public void CreateImage(string fileName, string parent, string inFilePath)
		{
			throw new NotImplementedException();
		}

		public void CreateImage(string fullPath, string inFilePath)
		{
			throw new NotImplementedException();
		}

		public void CreateImages(string parent, string[] filePathArray)
		{
			throw new NotImplementedException();
		}

		public void CreateTable()
		{
			return;
		}      

		public void DeleteDirectory(string fullPath)
		{
			throw new NotImplementedException();
		}

		public void DeleteDirectory(int id)
		{
			throw new NotImplementedException();
		}

		public void DeleteFile(string fullPath)
		{
			throw new NotImplementedException();
		}

		public void DeleteFile(int id)
		{
			throw new NotImplementedException();
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
			var dArray = jsonStructureManager.GetDirectoryStructureFromParent(rootId);
			//var list = sqlite.GetValues(TABLE_DIRECTORIES, "Parent = {0} and Name = '{1}'".FormatString(rootId, dirName));
			if (dArray != null && dArray.Length > 0)
				foreach (var dir in dArray)
					if (dir.Name.Equals(dirName))
						dirId = dir.Id;
            else
                dirId = -1;

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
        public string GetDirectoryPath(int id)
        {
            var pathList = new List<string>();

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
        public string GetFilePath(int id)
        {
			var file = jsonStructureManager.GetFileStructure(id);
			var fileName = file.Name;
			var parent = file.Parent;

            string dirPath = GetDirectoryPath(parent);
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
                sb.AppendFormat("\t\tPath:\t {0}\n", GetDirectoryPath(id));
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
                sb.AppendFormat("\t\tPath:\t {0}\n", GetFilePath(id));
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
            throw new NotImplementedException();
        }

		public void WriteToFile(string filePath, string outFilePath)
		{
			throw new NotImplementedException();
		}

		public void WriteToFile(int id, string outFilePath)
		{
			throw new NotImplementedException();
		}

		public void WriteToFile(DataFileInfo[] values, string outFilePath)
		{
			throw new NotImplementedException();
		}


		public void Dispose()
        {
            fManager?.Dispose();
            var json = jsonStructureManager?.ToString();
			Console.WriteLine(json);
			jsonStructureManager?.WriteToFile(jsonName);

			fManager = null;
			jsonStructureManager = null;
        }
	}
}
