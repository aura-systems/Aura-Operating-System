using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class Constant : IMetadataTableRow
    {
        public byte Type;
        public uint Parrent;
        public uint Value;
        public void Read(MetadataReader reader)
        {
            Type = reader.ReadByte();
            var zero = reader.ReadByte(); //Type is followed by a null byte
            Parrent = reader.ReadUInt16();
            Value = reader.ReadBlobStreamIndex();
        }
    }
}
