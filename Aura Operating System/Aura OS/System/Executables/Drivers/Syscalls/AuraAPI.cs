using System;
using static Cosmos.Core.INTs;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Executables.Drivers.Syscalls
{
	class AuraAPI : Driver
	{
		public override bool Init()
		{
			Name = "Aura Syscalls";
			SetIntHandler(0x80, SWI); //ints.setinthandler
			return true;
		}

		public static int x = Console.CursorLeft;
		public static int y = Console.CursorTop;

		public unsafe static void SWI(ref IRQContext aContext)
		{
			if (aContext.Interrupt == 0x80) //Interrupt.
            {
                Console.WriteLine("0x80");
                if (aContext.EAX == 1) //sys_exit
                {
                    Console.WriteLine("sys_exit");
                }
                else if (aContext.EAX == 4) //sys_write
                {
                    Console.WriteLine("sys_write");
                    if (aContext.EBX == 1) //stdout
                    {
                        Console.WriteLine("stdout");
                        uint maxBytes = aContext.EDX;
                        uint ptr = aContext.ECX;
                        byte* dat = (byte*)ptr;

                        for (int i = 0; dat[i] != 0 && i < maxBytes; i++)
                        {
                            if ((char)dat[i] == 0x0A)
                                Console.WriteLine("\n");
                            else
                                Console.Write((char)dat[i]);
                        }
                    }
                }
			}
		}
	}
}
