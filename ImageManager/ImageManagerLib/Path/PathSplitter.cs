using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.Path
{
    /// <summary>
    /// パスの分割系機能を提供します。
    /// </summary>
    public class PathSplitter
    {
        /// <summary>
        /// パス文字列を分割し、PathItemで返します。
        /// </summary>
        /// <param name="pathArg">対象のパス文字列</param>
        /// <returns>変換されたPathItem</returns>
        public static PathItem SplitPath(string pathArg)
        {
            var pathItem = new PathItem();

            if (pathArg.Equals("/"))
            {
                pathItem.AddPath("");
            }
            else
            {
                pathArg = pathArg.Replace("\\", "/");
                string[] pathArray = pathArg.Split('/');

                foreach (var path in pathArray)
                {
                    if (!string.IsNullOrEmpty(path))
                        pathItem.AddPath(path);
                }
            }

            return pathItem;
        }
    }
}
