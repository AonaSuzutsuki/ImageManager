using CommonExtensionLib.Extensions;
using FileManagerLib.Extentions.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.SQLite
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
            var attr = new StringBuilder();
            if (NotNull)
                attr.Append("not null ");
            if (Unique)
                attr.Append("unique ");

            string str = "'{0}' {1} {2}".FormatString(Name, TypeField.TypeToString(), attr.ToString());
            return str;
        }

        public override string ToString()
        {
            string str = "[\n\tName = {0},\n\tNotNull = {1},\n\tPrimaryKey = {2}\n\tUnique = {3}\n]";
            return str.FormatString();
        }
    }
}
