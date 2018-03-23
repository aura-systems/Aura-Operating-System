using System;
using System.Collections.Generic;
using System.Text;

namespace KsIL.Instructions
{
    public class TestGreaterThan : InstructionBase
    {

        int Location0;
        int Location1;

        public TestGreaterThan(Memory memory, byte[] Parameters) : base(memory)
        {

            Location0 = BitConverter.ToInt32(Parameters, 0);
            Location1 = BitConverter.ToInt32(Parameters, 4);

        }

        public override void Run()
        {

            int i0 = 0, i1 = 0;

            if (mMemory.Get(0) == 0x00)
            {

                i0 = mMemory.Get(Location0);
                i1 = mMemory.Get(Location1);

            }
            else if (mMemory.Get(0) == 0x01)
            {

                i0 = BitConverter.ToInt16(mMemory.Get(Location0, 2), 0);
                i1 = BitConverter.ToInt16(mMemory.Get(Location1, 2), 0);

            }
            else if (mMemory.Get(0) == 0x02)
            {

                i0 = BitConverter.ToInt32(mMemory.Get(Location0, 4), 0);
                i1 = BitConverter.ToInt32(mMemory.Get(Location1, 4), 0);

            }
            else if (mMemory.Get(0) == 0x03)
            {

                i0 = (int)BitConverter.ToInt64(mMemory.Get(Location0, 8), 0);
                i1 = (int)BitConverter.ToInt64(mMemory.Get(Location1, 8), 0);

            }

            if (i0 > i1)
            {

                mMemory.Set(2, 0x01);

            }
            else
            {
                
                mMemory.Set(2, 0x00);

            }

        }

    }
}
