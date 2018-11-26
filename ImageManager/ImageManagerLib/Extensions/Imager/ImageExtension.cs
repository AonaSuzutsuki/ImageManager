using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.Extensions.Imager
{
	/// <summary>
	/// Provides <c>Image</c> related extension methods.
    /// </summary>
	public static class ImageExtension
    {
		/// <summary>
        /// Bytes the array to <c>Image</c>.
        /// </summary>
        /// <returns>Converted <c>Image</c>.</returns>
        /// <param name="b">Target byte array.</param>
        public static Image ByteArrayToImage(this byte[] b)
        {
            var imgconv = new ImageConverter();
            var img = (Image)imgconv.ConvertFrom(b);
            return img;
        }

        /// <summary>
        /// <c>Image</c> to byte array.
        /// </summary>
        /// <returns>The to byte array.</returns>
        /// <param name="img">Target <c>Image</c>.</param>
        public static byte[] ImageToByteArray(this Image img)
        {
            var imgconv = new ImageConverter();
            var b = (byte[])imgconv.ConvertTo(img, typeof(byte[]));
            return b;
        }
    }
}
