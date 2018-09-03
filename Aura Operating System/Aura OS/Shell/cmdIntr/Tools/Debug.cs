/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Debug command (For now only VBE informations)
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using L = Aura_OS.System.Translation;
using Aura_OS.System.Computer;
using Aura_OS.System.Utils;
using Aura_OS.Apps.System;

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

            string cmd = settings.Remove(0, 6);

            if (cmd.StartsWith("debugger "))
            {
                Kernel.debugger.Send(cmd.Remove(0, 9));
            }
            else if (cmdargs.Length == 2) //No arg
            {
                if (cmdargs[1].Equals("vbeinfo"))
                {
                    DebugConsole.WriteLine("[VBE Mode Information]");
                    DebugConsole.WriteLine("BPP: " + System.Shell.VESAVBE.Graphics.ModeInfo.bpp);
                    DebugConsole.WriteLine("Height: " + System.Shell.VESAVBE.Graphics.ModeInfo.height);
                    DebugConsole.WriteLine("Width: " + System.Shell.VESAVBE.Graphics.ModeInfo.width);
                    DebugConsole.WriteLine("VBE Pointer: 0x" + Conversion.DecToHex((int)System.Shell.VESAVBE.Graphics.ModeInfo.framebuffer));
                    DebugConsole.WriteLine("VBE Mode: " + System.Shell.VESAVBE.Graphics.VESAMode);

                    DebugConsole.WriteLine();

                    DebugConsole.WriteLine("[VBE Controller Information]");
                    
                    DebugConsole.WriteLine("VBE Signature: " + System.Shell.VESAVBE.Graphics.VBESignature);

                    DebugConsole.WriteLine("VBE Version: " + System.Shell.VESAVBE.Graphics.VBEVersion);

                    DebugConsole.WriteLine("OEM String: " + System.Shell.VESAVBE.Graphics.VBEOEM);

                    DebugConsole.WriteLine("Capabilites: " + System.Shell.VESAVBE.Graphics.ControllerInfo.capabilities);

                    DebugConsole.WriteLine("Total Memory: " + (System.Shell.VESAVBE.Graphics.ControllerInfo.totalmemory * 64) + " kB");

                    DebugConsole.WriteLine("OEM Software Rev: " + System.Shell.VESAVBE.Graphics.ControllerInfo.oemSoftwareRev);

                    DebugConsole.WriteLine("Video RAM Pointer: 0x" + Conversion.DecToHex((int)System.Shell.VESAVBE.Graphics.ModeInfo.framebuffer));

                    DebugConsole.WriteLine("OEM String Pointer: 0x" + Conversion.DecToHex((int)System.Shell.VESAVBE.Graphics.ControllerInfo.oemStringPtr));

                    DebugConsole.WriteLine("OEM Vendor Name Pointer: 0x" + Conversion.DecToHex((int)System.Shell.VESAVBE.Graphics.ControllerInfo.oemVendorNamePtr));

                    DebugConsole.WriteLine("OEM Product Name Pointer: 0x" + Conversion.DecToHex((int)System.Shell.VESAVBE.Graphics.ControllerInfo.oemProductNamePtr));

                    DebugConsole.WriteLine("OEM Product Rev Pointer: 0x" + Conversion.DecToHex((int)System.Shell.VESAVBE.Graphics.ControllerInfo.oemProductRevPtr));
                }

                else if (cmdargs[1].Equals("vbemodes"))
                {
                    DebugConsole.WriteLine("[VBE Mode List]");
                    int counter = 0;
                    foreach (uint mode in System.Shell.VESAVBE.Graphics.Modes)
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
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 640x400x256");
                                break;
                            case 0x101:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 640x480x256");
                                break;
                            case 0x102:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 800x600x16");
                                break;
                            case 0x103:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 800x600x256");
                                break;
                            case 0x104:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1024x768x16");
                                break;
                            case 0x105:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1024x768x256");
                                break;
                            case 0x106:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1280x1024x16");
                                break;
                            case 0x107:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1280x1024x256");
                                break;
                            case 0x108:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 80x60 text");
                                break;
                            case 0x109:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 132x25 text");
                                break;
                            case 0x10A:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 132x43 text");
                                break;
                            case 0x10B:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 132x50 text");
                                break;
                            case 0x10C:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 132x60 text");
                                break;

                            //VBE v1.2+
                            case 0x10D:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 320x200 32k");
                                break;
                            case 0x10E:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 320x200 64k");
                                break;
                            case 0x10F:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 320x200 16M");
                                break;
                            case 0x110:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 640x480 32k");
                                break;
                            case 0x111:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 640x480 64k");
                                break;
                            case 0x112:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 640x480 16M");
                                break;
                            case 0x113:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 800x600 32k");
                                break;
                            case 0x114:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 800x600 64k");
                                break;
                            case 0x115:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 800x600 16M");
                                break;
                            case 0x116:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1024x768 32k");
                                break;
                            case 0x117:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1024x768 64k");
                                break;
                            case 0x118:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1024x768 16M");
                                break;
                            case 0x119:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1280x1024 32k");
                                break;
                            case 0x11A:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1280x1024 64k");
                                break;
                            case 0x11B:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - 1280x1024 16M");
                                break;

                            case 0x81FF:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - Special Mode");
                                break;

                            default:
                                DebugConsole.WriteLine("Mode: 0x" + Conversion.DecToHex((int)mode) + " - Unkown mode");
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
