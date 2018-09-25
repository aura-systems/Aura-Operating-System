using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosELFCore
{
    public unsafe class MemoryStream : Stream
    {
        public byte* Pointer;

        public MemoryStream(byte* data)
        {
            Pointer = data;
        }
        public override void Write(byte dat)
        {
            Pointer[Posistion++] = dat;
        }
        public override int Read()
        {
            return Pointer[Posistion++];
        }
    }
}
