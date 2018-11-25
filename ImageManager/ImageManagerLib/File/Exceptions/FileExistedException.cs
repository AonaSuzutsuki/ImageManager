using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.File.Exceptions
{
    /// <summary>
    /// ファイルまたはディレクトリが存在する時に発生する例外です。
    /// </summary>
    public class FileDirectoryExistedException : IOException
    {
        /// <summary>
        /// 独自のメッセージを受けて例外を初期化します。
        /// </summary>
        /// <param name="message"></param>
        public FileDirectoryExistedException(string message) : base(message) { }
    }
}
