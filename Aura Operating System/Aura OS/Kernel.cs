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
#endregion

namespace Aura_OS
{
    public class Kernel : Sys.Kernel
    {

        #region Global variables

        Setup setup = new Setup();
        public static bool running;
        public static string version = "0.2";
        public static string revision = "100820171748";
        public static string current_directory = @"0:\";
        public static string langSelected = "en_US";
        public static CosmosVFS FS { get; private set; }
        public static string userLogged;
        public static string userLevelLogged;
        public static bool Logged = false;
        public static string ComputerName = "Aura-PC";
        public static int color;
        public static string RootContent;

        #endregion

        #region Before Run

        protected override void BeforeRun()
        {

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
}
