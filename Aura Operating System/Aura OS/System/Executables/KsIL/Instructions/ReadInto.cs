using System;
using System.Collections.Generic;
using System.Text;

namespace KsIL.Instructions
{
    public class ReadInto : InstructionBase
    {
        int Location0;
        int Location1;
        public ReadInto(Memory memory, byte[] Parameters) : base(memory)
        {

            Location0 = BitConverter.ToInt32(Parameters, 0);
            Location1 = BitConverter.ToInt32(Parameters, 4);

        }

        public override void Run()
        {

            int l = BitConverter.ToInt32( mMemory.Get(Location0, 4), 0);

            mMemory.Set(Location1, mMemory.Get(Location0, l + 4));
            
        }

    }
}
