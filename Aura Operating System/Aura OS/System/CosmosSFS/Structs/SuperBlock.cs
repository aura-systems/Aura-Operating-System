using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleFileSystem.Structs
{
    public class SuperBlock : Structure
    {
        public long TimeStamp { get; set; }
        public long DataSizeInBlocks { get; set; }
        public long SizeOfIndexInBytes { get; set; }
        public byte VersionNumber { get; set; } = 0x10;
        public long TotalBlocks { get; set; }
        public long TotalReservedBlocks { get; set; }
        public byte BlockSize { get; set; }
        public byte Checksum { get; set; }

        public override byte EntryType => 0;

        public override void Write(BlockBuffer bb)
        {
            bb.Offset = 0x0194;
            bb.WriteLong(TimeStamp);
            bb.WriteLong(DataSizeInBlocks);
            bb.WriteLong(SizeOfIndexInBytes);
            bb.WriteByte((byte) 'S');
            bb.WriteByte((byte) 'F');
            bb.WriteByte((byte) 'S');
            bb.WriteByte(VersionNumber);
            bb.WriteLong(TotalBlocks);
            bb.WriteLong(TotalReservedBlocks);
            bb.WriteByte(BlockSize);
            bb.WriteByte(Checksum);
        }

        public override void Read(BlockBuffer bb)
        {
            bb.Offset = 0x0194;
            TimeStamp = bb.ReadLong();
            DataSizeInBlocks = bb.ReadLong();
            SizeOfIndexInBytes = bb.ReadLong();
            bb.Offset = 0x01AF;
            VersionNumber = bb.ReadByte();
            TotalBlocks = bb.ReadLong();
            TotalReservedBlocks = bb.ReadLong();
            BlockSize = bb.ReadByte();
            Checksum = bb.ReadByte();
        }
    }
}