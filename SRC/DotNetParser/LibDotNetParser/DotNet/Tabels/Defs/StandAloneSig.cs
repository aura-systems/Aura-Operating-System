using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class StandAloneSig : IMetadataTableRow
    {
        public uint Signature { get; private set; }
        public void Read(MetadataReader reader)
        {
            Signature = reader.ReadBlobStreamIndex();
        }
    }
}
