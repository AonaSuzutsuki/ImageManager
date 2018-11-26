using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FileManagerLib.File.Json
{
	/// <summary>
    /// Table structure.
    /// </summary>
	[JsonObject("table")]
    public class TableStructure
    {
		/// <summary>
        /// Gets or sets the directory.
        /// </summary>
        /// <value>The directory.</value>
        [JsonProperty("directory")]
        public DirectoryStructure[] Directory { get; set; }

        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>The file.</value>
        [JsonProperty("file")]
        public FileStructure[] File { get; set; }
        
        /// <summary>
		/// Whether consistency is confirmed using hash calculation
        /// </summary>
        /// <value><c>true</c> if is check hash; otherwise, <c>false</c>.</value>
        [JsonProperty("isCheckHash")]
        public bool IsCheckHash { get; set; } = true;
    }

    /// <summary>
    /// Directory structure.
    /// </summary>
	[JsonObject("directory")]
    public class DirectoryStructure
    {
		/// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        [JsonProperty("parent")]
		public int Parent { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:FileManagerLib.File.Json.DirectoryStructure"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:FileManagerLib.File.Json.DirectoryStructure"/>.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[DirectoryStructure\tId = {0}, Parent = {1}, Name = {2} ]", Id, Parent, Name);
            return sb.ToString();
        }
    }


    /// <summary>
    /// File structure.
    /// </summary>
    [JsonObject("file")]
    public class FileStructure
    {
		/// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [JsonProperty("id")]
		public int Id { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        [JsonProperty("parent")]
		public int Parent { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        [JsonProperty("location")]
		public long Location { get; set; }
        
        /// <summary>
        /// Gets or sets the hash.
        /// </summary>
        /// <value>The hash.</value>
		[JsonProperty("hash")]
		public string Hash { get; set; }

        /// <summary>
        /// Gets or sets the additional.
        /// </summary>
        /// <value>The additional.</value>
        [JsonProperty("additional")]
        public Dictionary<string, string> Additional { get; set; }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:FileManagerLib.File.Json.FileStructure"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:FileManagerLib.File.Json.FileStructure"/>.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
			sb.AppendFormat("[FileStructure\t\tId = {0}, Parent = {1}, Name = {2}, Location = {3}, Hash = {4} ]", Id, Parent, Name, Location, Hash);
            return sb.ToString();
        }
    }
}
