using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleFileSystem
{
    public abstract class Structure
    {
        public abstract byte EntryType { get; }

        public abstract void Write(BlockBuffer bb);
        public abstract void Read(BlockBuffer bb);

        public byte Continuations { get; set; }

        public string Name { get; set; }
    }
}