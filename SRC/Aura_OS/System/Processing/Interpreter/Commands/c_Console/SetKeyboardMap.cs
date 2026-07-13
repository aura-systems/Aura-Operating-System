using Aura_OS.System.Processing.Interpreter;
using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Kernel.System.Keyboard;
using Cosmos.Kernel.System.Keyboard.ScanMaps;

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
            switch (arguments[0])
            {
                case "azerty":
                case "fr":
                    // gen3 ships only the US layout; the AZERTY table is ported into Aura itself.
                    KeyboardManager.SetKeyLayout(new Aura_OS.System.Input.FRStandardLayout());
                    break;

                case "qwerty":
                case "us":
                    KeyboardManager.SetKeyLayout(new USStandardLayout());
                    break;
                default:
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
            Console.WriteLine("- setkeyboardmap azerty");
            Console.WriteLine("- setkeyboardmap qwerty");
        }
    }
}
