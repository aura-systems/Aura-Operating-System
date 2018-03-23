using System;
using System.Collections.Generic;
using System.Text;

namespace KsIL.Instructions
{
    public class Store : InstructionBase
    {

        int Location;
        byte[] mContent;
        public Store(Memory memory, byte[] Parameters) : base(memory)
        {

            mContent = new byte[Parameters.Length - 4];
            
            Array.Copy(Parameters, mContent, Parameters.Length - 4);
            Location = BitConverter.ToInt32(Parameters, Parameters.Length - 4);

        }

        public override void Run()
        {
            //mMemory.Set(Location, BitConverter.GetBytes( mContent.Length));
            mMemory.Set(Location, Utill.Read(mContent, mMemory));
        }

    }
}
