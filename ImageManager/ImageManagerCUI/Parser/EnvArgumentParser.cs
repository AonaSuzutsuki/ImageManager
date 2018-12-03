﻿using FileManagerLib.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManagerCUI.Parser
{
    public class EnvArgumentParser
    {
        private readonly string[] arguments;

        protected readonly Dictionary<string, string> parameters = new Dictionary<string, string>();

        protected readonly List<string> values = new List<string>();

        public Dictionary<string, int> optionCountMap = new Dictionary<string, int>();

        public EnvArgumentParser(string[] args)
        {
            arguments = args;
        }

        public void AddOptionCount(string key, int count)
        {
            optionCountMap.Put(key, count);
        }

        public void Analyze()
        {
            for (int i = 0; i < arguments.Length; i++)
            {
                var arg = arguments[i];
                if (arg.StartsWith("-"))
                {
                    var count = optionCountMap.Get(arg, 1);
                    if (count > 0)
                    {
                        if (arguments.Length > i + 1)
                        {
                            var value = arguments[++i];
                            parameters.Add(arg, value);
                        }
                    }
                    else
                    {
                        parameters.Add(arg, "");
                    }
                }
                else
                {
                    values.Add(arg);
                }
            }
        }

        public string GetOutputFilepath(string option = "-o")
        {
            return GetOption(option);
        }

        public bool IsExtract(string option = "-m")
        {
            var mode = GetOption(option);
            return mode.Equals("e");
        }

        public bool IsInsert(string option = "-m")
        {
            var mode = GetOption(option);
            return mode.Equals("i");
        }

        public string GetOption(string option)
        {
            if (parameters.ContainsKey(option))
                return parameters[option];
            return null;
        }

        public string[] GetValues()
        {
            return values.ToArray();
        }

        public string GetValue()
        {
            if (values.Count > 0)
                return values[0];
            return null;
        }
    }
}
