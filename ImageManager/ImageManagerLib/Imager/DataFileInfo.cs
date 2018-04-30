using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManagerLib.Imager
{
    public class DataFileInfo
    {
        public int Id { get; }

        public int Parent { get; }

        public string Filename { get; }

        public DataFileType Type { get; }

        public byte[] Image { get; set; }

        public byte[] Thumbnail { get; set; }

        public DataFileInfo(int id, int parent, string filename, DataFileType type)
        {
            Id = id;
            Parent = parent;
            Filename = filename;
            Type = type;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            else if (base.Equals(obj))
            {
                return true;
            }
            else if (obj is DataFileInfo dataFileInfo)
            {
                return dataFileInfo.Id == Id
                    && dataFileInfo.Parent == Parent
                    && Filename.Equals(dataFileInfo.Filename)
                    && dataFileInfo.Type == Type;
                //&& dataFileInfo.Image.SequenceEqual(Image)
                //&& dataFileInfo.Thumbnail.SequenceEqual(Thumbnail);
            }

            return false;
        }
    }
}
