/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Kernel
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

#region using;

using System;
using Cosmos.System.FileSystem;
using Sys = Cosmos.System;
using Lang = Alve_OS.System.Translation;
using Alve_OS.System;
using System.IO;
using Alve_OS.System.Users;


#endregion

namespace Alve_OS
{
    public class Kernel: Sys.Kernel
    {

        #region Global variables

        Setup setup = new Setup();
        public static bool running;
        public static string version = "0.1";
        public static string revision = "030820171425";
        public static string current_directory = @"0:\";
        public static string langSelected = "en_US";
        public static CosmosVFS FS { get; private set; }
        public static string userLogged;
        public static string userLevelLogged;
        public static bool Logged = false;
        public static string computerName = "Alve-PC";

        #endregion

        #region Before Run

        protected override void BeforeRun()
        {

            #region FileSystem Init
            Console.WriteLine("Initializing FileSystem...");
            FS = new CosmosVFS();
            FS.Initialize();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[OK]");
            Console.ForegroundColor = ConsoleColor.White;
            #endregion

            #region FileSystem Scan
            Console.WriteLine("Scanning FileSystem...");
            Sys.FileSystem.VFS.VFSManager.RegisterVFS(FS);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[OK]");
            Console.ForegroundColor = ConsoleColor.White;
            #endregion

            //setup.SetupVerifyCompleted();

            //langSelected = File.ReadAllText(@"0:\System\lang");

            #region Language
            //Lang.Keyboard.Init();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[OK]");
            Console.ForegroundColor = ConsoleColor.White;
            #endregion

            Console.Clear();

            //WelcomeMessage.Display();

            running = true;
        }

        #endregion

        #region Run

        protected override void Run()
        {
            try
            {

                //if (Logged) //si loggé
                //{
                    //LOGGED
                    Console.Write(UserLevel.TypeUser() + userLogged + "~ " + current_directory + "> ");
                    var cmd = Console.ReadLine();
                    Shell.Interpreter.Interpret(cmd);
                    Console.WriteLine();
                //} else
                //{
                //    Login.Init();
                //}

             
            }
            catch (Exception ex)
            {
                running = false;
                Crash.StopKernel(ex);
            }
            
        }

        #endregion

    }
}
