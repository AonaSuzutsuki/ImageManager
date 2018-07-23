using System;
using System.IO;
using System.Security.Cryptography;

namespace FileManagerLib.Crypto
{
    public class Sha256
    {
        public Sha256()
        {
        }

		public static string GetSha256(Stream stream, Action<Stream> disposeAction = null)
		{
			var bufferedStream = new BufferedStream(stream, 10485760);
            var sha = new SHA256Managed();
			byte[] checksum = sha.ComputeHash(bufferedStream);
            stream.Seek(0, SeekOrigin.Begin);
			disposeAction?.Invoke(bufferedStream);
            return BitConverter.ToString(checksum).Replace("-", String.Empty);
		}
    }
}
