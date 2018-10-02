using System;
using static Cosmos.Core.INTs;
using System.Collections.Generic;
using System.Text;
using Cosmos.Core;

namespace Aura_OS.System.Executables.Drivers.Syscalls
{
	class AuraAPI
	{
	    public static ManagedMemoryBlock sharedmemory;

		public static bool Init()
		{
			SetIntHandler(0x48, SWI); //ints.setinthandler
		    sharedmemory = new ManagedMemoryBlock(2048);

            return true;
		}

		public static int x = Console.CursorLeft;
		public static int y = Console.CursorTop;

	    enum SystemCalls
	    {
            SYS_OPEN = 0,
            SYS_EXIT = 1,
	        SYS_READ = 3,
            SYS_WRITE = 4
	    }

		public unsafe static void SWI(ref IRQContext aContext)
		{
			if (aContext.Interrupt == 0x48) //Interrupt.
			{

			    switch (aContext.EAX)
			    {

			        case (uint)SystemCalls.SYS_OPEN:
			            Console.WriteLine("sys_open");
			            Console.WriteLine("Offset= " + sharedmemory.Offset);
                        aContext.ECX = sharedmemory.Offset;
                        break;

                    case (uint)SystemCalls.SYS_EXIT:
                        Console.WriteLine("sys_exist");
                        for (uint i = 0; i < sharedmemory.Size; i++)
                        {
                            sharedmemory[i] = 0;
                        }
                        break;

			        case (uint)SystemCalls.SYS_READ:
			            Console.WriteLine("sys_read");
			            if (aContext.EBX == 0) //stdin
			            {
			                //Console.WriteLine("stdin");
			                uint Lenght = aContext.EDX;
			                uint ptr = aContext.ECX;
                            byte* data = (byte*)(ptr);

                            for (int i = 0; i < Lenght; i++)
			                {
			                    ConsoleKeyInfo info = Console.ReadKey();
			                    data[i] = (byte)info.KeyChar;
                                Console.WriteLine();
			                }

			                aContext.ECX = (uint)data;

                        }
			            break;

                    case (uint)SystemCalls.SYS_WRITE:
                        Console.WriteLine("sys_write");
                        if (aContext.EBX == 1) //stdout
                        {
                            Console.WriteLine("stdout");
                            uint Lenght = aContext.EDX;
                            Console.WriteLine("Lenght= " + Lenght);

                            uint ptr = aContext.ECX;
                            byte* dat = (byte*)(ptr + PlainBinaryProgram.ProgramAddress);

                            for (uint i = 0; i < Lenght && dat[i] != 0; i++)
                            {
                                if ((char)dat[i] == 0x0A)
                                    Console.Write('\n');
                                else
                                    Console.Write((char)dat[i]);
                            }
                        }
                        break;

                    default:
                        Console.WriteLine("Unknown SystemCall!");
                        Console.WriteLine("EAX= " + aContext.EAX);
                        break;

			    }

			}
		}
	}
}
