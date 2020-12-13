/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - CommandManager
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.System.Shell.cmdIntr.c_Console;
using Aura_OS.System.Shell.cmdIntr.Network;
using Aura_OS.System.Shell.cmdIntr.Power;
using Aura_OS.System.Shell.cmdIntr.SystemInfomation;
using Aura_OS.System.Shell.cmdIntr.Util;
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
        //TO-DO: Do for all commands:
        //       Windows like command, Linux like command, Aura original command (optional for the last one)
        //Example: else if ((cmd.Equals("ipconfig")) || (cmd.Equals("ifconfig")) || (cmd.Equals("netconf"))) {

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
            CMDs.Add(new CommandVersion(new string[] { "version", "about" }));
            CMDs.Add(new CommandSystemInfo(new string[] { "systeminfo", "sysinfo" }));
            CMDs.Add(new CommandTime(new string[] { "time" }));
            CMDs.Add(new CommandIPConfig(new string[] { "ipconfig" }));
            CMDs.Add(new CommandLspci(new string[] { "lspci" }));
            CMDs.Add(new CommandEnv(new string[] { "export", "set" }));
            /*
            Register("sha256");
            Register("help");
            Register("cd");
            Register("cp");
            Register("dir");
            Register("ls");
            Register("mkdir");
            Register("rmdir");
            Register("rmfil");
            Register("mkfil");
            Register("edit");
            Register("vol");
            Register("run");
            Register("logout");
            Register("passwd");
            Register("settings");
            Register("systeminfo");
            Register("version");
            Register("ipconfig");
            Register("ifconfig");
            Register("netconf");
            Register("time");
            Register("date");
            Register("beep");
            Register("snake");
            Register("md5");
            Register("export");
            Register("lspci");
            Register("about");
            Register("debug");
            Register("cat");*/
        }

        /// <summary>
        /// Shell Interpreter
        /// </summary>
        /// <param name="cmd">Command</param>
        public static void _CommandManger(string cmd)
        {
            CommandsHistory.Add(cmd); //adding last command to the commands history

            List<string> arguments = new List<string>();

            foreach (string arg in cmd.Split(' ')) //TODO parse quotes too
            {
                arguments.Add(arg);
            }

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
            #region Power

            if (cmd.Equals("shutdown") || cmd.Equals("sd"))
            {//NOTE: Why isn't it just the constructor? This leaves more room for <package>.<class>.HelpInfo;
                Power.Shutdown.c_Shutdown();
            }
            else if (cmd.Equals("reboot") || cmd.Equals("rb"))
            {
                Power.Reboot.c_Reboot();
            }

            #endregion Power

            #region Console

            else if ((cmd.Equals("clear")) || (cmd.Equals("cls")))
            {
                c_Console.Clear.c_Clear();
            }
            else if (cmd.StartsWith("echo "))
            {
                c_Console.Echo.c_Echo(cmd);
            }
            else if (cmd.Equals("help"))
            {
                System.Translation.List_Translation._Help();
            }

            #endregion Console

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

            #region System Infomation

            else if (cmd.Equals("systeminfo"))
            {
                SystemInfomation.SystemInfomation.c_SystemInfomation();
            }
            else if ((cmd.Equals("ver")) || (cmd.Equals("version")))
            {
                SystemInfomation.Version.c_Version();
            }
            else if ((cmd.StartsWith("ipconfig")) || (cmd.StartsWith("ifconfig")) || (cmd.StartsWith("netconf")))
            {
                SystemInfomation.IPConfig.c_IPConfig(cmd);
            }
            else if ((cmd.Equals("time")) || (cmd.Equals("date")))
            {
                SystemInfomation.Time.c_Time();
            }

            #endregion System Infomation

            #region Tests

            else if (cmd.Equals("crash"))
            {
                Tests.Crash.c_Crash();
            }

            else if (cmd.Equals("crashcpu"))
            {
                int value = 1;
                value = value - 1;
                int result = 1 / value; //Division by 0
            }

            else if (cmd.Equals("beep"))
            {
                Cosmos.System.PCSpeaker.Beep();
            }

            else if (cmd.Equals("play"))
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
            }

            else if (cmd.Equals("haship"))
            {
                Console.WriteLine(new HAL.MACAddress(new byte[] { 00, 01, 02, 03, 04, 05 }).Hash);
                Console.WriteLine(new System.Network.IPV4.Address(192, 168, 1, 12).Hash);
            }

            else if (cmd.Equals("net /refresh"))
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
            }

            //else if (cmd.StartsWith("xml "))
            //{
            //    Util.xml.CmdXmlParser.c_CmdXmlParser(cmd, 0, 4);
            //}

            #endregion Tests

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
            else if (cmd.StartsWith("ping"))
            {
                Network.Ping.c_Ping(cmd);
            }
            else if (cmd.StartsWith("udp"))
            {
                Network.Udp.c_Udp(cmd);
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

            #region Util           

            else if (cmd.StartsWith("export"))
            {
                Util.EnvVar.c_Export(cmd);
            }

            else if (cmd.Equals("lspci"))
            {
                Util.Lspci.c_Lspci();
            }

            else if (cmd.Equals("about"))
            {
                Util.About.c_About();
            }

            else
            {
                if (cmd.Length <= 0)
                {
                    Console.WriteLine();
                    return;
                }
                else if (cmd.Length == 2)
                {
                    FileSystem.ChangeVol.c_ChangeVol(cmd);
                }
                else
                {
                    Util.CmdNotFound.c_CmdNotFound();
                }
            } */

            //#endregion Util

        }
    }
}