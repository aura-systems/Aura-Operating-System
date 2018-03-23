using System;
using System.Collections.Generic;
using System.Text;

namespace KsIL.Instructions
{
    public class Clear : InstructionBase
    {

        int Location;

        public Clear(Memory memory, byte[] Parameters) : base(memory)
        {

            Location = BitConverter.ToInt32(Parameters, 0);
            
        }

        public override void Run()
        {

            int i = BitConverter.ToInt32(mMemory.Get(Location, 4), 0);
            byte[] tofill = new byte[i + 4];

            mMemory.Set(Location, tofill);
        }

    }
}
