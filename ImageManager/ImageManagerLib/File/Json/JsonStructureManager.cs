using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Clusterable.IO;
using CommonExtensionLib.Extensions;
using FileManagerLib.Dat;
using FileManagerLib.Extensions.Collections;
using Newtonsoft.Json;

namespace FileManagerLib.File.Json
{
	/// <summary>
    /// Structure manager for JSON.
    /// </summary>
	public class JsonStructureManager
	{
		private readonly SortedDictionary<int, DirectoryStructure> directories = new SortedDictionary<int, DirectoryStructure>();
		private readonly SortedDictionary<int, FileStructure> files = new SortedDictionary<int, FileStructure>();

        /// <summary>
        /// Gets the next directory identifier.
        /// </summary>
        /// <value>The next directory identifier.</value>
		public int NextDirectoryId
		{
			get;
			private set;
		}

        /// <summary>
        /// Gets the next file identifier.
        /// </summary>
        /// <value>The next file identifier.</value>
		public int NextFileId
		{
			get;
			private set;
		}

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:FileManagerLib.File.Json.JsonStructureManager"/> is chenged.
        /// </summary>
        /// <value><c>true</c> if is chenged; otherwise, <c>false</c>.</value>
		public bool IsChenged
		{
			get;
			private set;
		}

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:FileManagerLib.File.Json.JsonStructureManager"/> is check hash.
        /// </summary>
        /// <value><c>true</c> if is check hash; otherwise, <c>false</c>.</value>
        public bool IsCheckHash { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="T:FileManagerLib.File.Json.JsonStructureManager"/> class.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="isCheckhHash">If set to <c>true</c> is checkh hash.</param>
		public JsonStructureManager(string text, bool isCheckhHash)
		{
			var table = JsonConvert.DeserializeObject<TableStructure>(text);
            IsCheckHash = table == null ? isCheckhHash : table.IsCheckHash;
            table?.Directory?.ForEach((obj) => directories.Add(obj.Id, obj));
			table?.File?.ForEach((obj) => files.Add(obj.Id, obj));
			if (table == null)
				IsChenged = true;
;
            NextDirectoryId = directories.Count > 0 ? directories.Last().Value.Id + 1 : 1;
			NextFileId = files.Count > 0 ? files.Last().Value.Id + 1 : 1;
		}


		#region Directory
        /// <summary>
        /// Creates the directory.
        /// </summary>
        /// <param name="directoryStructure">Directory structure.</param>
		public void CreateDirectory(DirectoryStructure directoryStructure)
		{
			directories.Add(directoryStructure.Id, directoryStructure);
			NextDirectoryId++;
			IsChenged = true;
		}

        /// <summary>
        /// Creates the directory.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="name">Name.</param>
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

        /// <summary>
        /// Gets the directory structure.
        /// </summary>
        /// <returns>The directory structure.</returns>
        /// <param name="id">Identifier.</param>
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

        /// <summary>
        /// Gets the directory all structures from parent.
        /// </summary>
        /// <returns>The directory all structures from parent.</returns>
        /// <param name="parentId">Parent identifier.</param>
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

        /// <summary>
        /// Gets the directory structures.
        /// </summary>
        /// <returns>The directory structures.</returns>
        public DirectoryStructure[] GetDirectoryStructures()
		{
			var array = directories.Values.ToArray();
			return array;
		}

        /// <summary>
        /// Changes the directory.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="directoryStructure">Directory structure.</param>
		public void ChangeDirectory(int id, DirectoryStructure directoryStructure)
		{
			if (GetDirectoryStructure(id) == null)
				return;

			directories[id] = directoryStructure;
            IsChenged = true;
		}

        /// <summary>
        /// Deletes the directory.
        /// </summary>
        /// <param name="id">Identifier.</param>
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

        /// <summary>
        /// Deletes the directory from parent.
        /// </summary>
        /// <param name="parentId">Parent identifier.</param>
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

        /// <summary>
        /// Existeds the directory.
        /// </summary>
        /// <returns><c>true</c>, if directory was existeded, <c>false</c> otherwise.</returns>
        /// <param name="parentId">Parent identifier.</param>
        /// <param name="name">Name.</param>
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
        /// <summary>
        /// Creates the file.
        /// </summary>
        /// <param name="fileStructure">File structure.</param>
		public void CreateFile(FileStructure fileStructure)
		{
			files.Add(fileStructure.Id, fileStructure);
			NextFileId++;
            IsChenged = true;
		}

        /// <summary>
        /// Creates the file.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="name">Name.</param>
        /// <param name="location">Location.</param>
        /// <param name="hash">Hash.</param>
        /// <param name="additionals">Additionals.</param>
		public void CreateFile(int id, int parent, string name, long location, string hash, Dictionary<string, string> additionals = null)
		{
			var fileStructure = new FileStructure
			{
				Id = id,
				Parent = parent,
				Name = name,
				Location = location,
				Hash = hash,
                Additional = additionals
            };

            //if (additionals != null)
            //{
            //    fileStructure.Additional = new Dictionary<string, string>();
            //    var zip = additionals.Keys.Zip(additionals.Values, (_key, _value) => new { key = _key, value = _value });
            //    foreach (var val in zip)
            //        fileStructure.Additional.Add(val.key, val.value);
            //}

            CreateFile(fileStructure);
		}

        /// <summary>
        /// Gets the file structure.
        /// </summary>
        /// <returns>The file structure.</returns>
        /// <param name="id">Identifier.</param>
		public FileStructure GetFileStructure(int id)
		{
			if (files.ContainsKey(id))
				return files[id];
			return null;
		}

        /// <summary>
        /// Gets the file structures.
        /// </summary>
        /// <returns>The file structures.</returns>
		public FileStructure[] GetFileStructures()
        {
            var array = files.Values.ToArray();
            return array;
        }

        /// <summary>
        /// Gets the file structure from parent.
        /// </summary>
        /// <returns>The file structure from parent.</returns>
        /// <param name="parentId">Parent identifier.</param>
        /// <param name="name">Name.</param>
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

        /// <summary>
        /// Gets the file structures from parent.
        /// </summary>
        /// <returns>The file structures from parent.</returns>
        /// <param name="parentId">Parent identifier.</param>
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

        /// <summary>
        /// Gets the file all structures from parent.
        /// </summary>
        /// <returns>The file all structures from parent.</returns>
        /// <param name="parentId">Parent identifier.</param>
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

        /// <summary>
        /// Changes the file.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="fileStructure">File structure.</param>
        public void ChangeFile(int id, FileStructure fileStructure)
		{
			if (GetFileStructure(id) == null)
				return;

			files[id] = fileStructure;
            IsChenged = true;
		}

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public void DeleteFile(int id)
		{
			files.Remove(id);
            IsChenged = true;
		}

        /// <summary>
        /// Deletes the file from parent.
        /// </summary>
        /// <param name="parentId">Parent identifier.</param>
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

        /// <summary>
        /// Existeds the file.
        /// </summary>
        /// <returns><c>true</c>, if file was existeded, <c>false</c> otherwise.</returns>
        /// <param name="parentId">Parent identifier.</param>
        /// <param name="name">Name.</param>
        public bool ExistedFile(int parentId, string name)
        {
			foreach (var file in GetFileStructuresFromParent(parentId))
            {
                if (file.Name.Equals(name))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Existeds the file.
        /// </summary>
        /// <returns><c>true</c>, if file was existeded, <c>false</c> otherwise.</returns>
        /// <param name="id">Identifier.</param>
        public bool ExistedFile(int id)
        {
            return files.ContainsKey(id);
        }
        #endregion

        #region Vacuum
        /// <summary>
        /// Vacuum the specified srcfileManager, destfileManager, identifierLength and action.
        /// </summary>
        /// <param name="srcfileManager">Srcfile manager.</param>
        /// <param name="destfileManager">Destfile manager.</param>
        /// <param name="identifierLength">Identifier length.</param>
        /// <param name="action">Action.</param>
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
                    Additional = dataFileInfo.v.Additional,
                    Hash = dataFileInfo.v.Hash
                });
                action?.Invoke(dataFileInfo.i + 1, files.Length, dataFileInfo.v.Name);
            }
            IsChenged = true;
        }
        #endregion


        #region Common
        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:FileManagerLib.File.Json.JsonStructureManager"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:FileManagerLib.File.Json.JsonStructureManager"/>.</returns>
        public override string ToString()
		{
			var tableStructure = new TableStructure
			{
				Directory = directories.Values.ToArray(),
				File = files.Values.ToArray(),
                IsCheckHash = IsCheckHash
			};
			var json = JsonConvert.SerializeObject(tableStructure);
			return json;
		}

        /// <summary>
        /// Writes to file.
        /// </summary>
        /// <param name="filePath">File path.</param>
		public void WriteToFile(string filePath)
		{
			using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
			    using (var sw = new StreamWriter(fs))
				    sw.Write(ToString());
		}
		#endregion

        /// <summary>
        /// Writes the json.
        /// </summary>
        /// <param name="fileManager">File manager.</param>
        /// <param name="len">Length.</param>
		public void WriteJson(DatFileManager fileManager, int len)
		{
			var json = ToString();
			fileManager?.WriteToEnd(Encoding.UTF8.GetBytes(json), len);
			IsChenged = false;
		}
	}
}
