using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class TypeRef : IMetadataTableRow
    {
        public uint ResolutionScope { get; private set; }
        /// <summary>
        /// index into String heap 
        /// </summary>
        public uint TypeName { get; private set; }
        /// <summary>
        /// index into String heap
        /// </summary>
        public uint TypeNamespace { get; private set; }

        public void Read(MetadataReader reader)
        {
            ResolutionScope = reader.ReadUInt16();
            TypeName = reader.ReadStringStreamIndex();
            TypeNamespace = reader.ReadStringStreamIndex();

          //  Debug.Assert(ResolutionScope < 1000);
        }
    }
}
