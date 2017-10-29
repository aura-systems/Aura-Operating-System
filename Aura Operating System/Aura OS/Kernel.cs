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
using System.IO;
using Aura_OS.System.Users;
using Aura_OS.System.Computer;
using XSharp;
using XSharp.Assembler;
#endregion

namespace Aura_OS
{

    public class Kernel : Sys.Kernel
    {
        static exitUtils eu = new exitUtils();

        #region Global variables

        Setup setup = new Setup();
        public static bool running;
        public static string version = "0.3.1";
        public static string revision = "280920171000";
        public static string current_directory = @"0:\";
        public static string langSelected = "en_US";
        public static CosmosVFS FS { get; private set; }
        public static string userLogged;
        public static string userLevelLogged;
        public static bool Logged = false;
        public static string ComputerName = "Aura-PC";
        public static int color;
        public static string RootContent;
        public static string UserDir = @"0:\Users\" + userLogged + "\\";

        #endregion

        #region Before Run

        protected override void BeforeRun()
        {
            exitUtils.Vs8086Mode();
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[OK]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" ");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write("Aura Kernel Booted Successfully!\n");
            Console.ForegroundColor = ConsoleColor.White;

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

            setup.SetupVerifyCompleted();

            langSelected = File.ReadAllText(@"0:\System\lang.set");

            #region Language

            Lang.Keyboard.Init();

            #endregion

            RootContent = File.ReadAllText(@"0:\System\Users\root.usr");

            Info.getComputerName();

            Color.GetBackgroundColor();

            color = Color.GetTextColor();

            running = true;
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
                        Users user = new Users();
                        user.Login();
                    }
                }
            }
            catch (Exception ex)
            {
                running = false;
                Crash.StopKernel(ex);
            }
        }

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


    public class exitUtils
    {
        public static void Vs8086Mode()
        {
            //; you should declare this function as :
            //  ; extern void entering_v86(uint32_t ss, uint32_t esp, uint32_t cs, uint32_t eip);
            //  entering_v86:
            //   XS.LiteralCode("entering_v86:");
            //  XS.LiteralCode("mov epb, esp"); //save stack pointer

            //   XS.LiteralCode("push dword [ebp+4]"); //ss
            //  XS.LiteralCode("push dword [ebp+8]"); //esp
            // XS.LiteralCode("pushfd"); //eflags
            // XS.LiteralCode("or dword [esp], (1 << 17)"); //set vm flags
            //  XS.LiteralCode("push dword [ebp+12]"); //cs
            //  XS.LiteralCode("push dword [ebp+16]"); //eip
            //   XS.LiteralCode("iret");

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
            XS.LiteralCode("move eax, cr0");
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
