using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clusterable.IO
{
    public class ClusterableMemoryStream : IClusterableStream
    {

        #region Fields
        private MemoryStream stream;
        #endregion

        #region Properties
        public string[] Filenames => throw new NotImplementedException();

        public long Length => throw new NotImplementedException();

        public long Position => throw new NotImplementedException();

        public long SplitSize => throw new NotImplementedException();
        #endregion

        public ClusterableMemoryStream()
        {
            stream = new MemoryStream();
        }

        public string[] Delete(Func<string, string> func = null)
        {
            Dispose();
            return Filenames;
        }

        public int Read(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public void Seek(long offset, SeekOrigin seekOrigin)
        {
            throw new NotImplementedException();
        }

        public void Write(byte[] data, int offset, long length)
        {
            throw new NotImplementedException();
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    stream.Dispose();
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~ClusterableMemoryStream() {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
