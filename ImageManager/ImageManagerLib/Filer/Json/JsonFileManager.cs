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
	public class JsonFileManager : AbstractJsonResourceManager
	{

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Events      
        public event ReadWriteProgressEventHandler WriteToFilesProgress;
        public event ReadWriteProgressEventHandler WriteIntoResourceProgress;
		#endregion

		public JsonFileManager(string filePath, bool newFile = false, bool isCheckHash = true) : base(filePath, newFile, isCheckHash)
		{
		}


        #region File
        public void CreateFile(string fileName, string parent, byte[] data, string mimeType, string hash)
		{
			var (nextId, parentId) = ResolveTermParameters(fileName, parent);         
			var start = fManager.Write(data, LEN);         
			jsonStructureManager.CreateFile(nextId, parentId, fileName, start, hash, new Dictionary<string, string> { { "MimeType", mimeType } });
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
					jsonStructureManager.CreateFile(nextId, parentId, fileName, start, hash, new Dictionary<string, string> { { "MimeType", mimeType } });
                }
                else
                {
                    var data = ByteLoader.FromFile(stream);
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
        #endregion

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

		#region File Writer
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

				//Array.Sort(dirs);
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
            }
			return false;
        }
        #endregion
        
        public override string ToString()
		{
			return base.ToString();
		}      
	}
}
