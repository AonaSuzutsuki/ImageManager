﻿using CommonLib.Extentions;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManager.Models
{
    public class SQLiteWrapper : IDisposable
    {
        private SQLiteConnection connection;

        public string Filename { get; }

        public SQLiteWrapper(string filename)
        {
            Filename = filename;
            Open();
        }

        #region Public Methods
        public void Vacuum()
        {
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "VACUUM;";
                command.ExecuteNonQuery();
            }
        }

        public void CreateTable(string name, string arg)
        {
            string cmd = "CREATE TABLE {0}({1});";
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = cmd.FormatString(name, arg);
                command.ExecuteNonQuery();
            }
        }
        public void DeleteTable(string name)
        {
            string cmd = "DROP TABLE {0};";
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = cmd.FormatString(name);
                command.ExecuteNonQuery();
            }
        }
        public bool TableExist(string tablename)
        {
            string cmd = "SELECT name FROM sqlite_master WHERE type='table' AND name='{0}';";
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = cmd.FormatString(tablename);
                command.ExecuteNonQuery();
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    return reader.Read();
                }
            }
        }

        public string[][] GetValues(string tableName, string term = null)
        {
            string cmd;
            if (!string.IsNullOrEmpty(term))
                cmd = "select * from {0} where {1};".FormatString(tableName, term);
            else
                cmd = "select * from {0};".FormatString(tableName);

            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = cmd;
                using (SQLiteDataReader sdr = command.ExecuteReader())
                {
                    List<string[]> tuples = new List<string[]>();
                    for (int i = 0; sdr.Read(); i++)
                    {
                        string[] column = new string[sdr.FieldCount];
                        for (int j = 0; j < sdr.FieldCount; j++)
                        {
                            column[j] = sdr[j].ToString();
                        }
                        tuples.Add(column);
                    }

                    return tuples.ToArray();
                }
            }
        }

        public void InsertValue(string tableName, params string[] values)
        {
            //INSERT INTO テーブル名 VALUES(値1, 値2, ...);
            string cmd = "insert into {0} values({1});".FormatString(tableName, ArrayToString(values));

            using (SQLiteTransaction sqlt = connection.BeginTransaction())
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = cmd;
                    command.ExecuteNonQuery();
                }
                sqlt.Commit();
            }
        }
        #endregion

        #region Public Static Functions
        public static string ArrayToString(string[] array)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                if (i < array.Length - 1)
                    sb.AppendFormat("'{0}', ", array[i]);
                else
                    sb.AppendFormat("'{0}'", array[i]);
            }
            return sb.ToString();
        }
        #endregion

        #region Private Methods
        private void Open()
        {
            connection = new SQLiteConnection("Data Source=" + Filename);
            connection.Open();
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    connection.Dispose();
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~SQLiteWrapper() {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
