using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura_OS.System
{
    class Syscalls
    {
        Cosmos.Core.INTs.IRQContext Context = new Cosmos.Core.INTs.IRQContext();
        public static bool Init()
        {
            Console.WriteLine("Initilizing Aura API");
            setInterruptGate(0x80, SWI_0x80);
            Console.WriteLine("Aura API interrupts installed");
            return true;
        }

        public static void setInterruptGate(byte intnum, InterruptHandler.InterruptDelegate handler)
        {
            InterruptHandler i = new InterruptHandler();
            i.intNum = intnum;
            i.handler = handler;
            InterruptHandler.interruptHandlers.Add(i);
        }

        public unsafe static void SWI_0x80(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            if (aContext.Interrupt == 0x80)
            {
                if (aContext.EAX == 0x1) // Write to stdout
                {
                    uint ptr = aContext.ESI;
                    byte* dat = (byte*)(ptr + exe.COM.ProgramAddress);
                    for (int i = 0; dat[i] != 0; i++)
                    {
                        Console.Write((char)dat[i]);
                    }
                }
            }
        }
    }
}
