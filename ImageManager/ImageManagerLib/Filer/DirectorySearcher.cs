using System;
using System.Collections.Generic;
using System.IO;

namespace FileManagerLib.Filer
{
    public static class DirectorySearcher
    {
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
