using System;
using System.Collections.Generic;
using System.Linq;
using WMCommandFramework.Exceptions;
using System.Text;
using WMCommandFramework.Commands;
using System.Text.RegularExpressions;

namespace WMCommandFramework
{
    public class CommandInvoker
    {
        private List<Command> commands = new List<Command>();

        public CommandInvoker()
        {
            //TODO: Register Internal Commands.
            AddCommand(new HelpCommand());
            AddCommand(new ClearCommand());
            AddCommand(new EchoCommand());
        }

        /// <summary>
        /// Adds a command to the Command Registry.
        /// </summary>
        /// <param name="c">The command to add to the Command Registry.</param>
        public void AddCommand(Command c)
        {
            commands.Add(c);
        }

        /// <summary>
        /// Removes a pre-existing command from the Command Registry.
        /// </summary>
        /// <param name="c">The command to remove from the Command Registry.</param>
        public void RemoveCommand(Command c)
        {
            commands.Remove(c);
        }

        /// <summary>
        /// Gets a list of all commands in the Command Registry.
        /// </summary>
        /// <returns>A list of commands in the Command Registry.</returns>
        public List<Command> GetCommands()
        {
            return commands;
        }

        /// <summary>
        /// Gets a command with the specified name.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <returns>The command with the specified name. Null if none.</returns>
        public Command GetCommandByName(string name)
        {
            Command out_x = null;
            foreach (Command c in commands)
            {
                if (c.CommandName().ToLower() == name.ToLower())
                {
                    out_x = c;
                    break;
                }
            }
            return out_x;
        }

        /// <summary>
        /// Gets a command with the specified alias.
        /// </summary>
        /// <param name="alias">The alias of the command.</param>
        /// <returns>The command with the specified alias. Null if none.</returns>
        public Command GetCommandByAlias(string alias)
        {
            Command out_x = null;
            foreach (Command c in commands)
            {
                var next = false;
                foreach (string s in c.CommandAliases())
                {
                    if (s.ToLower() == alias.ToLower())
                    {
                        out_x = c;
                        next = true;
                        break;
                    }
                }
                if (next == true) break;
            }
            return out_x;
        }

        /// <summary>
        /// Gets a command based on the specified value.
        /// </summary>
        /// <param name="value">The name or alias of the command.</param>
        /// <returns>The command with the specified value. Null if none.</returns>
        public Command GetCommand(string value)
        {
            var cname = GetCommandByName(value);
            var calias = GetCommandByAlias(value);
            Command out_x = null;
            if ((cname != null) && (calias == null))
                out_x = cname;
            else if ((cname == null) && (calias != null))
                out_x = calias;
            else
                out_x = null;
            return out_x;
        }

        /// <summary>
        /// Parses the input string and if a command exists it gets invoked.
        /// </summary>
        /// <param name="input">The string to parse containing command and args.</param>
        public void InvokeCommand(string input)
        {
            var x = input.Split(' ');
            var value = x[0];
            CommandArgs args = Util.ParseArguments(value, x);
            var cmd = GetCommand(value);
                if (value == "--version")
                {
                    if (CommandUtils.DebugMode)
                        Console.WriteLine("--version was found at the begining of the input string.");
                    if (CommandUtils.AllowFrameworkVersion)
                    {
                        if (CommandUtils.DebugMode)
                            Console.WriteLine("Set CommandArgs to empty.");
                        Console.WriteLine($"-=-=-=-=-=-=-=-=-=-=-=-=-\n" +
                        $"CommandFramework:\n" +
                        $"   Version:{new CommandFrameworkVersion().GetVersion().ToString()}\n" +
                        $"   GitHub Repo: https://github.com/WinMister332/CommandFramework" +
                        $"\n   License: M.I.T.\n" +
                        $"-=-=-=-=-=-=-=-=-=-=-=-=-");
                    }
                    return;
                }
                else
                {
                    if (cmd == null)
                    {
                        if (CommandUtils.UnknownCommandMessage == null || CommandUtils.UnknownCommandMessage == "")
                            Console.WriteLine($"\"{value}\", is not a valid internal or external command.");
                        else
                            Console.WriteLine(CommandUtils.UnknownCommandMessage);
                        return;
                    }
                    else
                    {
                        if ((args.IsEmpty() != true) && (args.GetArgAtPosition(0) == "--version"))
                        {
                            if (CommandUtils.DebugMode)
                                Console.WriteLine("The --version argument was found for the command.");
                            if ((cmd.CommandVersion() != null) && (cmd.Copyright() != null))
                            {
                                Console.WriteLine($"-=-=-=-=-=-=-=-=-=-=-\n" +
                                    $"{cmd.CommandName()}:\n" +
                                    $"  {cmd.Copyright().ToString()}\n" +
                                    $"  Version: {cmd.CommandVersion().ToString()}\n" +
                                    $"-=-=-=-=-=-=-=-=-=-=-");

                                return;
                            }
                            else if ((cmd.CommandVersion() != null) && (cmd.Copyright() == null))
                            {
                                Console.WriteLine($"-=-=-=-=-=-=-=-=-=-=-\n" +
                                    $"{cmd.CommandName()}:\n" +
                                    $"  Version: {cmd.CommandVersion().ToString()}\n" +
                                    $"-=-=-=-=-=-=-=-=-=-=-");

                                return;
                            }
                            else
                            {
                                //When TerminalInvoker is setup inject the command to be run.

                                return;
                            }
                        }
                        else
                        {
                            try
                            {
                                if (CommandUtils.DebugMode)
                                    Console.WriteLine("Executing command!");
                                cmd.OnCommandInvoked(this, args);
                                return;
                            }
                            catch (Exceptions.SyntaxException sex)
                            {
                                if (CommandUtils.DebugMode)
                                    Console.WriteLine($"Syntax Error:\n<> Required, [] Optional, | || OR\nUsage: {value} {cmd.CommandSynt()}\n\n[ERROR: {sex.ToString()}]");
                                else
                                    Console.WriteLine($"Incorrect Syntax:\n<> Required, [] Optional, | || OR\nUsage: {value} {cmd.CommandSynt()}");
                                return;
                            }
                        }
                    }
                }
        }
    }
}