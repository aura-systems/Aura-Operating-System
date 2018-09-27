using System;
using static Cosmos.Core.INTs;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Executables.Drivers.Syscalls
{
	class AuraAPI
	{
		public static bool Init()
		{
			SetIntHandler(0x48, SWI); //ints.setinthandler
			return true;
		}

		public static int x = Console.CursorLeft;
		public static int y = Console.CursorTop;

		public unsafe static void SWI(ref IRQContext aContext)
		{
			if (aContext.Interrupt == 0x48) //Interrupt.
			{
			    Console.WriteLine("EAX=" + CosmosELFCore.Invoker.eax);
                if (aContext.EAX == 1) //sys_exit
                {
                    Console.WriteLine("sys_exit");
                }
                else if (aContext.EAX == 4) //sys_write
                {
                    Console.WriteLine("sys_write");
                    if (aContext.EBX == 1) //stdout
                    {
                        Console.WriteLine("sys_exit");
                        uint maxBytes = aContext.EDX;
                        uint ptr = aContext.ECX;
                        byte* dat = (byte*)(ptr + Executables.PlainBinaryProgram.ProgramAddress); //

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
