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


#endregion

namespace Alve_OS
{
    public class Kernel: Sys.Kernel
    {

        #region Global variables

        public static bool running;
        public static string version = "0.1";
        public static string revision = "21072017-1445";
        public static string current_directory = @"0:\";
        public static string langSelected = "en_US";
        public static CosmosVFS FS { get; private set; }

        #endregion

        #region Before Run

        protected override void BeforeRun()
        {
            #region Language
            Lang.Keyboard.Init();
            #endregion

            running = true;

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

            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Kernel has started successfully!");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Welcome to Alve Operating System v" + version + " !");
            Console.WriteLine("Made by Valentin CHARBONNIER (valentinbreiz) and Alexy DA CRUZ (GeomTech).");
            Console.WriteLine();
        }

        #endregion

        #region Run

        protected override void Run()
        {
            Console.Write(current_directory + "> ");
            var cmd = Console.ReadLine();
            Shell.Interpreter.Interpret(cmd);
            Console.WriteLine();
        }

        #endregion

    }
}
