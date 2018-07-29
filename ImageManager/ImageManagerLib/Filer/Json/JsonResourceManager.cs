using System;
using System.Text;
using FileManagerLib.Extensions.Path;
using FileManagerLib.MimeType;

namespace FileManagerLib.Filer.Json
{
	public class JsonResourceManager : AbstractJsonResourceManager
	{
		public JsonResourceManager(string filePath, bool newFile = false, bool isCheckHash = true) : base(filePath, newFile, isCheckHash)
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

        public string GetString(string fullPath)
        {
            var bytes = GetBytes(fullPath);
            return Encoding.UTF8.GetString(bytes);
        }
		#endregion

		#region Write
		public void WriteBytes(string fullPath, byte[] bytes)
		{
			var (parent, fileName) = fullPath.GetFilenameAndParent();
			int rootId = GetDirectoryId(parent);
			if (!jsonStructureManager.ExistedFile(rootId, fileName))
			{
                var nextId = jsonStructureManager.NextFileId;
                var hash = Crypto.Sha256.GetSha256(bytes);

                var start = fManager.Write(bytes, LEN);
                jsonStructureManager.CreateFile(nextId, rootId, fileName, start, MimeTypeMap.GetMimeTypeFromExtension("txt"), hash);
            }
		}

        public void WriteString(string fullPath, string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            WriteBytes(fullPath, bytes);
        }
		#endregion
	}
}
