using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class ImplMap : IMetadataTableRow
    {
        public ushort MappingFlags { get; private set; }
        public uint MemberForwarded { get; private set; }
        public uint ImportName { get; private set; }
        public uint ImportScope { get; private set; }
        public void Read(MetadataReader reader)
        {
            MappingFlags = reader.ReadUInt16();
            MemberForwarded = reader.ReadUInt16();
            ImportName = reader.ReadStringStreamIndex();
            ImportScope = reader.ReadUInt16();
        }
    }
}
