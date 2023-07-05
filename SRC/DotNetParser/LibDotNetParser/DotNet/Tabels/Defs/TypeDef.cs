using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class TypeDef : IMetadataTableRow
    {
        public uint Flags { get; private set; }

        public uint Name { get; private set; }

        public uint Namespace { get; private set; }

        public uint Extends { get; private set; }

        public uint FieldList { get; private set; }

        public uint MethodList { get; private set; }
        public void Read(MetadataReader reader)
        {
            Flags = reader.ReadUInt32();
            Name = reader.ReadStringStreamIndex();
            Namespace = reader.ReadStringStreamIndex();
            Extends = reader.ReadUInt16();
            FieldList = reader.ReadUInt16();
            MethodList = reader.ReadUInt16();
        }
    }
}
