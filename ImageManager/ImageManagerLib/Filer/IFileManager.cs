using FileManagerLib.Path;

namespace FileManagerLib.Filer
{
    public interface IFileManager
    {
        void CreateDirectory(string fullPath);
        void CreateImage(string fileName, string parent, byte[] data, string mimeType);
        void CreateImage(string fileName, string parent, string inFilePath);
        void CreateImage(string fullPath, string inFilePath);
        void CreateImages(string parent, string dirPath);
        void CreateTable();
        void DataVacuum();
        void DeleteDirectory(string fullPath);
        void DeleteDirectory(int id);
        void DeleteFile(string fullPath);
        void DeleteFile(int id);
        void Dispose();
        DataFileInfo[] GetDirectories(int did = 0);
        DataFileInfo[] GetDirectories(DataFileInfo dataFileInfo);
        int GetDirectoryId(string dirName, int rootId);
        int GetDirectoryId(PathItem pathItem);
        DataFileInfo[] GetFiles(int did = 0);
        DataFileInfo[] GetFiles(DataFileInfo dataFileInfo);
        string ToString();
        string TraceDirs();
        string TraceFiles();
        void WriteToFile(string filePath, string outFilePath);
        void WriteToDir(string filePath, string outFilePath);
        void WriteToFile(int id, string outFilePath);
        void WriteToDir(int id, string outFilePath);
        void WriteToFile(DataFileInfo[] values, string outFilePath);
    }
}