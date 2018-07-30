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

        public bool IsRoot
        {
            get;
        }

        public PathItem() { }
        public PathItem(string[] array)
        {
            foreach (var path in array)
            {
                pathList.Add(path);
            }
        }

        public void AddPath(string path)
        {
            pathList.Add(path);
        }

        public string GetPath(int index)
        {
            if (index < 0 || pathList.Count - 1 < index)
                return null;

            string path = pathList[index];
            return path;
        }

        public string Pop()
        {
            if (pathList.Count >= 1)
            {
                var item = pathList[pathList.Count - 1];
                pathList.RemoveAt(pathList.Count - 1);
                return item;
            }
            return null;
        }

        public string GetLast()
        {
            if (pathList.Count >= 1)
                return pathList[pathList.Count - 1];
            return null;
        }

        public PathItem GetPathItemFrom(int start)
        {
            var list = new List<string>();
            for (int i = start; i < pathList.Count; i++)
                list.Add(pathList[i]);
            return new PathItem(list.ToArray());
        }

        public PathItem GetPathItemFrom(string basePath)
        {
            var pathItemArray = new List<string>(PathSplitter.SplitPath(basePath).ToArray());
            var pathArray = pathList.ToArray();
            var list = new List<string>();

            if (pathItemArray.Count < pathArray.Length)
            {
                var cnt = (pathArray.Length - pathItemArray.Count);
                for (int i = 0; i < cnt; i++)
                    pathItemArray.Add("");
            }

            var numbersAndWords = pathArray.Zip(pathItemArray, (n, w) => new { basePath = w, targetPath = n });
            foreach (var nw in numbersAndWords)
            {
                if (!nw.basePath.Equals(nw.targetPath))
                    list.Add(nw.targetPath);
            }

            //var paths = pathArray.Except(pathItemArray).ToArray();

            return new PathItem(list.ToArray());
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is PathItem pathItem)
            {
                return pathList.SequenceEqual(pathItem.pathList);
            }
            return false;
        }

        public string[] ToArray() => pathList.ToArray();

        public string[] ToArray(int end)
        {
            var list = new List<string>(pathList);
            list.RemoveAt(list.Count - end);
            return list.ToArray();
        }

        public override string ToString()
        {
            var array = pathList.ToArray();
            var sb = new StringBuilder();
            sb.Append("/");
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
