using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.Extentions.Imager
{
    public static class ImageExtention
    {
        public static Image ByteArrayToImage(this byte[] b)
        {
            var imgconv = new ImageConverter();
            var img = (Image)imgconv.ConvertFrom(b);
            return img;
        }

        public static byte[] ImageToByteArray(this Image img)
        {
            var imgconv = new ImageConverter();
            var b = (byte[])imgconv.ConvertTo(img, typeof(byte[]));
            return b;
        }
    }
}
