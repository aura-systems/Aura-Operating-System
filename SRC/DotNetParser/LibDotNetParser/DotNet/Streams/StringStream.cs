using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibDotNetParser.DotNet.Streams
{
    public class StringsStream
    {
        private BinaryReader r;
        public StringsStream(byte[] data)
        {
            r = new BinaryReader(new MemoryStream(data));
        }

        public string GetByOffset(uint offset)
        {
            r.BaseStream.Seek(offset, SeekOrigin.Begin);
            return r.ReadNullTermString();
        }
    }
}
