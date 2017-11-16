using System;
using Aura_OS.HAL;

namespace Aura_OS.Core
{
    class MSDOS_Syscalls : Driver
    {
        public override bool Init()
        {
            Cosmos.Core.CPU.EnableInterrupts();
            this.Name = "MSDOS API";
            Console.WriteLine("Initilizing API");
            Cosmos.Core.INTs.SetIntHandler(0x21, SWI_0x21);
            Console.WriteLine("MSDOS API installed");
            return true;
        }
        public unsafe static void SWI_0x21(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            if (aContext.Interrupt == 0x21)
            {
                if ((aContext.EAX & 0xFF00) >> 8 == 0x09) // AH
                {
                    uint ptr = aContext.EDX & 0xFFFF; //DX
                    byte* dat = (byte*)(ptr + System.exe.COM.ProgramAddress);
                    for (int i = 0; dat[i] != 0; i++)
                    {
                        Console.Write((char)dat[i]);
                    }
                }
            }
        }
    }
}
