using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImageManager.ImageLoader
{
    public static class ImageConverter
    {
        public static BitmapImage GetImage(Stream stream)
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
    }
}
