using CommonExtensionLib.Extensions;
using FileManagerLib.Extentions.SQLite;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FileManagerLib.SQLite
{
    public class TableFieldList : IEnumerable<TableFieldInfo>
    {
        private List<TableFieldInfo> fieldList = new List<TableFieldInfo>();
        public TableFieldInfo this[int index]
        {
            get => fieldList[index];
            set => fieldList[index] = value;
        }

        public void Add(TableFieldInfo fieldInfo)
        {
            fieldList.Add(fieldInfo);
        }

        public override string ToString()
        {
            var primaryKeys = new List<string>();
            var sb = new StringBuilder();
            for (int i = 0; i < fieldList.Count; i++)
            {
                var field = fieldList[i];
                if (i >= fieldList.Count - 1)
                {
                    sb.AppendFormat("{0}", field.GetField());
                    if (field.Primarykey)
                        primaryKeys.Add(field.Name);
                }
                else
                {
                    sb.AppendFormat("{0}, ", field.GetField());
                    if (field.Primarykey)
                        primaryKeys.Add(field.Name);
                }
            }

            if (primaryKeys.Count > 0)
            {
                var primaryKeysSb = new StringBuilder();
                for (int i = 0; i < primaryKeys.Count; i++)
                {
                    var primaryKey = primaryKeys[i];
                    if (i >= primaryKeys.Count - 1)
                    {
                        primaryKeysSb.AppendFormat("'{0}'", primaryKey);
                    }
                    else
                    {
                        primaryKeysSb.AppendFormat("'{0}', ", primaryKey);
                    }
                }
                sb.AppendFormat(", primary key({0})", primaryKeysSb.ToString());
            }
            return sb.ToString();
        }

        public IEnumerator<TableFieldInfo> GetEnumerator() => fieldList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => fieldList.GetEnumerator();
    }
}
