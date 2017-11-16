using System;
using Aura_OS.HAL;

namespace Aura_OS.Core
{
    class Aura_Syscalls : Driver
    {
        public override bool Init()
        {
            this.Name = "Aura API";
            Cosmos.Core.INTs.SetIntHandler(0x49, SWI_0x49);
            return true;
        }
        public unsafe static void SWI_0x49(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            if (aContext.Interrupt == 0x49)
            {
                if (aContext.EAX == 0x01) // Write to stdout
                {
                    uint ptr = aContext.ESI;
                    byte* dat = (byte*)(ptr + System.Executable.COM.ProgramAddress);
                    for (int i = 0; dat[i] != 0; i++)
                    {
                        Console.Write((char)dat[i]);
                    }
                }
            }
        }
    }
}
