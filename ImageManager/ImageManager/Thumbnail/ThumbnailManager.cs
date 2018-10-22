using CommonExtensionLib.Extensions;
using FileManagerLib.File.Json;
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

        public const string ThumbnailChachePath = "thumb.dat";

        public ThumbnailManager()
        {
            var thumbPath = "{0}/{1}".FormatString(CommonStyleLib.AppInfo.GetAppPath(), ThumbnailChachePath);

            if (File.Exists(thumbPath))
                fileManager = new JsonFileManager(thumbPath);
            else
                fileManager = new JsonFileManager(thumbPath, true);
        }

        public BitmapImage GetThumbnail(string hash, Func<byte[]> func)
        {
            if (thumbCache.ContainsKey(hash))
                return thumbCache[hash];

            byte[] array;
            if (fileManager.ExistResource(hash))
                array = fileManager.GetBytes(hash);
            else
                array = ImageConverter.GetThumbnailBytes(func(), 50, 50);

            fileManager.WriteBytesWithoutException(hash, array);
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
