using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class MethodImpl : IMetadataTableRow
    {
        public uint Class { get; private set; }
        public uint MethodBody { get; private set; }
        public uint MethodDeclaration { get; private set; }
        public void Read(MetadataReader reader)
        {
            Class = reader.ReadUInt16();
            MethodBody = reader.ReadUInt16();
            MethodDeclaration = reader.ReadUInt16();
        }
    }
}
