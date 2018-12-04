using System;
using System.IO;

namespace FileManagerLib.CommonPath
{
    public static class PathUtils
    {
		/// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ResolvePathSeparator(string path)
        {
            return path.Replace('\\', '/').Replace('/', Path.DirectorySeparatorChar);
        }
    }
}
