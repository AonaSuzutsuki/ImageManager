using Clusterable.IO;
using System;
using System.IO;
using System.Linq;

namespace Dat
{
    public class DatFileManager : IDisposable
    {
        
		private const int LEN = 4;

		#region Fields
		private Clusterable.IO.ClusterableFileStream fileStream;
        private long lastPositionWithoutJson = 0;
        #endregion

        #region Properties
        public string FilePath
        {
            get;
            private set;
        }

		public int SplitSize { get; } = 134217728; //536870912

        public bool IsShiftJsonPosition { get; set; } = false;

		private long LastPositionWithoutJson
		{
			get => lastPositionWithoutJson > 0 ? 0 : lastPositionWithoutJson;
			set => lastPositionWithoutJson = value;
		}
        #endregion

        public DatFileManager(string filePath)
        {
			FilePath = filePath;
			fileStream = new Clusterable.IO.ClusterableFileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
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
				var rc = fileStream.Read(idArray, 0, idArray.Length);
				var length = BitConverter.ToInt32(idArray, 0);


				fileStream.Seek(-(length + idArray.Length), SeekOrigin.End);
                data = new byte[length];
                fileStream.Read(data, 0, data.Length);
				LastPositionWithoutJson = -(length + idArray.Length);
            }

            return data;
		}

        public (uint, byte[]) GetPartialBytes(long start, long length)
        {
            uint len = 0;
            byte[] data = null;

            if (fileStream != null)
            {
                len = (uint)GetIntAndSeek(fileStream, start, LEN);

                data = new byte[length];
                fileStream.Read(data, 0, data.Length);
            }

            return (len, data);
        }

		public void WriteToFile(long start, string outFilePath)
		{
            if (fileStream != null)
            {
                using (var fs = new FileStream(outFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var length = GetIntAndSeek(fileStream, start, LEN);
                    if (length > SplitSize)
                    {
                        while (true)
                        {
                            var data = new byte[SplitSize];
                            int readSize = fileStream.Read(data, 0, data.Length);
                            if (length <= readSize)
                            {
                                fs.Write(data, 0, (int)length);
                                break;
                            }

                            fs.Write(data, 0, readSize);
                            length -= (uint)readSize;
                        }
                    }
                    else
                    {
                        var data = new byte[length];
                        fileStream.Read(data, 0, data.Length);
                        fs.Write(data, 0, data.Length);
                    }
                }
            }
        }

		public void Dispose()
		{
			((IDisposable)fileStream).Dispose();
		}

        public void Rename(string suffix)
        {
            var filenames = fileStream.Delete();

            var prefs = new ClusterableFileStream(filenames[0] + suffix, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            var srcFilenames = prefs.Filenames;
            prefs.Dispose();

            foreach (var item in filenames.Select((value, index) => new { index, value }))
            {
                var src = srcFilenames[item.index];
                File.Move(src, item.value);
            }
            fileStream = new ClusterableFileStream(filenames[0], FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        }

        public long Write(byte[] data)
        {
            var len = data.Length;
            var lenArray = BitConverter.GetBytes(len);
            
            var pos = fileStream.Position;
			fileStream.Seek(LastPositionWithoutJson, SeekOrigin.End);
            fileStream.Write(lenArray, 0, LEN);
            fileStream.Write(data, 0, data.Length);
			LastPositionWithoutJson += len;

            return pos;
        }

		public long Write(Stream stream, Action<Clusterable.IO.ClusterableFileStream> writeAction)
		{
			var len = stream.Length;
            var lenArray = BitConverter.GetBytes(len);

            var pos = fileStream.Position;
			fileStream.Seek(LastPositionWithoutJson, SeekOrigin.End);
            fileStream.Write(lenArray, 0, LEN);

			writeAction?.Invoke(fileStream);

			LastPositionWithoutJson += len;

            return pos;
		}

		public long WriteToEnd(byte[] data)
		{
			var len = data.Length;
            var lenArray = BitConverter.GetBytes(len);
			var pos = fileStream.Position;

			if (IsShiftJsonPosition && len > -LastPositionWithoutJson)
				fileStream.Seek(LastPositionWithoutJson, SeekOrigin.End);
            else
                fileStream.Seek(0, SeekOrigin.End);

            fileStream.Write(data, 0, data.Length);
			fileStream.Write(lenArray, 0, LEN);

			return pos;
		}

		private static uint GetIntAndSeek(Clusterable.IO.ClusterableFileStream stream, long start, long length)
		{
			var idLenArray = new byte[length];
			stream.Seek(start, SeekOrigin.Begin);
			stream.Read(idLenArray, 0, idLenArray.Length);
            var idLength = BitConverter.ToUInt32(idLenArray, 0);
			//stream.Seek(idLenArray.Length, SeekOrigin.Current);

			return idLength;
		}

        public long WriteToTemp(long loc, DatFileManager dest)
        {
            var srcStream = this.fileStream;
            var destStream = dest.fileStream;

            srcStream.Seek(loc, SeekOrigin.Begin);
            uint length = GetIntAndSeek(srcStream, loc, LEN);

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
