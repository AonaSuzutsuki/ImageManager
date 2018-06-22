using System;
using System.Collections.Generic;
using FileManagerLib.Extensions.Collections;

namespace FileManagerLib.Filer.Json
{
	public class JsonStructureManager
	{
		private readonly Dictionary<long, DirectoryStructure> directories = new Dictionary<long, DirectoryStructure>();
		private readonly Dictionary<long, FileStructure> files = new Dictionary<long, FileStructure>();

		public JsonStructureManager(string text)
		{         
			var table = Database.Json.JsonSerializer.ToObject<TableStructure>(text);
			table.Directory.ForEach((obj) => directories.Add(obj.Id, obj));
			table.File.ForEach((obj) => files.Add(obj.Id, obj));
		}


		#region Directory
		public void CreateDirectory(DirectoryStructure directoryStructure)
		{
			directories.Add(directoryStructure.Id, directoryStructure);
		}

		public void CreateDirectory(long id, long parent, string name)
		{
			var directoryStructure = new DirectoryStructure
			{
				Id = id,
				Parent = parent,
				Name = name
			};
			CreateDirectory(directoryStructure);
		}

		public DirectoryStructure GetDirectoryStructure(long id)
		{
			if (directories.ContainsKey(id))
				return directories[id];
			return null;
		}

		public void ChangeDirectory(long id, DirectoryStructure directoryStructure)
		{
			if (GetDirectoryStructure(id) == null)
				return;

			directories[id] = directoryStructure;
		}

		public void DeleteDirectory(long id)
		{
			directories.Remove(id);
		}
		#endregion


		#region File
		public void CreateFile(FileStructure fileStructure)
		{
			files.Add(fileStructure.Id, fileStructure);
		}

		public void CreateFile(long id, long parent, string name, long location, string mtype)
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

		public FileStructure GetFileStructure(long id)
		{
			if (files.ContainsKey(id))
				return files[id];
			return null;
		}

		public void ChangeFile(long id, FileStructure fileStructure)
		{
			if (GetFileStructure(id) == null)
				return;

			files[id] = fileStructure;
		}

		public void DeleteFile(long id)
		{
			files.Remove(id);
		}
		#endregion
	}
}
