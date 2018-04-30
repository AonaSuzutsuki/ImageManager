using ImageManagerLib.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManagerLib.Extentions.SQLite
{
    public static class TableFieldExtentions
    {
        public static string AttributeToString(this TableFieldAttribute attr)
        {
            string res = string.Empty;
            switch (attr)
            {
                case TableFieldAttribute.NotNull:
                    res = "not null";
                    break;
                case TableFieldAttribute.PrimaryKey:
                    break;
                case TableFieldAttribute.Unique:
                    res = "unique";
                    break;
            }
            return res;
        }

        public static string TypeToString(this TableFieldType type)
        {
            string res = string.Empty;
            switch (type)
            {
                case TableFieldType.Integer:
                    res = "integer";
                    break;
                case TableFieldType.Text:
                    res = "text";
                    break;
                case TableFieldType.Real:
                    res = "real";
                    break;
                case TableFieldType.Blob:
                    res = "blob";
                    break;
            }
            return res;
        }
    }
}
