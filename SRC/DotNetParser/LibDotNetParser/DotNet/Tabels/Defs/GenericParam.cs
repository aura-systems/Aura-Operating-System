using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class GenericParam : IMetadataTableRow
    {
        public ushort Number { get; set; }
        public ushort Flags { get; set; }
        /// <summary>
        /// TypeOrMethodDef
        /// </summary>
        public uint Owner { get; set; }
        public uint Name { get; set; }
        public void Read(MetadataReader reader)
        {
            Number = reader.ReadUInt16();
            Flags = reader.ReadUInt16();
            Owner = reader.ReadUInt16();
            Name = reader.ReadStringStreamIndex();
        }
    }
}
