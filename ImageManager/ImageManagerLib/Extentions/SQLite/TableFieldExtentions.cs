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
            return string.Empty;
        }

        public static string TypeToString(this TableFieldType type)
        {
            return string.Empty;
        }
    }
}
