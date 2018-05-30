using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManagerLib.Imager
{
    public enum TextType
    {
        Base64,
    }

    public class ImageInfo
    {
        public ImageInfo(byte[] data, string base64)
        {
            Data = data;
            Base64 = base64;
        }

        public byte[] Data
        {
            get;
            private set;
        }

        public string Base64
        {
            get;
            private set;
        }
    }

    public class ImageLoader
    {

        #region PrivateMethods
        public static ImageInfo FromImageFile(string filename)
        {
            if (!File.Exists(filename))
                return null;

            byte[] data;
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();
            }

            var imageInfo = new ImageInfo(data, Convert.ToBase64String(data));
            return imageInfo;

        }
        #endregion
    }
}
