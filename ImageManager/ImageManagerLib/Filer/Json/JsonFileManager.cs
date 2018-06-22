using System;
using System.IO;
using CommonExtensionLib.Extensions;
using Dat;
using FileManagerLib.Path;

namespace FileManagerLib.Filer.Json
{
	public class JsonFileManager : IFileManager
	{

		#region Fields
		private JsonStructureManager jsonStructureManager;
		private DatFileManager fManager;
		#endregion

		public JsonFileManager(string filePath, bool newFile = false)
		{
			var datName = "{0}.dat".FormatString(System.IO.Path.GetFileNameWithoutExtension(filePath));

            if (newFile)
            {
				if (File.Exists(filePath))
					File.Delete(filePath);
                if (File.Exists(datName))
                    File.Delete(datName);
            }

            fManager = new DatFileManager(datName);
			jsonStructureManager = new JsonStructureManager("");
		}

		public (bool, string) CreateDirectory(string fullPath)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public void DataVacuum()
		{
			throw new NotImplementedException();
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

		public void Dispose()
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
			throw new NotImplementedException();
		}

		public int GetDirectoryId(PathItem pathItem)
		{
			throw new NotImplementedException();
		}

		public string GetDirectoryPath(int id)
		{
			throw new NotImplementedException();
		}

		public string GetFilePath(int id)
		{
			throw new NotImplementedException();
		}

		public DataFileInfo[] GetFiles(int did = 0)
		{
			throw new NotImplementedException();
		}

		public DataFileInfo[] GetFiles(DataFileInfo dataFileInfo)
		{
			throw new NotImplementedException();
		}

		public string TraceDirs()
		{
			throw new NotImplementedException();
		}

		public string TraceFiles()
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
	}
}
