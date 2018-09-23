using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleFileSystem.Structs
{
    public class FileEntry : Structure
    {
        public override byte EntryType => 0x12;
        public long TimeStamp { get; set; }
        public long StartingBlock { get; set; }
        public long EndingBlock { get; set; }
        public long Length { get; set; }

        public override void Write(BlockBuffer bb)
        {
            bb.WriteByte(EntryType);
           
            bb.WriteLong(TimeStamp);
            bb.WriteLong(StartingBlock);
            bb.WriteLong(EndingBlock);
            bb.WriteLong(Length);

            const int rem = 22;
            if (Name.Length > rem)
            {
                Continuations = (byte)(((Name.Length - rem) / 64) + 1);
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
            StartingBlock = bb.ReadLong();
            EndingBlock = bb.ReadLong();
            Length = bb.ReadLong();
            Continuations = bb.ReadByte();
            Name = bb.ReadString();
        }
    }
}