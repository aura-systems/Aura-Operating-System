using System;
using Aura_OS.HAL;
using static Cosmos.Core.INTs;
using System.Collections.Generic;
using XSharp;

namespace Aura_OS.Core
{
    class MSDOS_Syscalls : Driver
    {
        public override bool Init()
        {
            Name = "MSDOS API";
            SetIntHandler(0x48, SWI); //ints.setinthandler
            return true;
        }

        public static int x = Console.CursorLeft;
        public static int y = Console.CursorTop;

        public unsafe static void SWI(ref IRQContext aContext)
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
                            Console.WriteLine("\n");
                        else
                            Console.Write((char)dat[i]);
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


                    uint ptr2 = aContext.EDI;
                    byte* dat2 = (byte*)(ptr + System.Executable.COM.ProgramAddress);

                    //Console.WriteLine(aContext.ESI);
                    //Console.WriteLine((uint)dat);

                    //XS.Set(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: 8);

                    List<byte> list = new List<byte>();

                    foreach (char charr in output)
                    {
                        list.Add(System.Utils.Convert.StringToByte(charr));
                    }

                    byte[] test = list.ToArray();

                    for (int i = 0; i < test.Length; i++)
                    {
                        //ptr2[i] = code[i];
                        dat2[i] = test[i];
                    }

                    aContext.EDI = (uint)dat2 - System.Executable.COM.ProgramAddress;

                    //Console.WriteLine(aContext.EDI);
                    //Console.WriteLine((uint)dat2);

                    //aContext.ESI = aContext.ESI 

                    //for (int i = 0; test[i] != 0; i++)
                    //{
                    //    aContext.EDI = aContext.EDI + test[i];
                    //}

                    //aContext.EDI = (uint)BitConverter.ToInt32(list.ToArray(), 0);


                    // byte[] test = list.ToArray();
                    //
                    //for (int i = 0; test[i] != 0; i++)
                    //{
                    //    aContext.EDI = aContext.EDI + test[i];
                    // }


                    //aContext.EDI = aContext.ESI;

                    //System.Utils.Convert.StringToByte();

                    //Console.WriteLine(ptr1);

                    //byte* dat1 = (byte*)(ptr1 + System.Executable.COM.ProgramAddress);
                    //for (int i = 0; dat1[i] != 0; i++)
                    //{
                    //    Console.Write((char)dat1[i]);
                    //}

                }
            }

        }
    }
}
