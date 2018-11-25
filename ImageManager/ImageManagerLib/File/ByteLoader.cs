using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.File
{
    /// <summary>
    /// バイト関連の読み込み機能を提供します。
    /// </summary>
    public class ByteLoader
    {
        /// <summary>
        /// ストリームから全てのバイト配列を読み込みます。
        /// </summary>
        /// <param name="stream">読み込む対象のストリーム</param>
        /// <returns>読み込んだバイト配列</returns>
		public static byte[] FromStream(Stream stream)
        {
			if (stream == null)
                return null;

            byte[] data = new byte[stream.Length];
			stream.Read(data, 0, data.Length);

            return data;

        }
    }
}
