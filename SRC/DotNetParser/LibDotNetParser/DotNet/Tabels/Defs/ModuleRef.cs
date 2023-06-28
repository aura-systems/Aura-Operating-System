using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class ModuleRef : IMetadataTableRow
    {
        public uint Name { get; private set; }
        public void Read(MetadataReader reader)
        {
            Name = reader.ReadStringStreamIndex();
        }
    }
}
