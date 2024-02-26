/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - CommandManager
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using Aura_OS.System.Utils;
using Aura_OS.System.Processing.Interpreter.Commands.Util;
using Aura_OS.System.Processing.Interpreter.Commands.Filesystem;
using Aura_OS.System.Processing.Interpreter.Commands.Power;
using Aura_OS.System.Processing.Interpreter.Commands.c_Console;
using Aura_OS.System.Processing.Interpreter.Commands.Network;
using Aura_OS.System.Processing.Interpreter.Commands.SystemInfomation;
using Aura_OS.System.Processing.Interpreter.Commands.Graphics;
using Aura_OS.System.Processing.Interpreter.Commands.Processing;
using Cosmos.System.Network;

namespace Aura_OS.System.Processing.Interpreter.Commands
{
    public class CommandManager : IManager
    {
        public static List<ICommand> _commands;

        public void Initialize()
        {
            _commands = new List<ICommand>();

            RegisterAllCommands();
        }

        public void RegisterAllCommands()
        {
            _commands.Add(new CommandReboot(new string[] { "reboot", "rb" }));
            _commands.Add(new CommandShutdown(new string[] { "shutdown", "sd" }));

            _commands.Add(new CommandClear(new string[] { "clear", "clr" }));
            _commands.Add(new CommandKeyboardMap(new string[] { "setkeyboardmap", "setkeyboard" }));
            _commands.Add(new CommandEnv(new string[] { "export", "set" }));
            _commands.Add(new CommandEcho(new string[] { "echo" }));

            _commands.Add(new CommandIPConfig(new string[] { "ipconfig", "ifconfig", "netconf" }));
            _commands.Add(new CommandPing(new string[] { "ping" }));
            _commands.Add(new CommandUdp(new string[] { "udp" }));
            _commands.Add(new CommandDns(new string[] { "dns" }));
            _commands.Add(new CommandWget(new string[] { "wget" }));
            _commands.Add(new CommandFtp(new string[] { "ftp" }));
            _commands.Add(new CommandPackage(new string[] { "package", "pkg" }));

            _commands.Add(new CommandVersion(new string[] { "version", "ver", "about" }));
            _commands.Add(new CommandSystemInfo(new string[] { "systeminfo", "sysinfo" }));
            _commands.Add(new CommandTime(new string[] { "time", "date" }));
            _commands.Add(new CommandHelp(new string[] { "help" }));

            _commands.Add(new CommandChangeRes(new string[] { "changeres", "cr" }));
            _commands.Add(new CommandLspci(new string[] { "lspci" }));
            //_commands.Add(new CommandCrash(new string[] { "crash" }));

            _commands.Add(new CommandLsprocess(new string[] { "lsprocess" }));

            _commands.Add(new CommandVol(new string[] { "vol" }));
            _commands.Add(new CommandDir(new string[] { "dir", "ls", "l" }));
            _commands.Add(new CommandMkdir(new string[] { "mkdir", "md" }));
            _commands.Add(new CommandCat(new string[] { "cat" }));
            _commands.Add(new CommandCD(new string[] { "cd" }));
            _commands.Add(new CommandMkfil(new string[] { "touch", "mkfil", "mf" }));
            _commands.Add(new CommandRm(new string[] { "rm", "rmf", "rmd" }));
            _commands.Add(new CommandHex(new string[] { "hex" }));
            _commands.Add(new CommandTree(new string[] { "tree" }));
            _commands.Add(new CommandRun(new string[] { "run" }));
            _commands.Add(new CommandCopy(new string[] { "cp" }));
            _commands.Add(new CommandPicture(new string[] { "pic" }));

            _commands.Add(new CommandZip(new string[] { "zip" }));

            /*
            CMDs.Add(new CommandPCName(new string[] { "pcn" }));

            CMDs.Add(new CommandMIV(new string[] { "miv", "edit" }));*/

            _commands.Add(new CommandAction(new string[] { "beep" }, () =>
            {
                Cosmos.System.PCSpeaker.Beep();
            }));
            _commands.Add(new CommandAction(new string[] { "crash" }, () =>
            {
                throw new Exception("Exception test");
            }));
            _commands.Add(new CommandAction(new string[] { "crashn" }, () =>
            {
                string[] test =
                {
                    "test1",
                    "tert2"
                };
                test[2] = "test3"; //Should make a Null reference exception
            }));
            _commands.Add(new CommandAction(new string[] { "components" }, () =>
            {
                for (int i = 0; i < System.Graphics.UI.GUI.Components.Component.Components.Count; i++)
                {
                    var component = System.Graphics.UI.GUI.Components.Component.Components[i];

                    string text = $"{component.GetType().ToString()} X:{component.X} Y:{component.Y} W:{component.Width} H:{component.Height} Z:{component.zIndex} Visible:{component.Visible}";

                    if (component is System.Graphics.UI.GUI.Components.Button button)
                    {
                        text += $" Text:{button.Text}";
                    }
                    else if (component is System.Graphics.UI.GUI.Components.Window window)
                    {
                        text += $" Name:{window.Name}";
                    }

                    Console.WriteLine(text);
                }
            }));

        }

