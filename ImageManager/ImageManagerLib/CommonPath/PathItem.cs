using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.CommonPath
{
	/// <summary>
	/// Provide the function to manage the path.
    /// </summary>
    public class PathItem
    {
        private readonly List<string> pathList = new List<string>();

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:FileManagerLib.Path.PathItem"/> is root.
        /// </summary>
        /// <value><c>true</c> if is root; otherwise, <c>false</c>.</value>
        public bool IsRoot
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:FileManagerLib.Path.PathItem"/> class.
        /// </summary>
        public PathItem() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:FileManagerLib.Path.PathItem"/> class with path array.
        /// </summary>
        /// <param name="array">Array of string.</param>
        public PathItem(string[] array)
        {
            foreach (var path in array)
            {
                pathList.Add(path);
            }
        }

        /// <summary>
        /// Adds the path.
        /// </summary>
        /// <param name="path">Added the path.</param>
        public void AddPath(string path)
        {
            pathList.Add(path);
        }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <returns>The path.</returns>
        /// <param name="index">Index.</param>
        public string GetPath(int index)
        {
            if (index < 0 || pathList.Count - 1 < index)
                return null;

            string path = pathList[index];
            return path;
        }

        /// <summary>
        /// Pop a path from this instance.
        /// </summary>
        /// <returns>Path.</returns>
        public string Pop()
        {
            if (pathList.Count >= 1)
            {
                var item = pathList[pathList.Count - 1];
                pathList.RemoveAt(pathList.Count - 1);
                return item;
            }
            return null;
        }

        /// <summary>
        /// Gets the last path.
        /// </summary>
        /// <returns>The last path.</returns>
        public string GetLast()
        {
            if (pathList.Count >= 1)
                return pathList[pathList.Count - 1];
            return null;
        }

        /// <summary>
		/// Returns the path from the specified index as a <c>PathItem</c>.
        /// </summary>
		/// <returns>The <c>PathItem</c>.</returns>
        /// <param name="start">Start index.</param>
        public PathItem GetPathItemFrom(int start)
        {
            var list = new List<string>();
            for (int i = start; i < pathList.Count; i++)
                list.Add(pathList[i]);
            return new PathItem(list.ToArray());
        }

        /// <summary>
		/// Returns the path from the specified path as a <c>PathItem</c>.
        /// </summary>
        /// <returns>The <c>PathItem</c>.</returns>
		/// <param name="basePath">The specified path.</param>
        public PathItem GetPathItemFrom(string basePath)
        {
            var pathItemArray = new List<string>(PathSplitter.SplitPath(basePath).ToArray());
            var pathArray = pathList.ToArray();
            var list = new List<string>();

            if (pathItemArray.Count < pathArray.Length)
            {
                var cnt = (pathArray.Length - pathItemArray.Count);
                for (int i = 0; i < cnt; i++)
                    pathItemArray.Add("");
            }

            var numbersAndWords = pathArray.Zip(pathItemArray, (n, w) => new { basePath = w, targetPath = n });
            foreach (var nw in numbersAndWords)
            {
                if (!nw.basePath.Equals(nw.targetPath))
                    list.Add(nw.targetPath);
            }

            //var paths = pathArray.Except(pathItemArray).ToArray();

            return new PathItem(list.ToArray());
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:FileManagerLib.Path.PathItem"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="T:FileManagerLib.Path.PathItem"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="T:FileManagerLib.Path.PathItem"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="T:FileManagerLib.Path.PathItem"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is PathItem pathItem)
            {
                return pathList.SequenceEqual(pathItem.pathList);
            }
            return false;
        }

        /// <summary>
        /// To the array.
        /// </summary>
        /// <returns>The array.</returns>
        public string[] ToArray() => pathList.ToArray();
        
        /// <summary>
		/// To the array exclude specific count from end.
        /// </summary>
        /// <returns>The array.</returns>
        /// <param name="end">End.</param>
        public string[] ToArray(int end)
        {
            var list = new List<string>(pathList);
            list.RemoveAt(list.Count - end);
            return list.ToArray();
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:FileManagerLib.Path.PathItem"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:FileManagerLib.Path.PathItem"/>.</returns>
        public override string ToString()
        {
            var array = pathList.ToArray();
            var sb = new StringBuilder();
            sb.Append("/");
            for (int i = 0; i < array.Length; i++)
            {
                if (i < array.Length - 1)
                    sb.AppendFormat("{0}/", array[i]);
                else
                    sb.AppendFormat("{0}", array[i]);
            }
            return sb.ToString();
        }
    }
}
