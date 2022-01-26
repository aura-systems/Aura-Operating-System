/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - CommandManager
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.System.Shell.cmdIntr.c_Console;
using Aura_OS.System.Shell.cmdIntr.Network;
using Aura_OS.System.Shell.cmdIntr.Power;
using Aura_OS.System.Shell.cmdIntr.Util;
using Aura_OS.System.Utils;
using Cosmos.System.Network;
using Aura_OS;
using Aura_OS.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aura_OS.System.Shell.cmdIntr
{
    public class CommandManager : Process
    {
        public List<ICommand> CMDs = new List<ICommand>();

        public CommandManager() : base("cmdManager", ProcessType.KernelComponent)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            RegisterAllCommands();

            Kernel.ProcessManager.Register(this);
            Kernel.ProcessManager.Start(this);
        }

        public void RegisterAllCommands()
        {
            CMDs.Add(new CommandReboot(new string[] { "reboot", "rb" }));
            CMDs.Add(new CommandShutdown(new string[] { "shutdown", "sd" }));

            CMDs.Add(new CommandClear(new string[] { "clear", "clr" }));
            CMDs.Add(new CommandKeyboardMap(new string[] { "setkeyboardmap", "setkeyboard" }));

            CMDs.Add(new CommandHelp(new string[] { "help" }));

            CMDs.Add(new CommandLsprocess(new string[] { "lsprocess" }));

            CMDs.Add(new CommandIPConfig(new string[] { "ipconfig", "ifconfig", "netconf" }));
            CMDs.Add(new CommandPing(new string[] { "ping" }));
            CMDs.Add(new CommandUdp(new string[] { "udp" }));
            CMDs.Add(new CommandDns(new string[] { "dns" }));
            CMDs.Add(new CommandWget(new string[] { "wget" }));
            //CMDs.Add(new CommandFtp(new string[] { "ftp" }));
            CMDs.Add(new CommandHttpServer(new string[] { "httpserver" }));
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
                Kernel.console.WriteLine();
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

                    return;
                }
            }

            Kernel.console.Foreground = ConsoleColor.DarkRed;
            Kernel.console.WriteLine("Unknown command.");
            Kernel.console.Foreground = ConsoleColor.White;

            Kernel.console.WriteLine();
        }

        /// <summary>
        /// Show command description
        /// </summary>
        /// <param name="command">Command</param>
        private void ShowHelp(ICommand command)
        {
            Kernel.console.WriteLine("Description: " + command.Description + ".");
            Kernel.console.WriteLine();
            if (command.CommandValues.Length > 1)
            {
                Kernel.console.Write("Aliases: ");
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
                Kernel.console.WriteLine();
                Kernel.console.WriteLine();
            }
            command.PrintHelp();
        }

        /// <summary>
        /// Check command availability to avoid unwanted behavior.
        /// </summary>
        /// <param name="command">Command</param>
        private ReturnInfo CheckCommand(ICommand command)
        {
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
                Kernel.console.Foreground = ConsoleColor.DarkRed;
                Kernel.console.WriteLine("Command arguments are incorrectly formatted.");
                Kernel.console.Foreground = ConsoleColor.White;
            }
            else if (result.Code == ReturnCode.ERROR)
            {
                Kernel.console.Foreground = ConsoleColor.DarkRed;
                Kernel.console.WriteLine("Error: " + result.Info);
                Kernel.console.Foreground = ConsoleColor.White;
            }

            Kernel.console.WriteLine();
        }

    }
}
