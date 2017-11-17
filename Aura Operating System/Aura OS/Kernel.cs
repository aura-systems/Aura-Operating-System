/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Kernel
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

#region using;

using System;
using Cosmos.System.FileSystem;
using Sys = Cosmos.System;
using Lang = Aura_OS.System.Translation;
using Aura_OS.System;
using Aura_OS.System.Users;
using Aura_OS.System.Computer;
using Aura_OS.System.Utils;
using System.Collections.Generic;
using XSharp.Assembler;
using Cosmos.IL2CPU.API.Attribs;
using XSharp;

#endregion

namespace Aura_OS
{
    public class Kernel : Sys.Kernel
    {

        #region Global variables

        Setup setup = new Setup();
        public static bool running;
        public static string version = "0.4.1";
        public static string revision = "011020171159";
        public static string current_directory = @"0:\";
        public static string langSelected = "en_US";
        public static CosmosVFS FS { get; private set; }
        public static string userLogged;
        public static string userLevelLogged;
        public static bool Logged = false;
        public static string ComputerName = "aura-pc";
        public static int color = 7;
        public static string RootContent;
        public static string UserDir = @"0:\Users\" + userLogged + "\\";
        public static bool SystemExists = false;
        public static bool JustInstalled = false;
        public static List<HAL.Driver> Drivers = new List<HAL.Driver>();

        #endregion

        #region Before Run

        protected override void BeforeRun()
        {

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write("Booting Aura...\n");
            Console.ForegroundColor = ConsoleColor.White;

            #region FileSystem Init

            FS = new CosmosVFS();
            FS.Initialize();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[OK]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" ");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write("FileSystem Initialized\n");
            Console.ForegroundColor = ConsoleColor.White;

            #endregion

            #region FileSystem Scan
            Sys.FileSystem.VFS.VFSManager.RegisterVFS(FS);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[OK]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" ");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write("FileSystem Scanned\n");
            Console.ForegroundColor = ConsoleColor.White;

            #endregion

            setup.InitSetup();

            if (SystemExists)
            {
                if (!JustInstalled)
                {

                    Settings.LoadValues();
                  
                    langSelected = Settings.GetValue("language");

                    #region Language

                    Lang.Keyboard.Init();

                    #endregion

                    Info.getComputerName();

                    #region Drivers
                    
                    Core.Aura_Syscalls aura_syscalls = new Core.Aura_Syscalls(); //Aura API
                    Core.MSDOS_Syscalls msdos_syscalls = new Core.MSDOS_Syscalls(); //MSDOS API

                    for (int i = 0; i < Drivers.Count; i++)
                    {
                    if (Drivers[i].Init())
                        Console.WriteLine("Loading '" + Drivers[i].Name + "' loaded sucessfully");
                    else
                        Console.WriteLine("Failure loading module '" + Drivers[i].Name + "'");
                    }
                  
                    #endregion
                  
                    running = true;

                }
            }
            else
            {
                running = true;
            }
        }

        #endregion

        #region Run

        protected override void Run()
        {
            try
            {
                while (running)
                {
                    if (Logged) //If logged
                    {
                        BeforeCommand();                  

                        var cmd = Console.ReadLine();
                        Shell.cmdIntr.CommandManager._CommandManger(cmd);
                        Console.WriteLine();
                    }
                    else
                    {
                        Login.LoginForm();
                    }
                }
            }
            catch (Exception ex)
            {
                running = false;
                Crash.StopKernel(ex);
            }
        }

        #endregion

        #region BeforeCommand
        /// <summary>
        /// Display the line before the user input and set the console color.
        /// </summary>
        private static void BeforeCommand()
        {
            if (current_directory == @"0:\")
            {

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(UserLevel.TypeUser());

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(userLogged);

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("@");

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(ComputerName);

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("> ");

                if (color == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                else if (color == 1)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                }
                else if (color == 2)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else if (color == 3)
                {
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                }
                else if (color == 4)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else if (color == 5)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                }
                else if (color == 6)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else if (color == 7)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(UserLevel.TypeUser());

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(userLogged);

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("@");

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(ComputerName);

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("> ");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(current_directory + "~ ");

                if (color == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                else if (color == 1)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                }
                else if (color == 2)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else if (color == 3)
                {
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                }
                else if (color == 4)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else if (color == 5)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                }
                else if (color == 6)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else if (color == 7)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        } 
        #endregion

    }

    public static class exitUtils
    {
        [PlugMethod(Assembler = typeof(exitUtilsPlug))]
        public static void Vs8086Mode() { }
    }
    [Plug(Target = typeof(exitUtils))]
    public class exitUtilsPlug : AssemblerMethod// : PlugMethod // : Method
    {
        // public override void AssembleNew(object aAssembler, object aMethodInfo)
        // {
        //   XS.Set(XSRegisters.EBX, false, false, 8, true, null XSRegisters.RegisterSize.Byte8);
        // XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 8);
        //  XS.Call(XSRegisters.EAX);
        // }

        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {//I asked jp2masa
            XS.LiteralCode("[bits 16]");

            XS.LiteralCode("idt_real:");
            XS.LiteralCode("dw 0x3ff"); //; 256 entries, 4b each = 1K
            XS.LiteralCode("dd 0"); //; Real Mode IVT @ 0x0000

            XS.LiteralCode("savcr0:");
            XS.LiteralCode("dd 0"); //; Storage location for pmode CR0.

            XS.LiteralCode("Entry16:");
            //  ; We are already in 16-bit mode here!

            XS.LiteralCode("cli"); //; Disable interrupts.

            //; Need 16-bit Protected Mode GDT entries!
            XS.LiteralCode("mov eax, DATASEL16"); //; 16-bit Protected Mode data selector.
            XS.LiteralCode("mov ds, eax");
            XS.LiteralCode("mov es, eax");
            XS.LiteralCode("mov fs, eax");
            XS.LiteralCode("mov gs, eax");
            XS.LiteralCode("mov ss, eax");

            //; Disable paging (we need everything to be 1:1 mapped).
            XS.LiteralCode("mov eax, cr0");
            XS.LiteralCode("mov [savcr0], eax"); //; save pmode CR0
            XS.LiteralCode("and eax, 0x7FFFFFFe"); //; Disable paging bit & disable 16-bit pmode.
            XS.LiteralCode("mov cr0, eax");

            XS.LiteralCode("jmp 0:GoRMode"); //; Perform Far jump to set CS.

            XS.LiteralCode("GoRMode:");
            XS.LiteralCode("mov sp, 0x8000"); //; pick a stack pointer.
            XS.LiteralCode("mov ax, 0"); //; Reset segment registers to 0.
            XS.LiteralCode("mov ds, ax");
            XS.LiteralCode("mov es, ax");
            XS.LiteralCode("mov fs, ax");
            XS.LiteralCode("mov gs, ax");
            XS.LiteralCode("mov ss, ax");
            XS.LiteralCode("lidt [idt_real]");
            XS.LiteralCode("sti"); //; Restore interrupts -- be careful, unhandled int's will kill it.
        }
    }
}
