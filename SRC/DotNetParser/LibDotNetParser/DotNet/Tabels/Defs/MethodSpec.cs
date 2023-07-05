using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class MethodSpec : IMetadataTableRow
    {
        public ushort Method { get; set; }
        public ushort Instantiation { get; set; }
        public void Read(MetadataReader reader)
        {
            Method = reader.ReadUInt16();
            Instantiation = reader.ReadUInt16();
        }
    }
}
