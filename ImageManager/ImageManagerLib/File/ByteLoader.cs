using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.File
{
    /// <summary>
	/// Provides byte related reading function.
    /// </summary>
    public class ByteLoader
    {
        /// <summary>
		/// Read all byte array of <c>Stream</c>.
        /// </summary>
        /// <param name="stream">Target <c>Stream</c>.</param>
        /// <returns>Read byte array.</returns>
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
