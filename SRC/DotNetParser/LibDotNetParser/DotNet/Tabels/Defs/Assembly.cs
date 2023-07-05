using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels.Defs
{
    public class Assembly : IMetadataTableRow
    {
        public uint HashAlgId { get; private set; }
        public ushort MajorVersion { get; private set; }
        public ushort MinorVersion { get; private set; }
        public ushort BuildNumber { get; private set; }
        public ushort RevisionNumber { get; private set; }
        public uint Flags { get; private set; }
        public uint PublicKey { get; private set; }
        public uint Name { get; private set; }
        public uint Culture { get; private set; }
        public void Read(MetadataReader reader)
        {
            HashAlgId = reader.ReadUInt32();

            //Version consts
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            BuildNumber = reader.ReadUInt16();
            RevisionNumber = reader.ReadUInt16();

            //Rest of the cols
            Flags = reader.ReadUInt32();
            PublicKey = reader.ReadBlobStreamIndex();
            Name = reader.ReadStringStreamIndex();
            Culture = reader.ReadStringStreamIndex();
        }
    }
}
