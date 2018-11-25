using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommonCoreLib.File;
using CommonExtensionLib.Extensions;
using FileManagerLib.Dat;
using FileManagerLib.Extensions.Path;
using FileManagerLib.Path;

namespace FileManagerLib.File.Json
{
	public class JsonFileManager : JsonResourceManager
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
        public void CreateFile(string fileName, string parent, string inFilePath, Action<string> action = null)
		{
			if (!System.IO.File.Exists(inFilePath))
				return;

            var pId = GetDirectoryId(PathSplitter.SplitPath(parent));
            if (jsonStructureManager.ExistedFile(pId, fileName))
                return;
   
			var mimeType = MimeType.MimeTypeMap.GetMimeType(inFilePath);
			using (var stream = new FileStream(inFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
                string hash = string.Empty;
                if (jsonStructureManager.IsCheckHash)
                    hash = CommonCoreLib.Crypto.Sha256.GetSha256(stream);

                if (stream.Length > fManager.SplitSize)
                {
                    WriteBytes(fileName, parent, hash, () =>
                    {
                        return fManager.Write(stream, (writeStream) =>
                        {
                            while (true)
                            {
                                byte[] bs = new byte[fManager.SplitSize];
                                int readSize = stream.Read(bs, 0, bs.Length);
                                if (readSize == 0)
                                    break;
                                writeStream.Write(bs, 0, readSize);
                            }
                        }, LEN);
                    }, new Dictionary<string, string> { { "MimeType", mimeType } });
                }
                else
                {
                    var data = ByteLoader.FromStream(stream);
                    if (data != null)
                        WriteBytes(fileName, parent, data, hash, new Dictionary<string, string> { { "MimeType", mimeType } });
                }

                if (action != null)
                    action.Invoke(inFilePath);
                else
                    WriteIntoResourceProgress?.Invoke(this, new ReadWriteProgressEventArgs(1, 1, inFilePath, true));

            }         
		}

		public void CreateFile(string fullPath, string inFilePath)
		{
			var (parent, fileName) = fullPath.GetFilenameAndParent();
			CreateFile(fileName, parent.ToString(), inFilePath);
		}

        public void CreateFiles(string parent, string[] filePaths)
        {
            foreach (var tuple in filePaths.Select((v, i) => new { v, i }))
            {
                var filename = System.IO.Path.GetFileName(tuple.v);
                CreateFile(filename, parent, tuple.v,
                    (fname) => WriteIntoResourceProgress?.Invoke(this, new ReadWriteProgressEventArgs(tuple.i + 1, filePaths.Length, fname, true)));
                //if (action != null)
                //    action?.Invoke(tuple.i, tuple.v, true);
                //else
                //    WriteIntoResourceProgress?.Invoke(this, new ReadWriteProgressEventArgs(tuple.i + 1, filePaths.Length, tuple.v, true));
            }
        }

        public void CreateFiles(string parent, string dirPath, Action<int, string, bool> action = null)
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
                if (action != null)
                    CreateFile(System.IO.Path.GetFileName(path), System.IO.Path.GetDirectoryName(internalFilePathArray[file.i]), path,
                        (fname) => action.Invoke(file.i, fname, true));
                else
                    CreateFile(System.IO.Path.GetFileName(path), System.IO.Path.GetDirectoryName(internalFilePathArray[file.i]), path);
            }
		}

        public void CreateFilesOnDirectories(string fullPath, string[] inDirPaths)
        {
            var count = DirectorySearcher.CountFiles(inDirPaths);
            var dindex = 0;
            foreach (var target in inDirPaths)
            {
                string dirPath = "{0}/{1}".FormatString(fullPath, System.IO.Path.GetFileName(target));
                try
                {
                    CreateDirectory(dirPath);
                }
                catch { }
                CreateFiles(dirPath, target, (index, currentFilePath, isComplete) =>
                    WriteIntoResourceProgress?.Invoke(this, new ReadWriteProgressEventArgs(++dindex, count, currentFilePath, isComplete))
                );
            }
        }

        public void DeleteFile(string fullPath)
        {
            base.DeleteResource(fullPath);
        }
        public void DeleteFile(int id)
        {
            base.DeleteResource(id);
        }

        public bool ExistFile(string fullPath)
        {
            return base.ExistResource(fullPath);
        }

        public FileStructure[] GetFiles(string fullPath)
        {
            return base.GetResources(fullPath);
        }
        public FileStructure[] GetFiles(int dirId)
        {
            return base.GetResources(dirId);
        }
        #endregion

        /// <summary>
        /// Convert path on file system to internal path.
        /// </summary>
        /// <param name="basePath">dir path on file system</param>
        /// <param name="parent">internal parent directory path</param>
        /// <param name="dirPathArray">filepaths on file system</param>
        /// <returns></returns>
        private static string[] ResolveAbsolutePath(string basePath, string parent, string[] dirPathArray)
		{
			string func(string path, string referencePath)
			{
				//var fileUri = new Uri(path);
				//var referenceUri = new Uri(referencePath);
				//return parent + referenceUri.MakeRelativeUri(fileUri).ToString();
                return (parent.Remove(0, 1) + path.Replace(referencePath, "")).Replace("/", "\\");
			}

			var list = new List<string>();
			foreach (var dir in dirPathArray)
                list.Add(func(dir, basePath));
			return list.ToArray();
		}

		#region File Writer
		public bool WriteToFile(string filePath, string outFilePath, Action<string, bool> action = null)
		{
            var (parent, fileName) = filePath.GetFilenameAndParent();
            int rootId = GetDirectoryId(parent);
            bool isComp = false;
            if (jsonStructureManager.ExistedFile(rootId, fileName))
            {
                var file = jsonStructureManager.GetFileStructureFromParent(rootId, fileName);
                isComp = WriteToFile(file, outFilePath);
            }
            action?.Invoke(filePath, isComp);
            return isComp;
        }

		public void WriteToDir(string filePath, string outFilePath, Action<string, bool> action = null)
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
                    action?.Invoke(item.value.Name, isOk);
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

        public int GetAllFilesCount(string dirPath)
        {
            var (parent, fileName) = dirPath.GetFilenameAndParent();
            int rootId = GetDirectoryId(parent);
            if (jsonStructureManager.ExistedDirectory(rootId, fileName))
            {
                var dirId = GetDirectoryId(PathSplitter.SplitPath(dirPath));
                var files = jsonStructureManager.GetFileAllStructuresFromParent(dirId);
                return files.Length;
            }
            return 0;
        }
        
        public override string ToString()
		{
			return base.ToString();
		}      
	}
}
