using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleFileSystem.Structs
{
    public class StartingMarkerEntry : Structure
    {
        public override byte EntryType => 0x2;

        public override void Write(BlockBuffer bb)
        {
            bb.WriteByte(EntryType);
        }

        public override void Read(BlockBuffer bb)
        {
            
        }
    }
}
