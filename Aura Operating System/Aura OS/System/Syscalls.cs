/*******************************************************
 * Copyright (C) 2012-2013 GruntXProductions <sloan@gruntxproductions.net>
 * 
 * This file is part of Grunty OS Infinity
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential.
 *******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura_OS.HAL;

namespace Aura_OS.Core
{
    class Syscalls : Driver
    {
        public override bool Init()
        {
            Cosmos.Core.CPU.EnableInterrupts();
            this.Name = "infinity API";
            Console.WriteLine("Initilizing API");
            setIntHandler(0x80, SWI_0x80);
            Console.WriteLine("Infinity API interrupts installed");
            return true;
        }
        public unsafe static void SWI_0x80(ref Cosmos.Core.INTs.IRQContext aContext, ref bool getHandled)
        {
            Console.WriteLine("SWI_0x80");
            if (aContext.Interrupt == 0x80)
            {
                Console.WriteLine("0x80");
                //Terminal.WriteLine("int");
                //Assembly.RestoreDataSegment(); // Restore kernels data segment
                if (aContext.EAX == 0x1) // Write to stdout
                {
                    uint ptr = aContext.ESI;

                    byte* dat = (byte*)(ptr);
                    for (int i = 0; dat[i] != 0; i++)
                    {
                        Console.Write((char)dat[i]);
                    }
                }
                else if (aContext.EAX == 0x2)
                    Console.WriteLine("0x2");
                else if (aContext.EAX == 0x3)
                    Console.WriteLine("0x3");
                else if (aContext.EAX == 0x4)
                    Console.WriteLine("0x4");
            }
        }
    }
}
