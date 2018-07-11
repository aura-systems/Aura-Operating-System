/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Settings
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;
using L = Aura_OS.System.Translation;
using Aura_OS.System.Computer;
using Aura_OS.System.Utils;

namespace Aura_OS.Shell.cmdIntr.Tools
{
    class Debug
    {
        private static string HelpInfo = "";

        /// <summary>
        /// Getter and Setters for Help Info.
        /// </summary>
        public static string HI
        {
            get { return HelpInfo; }
            set { HelpInfo = value; /*PUSHED OUT VALUE (in)*/}
        }

        /// <summary>
        /// Empty constructor. (Good for debug)
        /// </summary>
        public Debug() { }

        /// <summary>
        /// c = command, c_Settings
        /// </summary>
        public static void c_Debug()
        {
            L.Text.Display("availabledebugcmd");
        }

        public static void c_Debug(string settings)
        {
            Char separator = ' ';
            string[] cmdargs = settings.Split(separator);

            if (cmdargs.Length == 2) //No arg
            {
                if (cmdargs[1].Equals("vbeinfo"))
                {
                    Console.WriteLine("[VBE Mode Information]");
                    Console.WriteLine("BPP: " + System.Shell.VESAVBE.Graphics.depthVESA);
                    Console.WriteLine("Height: " + System.Shell.VESAVBE.Graphics.heightVESA);
                    Console.WriteLine("Width: " + System.Shell.VESAVBE.Graphics.widthVESA);
                    Console.WriteLine("VBE Pointer: 0x" + Conversion.DecToHex((int)System.Shell.VESAVBE.Graphics.vbepointer));
                    Console.WriteLine("VBE Mode: " + System.Shell.VESAVBE.Graphics.VESAMode);
                    Console.WriteLine("[VBE Controller Information]");
                    Console.WriteLine("VBE Version: " + System.Shell.VESAVBE.Graphics.sversion);
                    Console.WriteLine("VBE Signature: " + System.Shell.VESAVBE.Graphics.ssignature);
                    Console.WriteLine("OEM String Pointer: 0x" + Conversion.DecToHex((int)System.Shell.VESAVBE.Graphics.oemStringPtr));
                    Console.WriteLine("Capabilites: " + System.Shell.VESAVBE.Graphics.capabilities);
                    Console.WriteLine("Video Mode Pointer: 0x" + Conversion.DecToHex((int)System.Shell.VESAVBE.Graphics.videoModePtr));
                    Console.WriteLine("Total Memory: " + System.Shell.VESAVBE.Graphics.totalmemory);

                    Console.WriteLine("OEM Software Rev: " + System.Shell.VESAVBE.Graphics.oemSoftwareRev);
                    Console.WriteLine("OEM Vendor Name Pointer: 0x" + Conversion.DecToHex((int)System.Shell.VESAVBE.Graphics.oemVendorNamePtr));
                    Console.WriteLine("OEM Product Name Pointer: 0x" + Conversion.DecToHex((int)System.Shell.VESAVBE.Graphics.oemProductNamePtr));
                    //Console.WriteLine("OEM Product Name: " + System.Shell.VESAVBE.Graphics.ProductName);
                    Console.WriteLine("OEM Product Rev Pointer: 0x" + Conversion.DecToHex((int)System.Shell.VESAVBE.Graphics.oemProductRevPtr));
                }

                else if (cmdargs[1].Equals("vbemodes"))
                {
                    Console.WriteLine("[VBE Mode List]");
                    int counter = 0;
                    foreach (uint mode in System.Shell.VESAVBE.Graphics.modelist)
                    {
                        counter++;
                        if (counter == 19)
                        {
                            Console.ReadKey();
                            counter = 0;
                        }
                        switch (mode)
                        {
                            case 0x100:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 640x400x256");
                                break;
                            case 0x101:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 640x480x256");
                                break;
                            case 0x102:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 800x600x16");
                                break;
                            case 0x103:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 800x600x256");
                                break;
                            case 0x104:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1024x768x16");
                                break;
                            case 0x105:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1024x768x256");
                                break;
                            case 0x106:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1280x1024x16");
                                break;
                            case 0x107:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1280x1024x256");
                                break;
                            case 0x108:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 80x60 text");
                                break;
                            case 0x109:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 132x25 text");
                                break;
                            case 0x10A:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 132x43 text");
                                break;
                            case 0x10B:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 132x50 text");
                                break;
                            case 0x10C:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 132x60 text");
                                break;

                            //VBE v1.2+
                            case 0x10D:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 320x200 32k");
                                break;
                            case 0x10E:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 320x200 64k");
                                break;
                            case 0x10F:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 320x200 16M");
                                break;
                            case 0x110:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 640x480 32k");
                                break;
                            case 0x111:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 640x480 64k");
                                break;
                            case 0x112:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 640x480 16M");
                                break;
                            case 0x113:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 800x600 32k");
                                break;
                            case 0x114:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 800x600 64k");
                                break;
                            case 0x115:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 800x600 16M");
                                break;
                            case 0x116:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1024x768 32k");
                                break;
                            case 0x117:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1024x768 64k");
                                break;
                            case 0x118:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1024x768 16M");
                                break;
                            case 0x119:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1280x1024 32k");
                                break;
                            case 0x11A:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1280x1024 64k");
                                break;
                            case 0x11B:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1280x1024 16M");
                                break;

                            case 0x81FF:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - Special Mode");
                                break;

                            default:
                                Console.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - Unkown mode");
                                break;
                        }
                    }
                }
                else
                {
                    L.Text.Display("UnknownCommand");
                    L.Text.Display("availabledebugcmd");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                L.Text.Display("UnknownCommand");
                Console.ForegroundColor = ConsoleColor.White;
            }

        }
    }
}
