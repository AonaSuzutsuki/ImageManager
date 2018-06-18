using CommonExtentionLib.Extentions;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace FileManagerLib.SQLite
{
    public class SQLiteWrapper : IDisposable, IDatabase
    {
        private SQLiteConnection connection;
        private SQLiteTransaction sqlt;
        private SQLiteCommand command;

        public string Filename { get; }

        public SQLiteWrapper(string filename)
        {
            Filename = filename;
            Open();
        }

        #region Public Methods
        public void Vacuum()
        {
            var cmd = "vacuum;";
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = cmd;
                command.ExecuteNonQuery();
            }
        }

        public void CreateTable(string name, string arg)
        {
            string cmd = "create table {0}({1});".FormatString(name, arg);
            DoCommand(cmd);
        }
        public void CreateTable(string name, TableFieldList fields)
        {
            string arg = fields.ToString();
            CreateTable(name, arg);
        }

        public void DeleteTable(string name)
        {
            string cmd = "drop table {0};".FormatString(name);
            DoCommand(cmd);
        }
        public bool TableExist(string tablename)
        {
            string cmd = "SELECT name FROM sqlite_master WHERE type='table' AND name='{0}';".FormatString(tablename);
            var result = DoTransaction(cmd, (command) =>
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    return reader.Read();
                }
            });
            return result;
        }

        public string[][] GetValues(string tableName, string term = null)
        {
            string cmd;
            if (!string.IsNullOrEmpty(term))
                cmd = "select * from {0} where {1};".FormatString(tableName, term);
            else
                cmd = "select * from {0};".FormatString(tableName);

            var result = DoTransaction<string[][]>(cmd, (command) =>
            {
                var tuples = new List<string[]>();
                try
                {
                    using (SQLiteDataReader sdr = command.ExecuteReader())
                    {
                        for (int i = 0; sdr.Read(); i++)
                        {
                            string[] column = new string[sdr.FieldCount];
                            for (int j = 0; j < sdr.FieldCount; j++)
                            {
                                column[j] = sdr[j].ToString();
                            }
                            tuples.Add(column);
                        }
                    }
                }
                catch (SQLiteException) { };
                return tuples.ToArray();
            });
            return result;
        }

        public void InsertValue(string tableName, params string[] values)
        {
            //INSERT INTO テーブル名 VALUES(値1, 値2, ...);
            string cmd = "insert into {0} values({1});".FormatString(tableName, ArrayToString(values));
            DoCommand(cmd);
        }

        public void InsertValue(string tableName, params object[] values)
        {
            var datObj = (byte[])values[3];
            var cmd = "{0}, {1}, '{2}', @0, '{4}'".FormatString(values[0], values[1], values[2], datObj, values[4]);

            var act = new Action<string, byte[]>((arg, data) => {
                using (var command = new SQLiteCommand(connection))
                {
                    command.Transaction = sqlt;
                    command.CommandText = "INSERT INTO {0} VALUES ({1});".FormatString(tableName, arg);
                    var param = new SQLiteParameter("@0", System.Data.DbType.Binary)
                    {
                        Value = data
                    };
                    command.Parameters.Add(param);
                    command.ExecuteNonQuery();
                }
            });

            if (sqlt == null)
            {
                StartTransaction();
                act(cmd, datObj);
                DoCommit();
                EndTransaction();
            }
            else
            {
                act(cmd, datObj);
            }
        }

        public void DeleteValue(string tableName, string term)
        {
            string cmd = "delete from {0} where {1};".FormatString(tableName, term);
            DoCommand(cmd);
        }

        public void Update(string tableName, string fiels, string value, string term)
        {
            var cmd = "update {0} set {1} = '{2}' where {3};".FormatString(tableName, fiels, value, term);
            DoCommand(cmd);
        }


        public void StartTransaction()
        {
            sqlt = connection.BeginTransaction();
        }
        public void EndTransaction()
        {
            sqlt.Dispose();
            sqlt = null;
        }
        public void DoCommit()
        {
            sqlt.Commit();
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
        private void DoCommand(string cmd)
        {
            //using (SQLiteTransaction sqlt = connection.BeginTransaction())
            //{
            var act = new Action<string>(arg => {
                using (var command = new SQLiteCommand(connection))
                {
                    command.Transaction = sqlt;
                    command.CommandText = cmd;
                    command.ExecuteNonQuery();
                }
            });

            if (sqlt == null)
            {
                StartTransaction();
                act(cmd);
                DoCommit();
                EndTransaction();
            }
            else
            {
                act(cmd);
            }
            //}
        }

        private T DoTransaction<T>(string cmd, Func<SQLiteCommand, T> func)
        {
            //using (SQLiteTransaction sqlt = connection.BeginTransaction())
            //{
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = cmd;
                return func(command);
            }
            //}
        }

        private void Open()
        {
            var builder = new SQLiteConnectionStringBuilder()
            {
                DataSource = Filename,
                Version = 3,
                LegacyFormat = false,
                PageSize = 8192,
                SyncMode = SynchronizationModes.Normal,
                JournalMode = SQLiteJournalModeEnum.Wal,
            };

            connection = new SQLiteConnection(builder.ToString()); //"Data Source=" + Filename
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
