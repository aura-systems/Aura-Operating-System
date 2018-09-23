using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleFileSystem.Structs
{
    public class UnusedEntry : Structure
    {
        public override byte EntryType => 0x10;

        public override void Write(BlockBuffer bb)
        {
            bb.WriteByte(EntryType);
        }

        public override void Read(BlockBuffer bb)
        {
        }
    }
}