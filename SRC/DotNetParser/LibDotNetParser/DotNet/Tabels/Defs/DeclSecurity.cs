using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class DeclSecurity : IMetadataTableRow
    {
        public ushort Action { get; private set; }
        public uint Parent { get; private set; }
        public uint PermissionSet { get; private set; }
        public void Read(MetadataReader reader)
        {
            Action = reader.ReadUInt16();
            Parent = reader.ReadUInt16();
            PermissionSet = reader.ReadBlobStreamIndex();
        }
    }
}
