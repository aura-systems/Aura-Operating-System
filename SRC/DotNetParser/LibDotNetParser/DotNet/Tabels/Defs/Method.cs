using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class Method : IMetadataTableRow
    {
        public uint RVA { get; private set; }
        public ushort ImplFlags { get; private set; }
        public ushort Flags { get; private set; }
        public uint Name { get; private set; }
        public uint Signature { get; private set; }
        public uint ParamList { get; private set; }
        public void Read(MetadataReader reader)
        {
            RVA = reader.ReadUInt32();
            ImplFlags = reader.ReadUInt16();
            Flags = reader.ReadUInt16();
            Name = reader.ReadStringStreamIndex();
            Signature = reader.ReadBlobStreamIndex();
            ParamList = reader.ReadUInt16();
        }
    }
}
