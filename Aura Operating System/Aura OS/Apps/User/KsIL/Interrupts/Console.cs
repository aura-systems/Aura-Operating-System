using System;
using System.Collections.Generic;
using System.Text;

namespace KsIL.Interrupts
{
    public class Console : Interrupt
    {

        public Console()
        {
            Code = 1;
        }


        public override void Run(byte[] Parameters, Memory mMemory)
        {


            if (Parameters[0] == 0x00)
            {

                System.Console.Write(Encoding.UTF8.GetString(mMemory.GetData(BitConverter.ToInt32(Parameters, 2))));

            }
            else if (Parameters[0] == 0x01)
            {

                System.Console.WriteLine(Encoding.UTF8.GetString(mMemory.GetData(BitConverter.ToInt32(Parameters, 2))));
                
            }
            else if (Parameters[0] == 0x02)
            {

                mMemory.SetData(BitConverter.ToInt32(Parameters, 2), Encoding.UTF8.GetBytes(System.Console.ReadLine()));

            }
            else if (Parameters[0] == 0x03)
            {

                System.Console.Clear();

            }

        }

    }
}
