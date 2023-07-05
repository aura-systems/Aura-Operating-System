using System;
using System.IO;
using System.Text;

namespace LibDotNetParser.PE
{
    public class MetadataReader : BinaryReader
    {
        public StreamOffsetSizeFlags StreamOffsetSizeFlags { get; set; }
        public HeapSizes HeapSizes { get; }
        public MetadataReader(Stream input, HeapSizes s) : base(input)
        {
            HeapSizes = s;
        }
        public uint ReadSize(uint size)
        {
            switch (size)
            {
                case 1:
                    return ReadByte();
                case 2:
                    return ReadUInt16();
                case 4:
                    return ReadUInt32();
                default:
                    throw new NotImplementedException("Unsupported size: " + size);
            }
        }
        public uint ReadStringStreamIndex()
        {
            return ReadSize(HeapSizes.String);
        }

        public uint ReadGuidStreamIndex()
        {
            return ReadSize(HeapSizes.Guid);
        }

        public uint ReadBlobStreamIndex()
        {
            return ReadSize(HeapSizes.Blob);
        }
    }
}
