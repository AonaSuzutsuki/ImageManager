using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FileManagerLib.Crypto
{
    public class Sha256
    {
        /// <summary>
        /// StreamからSHA256を計算します。
        /// </summary>
        /// <param name="stream">計算する対象のStream</param>
        /// <returns>変換されたSHA256の文字列</returns>
		public static string GetSha256(Stream stream)
		{
			var bufferedStream = new BufferedStream(stream, 10485760);
            var sha = new SHA256Managed();
			byte[] checksum = sha.ComputeHash(bufferedStream);
            stream.Seek(0, SeekOrigin.Begin);
            return BitConverter.ToString(checksum).Replace("-", String.Empty);
		}

        /// <summary>
        /// バイト配列からSHA256を計算します。
        /// </summary>
        /// <param name="bytes">計算する対象のバイト配列</param>
        /// <returns>変換されたSHA256の文字列</returns>
        public static string GetSha256(byte[] bytes)
        {
            var crypto256 = new SHA256CryptoServiceProvider();
            byte[] hash256Value = crypto256.ComputeHash(bytes);

            return BitConverter.ToString(hash256Value).Replace("-", String.Empty);
        }
    }
}
