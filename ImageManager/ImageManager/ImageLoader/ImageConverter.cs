using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace ImageManager.ImageLoader
{
    public static class ImageConverter
    {
        public static BitmapImage GetBitmapImage(Stream stream)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.DecodePixelWidth = 50;
            image.DecodePixelHeight = 50;
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }

        public static BitmapImage GetBitmapImage(byte[] array)
        {
            using (var stream = new MemoryStream(array))
            {
                return GetBitmapImage(stream);
            }
        }
        
        public static byte[] GetThumbnailBytes(byte[] array, int thumbWidth, int thumbHeight)
        {
            using (MemoryStream ms = new MemoryStream())
            using (var thumbnail = Image.FromStream(new MemoryStream(array)).GetThumbnailImage(thumbWidth, thumbHeight, null, new IntPtr()))
            {
                thumbnail.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}
