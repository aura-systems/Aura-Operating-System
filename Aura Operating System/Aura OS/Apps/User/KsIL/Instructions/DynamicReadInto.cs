using System;
using System.Collections.Generic;
using System.Text;

namespace KsIL.Instructions
{
    public class DynamicReadInto : InstructionBase
    {

        int Location0;
        int Location1;

        public DynamicReadInto(Memory memory, byte[] Parameters) : base(memory)
        {

            Location0 = BitConverter.ToInt32(Parameters, 0);
            Location1 = BitConverter.ToInt32(Parameters, 4);

        }


        public override void Run()
        {

            byte[] mContent = mMemory.GetData(Location0);

            int i;
            for (i = 13; i < mMemory.GetSize(); i++)
            {

                if (i + mContent.Length >= Location1)
                {

                    i = Location1 + 4;
                    continue;

                }

                if (mMemory.Get(i, mContent.Length) == new byte[mContent.Length])
                {
                    break;
                }

            }

            mMemory.Set(Location1, BitConverter.GetBytes(i));
            mMemory.SetData(i, mContent);

        }


    }
}