        /// <summary>
        /// Shell Interpreter
        /// </summary>
        /// <param name="cmd">Command</param>
        public void Execute(string cmd)
        {
            //CommandsHistory.Add(cmd); //adding last command to the commands history

            if (cmd.Length <= 0)
            {
                Console.WriteLine();
                return;
            }

            #region Parse command

            string[] parts = cmd.Split(new char[] { '>' }, 2);
            string redirectionPart = parts.Length > 1 ? parts[1].Trim() : null;
            cmd = parts[0].Trim();

            if (!string.IsNullOrEmpty(redirectionPart))
            {
                Kernel.Redirect = true;
                Kernel.CommandOutput = "";
            }

            List<string> arguments = Misc.ParseCommandLine(cmd);

            string firstarg = arguments[0]; //command name

            if (arguments.Count > 0)
            {
                arguments.RemoveAt(0); //get only arguments
            }

            #endregion

            foreach (var command in _commands)
            {
                if (command.ContainsCommand(firstarg))
                {
                    ReturnInfo result;

                    if (arguments.Count > 0 && (arguments[0] == "/help" || arguments[0] == "/h"))
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

                    if (Kernel.Redirect)
                    {
                        Kernel.Redirect = false;

                        Console.WriteLine();

                        HandleRedirection(redirectionPart, Kernel.CommandOutput);

                        Kernel.CommandOutput = "";
                    }

                    return;
                }
            }

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Unknown command.");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine();

            if (Kernel.Redirect)
            {
                Kernel.Redirect = false;

                HandleRedirection(redirectionPart, Kernel.CommandOutput);

                Kernel.CommandOutput = "";
            }
        }

        /// <summary>
        /// Show command description
        /// </summary>
        /// <param name="command">Command</param>
        private void ShowHelp(ICommand command)
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
        private ReturnInfo CheckCommand(ICommand command)
        {
            if (command.Type == CommandType.Filesystem)
            {
                if (Kernel.VirtualFileSystem == null || Kernel.VirtualFileSystem.GetVolumes().Count == 0)
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
        private void ProcessCommandResult(ReturnInfo result)
        {
            if (result.Code == ReturnCode.ERROR_ARG)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Command arguments are incorrectly formatted.");
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

        private void HandleRedirection(string filePath, string commandOutput)
        {
            string fullPath = Kernel.CurrentDirectory + filePath;

            File.WriteAllText(fullPath, commandOutput);
        }

        /// <summary>
        /// Returns the list of registered commands.
        /// </summary>
        /// <returns>The list of commands.</returns>
        public static List<ICommand> GetCommands()
        {
            return _commands;
        }

        /// <summary>
        /// Returns the name of the manager.
        /// </summary>
        /// <returns>The name of the manager.</returns>
        public string GetName()
        {
            return "Command Manager";
        }
    }
}
