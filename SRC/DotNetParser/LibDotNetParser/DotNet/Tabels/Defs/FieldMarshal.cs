using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class FieldMarshal : IMetadataTableRow
    {
        public uint Parent { get; private set; }
        public uint NativeType { get; private set; }
        public void Read(MetadataReader reader)
        {
            Parent = reader.ReadUInt16();
            NativeType = reader.ReadBlobStreamIndex();
        }
    }
}
