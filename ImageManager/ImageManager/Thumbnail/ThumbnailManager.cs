using CommonExtensionLib.Extensions;
using FileManagerLib.Filer.Json;
using ImageManager.ImageLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImageManager.Thumbnail
{
    public class ThumbnailManager : IDisposable
    {

        private readonly JsonResourceManager fileManager;
        private Dictionary<string, BitmapImage> thumbCache = new Dictionary<string, BitmapImage>();

        public ThumbnailManager()
        {
            var thumbPath = "{0}/{1}".FormatString(CommonStyleLib.AppInfo.GetAppPath(), "thumb.dat");

            if (File.Exists(thumbPath))
                fileManager = new JsonResourceManager(thumbPath);
            else
                fileManager = new JsonResourceManager(thumbPath, true);
        }

        public BitmapImage GetThumbnail(string hash, Func<byte[]> func)
        {
            if (thumbCache.ContainsKey(hash))
                return thumbCache[hash];

            byte[] array;
            if (fileManager.ExistFile(hash))
                array = fileManager.GetBytes(hash);

            var data = func();
            array = ImageConverter.GetThumbnailBytes(data, 50, 50);
            fileManager.WriteBytes(hash, array);
            var image = ImageConverter.GetBitmapImage(array);
            thumbCache.Add(hash, image);
            return image;
        }

        public void Dispose()
        {
            fileManager?.Dispose();
        }
    }
}
