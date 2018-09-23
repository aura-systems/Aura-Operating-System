using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleFileSystem.Structs
{
    public class DeletedDirectoryEntry : Structure
    {
        public override byte EntryType => 0x19;

        public long TimeStamp { get; set; }

        public override void Write(BlockBuffer bb)
        {
            bb.WriteByte(EntryType);
            bb.WriteLong(TimeStamp);

            const int rem = 46;
            if (Name.Length > rem)
            {
                Continuations = (byte) (((Name.Length - rem) / 64) + 1);
                bb.WriteByte(Continuations);
                bb.WriteString(Name.Remove(rem));
            }
            else
            {
                bb.WriteByte(Continuations);
                bb.WriteString(Name);
            }
        }

        public override void Read(BlockBuffer bb)
        {
            bb.ReadByte();
            TimeStamp = bb.ReadLong();
            Continuations = bb.ReadByte();
            Name = bb.ReadString();
        }
    }
}