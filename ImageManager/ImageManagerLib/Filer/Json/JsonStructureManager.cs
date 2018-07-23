using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommonExtensionLib.Extensions;
using FileManagerLib.Dat;
using FileManagerLib.Extensions.Collections;

namespace FileManagerLib.Filer.Json
{
	public class JsonStructureManager
	{
		private readonly SortedDictionary<int, DirectoryStructure> directories = new SortedDictionary<int, DirectoryStructure>();
		private readonly SortedDictionary<int, FileStructure> files = new SortedDictionary<int, FileStructure>();

		public int NextDirectoryId
		{
			get;
			private set;
		}

		public int NextFileId
		{
			get;
			private set;
		}

		public bool IsChenged
		{
			get;
			private set;
		}

        public bool IsCheckHash { get; }

		public JsonStructureManager(string text, bool isCheckhHash)
		{
			var table = Database.Json.JsonSerializer.ToObject<TableStructure>(text);
            IsCheckHash = table == null ? isCheckhHash : table.IsCheckHash;
            table?.Directory?.ForEach((obj) => directories.Add(obj.Id, obj));
			table?.File?.ForEach((obj) => files.Add(obj.Id, obj));
			if (table == null)
				IsChenged = true;

			NextDirectoryId = directories.Count + 1;
			NextFileId = files.Count + 1;
		}


		#region Directory
		public void CreateDirectory(DirectoryStructure directoryStructure)
		{
			directories.Add(directoryStructure.Id, directoryStructure);
			NextDirectoryId++;
			IsChenged = true;
		}

		public void CreateDirectory(int id, int parent, string name)
		{
			var directoryStructure = new DirectoryStructure
			{
				Id = id,
				Parent = parent,
				Name = name
			};
			CreateDirectory(directoryStructure);
		}

		public DirectoryStructure GetDirectoryStructure(int id)
		{
			if (directories.ContainsKey(id))
				return directories[id];
			return null;
		}

		public DirectoryStructure[] GetDirectoryStructuresFromParent(int parentId)
		{
			var dList = new List<DirectoryStructure>();
			foreach (var dir in directories.Values)
                if (dir.Parent == parentId)
                    dList.Add(dir);
			return dList.ToArray();
		}

        public DirectoryStructure[] GetDirectoryAllStructuresFromParent(int parentId)
        {
            var dirs = GetDirectoryStructuresFromParent(parentId);
            var dList = new List<DirectoryStructure>(dirs);
            foreach (var dir in dirs)
            {
                dList.AddRange(GetDirectoryAllStructuresFromParent(dir.Id));
            }

            return dList.ToArray();
        }

        public DirectoryStructure[] GetDirectoryStructures()
		{
			var array = directories.Values.ToArray();
			return array;
		}

		public void ChangeDirectory(int id, DirectoryStructure directoryStructure)
		{
			if (GetDirectoryStructure(id) == null)
				return;

			directories[id] = directoryStructure;
            IsChenged = true;
		}

		public void DeleteDirectory(int id)
		{
			var dirs = GetDirectoryStructuresFromParent(id);
			foreach (var dir in dirs)
			{
				DeleteDirectory(dir.Id);
			}

			directories.Remove(id);
			DeleteFileFromParent(id);
            IsChenged = true;
		}

		public void DeleteDirectoryFromParent(int parentId)
		{
			var removeList = new List<DirectoryStructure>(directories.Values);
			foreach (var dir in removeList)
			{
				if (dir.Parent == parentId)
				{
					if (directories.ContainsKey(dir.Id))
					{
						var _dir = directories[dir.Id];
						DeleteFileFromParent(_dir.Parent);
						directories.Remove(dir.Id);
                        IsChenged = true;
					}
				}
			}
		}

        public bool ExistedDirectory(int parentId, string name)
        {
            if (parentId == 0 && name.Equals(""))
                return true;

            var dirs = GetDirectoryStructuresFromParent(parentId);
            foreach (var dir in dirs)
            {
                if (dir.Name.Equals(name))
                    return true;
            }
            return false;
        }
		#endregion


