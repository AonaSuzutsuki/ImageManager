namespace ImageManagerLib.SQLite
{
    public interface IDatabase : IVacuumable
    {
        string Filename { get; }

        void CreateTable(string name, string arg);
        void CreateTable(string name, TableFieldList fields);
        void DeleteTable(string name);
        void Dispose();
        string[][] GetValues(string tableName, string term = null);
        void InsertValue(string tableName, params string[] values);
        void DeleteValue(string tableName, string term);
        bool TableExist(string tablename);
        void Update(string tableName, string fiels, string value, string term);
    }
}