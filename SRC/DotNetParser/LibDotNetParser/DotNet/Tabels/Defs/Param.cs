using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class Param : IMetadataTableRow
    {
        public ushort Flags { get; private set; }
        public ushort Sequence { get; private set; }
        public uint Name { get; private set; }
        public void Read(MetadataReader reader)
        {
            Flags = reader.ReadUInt16();
            Sequence = reader.ReadUInt16();
            Name = reader.ReadStringStreamIndex();
        }
    }
}
