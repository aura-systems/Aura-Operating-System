using System;
using System.IO;
using Sys = Cosmos.System;

namespace Alve_OS.Shell
{
    class Interpreter
    {
        public static void Interpret(string cmd)
        {
            if (cmd.Equals("shutdown"))
            {
                Kernel.running = false;
                Console.Clear();
                Console.WriteLine("Shutting Down...");
                Sys.Power.Shutdown();
            }
            else if (cmd.Equals("reboot"))
            {
                Kernel.running = false;
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
                Console.WriteLine("- cd .. (to navigate to the parent folder)");
                Console.WriteLine("- cd (to navigate to a folder)");
                Console.WriteLine("- dir (to list directories and files)");
                Console.WriteLine("- mkdir (to create a directory");
                Console.WriteLine("- rmdir (to remove a directory)");
                Console.WriteLine("- mkfil (to create a file)");
                Console.WriteLine("- rmfil (to remove a file)");
                Console.WriteLine("- vol (to list volumes)");
                Console.WriteLine("- echo (to echo text)");
                Console.WriteLine("- systeminfo (to display system informations)");
            }
            else if (cmd.Equals("cd .."))
            {
                Directory.SetCurrentDirectory(Kernel.current_directory);
                var dir = Kernel.FS.GetDirectory(Kernel.current_directory);
                if (Kernel.current_directory == @"0:\")
                {
                }
                else
                {
                    Kernel.current_directory = dir.mParent.mFullPath;
                }
            }
            else if (cmd.StartsWith("cd "))
            {
                string dir = cmd.Remove(0, 3);
                if (Directory.Exists(Kernel.current_directory + dir))
                {
                    Directory.SetCurrentDirectory(Kernel.current_directory);
                    Kernel.current_directory = Kernel.current_directory + dir + @"\";
                }
                else
                {
                    Console.WriteLine("This directory doesn't exist!");
                }
            }
            else if (cmd.Equals("dir"))
            {
                Console.WriteLine("Type\t Name");
                foreach (var dir in Directory.GetDirectories(Kernel.current_directory))
                {
                    Console.WriteLine("<DIR>\t" + dir);
                }
                foreach (var dir in Directory.GetFiles(Kernel.current_directory))
                {
                    Console.WriteLine("     \t" + dir);
                }

            }
            else if (cmd.StartsWith("mkdir "))
            {
                string dir = cmd.Remove(0, 6);
                if (!Directory.Exists(Kernel.current_directory + dir))
                {
                    Kernel.FS.CreateDirectory(Kernel.current_directory + dir);
                }
                else if (Directory.Exists(Kernel.current_directory + dir))
                {
                    Kernel.FS.CreateDirectory(Kernel.current_directory + dir + "-1");
                }
            }
            else if (cmd.StartsWith("rmdir "))
            {
                string dir = cmd.Remove(0, 6);
                if (Directory.Exists(Kernel.current_directory + dir))
                {
                    Directory.Delete(Kernel.current_directory + dir, true);
                }
                else
                {
                    Console.WriteLine(dir + " does not exist!");
                }
            }
            else if (cmd.StartsWith("rmfil "))
            {
                string file = cmd.Remove(0, 6);
                if (File.Exists(Kernel.current_directory + file))
                {
                    File.Delete(Kernel.current_directory + file);
                }
                else
                {
                    Console.WriteLine(file + " does not exist!");
                }
            }
            else if (cmd.StartsWith("mkfil "))
            {
                string file = cmd.Remove(0, 6);
                if (!File.Exists(Kernel.current_directory + file))
                {
                    File.Create(Kernel.current_directory + file);
                }
                else
                {
                    Console.WriteLine(file + " already exists!");
                }
            }
            else if (cmd.Equals("vol"))
            {
                var vols = Kernel.FS.GetVolumes();
                Console.WriteLine("Name\tSize\tParent");
                foreach (var vol in vols)
                {
                    Console.WriteLine(vol.mName + "\t" + vol.mSize + "\t" + vol.mParent);
                }
            }
            else if (cmd.Equals("systeminfo"))
            {
                Console.WriteLine("Operating system name:     Alve");
                Console.WriteLine("Operating system version:  " + Kernel.version);
                Console.WriteLine("Operating system revision: " + Kernel.revision);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Unknown command.");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

    }
}
