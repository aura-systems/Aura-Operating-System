using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleFileSystem.Structs
{
    public class UnusableEntry : Structure
    {
        public override byte EntryType => 0x18;

        public long StartingBlock { get; set; }
        public long EndingBlock { get; set; }

        public override void Write(BlockBuffer bb)
        {
            bb.WriteByte(EntryType);
            bb.WriteLong(StartingBlock);
            bb.WriteLong(EndingBlock);
        }

        public override void Read(BlockBuffer bb)
        {
            bb.ReadByte();
            StartingBlock = bb.ReadLong();
            EndingBlock = bb.ReadLong();
        }
    }
}
