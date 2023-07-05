using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class File : IMetadataTableRow
    {
        public uint Flags { get; set; }
        public uint Name { get; set; }
        public uint HashValue { get; set; }
        public void Read(MetadataReader reader)
        {
            Flags = reader.ReadUInt32();
            Name = reader.ReadStringStreamIndex();
            HashValue = reader.ReadBlobStreamIndex();
        }
    }
}
