using System;
using System.Collections.Generic;
using System.Text;

namespace KsIL.Instructions
{
    public class TestEqual : InstructionBase
    {

        int Location0;
        int Location1;

        public TestEqual(Memory memory, byte[] Parameters) : base(memory)
        {

            Location0 = BitConverter.ToInt32(Parameters, 0);
            Location1 = BitConverter.ToInt32(Parameters, 4);

        }

        public override void Run()
        {
            
            if (Encoding.UTF8.GetString(mMemory.GetData(Location0)) == Encoding.UTF8.GetString(mMemory.GetData(Location1)))
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
