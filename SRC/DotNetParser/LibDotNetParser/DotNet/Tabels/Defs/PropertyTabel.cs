using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class PropertyTabel : IMetadataTableRow
    {
        public ushort Flags { get;  private set; }
        public uint Name { get; private set; }

        /// <summary>
        /// The name of this column is misleading. It does not index a TypeDef or TypeRef table – instead it indexes the signature in the Blob heap of the Property.
        /// </summary>
        public uint Type { get; private set; }
        public void Read(MetadataReader reader)
        {
            Flags = reader.ReadUInt16();
            Name = reader.ReadStringStreamIndex();
            Type = reader.ReadUInt16();
        }
    }
}
