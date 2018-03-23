using System;
using System.Collections.Generic;
using System.Text;

namespace KsIL.Instructions
{
    public class InterruptInstruction : InstructionBase
    {

        public static List<Interrupt> Interrupts = new List<Interrupt>();

        byte[] mParameters;

        public InterruptInstruction(Memory memory, byte[] Parameters) : base(memory)
        {

            mParameters = Parameters;
         
        }

        public override void Run()
        {

            Int16 INT = BitConverter.ToInt16(mParameters, 0);


            foreach (Interrupt Int in Interrupts)
            {

                if (INT == Int.Code)
                {

                    byte[] Parameters = Utill.ArrayRemoveAt(mParameters, 1, mParameters.Length - 2);
                                        
                    Int.Run(Parameters, mMemory);

                }

            }

        }

    }
}
