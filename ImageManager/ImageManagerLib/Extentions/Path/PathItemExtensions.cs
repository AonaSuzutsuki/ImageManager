using FileManagerLib.Path;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.Extentions.Path
{
    public static class PathItemExtensions
    {
        public static (PathItem parent, string fileName) GetFilenameAndParent(this string fullPath)
        {
            var pathItem = PathSplitter.SplitPath(fullPath);
            var fileName = pathItem.GetLast();
            var parent = new PathItem(pathItem.ToArray(1));

            return (parent, fileName);
        }
    }
}
