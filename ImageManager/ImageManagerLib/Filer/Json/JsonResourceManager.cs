using System;
using FileManagerLib.Extensions.Path;

namespace FileManagerLib.Filer.Json
{
	public class JsonResourceManager : AbstractJsonResourceManager
	{
		public JsonResourceManager(string filePath, bool newFile = false, bool isCheckHash = true, Func<string, bool> fileExistAct = null) : base(filePath, newFile, isCheckHash, fileExistAct)
		{
		}

		#region Read
		public byte[] GetBytes(string fullPath)
		{
			var (parent, fileName) = fullPath.GetFilenameAndParent();
			int rootId = GetDirectoryId(parent);
			if (jsonStructureManager.ExistedFile(rootId, fileName))
			{
				var file = jsonStructureManager.GetFileStructureFromParent(rootId, fileName);
				return fManager.GetBytes(file.Location, LEN);
			}
			return null;
		}
		#endregion

		#region Write
		public void WriteBytes(string fullPath, byte[] bytes)
		{
			var (parent, fileName) = fullPath.GetFilenameAndParent();
			int rootId = GetDirectoryId(parent);
			if (!jsonStructureManager.ExistedFile(rootId, fileName))
			{
				fManager.Write(bytes, LEN);
			}
		}
		#endregion
	}
}
