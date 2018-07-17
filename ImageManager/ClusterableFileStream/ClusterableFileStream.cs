using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonExtensionLib.Extensions;

namespace ClusterableFileStream
{
    public class ClusterableFileStream : IDisposable
    {

        #region Fields
        private IList<Stream> streams = new List<Stream>();

		private string baseFilePath;
		private FileMode mode;
		private FileAccess access;
		private FileShare share;
		#endregion

		#region Properties
		public long Position { get; private set; } = 0;

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

		public long SplitSize { get; set; } = 5;
        #endregion

        public ClusterableFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
			if (mode == FileMode.Create || !File.Exists(path))
            {
                streams.Add(new FileStream(path, mode, access, share));
            }
            else
            {
				var root = Path.GetDirectoryName(path);
                var fname = Path.GetFileName(path).Replace(".", @"\.");
                var files = Directory.GetFiles(root);
                foreach (var file in files)
                {
                    if (Regex.IsMatch(file, fname + @"(\.([0-9]{0,3}))?"))
					{
						var stream = new FileStream(file, mode, access, share);
						streams.Add(stream);
						Position += stream.Length;
                    }
                }
            }

			this.baseFilePath = path;
			this.mode = mode;
			this.access = access;
			this.share = share;
        }

        
		public void Write(byte[] data, int offset, long length)
		{
			var startIndex = (int)Math.Floor((double)Position / (double)SplitSize);

			if (startIndex >= streams.Count)
				streams.Add(new FileStream("{0}.{1}".FormatString(baseFilePath, streams.Count), mode, access, share));
			var stream = streams[startIndex];
            
			Func<Stream, int> func = (stm) => {
				var wLen = (int)(SplitSize - stm.Length); // 書き込む長さ
                wLen = wLen > (int)length ? (int)length : wLen;
				return wLen;
			};

			var writeLen = func(stream);
			var remLen = length - writeLen; // 残りの長さ

			stream.Write(data, offset, writeLen);
			Position += writeLen;

			if (remLen > 0)
				Write(data, offset + writeLen, remLen);
		}

		public int Read(byte[] buffer, int offset, int length)
		{
			var prestreams = new List<Stream>();
			var remLens = new List<int>();
			var buffers = new List<byte[]>();
			var startIndex = (int)Math.Floor((double)Position / (double)SplitSize);
			var requres = (int)Math.Ceiling((double)length / (double)SplitSize);

            // 読み込むべきストリームと長さのリストを計算
			var prelen = length;
			for (int i = 0; i < requres; i++)
			{
				var index = startIndex + i;
				if (index < streams.Count)
				{
					prestreams.Add(streams[index]);
                    remLens.Add(prelen > (int)SplitSize ? (int)SplitSize : prelen);
                    prelen -= (int)SplitSize;
				}
			}

			int start = (int)Position + offset;
			int readCount = 0;
			foreach (var item in prestreams.Select((value, index) => new { value, index }))
			{
				var rlen = remLens[item.index];
				var buf = new byte[rlen];
				item.value.Seek(0, SeekOrigin.Begin);
				var prereadCount = item.value.Read(buf, start, buf.Length);
				foreach (var b in buf.Select((value, index) => new { value, index }))
                    buffer[readCount + b.index] = b.value;
				readCount += prereadCount;
			}
            
			return readCount;
		}

		public void Seek(long offset, SeekOrigin seekOrigin)
		{
			if (seekOrigin == SeekOrigin.Begin)
			{
				Position = offset;
			}
			else if (seekOrigin == SeekOrigin.Current)
			{
				Position += offset;
			}
			else
			{
				Position = Length + offset;         
			}
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
