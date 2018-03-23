using System;
using System.Collections.Generic;
using System.Text;

namespace KsIL
{
    public class InstructionBase
    {

        internal Memory mMemory;

        public InstructionBase(Memory memory)
        {
            mMemory = memory;
        }

        public virtual void Run()
        {
        }


        public byte[] GetData(int Addr, byte[] input)
        {
                        
            int l = BitConverter.ToInt32(input, Addr);
            byte[] r = new byte[l];

            Array.Copy(input, r, l);

            return r;

        }


    }
}
