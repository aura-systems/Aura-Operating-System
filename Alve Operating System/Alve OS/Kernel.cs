/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Kernel
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

#region using;

using System;
using Cosmos.System.FileSystem;
using Sys = Cosmos.System;

#endregion

namespace Alve_OS
{
    public class Kernel: Sys.Kernel
    {

        #region Global variables

        bool running;
        string version = "0.1";
        string current_directory = @"C:\";
        public CosmosVFS FS { get; private set; }

        #endregion

        #region Before Run

        protected override void BeforeRun()
        {
            Console.Clear();
            running = true;

            FS = new CosmosVFS();
            FS.Initialize();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Kernel has started successfully!");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Welcome to Alve Operating System v" + version + " !");
        }

        #endregion

        #region Run

        protected override void Run()
        {
            Console.Write("alve> ");
            var cmd = Console.ReadLine();
            Interpret(cmd);
            Console.WriteLine();
        }

        #endregion

        #region Command interpreter

        public void Interpret(string cmd)
        {
            if (cmd.Equals("shutdown"))
            {
                running = false;
                Console.Clear();
                Console.WriteLine("Shutting Down...");
                Sys.Power.Shutdown();
            }
            else if (cmd.Equals("reboot"))
            {
                running = false;
                Console.Clear();
                Console.WriteLine("Restarting...");
                Sys.Power.Reboot();
            }
            else if (cmd.Equals("clear"))
            {
                Console.Clear();
            }
            else if (cmd.StartsWith("echo "))
            {
                cmd = cmd.Remove(0, 5);
                Console.WriteLine(cmd);
            }
            else if (cmd.Equals("help"))
            {
                Console.WriteLine("Available commands:");
                Console.WriteLine("- shutdown (to do a ACPI Shutdown)");
                Console.WriteLine("- reboot (to do a CPU Reboot)");
                Console.WriteLine("- clear (to clear the console)");
                Console.WriteLine("- echo text (to echo text)");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Unknown command.");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        #endregion
    }
}
