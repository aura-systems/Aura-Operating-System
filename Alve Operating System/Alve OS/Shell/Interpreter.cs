/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Interpreter
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using Sys = Cosmos.System;
using L = Alve_OS.System.Translation;
using Alve_OS.System;
using Alve_OS.System.Users;
using Alve_OS.System.Computer;

namespace Alve_OS.Shell
{
    class Interpreter
    {
        public static void Interpret(string cmd)
        {

            #region Power

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

            #endregion

            #region Console
            
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
                L.Help.HelpD();
            }

            else if (cmd.Equals("textcolor"))
            {
                L.Color.Display();
            }

            else if (cmd.StartsWith("textcolor "))
            {
                string color = cmd.Remove(0, 10);
                if (color.Equals("0"))
                {
                    Color.SetTextColor("0");
                    Kernel.color = 0;
                }
                else if (color.Equals("1"))
                {
                    Color.SetTextColor("1");
                    Kernel.color = 1;
                }
                else if (color.Equals("2"))
                {
                    Color.SetTextColor("2");
                    Kernel.color = 2;
                }
                else if (color.Equals("3"))
                {
                    Color.SetTextColor("3");
                    Kernel.color = 3;
                }
                else if (color.Equals("4"))
                {
                    Color.SetTextColor("4");
                    Kernel.color = 4;
                }
                else if (color.Equals("5"))
                {
                    Color.SetTextColor("5");
                    Kernel.color = 5;
                }
                else if (color.Equals("6"))
                {
                    Color.SetTextColor("6");
                    Kernel.color = 6;
                }
                else if (color.Equals("7"))
                {
                    Color.SetTextColor("7");
                    Kernel.color = 7;
                }
                else
                {
                    L.Text.Display("unknowncolor");
                    Kernel.color = -1;
                }
            }

            else if (cmd.Equals("backgroundcolor"))
            {
                L.Color.Display();
            }

            else if (cmd.StartsWith("backgroundcolor "))
            {
                string color = cmd.Remove(0, 16);
                if (color.Equals("0"))
                {
                    Color.SetBackgroundColor("0");
                }
                else if (color.Equals("1"))
                {
                    Color.SetBackgroundColor("1");
                }
                else if (color.Equals("2"))
                {
                    Color.SetBackgroundColor("2");
                }
                else if (color.Equals("3"))
                {
                    Color.SetBackgroundColor("3");
                }
                else if (color.Equals("4"))
                {
                    Color.SetBackgroundColor("4");
                }
                else if (color.Equals("5"))
                {
                    Color.SetBackgroundColor("5");
                }
                else if (color.Equals("6"))
                {
                    Color.SetBackgroundColor("6");
                }
                else if (color.Equals("7"))
                {
                    Color.SetBackgroundColor("7");
                }
                else
                {
                    L.Text.Display("unknowncolor");
                }
            }

            #endregion

            #region FileSystem

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

            else if ((cmd.Equals("dir")) || (cmd.Equals("ls")))
            {
                foreach (string dir in Directory.GetDirectories(Kernel.current_directory))
                {
                    Color.DisplayTextColor("6");
                    Console.Write(dir + "\t");
                }
                foreach (string file in Directory.GetFiles(Kernel.current_directory))
                {                    
                    Char formatDot = '.';
                    string[] ext = file.Split(formatDot);
                    string lastext = ext[ext.Length - 1];

                    if ((lastext == "set") || (lastext == "nam") || (lastext == "usr"))
                    {
                        Color.DisplayTextColor("4");
                        Console.Write(file + "\t");
                    }
                    else
                    {
                        Color.DisplayTextColor("1");
                        Console.Write(file + "\t");
                    }
                }
                Console.WriteLine();
            }

