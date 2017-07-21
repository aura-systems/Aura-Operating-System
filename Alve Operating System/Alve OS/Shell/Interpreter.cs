/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Translation system
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using Sys = Cosmos.System;
using L = Alve_OS.System.Translation;

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
                L.Text.Display("shutdown");
                Sys.Power.Shutdown();
            }
            else if (cmd.Equals("reboot"))
            {
                Kernel.running = false;
                Console.Clear();
                L.Text.Display("restart");
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
                L.Help.Display();
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
                    L.Text.Display("directorydoesntexist");
                }
            }
            else if (cmd.Equals("dir"))
            {
                L.Text.Display("typename");
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
                    L.Text.Display("doesnotexist");
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
                    L.Text.Display("doesnotexist");
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
                    L.Text.Display("alreadyexist");
                }
            }
            else if (cmd.Equals("vol"))
            {
                var vols = Kernel.FS.GetVolumes();
                L.Text.Display("NameSizeParent");
                foreach (var vol in vols)
                {
                    Console.WriteLine(vol.mName + "\t" + vol.mSize + "\t" + vol.mParent);
                }
            }
            else if (cmd.Equals("systeminfo"))
            {
                L.Text.Display("OSName");
                L.Text.Display("OSVersion");
                L.Text.Display("OSRevision");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                L.Text.Display("UnknownCommand");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
