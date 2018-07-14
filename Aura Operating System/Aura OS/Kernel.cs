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
using Cosmos.Debug.Kernel;
using Aura_OS.Core;

#endregion

namespace Aura_OS
{
    public class Kernel : Sys.Kernel
    {

        #region Global variables

        Setup setup = new Setup();
        public static bool running;
        public static string version = "0.4.5";
        public static string revision = "030620182120";
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
        public static HAL.PCSpeaker speaker = new HAL.PCSpeaker();
        public static string boottime = Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true);
        public static System.Shell.Console AConsole;
        public static string Consolemode = "VGATextmode";
        public static MemoryManager MemMon;
        public static Debugger debugger = new Debugger("Aura Operating System", "Kernel");

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

                //AConsole = new System.Shell.VGA.VGAConsole(null);

                Encoding.RegisterProvider(CosmosEncodingProvider.Instance);
                Console.InputEncoding = Encoding.GetEncoding(437);
                Console.OutputEncoding = Encoding.GetEncoding(437);

                CustomConsole.WriteLineInfo("Booting Aura Operating System...");

                CustomConsole.WriteLineInfo("VBE Informations:");
                CustomConsole.WriteLineInfo("VBE Version: " + System.Shell.VESAVBE.Graphics.sversion);
                CustomConsole.WriteLineInfo("VBE Signature: " + System.Shell.VESAVBE.Graphics.ssignature);
                CustomConsole.WriteLineInfo("BPP: " + System.Shell.VESAVBE.Graphics.depthVESA);
                CustomConsole.WriteLineInfo("Height: " + System.Shell.VESAVBE.Graphics.heightVESA);
                CustomConsole.WriteLineInfo("Width: " + System.Shell.VESAVBE.Graphics.widthVESA);

                #region Register Filesystem
                Sys.FileSystem.VFS.VFSManager.RegisterVFS(vFS);
                if (ContainsVolumes())
                {
                    CustomConsole.WriteLineOK("FileSystem Registration");
                }
                else
                {
                    CustomConsole.WriteLineError("FileSystem Registration");
                }
                #endregion

                NetworkInit.Init();
              
                CustomConsole.WriteLineOK("Aura successfully started!");

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
                while (running)
                {
                    if (Logged) //If logged
                    {
                        BeforeCommand();

                        Sys.Console.writecommand = true;

                        var cmd = Console.ReadLine();
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
