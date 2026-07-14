using Aura_OS.System.Processing.Interpreter;
using System;
using System.Collections.Generic;

namespace Aura_OS.System.Processing.Interpreter.Commands.c_Console
{
    class CommandKeyboardMap : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandKeyboardMap(string[] commandvalues) : base(commandvalues)
        {
            Description = "to change keyboard map";
        }

        /// <summary>
        /// CommandEcho
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            string code = arguments[0];

            // gen2 aliases kept for muscle memory.
            if (code == "azerty")
            {
                code = "fr";
            }
            else if (code == "qwerty")
            {
                code = "us";
            }

            if (!Aura_OS.System.Input.KeyboardLayouts.Set(code))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "This keyboardmap isn't supported, please type: setkeyboardmap /help");
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Available keyboards map:");
            foreach (string code in Aura_OS.System.Input.KeyboardLayouts.Codes)
            {
                Console.WriteLine("- setkeyboardmap " + code.ToLower() + "    " + Aura_OS.System.Input.KeyboardLayouts.GetDisplayName(code));
            }
        }
    }
}
