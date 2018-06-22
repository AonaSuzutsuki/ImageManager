﻿using System;
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
    }

	[JsonObject("directory")]
    public class DirectoryStructure
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("parent")]
		public long Parent { get; set; }
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
		public long Id { get; set; }
        [JsonProperty("parent")]
		public long Parent { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("location")]
		public long Location { get; set; }
        [JsonProperty("mimetype")]
        public string MimeType { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[FileStructure\t\tId = {0}, Parent = {1}, Name = {2}, Location = {3}, MimeType = {4} ]", Id, Parent, Name, Location, MimeType);
            return sb.ToString();
        }
    }
}
