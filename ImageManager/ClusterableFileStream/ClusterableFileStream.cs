using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonExtensionLib.Extensions;

namespace Clusterable.IO
{
    /// <summary>
    /// FileStreamを用いて指定されたサイズ分に分割したファイルアクセスを提供します。
    /// </summary>
    public class ClusterableFileStream : IDisposable
    {

        #region Constants
        private const int SPLIT_LEN = 8;
        #endregion

        #region Fields
        private List<FileStream> streams = new List<FileStream>();

		private long position = 0;
		private string baseFilePath;
		private FileMode mode;
		private FileAccess access;
		private FileShare share;
		#endregion

		#region Properties
		public long Position
		{
			get
			{
                var prep = position - SPLIT_LEN;
				if (prep < 0)
					return 0;
				return prep;
			}
		}

		public long Length
        {
            get
            {
                long length = 0;
                foreach (var stream in streams)
                    length += stream.Length;
                return length;
            }
        }

		public long SplitSize { get; } //1073741824:1gb 10485760:10mb

        public string[] Filenames
        {
            get
            {
                var names = new List<string>();
                foreach (var stream in streams)
                    names.Add(stream.Name);
                return names.ToArray();
            }
        }
        #endregion

        /// <summary>
        /// FileStreamを用いて指定されたサイズ分に分割したファイルアクセスを提供します。
        /// </summary>
        /// <param name="path">書き込むあるいは読み込むファイルのパスを指定します。</param>
        /// <param name="mode">ファイルを開く方法を指定します。</param>
        /// <param name="access">ファイルへのアクセス制限を指定します。</param>
        /// <param name="share">その他のインスタンスに対してアクセス制限を指定します。</param>
        /// <param name="splitSize">ファイルの分割サイズをバイト単位で指定します。</param>
        /// <param name="assemblyDirPath">実行ファイルのディレクトリパスを指定します。</param>
        public ClusterableFileStream(string path, FileMode mode, FileAccess access, FileShare share, long splitSize = 1073741824, string assemblyDirPath = null)
        {
            this.baseFilePath = path;
            this.mode = mode;
            this.access = access;
            this.share = share;

            bool v = mode == FileMode.Create || !File.Exists(path);
            if (v)
            {
                var stream = new FileStream(path, mode, access, share);
                streams.Add(stream);
                var splistSizeArray = BitConverter.GetBytes(splitSize);
                SplitSize = splitSize;
                Write(splistSizeArray, 0, splistSizeArray.Length);
                //stream.Write(splistSizeArray, 0, SPLIT_LEN);
                //position += SPLIT_LEN;
            }
            else
            {
                if (assemblyDirPath == null)
                    assemblyDirPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var absolutePath = CommonCoreLib.Path.PathConverter.ToAbsolutePath(path, () => assemblyDirPath);
                var root = Path.GetDirectoryName(absolutePath);
                var fname = Path.GetFileName(path).Replace(".", @"\.");
                var files = Directory.GetFiles(root);
                var sorted = new SortedDictionary<int, FileStream>();
                foreach (var file in files)
                {
                    var reg = new Regex(fname + @"(\.(?<num>[0-9]{0,3}))?", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    var match = reg.Match(file);
                    if (match.Success)
                    {
                        var numstr = match.Groups["num"].Value;
                        var num = string.IsNullOrEmpty(numstr) ? 0 : numstr.ToInt();
                        var _stream = new FileStream(file, mode, access, share);
                        sorted.Add(num, _stream);
                        //position += _stream.Length;
                    }
                }
                streams.AddRange(sorted.Values);

                var stream = streams[0];
                var splistSizeArray = new byte[SPLIT_LEN];
                stream.Read(splistSizeArray, 0, SPLIT_LEN);
                SplitSize = BitConverter.ToInt64(splistSizeArray, 0);
                position += SPLIT_LEN;
            }
        }

        //public ClusterableFileStream(string path, FileMode mode, FileAccess access, FileShare share, string assemblyDirPath, long splitSize = 1073741824) : this(path, mode, access, share, splitSize, assemblyDirPath)
        //{
        //}

        
        /// <summary>
        /// ストリームにバイトのブロックを書き込みます。
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void Write(byte[] data, int offset, long length)
		{
			var startIndex = (int)Math.Floor((double)position / (double)SplitSize);

			if (startIndex >= streams.Count)
				streams.Add(new FileStream("{0}.{1}".FormatString(baseFilePath, streams.Count), mode, access, share));
			var stream = streams[startIndex];
            
			Func<Stream, int> func = (stm) => {
				var wLen = (int)(((startIndex + 1) * SplitSize) - position); // 書き込む長さ
                wLen = wLen > (int)length ? (int)length : wLen;
				return wLen;
			};

            var writeLen = func(stream);
			var remLen = length - writeLen; // 残りの長さ

            int start = (int)(position - SplitSize * startIndex);
            stream.Seek(start, SeekOrigin.Begin);
            stream.Write(data, offset, writeLen);
            position += writeLen;

			if (remLen > 0)
				Write(data, offset + writeLen, remLen);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
		public int Read(byte[] buffer, int offset, int length)
		{
            var prestreams = new List<Stream>();
            var remLens = new List<int>();
            var startIndex = (int)Math.Floor((double)position / (double)SplitSize);
			var pres = Math.Floor(((double)position + (double)length) / (double)SplitSize) - startIndex;
			var requres = (int)Math.Ceiling((double)length / (double)SplitSize) + (int)pres;

            // 読み込むべきストリームと長さのリストを計算
            var prelen = length;
			var preremlen = 0;
			for (int i = 0; i < requres; i++)
			{
				var index = startIndex + i;
				if (index < streams.Count)
				{               
					var isOver = position + prelen + preremlen > SplitSize * (startIndex + 1 + i);
                    if (isOver)
                    {
						var overSize = SplitSize * (startIndex + 1 + i) - (position + preremlen);
						remLens.Add((int)overSize);
                        prestreams.Add(streams[index]);
						prelen -= (int)overSize;
						preremlen += (int)overSize;
                    }
                    else
                    {
						if (prelen > 0)
						{
                            remLens.Add(prelen);
							prestreams.Add(streams[index]);
							break;
						}
                    }
				}
			}

			int start = (int)(position - SplitSize * startIndex) + offset;
			int readCount = 0;
			foreach (var item in prestreams.Select((value, index) => new { value, index }))
			{
				var rlen = remLens[item.index];

                byte[] buf;
                if (rlen < buffer.Length)
                    buf = new byte[rlen];
                else
                    buf = buffer;
                
                item.value.Seek(start, SeekOrigin.Begin);
				var prereadCount = item.value.Read(buf, 0, buf.Length);

                if (rlen < buffer.Length)
                    Buffer.BlockCopy(buf, 0, buffer, readCount, buf.Length);
				readCount += prereadCount;
                position += prereadCount;
                start = 0;
            }
            
			return readCount;
		}

		public void Seek(long offset, SeekOrigin seekOrigin)
		{
			if (seekOrigin == SeekOrigin.Begin)
			{
                position = SPLIT_LEN + offset;
			}
			else if (seekOrigin == SeekOrigin.Current)
			{
                position += offset;
			}
			else
			{
                position = Length + offset;         
			}
		}

        public string[] Delete(Func<string, string> func = null)
        {
            var filenames = new List<string>(streams.Count);
            foreach (var stream in streams)
            {
                var filename = stream.Name;
                stream.Dispose();
                File.Delete(filename);
                if (func != null)
                    filename = func(filename);
                filenames.Add(filename);
            }
            Dispose();
            return filenames.ToArray();
        }


		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					foreach (var stream in streams)
                        stream?.Dispose();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~ClusterableFileStream() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion



	}
}
