/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Kernel
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

#region using;

using System;
using Cosmos.System.FileSystem;
using Sys = Cosmos.System;
using System.IO;

#endregion

namespace Alve_OS
{
    public class Kernel: Sys.Kernel
    {

        #region Global variables

        bool running;
        string version = "0.1";
        string current_directory = @"0:\";
        public CosmosVFS FS { get; private set; }

        #endregion

        #region Before Run

        protected override void BeforeRun()
        {
            
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
        }

        #endregion

        #region Run

        protected override void Run()
        {
            Console.Write(current_directory + "> ");
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
            else if (cmd.Equals("cd .."))
            {
                Directory.SetCurrentDirectory(current_directory);
                var dir = FS.GetDirectory(current_directory);
                if (current_directory == @"0:\")
                {
                }
                else
                {
                    current_directory = dir.mParent.mFullPath;
                }
            }
            else if (cmd.StartsWith("cd "))
            {
                string dir = cmd.Remove(0, 3);
                if (Directory.Exists(current_directory + dir))
                {
                    Directory.SetCurrentDirectory(current_directory);
                    current_directory = current_directory + dir + @"\";
                }
                else
                {
                    Console.WriteLine("This directory doesn't exist!");
                }
            }
            else if (cmd.Equals("dir"))
            {
                Console.WriteLine("Type\t Name");
                foreach (var dir in Directory.GetDirectories(current_directory))
                {
                    Console.WriteLine("<DIR>\t" + dir);
                }
                foreach (var dir in Directory.GetFiles(current_directory))
                {
                    Console.WriteLine("     \t" + dir);
                }

            }
            else if (cmd.StartsWith("mkdir "))
            {
                string dir = cmd.Remove(0, 6);
                if (!Directory.Exists(current_directory + dir))
                {
                    FS.CreateDirectory(current_directory + dir);
                }
                else if (Directory.Exists(current_directory + dir))
                {
                    FS.CreateDirectory(current_directory + dir + "-1");
                }
            }
            else if (cmd.StartsWith("rmdir "))
            {
                string dir = cmd.Remove(0, 6);
                if (Directory.Exists(current_directory + dir))
                {
                    Directory.Delete(current_directory + dir, true);
                }
                else
                {
                    Console.WriteLine(dir + " does not exist!");
                }
            }
            else if (cmd.StartsWith("rmfil "))
            {
                string file = cmd.Remove(0, 6);
                if (File.Exists(current_directory + file))
                {
                    File.Delete(current_directory + file);
                }
                else
                {
                    Console.WriteLine(file + " does not exist!");
                }
            }
            else if (cmd.StartsWith("mkfil "))
            {
                string file = cmd.Remove(0, 6);
                if (!File.Exists(current_directory + file))
                {
                    File.Create(current_directory + file); 
                }
                else
                {
                    Console.WriteLine(file + " already exists!");
                }
            }
            else if (cmd.Equals("vol"))
            {
                var vols = FS.GetVolumes();
                Console.WriteLine("Name\tSize\tParent");
                foreach (var vol in vols)
                {
                    Console.WriteLine(vol.mName + "\t" + vol.mSize + "\t" + vol.mParent);
                }
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
