using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManagerLib.Path
{
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
