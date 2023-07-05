using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class MethodSemantics : IMetadataTableRow
    {
        public ushort Semantics { get; private set; }
        public uint Method { get; private set; }
        public uint Association { get; private set; }
        public void Read(MetadataReader reader)
        {
            Semantics = reader.ReadUInt16();
            Method = reader.ReadUInt16();
            Association = reader.ReadUInt16();
        }
    }
}
