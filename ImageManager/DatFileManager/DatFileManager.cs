using System;
using System.IO;

namespace DatFileManager
{
	public class DatFileManager : IDisposable
    {

		private const int ID_LEN = 4;
		private const int LEN = 4;

		#region Fields
		private string filePath;
		private FileStream fileStream;
		#endregion

		public DatFileManager(string filePath)
        {
			this.filePath = filePath;
			fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }

		public (int, byte[]) GetBytes(int start)
		{
			int id = -1;
			byte[] data = null;

			if (fileStream != null)
			{
				var idLength = GetIntAndSeek(fileStream, start, ID_LEN);
				var length = GetIntAndSeek(fileStream, 0, LEN);

				data = new byte[length];
				fileStream.Read(data, 0, data.Length);
			}

			return (id, data);
		}

		public void Dispose()
		{
			((IDisposable)fileStream).Dispose();
		}

        

		private static int GetIntAndSeek(Stream stream, long start, long length)
		{
			var idLenArray = new byte[length];
			stream.Seek(start, SeekOrigin.Current);
			stream.Read(idLenArray, 0, idLenArray.Length);
            var idLength = BitConverter.ToInt32(idLenArray, 0);
			stream.Seek(idLenArray.Length, SeekOrigin.Current);

			return idLength;
		}
    }
}
