using System;
using System.IO;

namespace Clusterable.IO
{
    public interface IClusterableStream : IDisposable
    {
        string[] Filenames { get; }
        long Length { get; }
        long Position { get; }
        long SplitSize { get; }

        string[] Delete(Func<string, string> func = null);
        int Read(byte[] buffer, int offset, int length);
        void Seek(long offset, SeekOrigin seekOrigin);
        void Write(byte[] data, int offset, long length);
    }
}