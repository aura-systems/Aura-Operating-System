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
            this.Name = "Aura API";
            Console.WriteLine("Initilizing API");
            //Cosmos.Core.INTs.SetIrqHandler(0x49, SWI_0x49);
            Cosmos.Core.INTs.SetIntHandler(0x49, SWI_0x49);
            Console.WriteLine("Aura API interrupts installed");
            return true;
        }
        public unsafe static void SWI_0x49(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            if (aContext.Interrupt == 0x49)
            {
                if (aContext.EAX == 0x01) // Write to stdout
                {
                    uint ptr = aContext.ESI;
                    byte* dat = (byte*)(ptr + System.exe.COM.ProgramAddress);
                    for (int i = 0; dat[i] != 0; i++)
                    {
                        Console.Write((char)dat[i]);
                    }
                }
                else if (aContext.EAX == 0x02) // Read from stdin
                {

                }
                else if (aContext.EAX == 0x03)
                {
                    Console.Clear();
                }   
                else if (aContext.EAX == 0x04) // set cursor pos
                {

                }
                else if (aContext.EAX == 0x05)
                {

                }
                else if (aContext.EAX == 0x06) // Push screen state
                {

                }
                else if (aContext.EAX == 0x61) // Push screen state
                {

                }
                else if (aContext.EAX == 0x07) // String cmp
                {

                }
                else if (aContext.EAX == 0x08) // String cmp
                {
                    
                }
                else if (aContext.EAX == 0x09) // Get File count in CD
                {

                }
                else if (aContext.EAX == 0x0A) // Get File count in CD
                {

                }
            }
        }
    }
}
