using CommonExtentionLib.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImageManagerCUI.Parser
{
    public class CmdParser
    {
        /// <summary>
        /// Command
        /// </summary>
        public string Command { get; private set; }

        private Dictionary<string, string> attributes = new Dictionary<string, string>();
        private List<string> listAttributes = new List<string>();

        public CmdParser(string cmd)
        {
            var parts = SplitWithoutDoubleQuote(cmd);

            Command = parts[0];
            for (int i = 1; i < parts.Count; i++)
            {
                var name = parts[i];
                if (name[0].Equals('-'))
                {
                    name = name.Remove(0, 1);
                    attributes.Add(name, parts[i + 1]);
                    i++;
                }
                else
                {
                    listAttributes.Add(name);
                }
            }
        }
        
        public string GetAttribute(string attrName)
        {
            if (attributes.ContainsKey(attrName))
                return attributes[attrName];
            return null;
        }

        public string GetAttribute(int index)
        {
            if (index >= 0 && index < listAttributes.Count)
                return listAttributes[index];
            return null;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[\n  Command:\t{0}\n", Command);
            foreach (var attribute in attributes)
            {
                sb.AppendFormat("  Attribute:\t{0}:{1}\n", attribute.Key, attribute.Value);
            }
            sb.Append("]\n");

            return sb.ToString();
        }
        
        #region Static Function
        public static List<string> SplitWithoutDoubleQuote(string text)
        {
            var table = new List<string>();

            RegexDoMached(text, "(?<quote>\"+([^\"]+)\"+)", m =>
            {
                var val = m.Groups["quote"].Value;
                table.Add(val);
                text = text.Replace(val, "[{0}]".FormatString(table.Count - 1));
            });

            var spl = text.Split(' ');
            var res = new List<string>();
            foreach (var elem in spl)
            {
                int num = RegexGetNumber(elem, "\\[(?<num>[0-9]+)\\]");
                if (num > -1)
                    res.Add(elem.Replace("[{0}]".FormatString(num), table[num]).Replace("\"", ""));
                else
                    res.Add(elem);
            }

            return res;
        }

        public static void RegexDoMached(string text, string pattern, Action<Match> callback)
        {
            var r = new Regex(pattern, RegexOptions.IgnoreCase);
            var m = r.Match(text);
            while (m.Success)
            {
                callback(m);
                m = m.NextMatch();
            }
        }

        public static int RegexGetNumber(string text, string pattern)
        {
            var r = new Regex(pattern, RegexOptions.IgnoreCase);
            var m = r.Match(text);
            if (m.Success)
            {
                return m.Groups["num"].Value.ToInt();
            }
            return -1;
        }
        #endregion
    }
}
