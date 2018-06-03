using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.Path
{
    public class PathItem
    {
        private List<string> pathList = new List<string>();

        public void AddPath(string path)
        {
            pathList.Add(path);
        }

        public string GetPath(int index)
        {
            if (index < 0)
                return string.Empty;

            string path = pathList[index];
            return path;
        }

        public string GetLast() => pathList[pathList.Count - 1];

        public string[] ToArray() => pathList.ToArray();

        public override string ToString()
        {
            var array = pathList.ToArray();
            var sb = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                if (i < array.Length - 1)
                    sb.AppendFormat("{0}/", array[i]);
                else
                    sb.AppendFormat("{0}", array[i]);
            }
            return sb.ToString();
        }
    }
}
