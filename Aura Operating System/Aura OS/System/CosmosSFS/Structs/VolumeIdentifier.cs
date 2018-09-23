using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleFileSystem.Structs
{
    public class VolumeIdentifier : Structure
    {
       
        public long TimeStamp { get; set; }
        public string VolumeName { get; set; }

        public override byte EntryType => 0x01;

        public override void Write(BlockBuffer bb)
        {
            bb.WriteByte(EntryType);
            bb.WriteLong(TimeStamp);
            bb.WriteString(VolumeName);
        }

        public override void Read(BlockBuffer bb)
        {
            bb.ReadByte();
            TimeStamp = bb.ReadLong();
            VolumeName = bb.ReadString();
        }
    }
}