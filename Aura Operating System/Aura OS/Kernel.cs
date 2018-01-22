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

#endregion

namespace Aura_OS
{
    public class Kernel : Sys.Kernel
    {

        #region Global variables

        Setup setup = new Setup();
        public static bool running;
        public static string version = "0.4.3";
        public static string revision = "050120182138";
        public static string current_directory = @"0:\";
        public static string langSelected = "en_US";
        public static string userLogged;
        public static string userLevelLogged;
        public static bool Logged = false;
        public static string ComputerName = "aura-pc";
        public static int color = 7;
        public static string RootContent;
        public static string UserDir = @"0:\Users\" + userLogged + "\\";
        public static bool SystemExists = false;
        public static bool JustInstalled = false;
        public static CosmosVFS vFS = new CosmosVFS();
		public static bool Safemode = true;

        #endregion

        #region Before Run

        public static bool ContainsVolumes()
        {
            var vols = vFS.GetVolumes();
            foreach (var vol in vols)
            {
                return true;
            }
            return false;
        }

        protected override void BeforeRun()
        {
            try
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write("Booting Aura...\n");
                Console.ForegroundColor = ConsoleColor.White;

                #region Register Filesystem
                Sys.FileSystem.VFS.VFSManager.RegisterVFS(vFS);
                if (ContainsVolumes())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[OK]");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" ");
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.Write("FileSystem Registration\n");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("[Error]");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" ");
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.Write("FileSystem Registration\n");
                    Console.ForegroundColor = ConsoleColor.White;
                }
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

                        running = true;

                    }
                }
                else
                {
                    running = true;
                }
            }
            catch (Exception ex)
            {
                running = false;
                Crash.StopKernel(ex);
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
}
