using System;
using Aura_OS.HAL;

namespace Aura_OS.Core
{
    class MSDOS_Syscalls : Driver
    {
        public override bool Init()
        {
            this.Name = "MSDOS API";
            Cosmos.Core.INTs.SetIntHandler(0x48, SWI);
            return true;
        }

        public static int x = Console.CursorLeft;
        public static int y = Console.CursorTop;
        static int result;

        public unsafe static void SWI(ref Cosmos.Core.INTs.IRQContext aContext)
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
                        if ((char)dat[i] == 0x0A)
                        {
                            Console.Write("\n");
                        }
                        else
                        {
                            Console.Write((char)dat[i]);
                        }
                    }
                }
                else if (aContext.EAX == 0x02)
                {
                    Console.Clear();
                }
                else if (aContext.EAX == 0x03)
                {
                    uint xesi = aContext.ESI;
                    uint yedi = aContext.EDI;
                    Console.SetCursorPosition((int)xesi, (int)yedi);
                }
                else if (aContext.EAX == 0x31)
                {
                    Console.SetCursorPosition(x, y);
                }
                else if (aContext.EAX == 0x04) // Read from line
                {
                    uint ptr = aContext.ESI;
                    byte* dat = (byte*)(ptr + System.Executable.COM.ProgramAddress);

                    string input = "";

                    for (int i = 0; dat[i] != 0; i++)
                    {
                        input = input + (char)dat[i];
                    }

                    Console.Write(input);
                    string output = Console.ReadLine();

                    uint ramaddress = aContext.EDI + System.Executable.COM.ProgramAddress;
                    
                    unsafe
                    {
                        byte* data = (byte*)(ramaddress);
                        for (int i = 0; data[i] != 0; i++)
                        {
                            result = result + (byte)output[i];
                        }
                    }

                    aContext.EDI = (uint)result;

                    uint ptr1 = aContext.EDI;
                    byte* dat1 = (byte*)(ptr1 + System.Executable.COM.ProgramAddress);
                    for (int i = 0; dat1[i] != 0; i++)
                    {
                        if ((char)dat1[i] == 0x0A)
                        {
                            Console.Write("\n");
                        }
                        else
                        {
                            Console.Write((char)dat1[i]);
                        }
                    }

                }
            }
        }
    }
}
