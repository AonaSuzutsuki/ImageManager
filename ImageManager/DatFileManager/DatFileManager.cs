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

		public byte[] GetBytes(int start)
		{
			byte[] data = null;

			if (fileStream != null)
			{
				//id = GetIntAndSeek(fileStream, start, ID_LEN);
				var length = GetIntAndSeek(fileStream, start, LEN);

				data = new byte[length];
				fileStream.Read(data, 0, data.Length);
			}

			return data;
		}

		public void Dispose()
		{
			((IDisposable)fileStream).Dispose();
		}

        public void Write(byte[] data)
        {
            var len = data.Length;
            var lenArray = BitConverter.GetBytes(len);

            fileStream.Seek(0, SeekOrigin.End);
            fileStream.Write(lenArray, 0, lenArray.Length);
            fileStream.Write(data, 0, data.Length);
            fileStream.Flush();
        }

		private static int GetIntAndSeek(Stream stream, long start, long length)
		{
			var idLenArray = new byte[length];
			stream.Seek(start, SeekOrigin.Begin);
			stream.Read(idLenArray, 0, idLenArray.Length);
            var idLength = BitConverter.ToInt32(idLenArray, 0);
			//stream.Seek(idLenArray.Length, SeekOrigin.Current);

			return idLength;
		}
    }
}
