using System;
using Aura_OS.HAL;
using static Cosmos.Core.INTs;
namespace Aura_OS.Core
{
    class MSDOS_Syscalls : Driver
    {
        public override bool Init()
        {
            Name = "MSDOS API";
            SetIntHandler(0x48, SWI); //ints.setinthandler
            return true;
        }
        public unsafe static void SWI(ref IRQContext aContext)
        {
            if (aContext.Interrupt == 0x48)
            {
                //Console.WriteLine("'" + aContext.EAX + "'");
                if (aContext.EAX == 0x01)
                {
                    //Console.WriteLine("EAX is 0x01");
                    uint ptr = aContext.ESI;
                    byte* dat = (byte*)(ptr + System.Executable.COM.ProgramAddress);
                    for (int i = 0; dat[i] != 0; i++)
                    {
                        Console.Write((char)dat[i]);
                    }
                }
                else if (aContext.EAX == 0x02)
                {
                    Console.Clear();
                }
                
            }

        }
    }
}
