using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class FieldLayout : IMetadataTableRow
    {
        public uint Offset { get; private set; }
        public uint Field { get; private set; }
        public void Read(MetadataReader reader)
        {
            Offset = reader.ReadUInt32();
            Field = reader.ReadUInt16();
        }
    }
}
