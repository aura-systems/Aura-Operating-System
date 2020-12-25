/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Base Command Class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;

namespace Aura_OS.System.Shell.cmdIntr
{

    /// <summary>
    /// Enum of all command types;
    /// </summary>
    public enum CommandType
    {
        Filesystem,
        Network,
        Utils,
        Unknown
    }

    /// <summary>
    /// Enum of all return codes that can be used by commands
    /// </summary>
    public enum ReturnCode
    {
        ERROR_ARG,
        ERROR,
        CRASH,
        OK
    }

    /// <summary>
    /// Class used to return info
    /// </summary>
    public class ReturnInfo
    {
        private ICommand _command;
        internal ICommand Command
        {
            get { return _command; }
        }

        private ReturnCode _code;
        internal ReturnCode Code
        {
            get { return _code; }
        }

        private string _info;
        public string Info
        {
            get { return _info; }
        }

        /// <summary>
        /// Constructor for ReturnInfo class
        /// </summary>
        /// <param name="command">Current command</param>
        /// <param name="code">Code returned by the command.</param>
        public ReturnInfo(ICommand command, ReturnCode code, string info = "Unknown error.")
        {
            _command = command;
            _code = code;
            _info = info;
        }
    }

    /// <summary>
    /// Base class for commands
    /// </summary>
    public class ICommand
    {
        /// <summary>
        /// Command type
        /// </summary>
        public CommandType Type { get; set; }

        /// <summary>
        /// Command description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Text values
        /// </summary>
        public string[] CommandValues;

        /// <summary>
        /// Command constructor
        /// </summary>
        public ICommand(string[] commandvalues, CommandType type = CommandType.Unknown)
        {
            CommandValues = commandvalues;
            Type = type;
            Description = "unknown";
        }

        /// <summary>
        /// Execute command without args
        /// </summary>
        /// <param name="txt">The string you wish to pass in. (to be echoed)</param>
        /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public virtual ReturnInfo Execute()
        {
            return new ReturnInfo(this, ReturnCode.ERROR_ARG);
        }

        /// <summary>
        /// Execute command
        /// </summary>
        /// <param name="txt">The string you wish to pass in. (to be echoed)</param>
        /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public virtual ReturnInfo Execute(List<string> arguments)
        {
            return new ReturnInfo(this, ReturnCode.ERROR_ARG);
        }

        /// <summary>
        /// Print help information
        /// </summary>
        public virtual void PrintHelp()
        {
            Console.WriteLine("No help information for this command!");
        }

        /// <summary>
        /// Used by the CommandManager
        /// </summary>
        public bool ContainsCommand(string command)
        {
            foreach (string commandvalue in CommandValues)
            {
                if (commandvalue == command)
                {
                    return true;
                }
            }
            return false;
        }

        public string CommandStarts(string cMDToComplete)
        {
            foreach (string value in CommandValues)
            {
                if (value.StartsWith(cMDToComplete))
                {
                    return value;
                }
            }
            return null;
        }
    }
}
