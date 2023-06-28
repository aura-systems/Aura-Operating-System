using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class ClassLayout : IMetadataTableRow
    {
        public ushort PackingSize { get; private set; }
        public uint ClassSize { get; private set; }
        public uint Parent { get; private set; }
        public void Read(MetadataReader reader)
        {
            PackingSize = reader.ReadUInt16();
            ClassSize = reader.ReadUInt32();
            Parent = reader.ReadUInt16();
        }
    }
}
