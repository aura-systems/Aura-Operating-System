using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class NestedClass : IMetadataTableRow
    {
        /// <summary>
        /// TypeDef index of the nested class
        /// </summary>
        public ushort NestedClassType { get; set; }
        /// <summary>
        /// TypeDef index of the parent class
        /// </summary>
        public ushort EnclosingClassType { get; set; }
        public void Read(MetadataReader reader)
        {
            NestedClassType = reader.ReadUInt16();
            EnclosingClassType = reader.ReadUInt16();
        }
    }
}
