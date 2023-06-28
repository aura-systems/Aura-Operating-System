using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class CustomAttribute : IMetadataTableRow
    {
        public uint Parent { get; set; }
        public uint Type { get; set; }
        public uint Value { get; set; }
        public void Read(MetadataReader reader)
        {
            Parent = reader.ReadUInt16();
            Type = reader.ReadUInt16();
            Value = reader.ReadUInt16();
        }
    }
}
