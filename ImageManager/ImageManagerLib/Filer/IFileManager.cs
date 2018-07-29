using FileManagerLib.Filer.Json;
using FileManagerLib.Path;
using System;

namespace FileManagerLib.Filer
{
    public interface IFileManager : IDisposable
    {
        /// <summary>
        /// Create the directory.
        /// </summary>
        /// <param name="fullPath">Fullpath of directory. ex:/dir/subdir</param>
        void CreateDirectory(string fullPath);
        void CreateFile(string fileName, string parent, byte[] data, string mimeType, string hash);
        void CreateFile(string fileName, string parent, string inFilePath);
        void CreateFile(string fullPath, string inFilePath);
        void CreateFiles(string parent, string dirPath);
        void DataVacuum();
        void DeleteDirectory(string fullPath);
        void DeleteDirectory(int id);
        void DeleteFile(string fullPath);
        void DeleteFile(int id);
        int GetDirectoryId(string dirName, int rootId);
        int GetDirectoryId(PathItem pathItem);
		FileStructure[] GetFiles(int dirId);
		DirectoryStructure[] GetDirectories(int dirId);
        string ToString();
        string TraceDirs();
        string TraceFiles();
		bool WriteToFile(string filePath, string outFilePath);
		void WriteToDir(string filePath, string outFilePath);
        bool WriteToFile(int id, string outFilePath);
		void WriteToDir(int id, string outFilePath);
    }
}