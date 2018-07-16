using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterableFileStream
{
    public class ClusterableFileStream : IDisposable
    {

        #region Fields
        IList<Stream> streams = new List<Stream>();
        #endregion

        #region Properties

        #endregion

        public ClusterableFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
            if (!File.Exists(path))
            {
                streams.Add(new FileStream(path, mode, access, share));
            }
            else
            {

            }

        }

        public void Dispose()
        {

        }
    }
}
