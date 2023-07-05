using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class MemberRef : IMetadataTableRow
    {
        public uint Class { get; private set; }
        public uint Name { get; private set; }
        public uint Signature { get; private set; }
        public void Read(MetadataReader reader)
        {
            Class = reader.ReadUInt16();
            Name = reader.ReadStringStreamIndex();
            Signature = reader.ReadBlobStreamIndex();
        }
    }
}
