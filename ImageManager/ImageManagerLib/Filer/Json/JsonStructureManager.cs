using System;
using System.Collections.Generic;
using System.IO;
using FileManagerLib.Extensions.Collections;

namespace FileManagerLib.Filer.Json
{
	public class JsonStructureManager
	{
		private readonly SortedDictionary<int, DirectoryStructure> directories = new SortedDictionary<int, DirectoryStructure>();
		private readonly SortedDictionary<int, FileStructure> files = new SortedDictionary<int, FileStructure>();

		public JsonStructureManager(string text)
		{
			var table = Database.Json.JsonSerializer.ToObject<TableStructure>(text);
			table?.Directory?.ForEach((obj) => directories.Add(obj.Id, obj));
			table?.File?.ForEach((obj) => files.Add(obj.Id, obj));
		}


		#region Directory
		public void CreateDirectory(DirectoryStructure directoryStructure)
		{
			directories.Add(directoryStructure.Id, directoryStructure);
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

		public DirectoryStructure[] GetDirectoryStructureFromParent(int parentId)
		{
			var dList = new List<DirectoryStructure>();
			foreach (var dir in directories.Values)
				if (dir.Parent == parentId)
					dList.Add(dir);
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
		}

		public void DeleteDirectory(int id)
		{
			directories.Remove(id);
		}
		#endregion


		#region File
		public void CreateFile(FileStructure fileStructure)
		{
			files.Add(fileStructure.Id, fileStructure);
		}

		public void CreateFile(int id, int parent, string name, int location, string mtype)
		{
			var fileStructure = new FileStructure
			{
				Id = id,
				Parent = parent,
				Name = name,
				Location = location,
				MimeType = mtype
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

		public void ChangeFile(int id, FileStructure fileStructure)
		{
			if (GetFileStructure(id) == null)
				return;

			files[id] = fileStructure;
		}

		public void DeleteFile(int id)
		{
			files.Remove(id);
		}
		#endregion


		#region Common
		public override string ToString()
		{
			var tableStructure = new TableStructure
			{
				Directory = directories.Values.ToArray(),
				File = files.Values.ToArray()
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
