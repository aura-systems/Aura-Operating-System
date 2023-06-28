using LibDotNetParser.PE;
using System;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class ManifestResource : IMetadataTableRow
    {
        public uint Offset { get; set; }
        public uint Flags { get; set; }
        public uint Name { get; set; }
        public uint Implementation { get; set; }
        public void Read(MetadataReader reader)
        {
            Offset = reader.ReadUInt32();
            Flags = reader.ReadUInt32();
            Name = reader.ReadStringStreamIndex();
            Implementation = reader.ReadUInt32();
        }
    }
}
