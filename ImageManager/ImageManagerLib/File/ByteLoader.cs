﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.File
{
    public class ByteLoader
    {
		public static byte[] FromStream(Stream stream)
        {
			if (stream == null)
                return null;

            byte[] data = new byte[stream.Length];
			stream.Read(data, 0, data.Length);

            return data;

        }
    }
}
