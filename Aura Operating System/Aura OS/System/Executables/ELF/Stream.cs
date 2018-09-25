using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosELFCore
{
    public class Stream
    {
        public uint Posistion = 0;

        public virtual int Read()
        {
            return -1;
        }

        public virtual void Write(byte dat)
        {
        }

    }
}
