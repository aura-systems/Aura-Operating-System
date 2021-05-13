/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - CommandManager
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Cosmos.System.Network;
using Aura_OS.System.Shell.cmdIntr.c_Console;
using Aura_OS.System.Shell.cmdIntr.FileSystem;
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
            CMDs.Add(new CommandReboot(new string[] { "reboot", "rb" }));
            CMDs.Add(new CommandShutdown(new string[] { "shutdown", "sd" }));

            CMDs.Add(new CommandClear(new string[] { "clear", "clr" }));
            CMDs.Add(new CommandKeyboardMap(new string[] { "setkeyboardmap", "setkeyboard" }));
            CMDs.Add(new CommandEnv(new string[] { "export", "set" }));
            CMDs.Add(new CommandEcho(new string[] { "echo" }));

            CMDs.Add(new CommandIPConfig(new string[] { "ipconfig", "ifconfig", "netconf" }));
            CMDs.Add(new CommandPing(new string[] { "ping" }));
            CMDs.Add(new CommandUdp(new string[] { "udp" }));
            CMDs.Add(new CommandDns(new string[] { "dns" }));
            CMDs.Add(new CommandWget(new string[] { "wget" }));

            CMDs.Add(new CommandVersion(new string[] { "version", "ver" }));
            CMDs.Add(new CommandSystemInfo(new string[] { "systeminfo", "sysinfo" }));
            CMDs.Add(new CommandTime(new string[] { "time", "date" }));
            CMDs.Add(new CommandAbout(new string[] { "about" }));
            CMDs.Add(new CommandHelp(new string[] { "help" }));

            CMDs.Add(new CommandLspci(new string[] { "lspci" }));
            CMDs.Add(new CommandCrash(new string[] { "crash" }));

            CMDs.Add(new CommandVol(new string[] { "vol" }));
            CMDs.Add(new CommandDir(new string[] { "dir", "ls", "l" }));
            CMDs.Add(new CommandMkdir(new string[] { "mkdir", "md" }));
            CMDs.Add(new CommandRmdir(new string[] { "rmdir", "rmd" }));
            CMDs.Add(new CommandCat(new string[] { "cat" }));
            CMDs.Add(new CommandCD(new string[] { "cd" }));
            CMDs.Add(new CommandChangeVol(new string[] { "chgvol", "cv" }));
            CMDs.Add(new CommandMkfil(new string[] { "touch", "mkfil", "mf" }));
            CMDs.Add(new CommandRmfil(new string[] { "rmfil", "rmf" }));
            CMDs.Add(new CommandHex(new string[] { "hex" }));
            CMDs.Add(new CommandTree(new string[] { "tree" }));

            CMDs.Add(new CommandAction(new string[] { "beep" }, () =>
            {
                Cosmos.System.PCSpeaker.Beep();
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

            #region Parse command

            List<string> arguments = Misc.ParseCommandLine(cmd);

            string firstarg = arguments[0]; //command name

            if (arguments.Count > 0)
            {
                arguments.RemoveAt(0); //get only arguments
            }

            #endregion

            foreach (var command in CMDs)
            {
                if (command.ContainsCommand(firstarg))
                {
                    ReturnInfo result;

                    if (arguments.Count > 0 && arguments[0] == "/help")
                    {
                        ShowHelp(command);
                        result = new ReturnInfo(command, ReturnCode.OK);
                    }
                    else
                    {
                        result = CheckCommand(command);

                        if (result.Code == ReturnCode.OK)
                        {
                            if (arguments.Count == 0)
                            {
                                result = command.Execute();
                            }
                            else
                            {
                                result = command.Execute(arguments);
                            }
                        }
                    }

                    ProcessCommandResult(result);

                    return;
                }
            }

            Console.ForegroundColor = ConsoleColor.DarkRed;
            L.Text.Display("UnknownCommand");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine();
        }

        /// <summary>
        /// Show command description
        /// </summary>
        /// <param name="command">Command</param>
        private static void ShowHelp(ICommand command)
        {
            Console.WriteLine("Description: " + command.Description + ".");
            Console.WriteLine();
            if (command.CommandValues.Length > 1)
            {
                Console.Write("Aliases: ");
                for (int i = 0; i < command.CommandValues.Length; i++)
                {
                    if (i != command.CommandValues.Length - 1)
                    {
                        Console.Write(command.CommandValues[i] + ", ");
                    }
                    else
                    {
                        Console.Write(command.CommandValues[i]);
                    }
                }
                Console.WriteLine();
                Console.WriteLine();
            }
            command.PrintHelp();
        }

        /// <summary>
        /// Check command availability to avoid unwanted behavior.
        /// </summary>
        /// <param name="command">Command</param>
        private static ReturnInfo CheckCommand(ICommand command)
        {
            if (command.Type == CommandType.Filesystem)
            {
                if (Kernel.ContainsVolumes() == false)
                {
                    return new ReturnInfo(command, ReturnCode.ERROR, "No volume detected!");
                }
            }
            if (command.Type == CommandType.Network)
            {
                if (NetworkStack.ConfigEmpty())
                {
                    return new ReturnInfo(command, ReturnCode.ERROR, "No network configuration detected! Use ipconfig /set.");
                }
            }
            return new ReturnInfo(command, ReturnCode.OK);
        }

        /// <summary>
        /// Process result info of the command
        /// </summary>
        /// <param name="result">Result information</param>
        private static void ProcessCommandResult(ReturnInfo result)
        {
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
        }

    }
}