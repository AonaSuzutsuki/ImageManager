using System;
using System.IO;

namespace Dat
{
	public class DatFileManager : IDisposable
    {
        
		private const int LEN = 4;

		#region Fields
		private FileStream fileStream;
        #endregion

        #region Properties
        public string FilePath
        {
            get;
            private set;
        }
        #endregion

        public DatFileManager(string filePath)
        {
			FilePath = filePath;
			fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }

		public byte[] GetBytes(long start)
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

		public byte[] GetBytesFromEnd()
		{
			byte[] data = null;

			if (fileStream != null)
            {
				var idArray = new byte[LEN];
				fileStream.Seek(-idArray.Length, SeekOrigin.End);
				fileStream.Read(idArray, 0, idArray.Length);
				var length = BitConverter.ToInt32(idArray, 0);


				fileStream.Seek(-(length + idArray.Length), SeekOrigin.End);
                data = new byte[length];
                fileStream.Read(data, 0, data.Length);
            }

            return data;
		}

        public (long, byte[]) GetPartialBytes(long start, long length)
        {
            long len = 0;
            byte[] data = null;

            if (fileStream != null)
            {
                len = GetIntAndSeek(fileStream, start, LEN);

                data = new byte[length];
                fileStream.Read(data, 0, data.Length);
            }

            return (len, data);
        }

		public void Dispose()
		{
			((IDisposable)fileStream).Dispose();
		}

        public long Write(byte[] data)
        {
            var len = data.Length;
            var lenArray = BitConverter.GetBytes(len);
            
            var pos = fileStream.Position;
            fileStream.Seek(0, SeekOrigin.End);
            fileStream.Write(lenArray, 0, lenArray.Length);
            fileStream.Write(data, 0, data.Length);

            return pos;
        }

		public long WriteToEnd(byte[] data)
		{
			var len = data.Length;
            var lenArray = BitConverter.GetBytes(len);
			var pos = fileStream.Position;

			fileStream.Seek(0, SeekOrigin.End);
			fileStream.Write(data, 0, data.Length);
			fileStream.Write(lenArray, 0, lenArray.Length);

			return pos;
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

        public long WriteToTemp(long loc, DatFileManager dest)
        {
            var srcStream = this.fileStream;
            var destStream = dest.fileStream;

            srcStream.Seek(loc, SeekOrigin.Begin);
            int length = GetIntAndSeek(srcStream, loc, LEN);

            var data = new byte[length];
            srcStream.Read(data, 0, data.Length);

            long retloc = destStream.Position;
            destStream.Write(BitConverter.GetBytes(length), 0, LEN);
            destStream.Write(data, 0, data.Length);
            return retloc;


            //var srcStream = this.fileStream;
            //var destStream = dest.fileStream;
            //int length = GetIntAndSeek(srcStream, loc, LEN);
            //srcStream.Seek(loc, SeekOrigin.Begin);

            //var lengthArray = BitConverter.GetBytes(length);
            //destStream.Write(lengthArray, 0, LEN);
            //byte[] bs = new byte[4096];
            //while (true)
            //{
            //    int readSize = srcStream.Read(bs, 0, bs.Length);
            //    if (readSize == 0)
            //        break;

            //    destStream.Write(bs, 0, readSize);
            //}
        }
    }
}
