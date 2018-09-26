using System;
using System.Collections.Generic;
using System.IO;

namespace FileManagerLib.File
{
    public static class DirectorySearcher
    {
		/// <summary>
        /// Gets all nested directories.
        /// </summary>
        /// <returns>The all directories.</returns>
        /// <param name="path">Directory path.</param>
		public static string[] GetAllDirectories(string path)
		{
			var topDirArray = Directory.GetDirectories(path);
			var dList = new List<string>(topDirArray);
			foreach (var subDirPath in topDirArray)
			{
				dList.AddRange(GetAllDirectories(subDirPath));
			}
			return dList.ToArray();
		}

        /// <summary>
		/// Gets all nested files.
        /// </summary>
        /// <returns>The all files.</returns>
		/// <param name="path">Directory path.</param>
		public static string[] GetAllFiles(string path)
		{
			var fList = new List<string>(Directory.GetFiles(path));
			var dirs = GetAllDirectories(path);
			foreach (var dir in dirs)
			{
				fList.AddRange(Directory.GetFiles(dir));
			}
			return fList.ToArray();
		}
    }
}
