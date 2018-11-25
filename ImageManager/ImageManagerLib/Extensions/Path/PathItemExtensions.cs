using FileManagerLib.Path;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.Extensions.Path
{
    /// <summary>
    /// PathItemの拡張メソッドを提供します。
    /// </summary>
    public static class PathItemExtensions
    {
        /// <summary>
        /// 文字列をファイル名と親ディレクトリに分解してタプルで返します。
        /// </summary>
        /// <param name="fullPath">対象のパス文字列</param>
        /// <returns>親ディレクトリのPathItemとファイル名文字列のタプル</returns>
        public static (PathItem parent, string fileName) GetFilenameAndParent(this string fullPath)
        {
            var pathItem = PathSplitter.SplitPath(fullPath);
            var fileName = pathItem.GetLast();
            var parent = new PathItem(pathItem.ToArray(1));

            return (parent, fileName);
        }
    }
}
