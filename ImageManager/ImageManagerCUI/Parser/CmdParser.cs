using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManagerCUI.Parser
{
    public class CmdParser
    {
        public string Command { get; private set; }

        private Dictionary<string, string> attributes = new Dictionary<string, string>();

        public CmdParser(string cmd)
        {
            var parts = new List<string>(cmd.Split(' '));
            parts.Remove("");

            Command = parts[0];
            for (int i = 1; i < parts.Count; i += 2)
            {
                var name = parts[i];
                if (name[0].Equals('-'))
                    name = name.Remove(0, 1);
                attributes.Add(name, parts[i + 1]);
            }
        }

        public string GetAttribute(string attrName)
        {
            if (attributes.ContainsKey(attrName))
                return attributes[attrName];
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
    }
}
