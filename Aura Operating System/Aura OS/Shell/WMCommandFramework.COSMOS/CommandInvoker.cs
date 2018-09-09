using System;
using System.Collections.Generic;
using WMCommandFramework.COSMOS.Commands;

namespace WMCommandFramework.COSMOS
{
    public class CommandInvoker
    {
        private List<Command> commands = null;

        /// <summary>
        /// Creates a new instance of the command invoker class and implements default commands.
        /// </summary>
        public CommandInvoker()
        {
            commands = new List<Command>();
            //Register Internal Commands.
            AddCommand(new Help());
        }

        /// <summary>
        /// Adds a command to the internal command registry.
        /// </summary>
        /// <param name="command">The command to register.</param>
        public void AddCommand(Command command)
        {
            commands.Add(command);
        }

        /// <summary>
        /// Adds a group of commands to the internal command registry.
        /// </summary>
        /// <param name="commands">The commands to register.</param>
        public void AddCommands(Command[] commands)
        {
            foreach (Command command in commands)
                AddCommand(command);
        }

        /// <summary>
        /// Removes a command to the internal command registry.
        /// </summary>
        /// <param name="command">The command to unregister.</param>
        public void RemoveCommand(Command command)
        {
            commands.Remove(command);
        }

        /// <summary>
        /// Removes a group of commands from the internal command registry.
        /// </summary>
        /// <param name="commands">The commands to unregister.</param>
        public void RemoveCommands(Command[] commands)
        {
            foreach (Command command in commands)
                RemoveCommand(command);
        }

        /// <summary>
        /// Gets a list of all commands in the command registery.
        /// </summary>
        /// <returns>A list of valid commands.</returns>
        public List<Command> GetCommands()
        {
            return commands;
        }

        /// <summary>
        /// Gets the first instance of a command with the specified name.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <returns>The command with the specified name.</returns>
        public Command GetCommandByName(string name)
        {
            Command result = null;
            foreach (Command command in GetCommands())
            {
                if (command.Name().ToLower() == name.ToLower())
                {
                    result = command;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the first instance of a command with the specified alias.
        /// </summary>
        /// <param name="alias">The alias of the command.</param>
        /// <returns>The command with the specified alias.</returns>
        public Command GetCommandByAlias(string alias)
        {
            Command result = null;
            foreach (Command command in GetCommands())
            {
                var allowBreak = false;
                foreach (string ax in command.Aliases())
                {
                    if (ax.ToLower() == alias.ToLower())
                    {
                        result = command;
                        allowBreak = true;
                        break;
                    }
                }
                if (allowBreak) break;
            }
            return result;
        }

        /// <summary>
        /// Gets the first instance of a command with the specified name or alias.
        /// </summary>
        /// <param name="value">The name or alias of the value.</param>
        /// <returns>The command with the specified name or alias.</returns>
        public Command GetCommand(string value)
        {
            var cname = GetCommandByName(value);
            var calias = GetCommandByAlias(value);
            if (cname != null && calias == null)
                return cname;
            else if (cname == null && calias != null)
                return calias;
            else if (cname != null && calias != null)
                return cname;
            else return null;
        }

        /// <summary>
        /// Takes a string of inputed text from a terminal and invokes the command with the specified name then parses its arguments.
        /// </summary>
        /// <param name="input">The input string containing the command and the arguments.</param>
        public void InvokeCommand(string input)
        {
            var dat = input.Split(' ');
            var value = dat[0];
            var args = Util.ParseArguments(value, dat);
            var cmd = GetCommand(value);
            if (cmd == null)
            {
                Console.WriteLine($"\"{value}\", is not a valid internal or external command!");
            }
            else
            {
                if (value.ToLower() == "--version")
                {
                    Console.WriteLine($"-=-=-=-=-=-=-=-=-=-=-\n" +
                        $"{CommandUtils.Version.Name()}\n" +
                        $"    {CommandUtils.Version.Copyright()}\n" +
                        $"    Version: {CommandUtils.Version.Version()}\n" +
                        $"-=-=-=-=-=-=-=-=-=-=-");
                }
                else
                {
                    if ((!(args.IsEmpty())) && (args.StartsWithSwitch("-version")))
                    {
                        if (!(cmd.Copyright() == new CommandCopyright()))
                        {
                            //Show Copyright.
                            Console.WriteLine($"-=-=-=-=-=-=-=-=-=-=-\n" +
                                $"{cmd.Name()}:" +
                                $"    {cmd.Copyright()}\n" +
                                $"    Version: {cmd.Version()}\n" +
                                $"-=-=-=-=-=-=-=-=-=-=-");
                        }
                        else
                        {
                            //Do not show Copyright.
                            Console.WriteLine($"-=-=-=-=-=-=-=-=-=-=-\n" +
                                $"{cmd.Name()}:" +
                                $"    Version: {cmd.Version()}\n" +
                                $"-=-=-=-=-=-=-=-=-=-=-");
                        }
                    }
                    else
                    {
                        if (cmd.Copyright() != new CommandCopyright())
                            Console.WriteLine(cmd.Copyright());
                        cmd.Invoke(this, args);
                        if (cmd.ThrowSyntax == true)
                        {
                            Console.WriteLine($"Incorrect Syntax:\n" +
                                $"(LEGEND: '<> REQUIRED', '[] OPTIONAL', '|, || OR', '&, && AND')\n" +
                                $"Usage: {value} {cmd.Syntax()}");
                        }
                    }
                }
            }
        }
    }

    public class Util
    {
        public static CommandArgs ParseArguments(string value, string[] rawIndex)
        {
            if ((value == null || value == "") || (rawIndex == null || rawIndex.Length == 0 || rawIndex == new string[0]))
                return new CommandArgs();
            List<string> x = new List<string>(rawIndex.Length);
            foreach (string s in rawIndex)
            {
                x.Add(s);
            }
            var index = x.IndexOf(value);
            x.RemoveAt(index);
            return new CommandArgs(x.ToArray());
        }
    }
}