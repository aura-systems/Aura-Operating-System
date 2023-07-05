using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class Event : IMetadataTableRow
    {
        public ushort EventFlags { get; private set; }
        public uint Name { get; private set; }
        public uint EventType { get; private set; }
        public void Read(MetadataReader reader)
        {
            EventFlags = reader.ReadUInt16();
            Name = reader.ReadStringStreamIndex();
            EventType = reader.ReadUInt16();
        }
    }
}