            else if ((cmd.StartsWith("dir ")) || (cmd.StartsWith("ls ")))
            {
                string cmddir;
                if (cmd.StartsWith("dir "))
                {
                    cmddir = cmd.Remove(0, 4);

                    foreach (string dir in Directory.GetDirectories(cmddir))
                    {
                        Color.DisplayTextColor("6");
                        Console.Write(dir + "\t");
                    }
                    foreach (string file in Directory.GetFiles(cmddir))
                    {
                        Char formatDot = '.';
                        string[] ext = file.Split(formatDot);
                        string lastext = ext[ext.Length - 1];

                        if ((lastext == "set") || (lastext == "nam") || (lastext == "usr"))
                        {
                            Color.DisplayTextColor("4");
                            Console.Write(file + "\t");
                        }
                        else
                        {
                            Color.DisplayTextColor("1");
                            Console.Write(file + "\t");
                        }
                    }
                    Console.WriteLine();
                }

                else if (cmd.StartsWith("ls "))
                {
                    cmddir = cmd.Remove(0, 3);

                    foreach (string dir in Directory.GetDirectories(cmddir))
                    {
                        Color.DisplayTextColor("6");
                        Console.Write(dir + "\t");
                    }
                    foreach (string file in Directory.GetFiles(cmddir))
                    {
                        Char formatDot = '.';
                        string[] ext = file.Split(formatDot);
                        string lastext = ext[ext.Length - 1];

                        if ((lastext == "set") || (lastext == "nam") || (lastext == "usr"))
                        {
                            Color.DisplayTextColor("4");
                            Console.Write(file + "\t");
                        }
                        else
                        {
                            Color.DisplayTextColor("1");
                            Console.Write(file + "\t");
                        }
                    }
                    Console.WriteLine();
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

            else if (cmd.Equals("mkfil"))
            {
                L.Text.Display("mkfil");
            }

            else if (cmd.StartsWith("mkfil "))
            {
                string file = cmd.Remove(0, 6);
                if (!File.Exists(Kernel.current_directory + file))
                {
                    Apps.User.Editor application = new Apps.User.Editor();
                    application.Start(file, Kernel.current_directory);
                }
                else
                {
                    L.Text.Display("alreadyexist");
                }
            }

            else if (cmd.StartsWith("prfil "))
            {
                string file = cmd.Remove(0, 6);
                if (File.Exists(Kernel.current_directory + file))
                {
                    Apps.User.Editor application = new Apps.User.Editor();
                    application.Start(file, Kernel.current_directory);
                }
                else
                {
                    L.Text.Display("doesnotexit");
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

            #endregion

            #region Settings

            else if (cmd.Equals("langset"))
            {
                L.Text.Display("availablelanguage");
            }

            else if (cmd.StartsWith("langset "))
            {
                cmd = cmd.Remove(0, 8);
                if ((cmd.Equals("en_US")) || cmd.Equals("en-US"))
                {
                    Kernel.langSelected = "en_US";
                    L.Keyboard.Init();
                }
                else if ((cmd.Equals("fr_FR")) || cmd.Equals("fr-FR"))
                {
                    Kernel.langSelected = "fr_FR";
                    L.Keyboard.Init();
                }
                else
                {
                    L.Text.Display("unknownlanguage");
                    L.Text.Display("availablelanguage");
                }
            }

            else if (cmd.Equals("setup"))
            {
                L.Text.Display("setupcmd");
                string setup = Console.ReadLine();
                if (setup == "o")
                {
                    SetupInit.Init();
                }
            }

            else if (cmd.Equals("logout"))
            {
                Kernel.Logged = false;
                Kernel.userLevelLogged = "";
                Kernel.userLogged = "";
                Console.Clear();
                WelcomeMessage.Display();
            }

            else if (cmd.Equals("settings"))
            {
                L.Help.Settings();
            }

            else if (cmd.StartsWith("settings "))
            {
                string argsettings = cmd.Remove(0, 9);
                if (argsettings.Equals("adduser"))
                {
                    //method user
                    string argsuser = argsettings.Remove(0, 7);
                    Users users = new Users();

                    users.Create(argsuser);

                }
                else if (argsettings.Equals("setcomputername"))
                {
                    //method computername
                    string argspcname = argsettings.Remove(0, 15);
                    System.Computer.Info.AskComputerName();

                }
            }

            #endregion

            #region System Infos
            
            else if (cmd.Equals("systeminfo"))
            {
                L.Text.Display("OSName");
                L.Text.Display("OSVersion");
                L.Text.Display("OSRevision");
                L.Text.Display("AmountRAM");
            }

            else if (cmd.Equals("ver"))
            {
                Console.WriteLine("Alve [version " + Kernel.version + "-" + Kernel.revision + "]");
            }

            #endregion

            #region Tests

            else if (cmd.Equals("crash"))
            {
                throw new Exception("Crash test");
            }

            #endregion

            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                L.Text.Display("UnknownCommand");
                Console.ForegroundColor = ConsoleColor.White;
            }

        } 
    }
}
