using CommonExtentionLib.Extentions;
using System.Collections;
using System.Collections.Generic;

namespace ImageManagerLib.SQLite
{
    public class TableFieldInfo
    {
        public string Name { get; private set; }

        public TableFieldType TypeField { get; private set; }

        public bool NotNull { get; private set; }

        public bool Primarykey { get; private set; }

        public bool Unique { get; private set; }

        public TableFieldInfo(string name, TableFieldType type, params TableFieldAttribute[] attributes)
        {
            Name = name;
            TypeField = type;

            foreach (var attribute in attributes)
            {
                switch (attribute)
                {
                    case TableFieldAttribute.NotNull:
                        NotNull = true;
                        break;
                    case TableFieldAttribute.PrimaryKey:
                        Primarykey = true;
                        break;
                    case TableFieldAttribute.Unique:
                        Unique = true;
                        break;
                }
            }
        }

        public string GetField()
        {
            string str = "'{0}' {1} ";
            return str.FormatString(Name, );
        }

        public override string ToString()
        {
            string str = "[\n\tName = {0},\n\tNotNull = {1},\n\tPrimaryKey = {2}\n\tUnique = {3}\n]";
            return str.FormatString();
        }
    }

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

        public IEnumerator<TableFieldInfo> GetEnumerator() => fieldList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => fieldList.GetEnumerator();
    }
}
