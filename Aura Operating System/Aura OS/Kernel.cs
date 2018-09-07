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
using System.Text;
using Cosmos.System.ExtendedASCII;
using Aura_OS.Core;
using Aura_OS.Apps.System;
using Aura_OS.System.Network.IPV4;
using Cosmos.HAL;

#endregion

namespace Aura_OS
{
    public class Kernel : Sys.Kernel
    {

        #region Global variables

        Setup setup = new Setup();
        public static bool running;
        public static string version = "0.4.5";
        public static string revision = "280820180021";
        public static string current_directory = @"0:\";
        public static string langSelected = "en_US";
        public static string userLogged;
        public static string userLevelLogged;
        public static bool Logged = false;
        public static string ComputerName = "aura-pc";
        public static string UserDir = @"0:\Users\" + userLogged + "\\";
        public static bool SystemExists = false;
        public static bool JustInstalled = false;
        public static CosmosVFS vFS = new CosmosVFS();
		public static Dictionary<string, string> environmentvariables = new Dictionary<string, string>();
        //public static HAL.PCSpeaker speaker = new HAL.PCSpeaker();
        public static string boottime = Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true);
        public static System.Shell.Console AConsole;
        public static string Consolemode = "VGATextmode";
        public static MemoryManager MemMon;

        public static Debugger debugger = new Debugger(new Address(192, 168, 1, 12), 4224);

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
                Shell.cmdIntr.CommandManager.RegisterAllCommands();

                AConsole = new System.Shell.VGA.VGAConsole(null);

                Encoding.RegisterProvider(CosmosEncodingProvider.Instance);
                Console.InputEncoding = Encoding.GetEncoding(437);
                Console.OutputEncoding = Encoding.GetEncoding(437);

                System.CustomConsole.WriteLineInfo("Booting Aura Operating System...");

                System.CustomConsole.WriteLineInfo("VBE Informations:");
                System.CustomConsole.WriteLineInfo("VBE Version: " + System.Shell.VESAVBE.Graphics.sversion);
                System.CustomConsole.WriteLineInfo("VBE Signature: " + System.Shell.VESAVBE.Graphics.ssignature);
                System.CustomConsole.WriteLineInfo("BPP: " + System.Shell.VESAVBE.Graphics.depthVESA);
                System.CustomConsole.WriteLineInfo("Height: " + System.Shell.VESAVBE.Graphics.heightVESA);
                System.CustomConsole.WriteLineInfo("Width: " + System.Shell.VESAVBE.Graphics.widthVESA);

                #region Register Filesystem
                Sys.FileSystem.VFS.VFSManager.RegisterVFS(vFS);
                if (ContainsVolumes())
                {
                    System.CustomConsole.WriteLineOK("FileSystem Registration");
                }
                else
                {
                    System.CustomConsole.WriteLineError("FileSystem Registration");
                }
                #endregion

                NetworkInit.Init();

                Console.ReadKey();

                System.CustomConsole.WriteLineOK("Aura successfully started!");

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

                boottime = Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true);

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

                //Sys.Thread TBAR = new Sys.Thread(TaskBar);

                while (running)
                {
                    if (Logged) //If logged
                    {
                        //TBAR.Start();
                        BeforeCommand();

                        Sys.Console.writecommand = true;

                        var cmd = Console.ReadLine();
                        //TBAR.Stop();
                        Shell.cmdIntr.CommandManager._CommandManger(cmd);
                        //Console.WriteLine();

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

        int _deltaT = 0;

        private void TaskBar()
        {

            int oldx = 0;
            int oldy = 0;

            while (true)
            {
                if (_deltaT != RTC.Second)
                {
                    oldx = AConsole.X;
                    oldy = AConsole.Y;
                    _deltaT = RTC.Second;
                    AConsole.X = AConsole.Width - 8;
                    AConsole.Y = 0;
                    AConsole.Write(Encoding.ASCII.GetBytes(Time.TimeString(true, true, true)));
                    AConsole.X = oldx;
                    AConsole.Y = oldy;
                }
            }
            
        }

        #endregion

        #region BeforeCommand
        /// <summary>
        /// Display the line before the user input and set the console color.
        /// </summary>
        public static void BeforeCommand()
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

                Console.ForegroundColor = ConsoleColor.White;
                
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

                Console.ForegroundColor = ConsoleColor.White;

            }
        } 
        #endregion

    }
}
