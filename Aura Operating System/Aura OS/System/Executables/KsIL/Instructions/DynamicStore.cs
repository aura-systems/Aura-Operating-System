using System;

namespace KsIL.Instructions
{
    public class DynamicStore : InstructionBase
    {

        int Location;
        byte[] mContent;

        public DynamicStore(Memory memory, byte[] Parameters) : base(memory)
        {

            mContent = new byte[Parameters.Length - 4];
            Array.Copy(Parameters, mContent, Parameters.Length - 4);
            Location = BitConverter.ToInt32(Parameters, Parameters.Length - 4);

        }

        public override void Run()
        {
            
            int i;
            for (i = 13; i < mMemory.GetSize(); i++)
            {
                
                if (i + mContent.Length >= Location )
                {

                    i = Location + 4;
                    continue;

                }

                if (mMemory.Get(i, mContent.Length) == new byte[mContent.Length])
                {
                    break;
                }

            }

            mMemory.Set(Location, BitConverter.GetBytes(i));
            mMemory.Set(i, mContent);
            
        }

    }
}
