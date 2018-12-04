using Clusterable.IO;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace FileManagerLib.Dat
{
	/// <summary>
	/// Provides a method of managing data files.
    /// </summary>
	public class DatFileManager : IDisposable
    {

        private const int LEN = 4;

        #region Fields
        private IClusterableStream fileStream;
        private long lastPositionWithoutJson = 0;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the file path.
        /// </summary>
        /// <value>The file path.</value>
        public string FilePath
        {
            get;
            private set;
        }

        /// <summary>
		/// Threshold for split reading when reading or writing files.
        /// </summary>
		public int SplitSize { get; } = 134217728; //536870912
  
		private long LastPositionWithoutJson
		{
			get => lastPositionWithoutJson > 0 ? 0 : lastPositionWithoutJson;
			set => lastPositionWithoutJson = value;
		}

        /// <summary>
		/// Set or get whether to check the consistency of data by hash calculation.
        /// </summary>
        public bool IsCheckHash { get; set; } = true;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="T:FileManagerLib.Dat.DatFileManager"/> class.
        /// </summary>
        /// <param name="filePath">File path.</param>
        public DatFileManager(string filePath)
        {
			FilePath = filePath;
            fileStream = new ClusterableFileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read); //18077000
        }

        /// <summary>
		/// Returns the actual data excluding the data length from the specified position as a byte array.
        /// </summary>
		/// <param name="start">Start position of data.</param>
		/// <param name="identifierLength">Byte size of the data length.</param>
        /// <returns>Byte array of actual data.</returns>
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
        
        /// <summary>
		/// Reads the actual data little by little the data length from the specified position and executes the designated delegate every time it reads it.
        /// </summary>
        /// <param name="start">Start position.</param>
		/// <param name="size">Threshold value for division and reading.</param>
		/// <param name="action">Delegate for execution.</param>
		/// <param name="identifierLength">Byte size of the data length.</param>
        public void ActionBytes(long start, int size, Action<byte[], int> action, int identifierLength = LEN)
        {
            if (fileStream != null)
            {
                long length = GetIntAndSeek(fileStream, start, identifierLength);

                while (true)
                {
                    var data = new byte[size];
                    int readSize = fileStream.Read(data, 0, data.Length);
                    if (length <= readSize)
                    {
                        int arglen = length < 0 ? 0 : (int)length;
                        action?.Invoke(data, arglen);
                        break;
                    }

                    action?.Invoke(data, readSize);
                    length -= (uint)readSize;
                }
            }
        }

        /// <summary>
		/// Returns the data excluding the actual data length from the end as a byte array.
        /// </summary>
		/// <param name="identifierLength">Byte size of the data length.</param>
        /// <returns>Byte array of actual data.</returns>
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

        /// <summary>
		/// Returns the part of the data from the specified position as a byte array.
        /// </summary>
        /// <param name="start">Start position.</param>
		/// <param name="length">The length of the data.</param>
		/// <param name="identifierLength">Byte size of the data length.</param>
        /// <returns>Byte array of actual data.</returns>
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
        
        /// <summary>
        /// Write to the file system.
        /// </summary>
        /// <returns><c>true</c>, if to file was writed, <c>false</c> otherwise.</returns>
        /// <param name="start">Start position.</param>
        /// <param name="outFilePath">Output file path.</param>
		/// <param name="expHash">Expected hash.</param>
		/// <param name="identifierLength">Byte size of the data length.</param>
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
                        var hash = CommonCoreLib.Crypto.Sha256.GetSha256(stream);
                        if (expHash.Equals(hash))
                            isOk = true;
                        else
                            isOk = false;
                    }
                }
			}
            return IsCheckHash ? isOk : true;
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:FileManagerLib.Dat.DatFileManager"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:FileManagerLib.Dat.DatFileManager"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="T:FileManagerLib.Dat.DatFileManager"/> in an unusable state. After calling <see cref="Dispose"/>,
        /// you must release all references to the <see cref="T:FileManagerLib.Dat.DatFileManager"/> so the garbage
        /// collector can reclaim the memory that the <see cref="T:FileManagerLib.Dat.DatFileManager"/> was occupying.</remarks>
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
                System.IO.File.Move(item.value, dest);
            }
            var datFileManager = new DatFileManager(filenames[0]);
            return datFileManager;
            //fileStream = new ClusterableFileStream(filenames[0], FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        }

        /// <summary>
        /// Write the specified <paramref name="data"/>.
        /// </summary>
        /// <returns>The write.</returns>
        /// <param name="data">Byte array of data.</param>
		/// <param name="identifierLength">Byte size of the data length.</param>
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

        /// <summary>
		/// Write to the specified stream with the writer delegate.
        /// </summary>
        /// <returns>Start position of writen data.</returns>
        /// <param name="stream">Written stream.</param>
        /// <param name="writeAction">Write action delegate.</param>
		/// <param name="identifierLength">Byte size of the data length.</param>
		public long Write(Stream stream, Action<IClusterableStream> writeAction, int identifierLength = LEN)
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

        /// <summary>
        /// Writes to end.
        /// </summary>
        /// <returns>Start position.</returns>
        /// <param name="data">Written data.</param>
		/// <param name="identifierLength">Byte size of the data length.</param>
		public long WriteToEnd(byte[] data, int identifierLength = LEN)
		{
			var len = data.LongLength;
            var lenArray = BitConverter.GetBytes(len);
			var pos = fileStream.Position;

			if (len > -LastPositionWithoutJson)
				fileStream.Seek(LastPositionWithoutJson, SeekOrigin.End);
            else
                fileStream.Seek(0, SeekOrigin.End);

            fileStream.Write(data, 0, data.Length);
			fileStream.Write(lenArray, 0, identifierLength);
   
			LastPositionWithoutJson = 0 - (len + lenArray.LongLength);

			return pos;
		}

		private static uint GetIntAndSeek(IClusterableStream stream, long start, long length)
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