		#region File
		public void CreateFile(FileStructure fileStructure)
		{
			files.Add(fileStructure.Id, fileStructure);
			NextFileId++;
            IsChenged = true;
		}

		public void CreateFile(int id, int parent, string name, long location, string mtype, string hash)
		{
			var fileStructure = new FileStructure
			{
				Id = id,
				Parent = parent,
				Name = name,
				Location = location,
				MimeType = mtype,
				Hash = hash
			};
			CreateFile(fileStructure);
		}

		public FileStructure GetFileStructure(int id)
		{
			if (files.ContainsKey(id))
				return files[id];
			return null;
		}

		public FileStructure[] GetFileStructures()
        {
            var array = files.Values.ToArray();
            return array;
        }

		public FileStructure GetFileStructureFromParent(int parentId, string name)
		{
			foreach (var file in files.Values)
			{
				if (file.Parent == parentId && file.Name.Equals(name))
				{
					return file;
				}
			}
			return null;
		}

        public FileStructure[] GetFileStructuresFromParent(int parentId)
        {
            var list = new List<FileStructure>();
            foreach (var file in files.Values)
            {
                if (file.Parent == parentId)
                {
                    list.Add(file);
                }
            }
            return list.ToArray();
        }

        public FileStructure[] GetFileAllStructuresFromParent(int parentId)
        {
            var files = GetFileStructuresFromParent(parentId);
            var dirs = GetDirectoryStructuresFromParent(parentId);
            var dList = new List<FileStructure>(files);
            foreach (var dir in dirs)
            {
                dList.AddRange(GetFileAllStructuresFromParent(dir.Id));
            }

            return dList.ToArray();
        }

        public void ChangeFile(int id, FileStructure fileStructure)
		{
			if (GetFileStructure(id) == null)
				return;

			files[id] = fileStructure;
            IsChenged = true;
		}

		public void DeleteFile(int id)
		{
			files.Remove(id);
            IsChenged = true;
		}

		public void DeleteFileFromParent(int parentId)
		{
			var removeList = new List<FileStructure>(files.Values);
			foreach (var file in removeList)
            {
				if (file.Parent == parentId)
                {
					if (files.ContainsKey(file.Id))
					{
						files.Remove(file.Id);
                        IsChenged = true;
					}
                }
            }
		}

        public bool ExistedFile(int parentId, string name)
        {
			foreach (var file in GetFileStructuresFromParent(parentId))
            {
                if (file.Name.Equals(name))
                    return true;
            }
            return false;
        }
        #endregion

        #region Vacuum
        public void Vacuum(DatFileManager srcfileManager, DatFileManager destfileManager, int identifierLength, Action<int, int, string> action)
        {
            var files = GetFileStructures();
            foreach (var dataFileInfo in files.Select((v, i) => new { v, i }))
            {
                var id = dataFileInfo.v.Id;
                var loc = dataFileInfo.v.Location;
                var nloc = srcfileManager.WriteToTemp(loc, destfileManager, identifierLength);

                ChangeFile(id, new FileStructure
                {
                    Id = dataFileInfo.v.Id,
                    Parent = dataFileInfo.v.Parent,
                    Name = dataFileInfo.v.Name,
                    Location = nloc,
                    MimeType = dataFileInfo.v.MimeType,
                    Hash = dataFileInfo.v.Hash
                });
                action?.Invoke(dataFileInfo.i + 1, files.Length, dataFileInfo.v.Name);
            }
            IsChenged = true;
        }
        #endregion


        #region Common
        public override string ToString()
		{
			var tableStructure = new TableStructure
			{
				Directory = directories.Values.ToArray(),
				File = files.Values.ToArray(),
                IsCheckHash = IsCheckHash
			};
			var json = Database.Json.JsonSerializer.ToJson(tableStructure);
			return json;
		}

		public void WriteToFile(string filePath)
		{
			using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
			    using (var sw = new StreamWriter(fs))
				    sw.Write(ToString());
		}
		#endregion
	}
}
