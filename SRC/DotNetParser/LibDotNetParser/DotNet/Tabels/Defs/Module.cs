using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDotNetParser.DotNet.Tabels
{
    public class Module : IMetadataTableRow
    {
        /// <summary>
        /// a 2-byte value, reserved, shall be zero
        /// </summary>
        public ushort Generation { get; set; }

        /// <summary>
        /// an index into the String heap
        /// </summary>
        public uint Name { get; set; }

        /// <summary>
        /// an index into the Guid heap; simply a Guid used to distinguish between two versions of the same module
        /// </summary>
        public uint Mvid { get; set; }

        /// <summary>
        /// an index into the Guid heap; reserved, shall be zero
        /// </summary>
        public uint EncId { get; set; }

        /// <summary>
        /// an index into the Guid heap; reserved, shall be zero
        /// </summary>
        public uint EncBaseId { get; set; }

        public void Read(MetadataReader reader)
        {
            Generation = reader.ReadUInt16();
            Name = reader.ReadStringStreamIndex();
            Mvid = reader.ReadGuidStreamIndex();
            EncId = reader.ReadGuidStreamIndex();
            EncBaseId = reader.ReadGuidStreamIndex();
        }
    }
}
