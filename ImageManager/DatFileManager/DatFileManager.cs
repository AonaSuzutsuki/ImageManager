using System;
using System.IO;

namespace DatFileManager
{
	public class DatFileManager : IDisposable
    {
        
		private const int LEN = 4;

		#region Fields
		private string filePath;
		private FileStream fileStream;
		#endregion

		public DatFileManager(string filePath)
        {
			this.filePath = filePath;
			Open();
        }

		public byte[] GetBytes(int start)
		{
			byte[] data = null;

			if (fileStream != null)
			{
				var lenArray = new byte[LEN];
				fileStream.Read(lenArray, start, lenArray.Length);
				var length = BitConverter.ToInt32(lenArray, 0);

				data = new byte[length];
				fileStream.Read(data, start + 4, data.Length);
			}

			return data;
		}

		public void Dispose()
        {
            ((IDisposable)fileStream).Dispose();
        }


		private void Open()
		{
			fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
		}
    }
}
