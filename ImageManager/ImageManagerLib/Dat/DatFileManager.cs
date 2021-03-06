﻿using Clusterable.IO;
using System;
using System.IO;
using System.Linq;

namespace FileManagerLib.Dat
{
    public class DatFileManager : IDisposable
    {

        private const int LEN = 4;

        #region Fields
        private ClusterableFileStream fileStream;
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

        public bool IsCheckHash { get; set; } = true;
        #endregion

        public DatFileManager(string filePath)
        {
			FilePath = filePath;
            fileStream = new ClusterableFileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read); //18077000
        }

		public byte[] GetBytes(long start, int identifierLength = LEN)
		{
			byte[] data = null;

			if (fileStream != null)
			{
				//id = GetIntAndSeek(fileStream, start, ID_LEN);
				var length = GetIntAndSeek(fileStream, start, identifierLength);

				data = new byte[length];
				fileStream.Read(data, 0, data.Length);
			}

			return data;
		}

		public byte[] GetBytesFromEnd(int identifierLength = LEN)
		{
			byte[] data = null;

			if (fileStream != null)
            {
				var idArray = new byte[identifierLength];
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

        public (uint, byte[]) GetPartialBytes(long start, long length, int identifierLength = LEN)
        {
            uint len = 0;
            byte[] data = null;

            if (fileStream != null)
            {
                len = (uint)GetIntAndSeek(fileStream, start, identifierLength);

                data = new byte[length];
                fileStream.Read(data, 0, data.Length);
            }

            return (len, data);
        }
        
		public bool WriteToFile(long start, string outFilePath, string expHash, int identifierLength = LEN)
		{
            bool isOk = false;
            if (fileStream != null)
            {
                using (var fs = new FileStream(outFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var length = GetIntAndSeek(fileStream, start, identifierLength);
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
    
                if (IsCheckHash)
                {
                    using (var stream = new FileStream(outFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var hash = Crypto.Sha256.GetSha256(stream);
                        if (expHash.Equals(hash))
                            isOk = true;
                        else
                            isOk = false;
                    }
                }
			}
            return IsCheckHash ? isOk : true;
        }

		public void Dispose()
		{
            fileStream.Dispose();
		}

        public DatFileManager Rename(string suffix)
        {
			var splitSize = fileStream.SplitSize;
            var filenames = fileStream.Delete();
			fileStream.Dispose();

            var prefs = new ClusterableFileStream(filenames[0] + suffix, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, splitSize);
            var srcFilenames = prefs.Filenames;
            prefs.Dispose();

			foreach (var item in srcFilenames.Select((value, index) => new { index, value }))
            {
				var dest = filenames[item.index];
				File.Move(item.value, dest);
            }
            var datFileManager = new DatFileManager(filenames[0]);
            return datFileManager;
            //fileStream = new ClusterableFileStream(filenames[0], FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        }

        public long Write(byte[] data, int identifierLength = LEN)
        {
            var len = data.Length;
            var lenArray = BitConverter.GetBytes(len);
            
			fileStream.Seek(LastPositionWithoutJson, SeekOrigin.End);
            var pos = fileStream.Position;
            fileStream.Write(lenArray, 0, identifierLength);
            fileStream.Write(data, 0, data.Length);
			LastPositionWithoutJson += (len + lenArray.Length);

            return pos;
        }

		public long Write(Stream stream, Action<Clusterable.IO.ClusterableFileStream> writeAction, int identifierLength = LEN)
		{
			var len = stream.Length;
            var lenArray = BitConverter.GetBytes(len);
   
			fileStream.Seek(LastPositionWithoutJson, SeekOrigin.End);
            var pos = fileStream.Position;
            fileStream.Write(lenArray, 0, identifierLength);

			writeAction?.Invoke(fileStream);

			LastPositionWithoutJson += len;

            return pos;
		}

		public long WriteToEnd(byte[] data, int identifierLength = LEN)
		{
			var len = data.LongLength;
            var lenArray = BitConverter.GetBytes(len);
			var pos = fileStream.Position;

			if (IsShiftJsonPosition && len > -LastPositionWithoutJson)
				fileStream.Seek(LastPositionWithoutJson, SeekOrigin.End);
            else
                fileStream.Seek(0, SeekOrigin.End);

            fileStream.Write(data, 0, data.Length);
			fileStream.Write(lenArray, 0, identifierLength);
   
			LastPositionWithoutJson = 0 - (len + lenArray.LongLength);

			return pos;
		}

		private static uint GetIntAndSeek(ClusterableFileStream stream, long start, long length)
		{
			var idLenArray = new byte[length];
			stream.Seek(start, SeekOrigin.Begin);
			stream.Read(idLenArray, 0, idLenArray.Length);
            var idLength = BitConverter.ToInt32(idLenArray, 0);
			//stream.Seek(idLenArray.Length, SeekOrigin.Current);

			return (uint)idLength;
		}

        public long WriteToTemp(long loc, DatFileManager dest, int identifierLength = LEN)
        {
            var srcStream = this.fileStream;
            var destStream = dest.fileStream;

            srcStream.Seek(loc, SeekOrigin.Begin);
            uint length = GetIntAndSeek(srcStream, loc, identifierLength);

            var data = new byte[length];
            srcStream.Read(data, 0, data.Length);

            long retloc = destStream.Position;
            destStream.Write(BitConverter.GetBytes(length), 0, identifierLength);
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
