using Clusterable.IO;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace FileManagerLib.Dat
{
	/// <summary>
    /// データファイルの管理を提供します。
    /// </summary>
    public class DatFileManager : IDisposable
    {

        private const int LEN = 4;

        #region Fields
        private IClusterableStream fileStream;
        private long lastPositionWithoutJson = 0;
        #endregion

        #region Properties
        public string FilePath
        {
            get;
            private set;
        }

        /// <summary>
        /// ファイルへの書き込みの際に分割読み込みをするしきい値
        /// </summary>
		public int SplitSize { get; } = 134217728; //536870912

        /// <summary>
        /// 既に書き込まれているJSONをスキップして書き込むか上書きするかどうかを設定または取得します。
        /// </summary>
        public bool IsShiftJsonPosition { get; set; } = false;

        /// <summary>
        /// 取り除くべき既に書き込まれているJSONのサイズを負数ベースで返します。
        /// </summary>
		private long LastPositionWithoutJson
		{
			get => lastPositionWithoutJson > 0 ? 0 : lastPositionWithoutJson;
			set => lastPositionWithoutJson = value;
		}

        /// <summary>
        /// ハッシュ計算によるデータの整合性を確認するかどうかを設定あるいは取得します。
        /// </summary>
        public bool IsCheckHash { get; set; } = true;
        #endregion

        public DatFileManager(string filePath)
        {
			FilePath = filePath;
            fileStream = new ClusterableFileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read); //18077000
        }

        /// <summary>
        /// 指定された位置からデータの長さを除いたデータをバイト配列で返します。
        /// </summary>
        /// <param name="start">データの開始位置を指定します。</param>
        /// <param name="identifierLength">データの長さを格納するバイトサイズを指定します。</param>
        /// <returns>実データのバイト配列</returns>
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
        /// 末尾から実データの長さを除いたデータをバイト配列で返します。
        /// </summary>
        /// <param name="identifierLength">データの長さを格納するバイトサイズ</param>
        /// <returns>実データのバイト配列</returns>
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
        /// 指定された位置からデータの長さを除いたデータの部分をバイト配列で返します。
        /// </summary>
        /// <param name="start">データの開始位置を指定します。</param>
        /// <param name="length">取得したいデータの長さを指定します。</param>
        /// <param name="identifierLength">データの長さを格納するバイトサイズを指定します。</param>
        /// <returns>実データの部分バイト配列</returns>
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
