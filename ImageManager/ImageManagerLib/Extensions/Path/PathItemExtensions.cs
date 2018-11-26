using FileManagerLib.Path;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.Extensions.Path
{
    /// <summary>
	/// Provides <c>PathItem</c> related extension methods.
    /// </summary>
    public static class PathItemExtensions
    {
        /// <summary>
		/// It decomposes a character string into a file name and parent directory and returns it as a tuple.
        /// </summary>
		/// <param name="fullPath">Target string of full path.</param>
		/// <returns>Tuple of parent directory <c>PathItem</c> and file name string.</returns>
        public static (PathItem parent, string fileName) GetFilenameAndParent(this string fullPath)
        {
            var pathItem = PathSplitter.SplitPath(fullPath);
            var fileName = pathItem.GetLast();
            var parent = new PathItem(pathItem.ToArray(1));

            return (parent, fileName);
        }
    }
}
