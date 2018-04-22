using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManager.Models
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

        public string[] ToArray() => pathList.ToArray();
    }

    public class PathSplitter
    {
        public static PathItem SplitPath(string pathArg)
        {
            var pathItem = new PathItem();

            pathArg = pathArg.Replace("\\", "/");
            string[] pathArray = pathArg.Split('/');

            foreach (var path in pathArray)
            {
                if (!string.IsNullOrEmpty(path))
                    pathItem.AddPath(path);
            }

            return pathItem;
        }
    }
}
