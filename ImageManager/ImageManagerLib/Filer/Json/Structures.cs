﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FileManagerLib.Filer.Json
{
	[JsonObject("table")]
    public class TableStructure
    {
        [JsonProperty("directory")]
        public DirectoryStructure[] Directory { get; set; }
        [JsonProperty("file")]
        public FileStructure[] File { get; set; }
        [JsonProperty("isCheckHash")]
        public bool IsCheckHash { get; set; } = true;
    }

	[JsonObject("directory")]
    public class DirectoryStructure
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("parent")]
		public int Parent { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[DirectoryStructure\tId = {0}, Parent = {1}, Name = {2} ]", Id, Parent, Name);
            return sb.ToString();
        }
    }

    [JsonObject("file")]
    public class FileStructure
    {
        [JsonProperty("id")]
		public int Id { get; set; }
        [JsonProperty("parent")]
		public int Parent { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("location")]
		public long Location { get; set; }
        //[JsonProperty("mimetype")]
        //public string MimeType { get; set; }
		[JsonProperty("hash")]
		public string Hash { get; set; }

        [JsonProperty("additional")]
        public Dictionary<string, string> Additional { get; set; }


        public override string ToString()
        {
            var sb = new StringBuilder();
			sb.AppendFormat("[FileStructure\t\tId = {0}, Parent = {1}, Name = {2}, Location = {3}, Hash = {4} ]", Id, Parent, Name, Location, Hash);
            return sb.ToString();
        }
    }
}
