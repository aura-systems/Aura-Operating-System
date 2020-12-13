/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - CommandManager
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.System.Shell.cmdIntr.c_Console;
using Aura_OS.System.Shell.cmdIntr.Network;
using Aura_OS.System.Shell.cmdIntr.Power;
using Aura_OS.System.Shell.cmdIntr.SystemInfomation;
using Aura_OS.System.Shell.cmdIntr.Tests;
using Aura_OS.System.Shell.cmdIntr.Util;
using Aura_OS.System.Translation;
using Aura_OS.System.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr
{
    public class CommandManager
    {
        /// <summary>
        /// Empty constructor. (Good for debug)
        /// </summary>
        public CommandManager() { }

        public static List<ICommand> CMDs = new List<ICommand>();

        public static void RegisterAllCommands()
        {
            CMDs.Add(new CommandEcho(new string[] { "echo" }));
            CMDs.Add(new CommandReboot(new string[] { "reboot", "rb" }));
            CMDs.Add(new CommandShutdown(new string[] { "shutdown", "sd" }));
            CMDs.Add(new CommandClear(new string[] { "clear", "clr" }));
            CMDs.Add(new CommandPing(new string[] { "ping" }));
            CMDs.Add(new CommandUdp(new string[] { "udp" }));
            CMDs.Add(new CommandVersion(new string[] { "version", "ver" }));
            CMDs.Add(new CommandSystemInfo(new string[] { "systeminfo", "sysinfo" }));
            CMDs.Add(new CommandTime(new string[] { "time", "date" }));
            CMDs.Add(new CommandIPConfig(new string[] { "ipconfig", "ifconfig", "netconf" }));
            CMDs.Add(new CommandLspci(new string[] { "lspci" }));
            CMDs.Add(new CommandEnv(new string[] { "export", "set" }));
            CMDs.Add(new CommandAbout(new string[] { "about" }));
            CMDs.Add(new CommandCrash(new string[] { "crash" }));

            CMDs.Add(new CommandAction(new string[] { "beep" }, () =>
            {
                Cosmos.System.PCSpeaker.Beep();
            }));
            CMDs.Add(new CommandAction(new string[] { "help" }, () =>
            {
                List_Translation._Help();
            }));
            CMDs.Add(new CommandAction(new string[] { "play" }, () =>
            {
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.F5, 432);

                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.A5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.B5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.F5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.A5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.B5, 432);

                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.F5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.A5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.B5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.E6, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.D6, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.B5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.C6, 432);

                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.B5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.G5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.E5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.D5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.E5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.G5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.E5, 432);

                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.F5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.A5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.B5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.F5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.A5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.B5, 432);

                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.F5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.A5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.B5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.E6, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.D6, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.B5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.C6, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.E6, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.B5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.G5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.B5, 432);

                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.G5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.D5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.E5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.D5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.E5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.F5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.G5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.A5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.B5, 432);

                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.C6, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.B5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.E5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.D5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.E5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.F5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.G5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.A5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.B5, 432);

                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.C6, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.D6, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.E6, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.D5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.E5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.F5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.G5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.A5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.B5, 432);

                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.C6, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.B5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.E5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.D5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.C6, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.F5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.E5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.G5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.F5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.A5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.G5, 432);

                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.B5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.A5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.C6, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.B5, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.D6, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.C6, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.E6, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.F6, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.D6, 432);
                Cosmos.System.PCSpeaker.Beep((uint)Cosmos.System.Notes.E6, 432);
            }));
            CMDs.Add(new CommandAction(new string[] { "netrefresh" }, () =>
            {
                foreach (HAL.Drivers.Network.NetworkDevice networkDevice in HAL.Drivers.Network.NetworkDevice.Devices)
                {
                    File.Create(@"0:\System\" + networkDevice.Name + ".conf");
                    Utils.Settings settings = new Utils.Settings(@"0:\System\" + networkDevice.Name + ".conf");
                    settings.Edit("ipaddress", "0.0.0.0");
                    settings.Edit("subnet", "0.0.0.0");
                    settings.Edit("gateway", "0.0.0.0");
                    settings.Edit("dns01", "0.0.0.0");
                    settings.Push();
                }
            }));
        }

        /// <summary>
        /// Shell Interpreter
        /// </summary>
        /// <param name="cmd">Command</param>
        public static void _CommandManger(string cmd)
        {
            CommandsHistory.Add(cmd); //adding last command to the commands history

            if (cmd.Length <= 0)
            {
                Console.WriteLine();
                return;
            }

            List<string> arguments = Misc.ParseCommandLine(cmd);

            string firstarg = arguments[0]; //command name

            if (arguments.Count > 0)
            {
                arguments.RemoveAt(0); //get only arguments
            }

            foreach (var command in CMDs)
            {
                if (command.ContainsCommand(firstarg))
                {

                    ReturnInfo result;

                    if (arguments.Count == 0)
                    {
                        result = command.Execute();
                    }
                    else
                    {
                        result = command.Execute(arguments);
                    }

                    if (result.Code == ReturnCode.ERROR_ARG)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        L.Text.Display("invalidargcommand");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (result.Code == ReturnCode.ERROR)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("Error: " + result.Info);
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    Console.WriteLine();

                    return;
                }
            }

            Console.ForegroundColor = ConsoleColor.DarkRed;
            L.Text.Display("UnknownCommand");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine();

            /*

            #region FileSystem

            else if (cmd.StartsWith("cd "))
            {
                FileSystem.CD.c_CD(cmd);
            }
            else if (cmd.Equals("cp"))
            {
                FileSystem.CP.c_CP_only();
            }
            else if (cmd.StartsWith("cp "))
            {
                FileSystem.CP.c_CP(cmd);
            }
            else if ((cmd.Equals("dir")) || (cmd.Equals("ls")))
            {
                FileSystem.Dir.c_Dir();
            }
            else if ((cmd.StartsWith("dir ")) || (cmd.StartsWith("ls ")))
            {
                FileSystem.Dir.c_Dir(cmd);
            }
            else if (cmd.Equals("mkdir"))
            {
                FileSystem.Mkdir.c_Mkdir();
            }
            else if (cmd.StartsWith("mkdir "))
            {
                FileSystem.Mkdir.c_Mkdir(cmd);
            }
            else if (cmd.StartsWith("rmdir "))
            {
                FileSystem.Rmdir.c_Rmdir(cmd);
            }//TODO: orgainize
            else if (cmd.StartsWith("rmfil "))
            {
                FileSystem.Rmfil.c_Rmfil(cmd);
            }
            else if (cmd.Equals("mkfil"))
            {
                FileSystem.Mkfil.c_mkfil();
            }
            else if (cmd.StartsWith("mkfil "))
            {
                FileSystem.Mkfil.c_mkfil(cmd);
            }
            else if (cmd.StartsWith("edit "))
            {
                FileSystem.Edit.c_Edit(cmd);
            }
            else if (cmd.Equals("vol"))
            {
                FileSystem.Vol.c_Vol();
            }
            else if (cmd.StartsWith("run "))
            {
                FileSystem.Run.c_Run(cmd);
            }
            else if (cmd.StartsWith("cat"))
            {
                FileSystem.Cat.c_Cat(cmd);
            }

            #endregion FileSystem

            #region Settings

            else if (cmd.Equals("logout"))
            {
                Settings.Logout.c_Logout();
            }
            else if (cmd.Equals("settings"))
            {
                Settings.Settings.c_Settings();
            }
            else if (cmd.StartsWith("settings "))
            {
                Settings.Settings.c_Settings(cmd);
            }
            else if (cmd.StartsWith("passwd "))
            {
                Settings.Passwd.c_Passwd(cmd);
            }
            else if (cmd.Equals("passwd"))
            {
                Settings.Passwd.c_Passwd(Kernel.userLogged);
            }

            #endregion Settings

            #region Tools

            else if (cmd.Equals("snake"))
            {
                Tools.Snake.c_Snake();
            }
            else if (cmd.StartsWith("md5"))
            {
                Tools.MD5.c_MD5(cmd);
            }
            else if (cmd.StartsWith("sha256"))
            {
                Tools.SHA256.c_SHA256(cmd);
            }
            else if (cmd.Equals("debug"))
            {
                Tools.Debug.c_Debug();
            }
            else if (cmd.StartsWith("debug "))
            {
                Tools.Debug.c_Debug(cmd);
            }

            #endregion
            */

        }
    }
}