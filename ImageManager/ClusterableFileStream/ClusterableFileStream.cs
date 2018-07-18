﻿using System;
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
    public class ClusterableFileStream : IDisposable
    {

        #region Fields
        private List<FileStream> streams = new List<FileStream>();

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

		public long SplitSize { get; set; } = 1073741824;

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

        public ClusterableFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
			if (mode == FileMode.Create || !File.Exists(path))
            {
                streams.Add(new FileStream(path, mode, access, share));
            }
            else
            {
                var absolutePath = CommonCoreLib.Path.PathConverter.ToAbsolutePath(path, () => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
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
                        var stream = new FileStream(file, mode, access, share);
                        sorted.Add(num, stream);
                        Position += stream.Length;
                    }
                }
                streams.AddRange(sorted.Values);
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

            int start = (int)(Position - SplitSize * startIndex);
            stream.Seek(start, SeekOrigin.Begin);
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

			int start = (int)(Position - SplitSize * startIndex) + offset;
			int readCount = 0;
			foreach (var item in prestreams.Select((value, index) => new { value, index }))
			{
				var rlen = remLens[item.index];
				var buf = new byte[rlen];
				item.value.Seek(start, SeekOrigin.Begin);
				var prereadCount = item.value.Read(buf, 0, buf.Length);

                Array.Copy(buf, 0, buffer, readCount, buf.Length);
				//foreach (var b in buf.Select((value, index) => new { value, index }))
    //                buffer[readCount + b.index] = b.value;
				readCount += prereadCount;
                Position += prereadCount;
                start = 0;
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