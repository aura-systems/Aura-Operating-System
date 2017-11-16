using System;
using Aura_OS.HAL;

namespace Aura_OS.Core
{
    class MSDOS_Syscalls : Driver
    {
        public override bool Init()
        {
            this.Name = "MSDOS API";
            Cosmos.Core.INTs.SetIntHandler(0x48, SWI_0x48);
            return true;
        }
        public unsafe static void SWI_0x48(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            if (aContext.Interrupt == 0x48)
            {
                Console.WriteLine("0x48");
                if ((aContext.EAX & 0xFF00) >> 8 == 0x09) // AH
                {
                    Console.WriteLine("AH");
                    //uint ptr = aContext.EDX & 0xFFFF; //DX
                    //byte* dat = (byte*)(ptr + System.Executable.Executables.ProgramAddress);
                    //for (int i = 0; dat[i] != 0; i++)
                    //{
                    //    Console.Write((char)dat[i]);
                    //}
                }
            }
        }
    }
}
