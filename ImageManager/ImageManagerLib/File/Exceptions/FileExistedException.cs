using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.File.Exceptions
{
    /// <summary>
	/// Exception raised when a file or directory exists.
    /// </summary>
    public class FileDirectoryExistedException : IOException
    {
        /// <summary>
        /// Initialize exception with original message.
        /// </summary>
		/// <param name="message">Message to initialize.</param>
        public FileDirectoryExistedException(string message) : base(message) { }
    }
}
