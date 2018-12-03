using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.Extensions
{
    public static class StringExtension
    {
        public static string MakeSpace(this string text, int max)
        {
            int count = max - text.Length;
            count = count < 0 ? 1 : count;
            var sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(" ");
            }
            return sb.ToString();
        }
    }
}
