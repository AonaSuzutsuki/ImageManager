using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FileManagerLib.Crypto
{
    public class Sha256
    {
		public static string GetSha256(Stream stream)
		{
			var bufferedStream = new BufferedStream(stream, 10485760);
            var sha = new SHA256Managed();
			byte[] checksum = sha.ComputeHash(bufferedStream);
            stream.Seek(0, SeekOrigin.Begin);
            return BitConverter.ToString(checksum).Replace("-", String.Empty);
		}

        public static string GetSha256(byte[] bytes)
        {
            var crypto256 = new SHA256CryptoServiceProvider();
            byte[] hash256Value = crypto256.ComputeHash(bytes);

            return BitConverter.ToString(hash256Value).Replace("-", String.Empty);
        }
    }
}
