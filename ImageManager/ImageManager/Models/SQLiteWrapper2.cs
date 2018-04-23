using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace ImageManager.Models
{
    public class SQLiteWrapper2
    {
        public string Filename
        {
            get;
        }


        public SQLiteWrapper2(string path)
        {
            Filename = path;
        }



        public void VACUUM()
        {
            using (var conn = new SQLiteConnection("Data Source=" + Filename))
            {
                conn.Open();
                using (SQLiteCommand command = conn.CreateCommand())
                {
                    command.CommandText = "VACUUM";
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }
        public void Command(string cmd)
        {
            using (var conn = new SQLiteConnection("Data Source=" + Filename))
            {
                conn.Open();
                using (SQLiteCommand command = conn.CreateCommand())
                {
                    command.CommandText = cmd;
                    command.ExecuteNonQuery();

                }
                conn.Close();
            }
        }
        public void CreateFile(string filename)
        {
            SQLiteConnection.CreateFile(filename);
        }

        public void CreateTable(string name, string arg)
        {
            using (var conn = new SQLiteConnection("Data Source=" + Filename))
            {
                conn.Open();
                using (SQLiteCommand command = conn.CreateCommand())
                {
                    command.CommandText = "CREATE TABLE " + name + "(" + arg + ")";
                    command.ExecuteNonQuery();

                }
                conn.Close();
            }
        }

        public bool TableExist(string tablename)
        {
            using (var conn = new SQLiteConnection("Data Source=" + Filename))
            {
                conn.Open();
                using (SQLiteCommand command = conn.CreateCommand())
                {
                    command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='" + tablename + "'";
                    command.ExecuteNonQuery();
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //reader[0].ToString();
                            return true;
                        }
                    }
                }
                conn.Close();

                return false;
            }
        }

        public void DeleteTable(string name)
        {
            using (var conn = new SQLiteConnection("Data Source=" + Filename))
            {
                conn.Open();
                using (SQLiteCommand command = conn.CreateCommand())
                {
                    command.CommandText = "DROP TABLE " + name;
                    command.ExecuteNonQuery();
                    command.CommandText = "VACUUM";
                    command.ExecuteNonQuery();

                }
                conn.Close();
            }
        }

        public void DeleteData(string name, string ifstr)
        {
            using (var conn = new SQLiteConnection("Data Source=" + Filename))
            {
                conn.Open();
                using (SQLiteCommand command = conn.CreateCommand())
                {
                    command.CommandText = "DELETE FROM " + name + " WHERE " + ifstr;
                    command.ExecuteNonQuery();
                    command.CommandText = "VACUUM";
                    command.ExecuteNonQuery();

                }
                conn.Close();
            }
        }

        public string Exists(string name)
        {
            string count = default(string);
            try
            {
                using (SQLiteConnection cn = new SQLiteConnection("Data Source=" + Filename))
                {
                    cn.Open();
                    SQLiteCommand cmd = cn.CreateCommand();
                    cmd.CommandText = "select count(*) from " + name;
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            count = reader.VisibleFieldCount.ToString();
                        }
                    }
                    cn.Close();
                }
            }
            catch
            {
                count = "0";
            }
            return count;
        }
        public int FieldLenght(string table, string field)
        {
            List<string> datas = new List<string>();
            using (SQLiteConnection cn = new SQLiteConnection("Data Source=" + Filename))
            {
                cn.Open();
                SQLiteCommand cmd = cn.CreateCommand();
                cmd.CommandText = "select count('" + field + "') from '"+ table + "'";
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        datas.Add(reader[0].ToString());
                    }
                    return int.Parse(datas[0]);
                }
            }
        }
        public int FieldLenght(string table, string field, string terms)
        {
            //cmd.CommandText = "SELECT * FROM " + table + " where " + terms;
            List<string> datas = new List<string>();
            using (SQLiteConnection cn = new SQLiteConnection("Data Source=" + Filename))
            {
                cn.Open();
                SQLiteCommand cmd = cn.CreateCommand();
                cmd.CommandText = "select count('" + field + "') from '" + table + "'" +" where " + terms;
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        datas.Add(reader[0].ToString());
                    }
                    return int.Parse(datas[0]);
                }
            }
        }
        public bool Exist(string table, string field, string terms)
        {
            List<string> datas = new List<string>();

            using (SQLiteConnection cn = new SQLiteConnection("Data Source=" + Filename))
            {
                cn.Open();
                SQLiteCommand cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT * FROM " + table + " where " + terms;
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        datas.Add(reader[field].ToString());
                    }
                }
                cn.Close();

                if (datas.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// テーブルから値を取得します。
        /// </summary>
        /// <param name="table">テーブル名</param>
        /// <param name="field">フィールド名</param>
        /// <returns></returns>
        public List<string> getString(string table, string field)
        {
            List<string> datas = new List<string>();

            using (SQLiteConnection cn = new SQLiteConnection("Data Source=" + Filename))
            {
                cn.Open();
                SQLiteCommand cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT * FROM " + table;
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        datas.Add(reader[field].ToString());
                    }
                }
                cn.Close();
            }

            return datas;
        }

        /// <summary>
        /// テーブルから値を取得します。
        /// </summary>
        /// <param name="table">テーブル名</param>
        /// <param name="field">フィールド名</param>
        /// <param name="terms">条件式</param>
        /// <returns></returns>
        public List<string> getString(string table, string field, string terms)
        {
            List<string> datas = new List<string>();

            using (SQLiteConnection cn = new SQLiteConnection("Data Source=" + Filename))
            {
                cn.Open();
                SQLiteCommand cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT * FROM " + table + " where " + terms;
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        datas.Add(reader[field].ToString());
                    }
                }
                cn.Close();
            }

            return datas;
        }

        /// <summary>
        /// テーブルから値を取得します。
        /// </summary>
        /// <param name="table">テーブル名</param>
        /// <param name="field">フィールド名</param>
        /// <returns></returns>
        public List<int> getInt(string table, string field)
        {
            List<int> datas = new List<int>();

            using (SQLiteConnection cn = new SQLiteConnection("Data Source=" + Filename))
            {
                cn.Open();
                SQLiteCommand cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT * FROM " + table;
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        datas.Add(int.Parse(reader[field].ToString()));
                    }
                }
                cn.Close();
            }

            return datas;
        }

        /// <summary>
        /// テーブルから値を取得します。
        /// </summary>
        /// <param name="table">テーブル名</param>
        /// <param name="field">フィールド名</param>
        /// <param name="terms">条件式</param>
        /// <returns></returns>
        public List<int> getInt(string table, string field, string terms)
        {
            List<int> datas = new List<int>();

            using (SQLiteConnection cn = new SQLiteConnection("Data Source=" + Filename))
            {
                cn.Open();
                SQLiteCommand cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT * FROM " + table + " where " + terms;
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        datas.Add(int.Parse(reader[field].ToString()));
                    }
                }
                cn.Close();
            }

            return datas;
        }

        public void InsertString(string table, string field, List<string> arg)
        {
            using (var conn = new SQLiteConnection("Data Source=" + Filename))
            {
                conn.Open();
                using (SQLiteTransaction sqlt = conn.BeginTransaction())
                {
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        foreach (string m in arg)
                        {
                            command.CommandText = "insert into " + table + " (" + field + ") values('" + m + "')";
                            command.ExecuteNonQuery();
                        }
                    }
                    sqlt.Commit();
                }
                conn.Close();
            }
        }

        public void InsertString(string table, string field, string arg)
        {
            using (var conn = new SQLiteConnection("Data Source=" + Filename))
            {
                conn.Open();
                using (SQLiteTransaction sqlt = conn.BeginTransaction())
                {
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        command.CommandText = "insert into " + table + " (" + field + ") values('" + arg + "')";
                        command.ExecuteNonQuery();
                    }
                    sqlt.Commit();
                }
                conn.Close();
            }
        }

        public void InsertInteger(string table, string field, List<int> arg)
        {
            using (var conn = new SQLiteConnection("Data Source=" + Filename))
            {
                conn.Open();
                using (SQLiteTransaction sqlt = conn.BeginTransaction())
                {
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        foreach (int m in arg)
                        {
                            command.CommandText = "insert into " + table + " (" + field + ") values('" + m.ToString() + "')";
                            command.ExecuteNonQuery();
                        }
                    }
                    sqlt.Commit();
                }
                conn.Close();
            }
        }

        public void InsertInteger(string table, string field, int arg)
        {
            using (var conn = new SQLiteConnection("Data Source=" + Filename))
            {
                conn.Open();
                using (SQLiteTransaction sqlt = conn.BeginTransaction())
                {
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        command.CommandText = "insert into " + table + " (" + field + ") values('" + arg.ToString() + "')";
                        command.ExecuteNonQuery();
                    }
                    sqlt.Commit();
                }
                conn.Close();
            }
        }


        public void Update(string table, string field, string arg, string terms)
        {
            using (var conn = new SQLiteConnection("Data Source=" + Filename))
            {
                conn.Open();
                using (SQLiteTransaction sqlt = conn.BeginTransaction())
                {
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        //command.CommandText = "update " + table + " set " + field + @" = '" + arg + @"' where " + terms + "";
                        command.CommandText = "update " + table + " set " + field + @" = @arg where " + terms + "";
                        command.Parameters.Add(new SQLiteParameter("@arg", arg));
                        //command.CommandText = @"update blog set BODY = '<font color=""#ff0000"">test</font>' where id = '10'";
                        command.ExecuteNonQuery();
                    }
                    sqlt.Commit();
                }
                conn.Close();
            }
        }
        public void Update(string table, string field, string arg)
        {
            using (var conn = new SQLiteConnection("Data Source=" + Filename))
            {
                conn.Open();
                using (SQLiteTransaction sqlt = conn.BeginTransaction())
                {
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        //command.CommandText = "update " + table + " set " + field + @" = '" + arg + @"' where " + terms + "";
                        command.CommandText = "update " + table + " set " + field + @" = @arg";
                        command.Parameters.Add(new SQLiteParameter("@arg", arg));
                        //command.CommandText = @"update blog set BODY = '<font color=""#ff0000"">test</font>' where id = '10'";
                        command.ExecuteNonQuery();
                    }
                    sqlt.Commit();
                }
                conn.Close();
            }
        }
    }
}