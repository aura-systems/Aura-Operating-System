using Aura_OS.System.Processing.Interpreter;
using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Kernel.System;
using Aura_OS.System.Input;

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
            if (arguments == null || arguments.Count == 0)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            if (!KernelFeatures.Keyboard)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "Keyboard support is disabled in this kernel.");
            }

            // Accepts a layout code (us, gb, fr, de, es, tr, dv) or an alias (azerty, qwerty, qwertz, dvorak).
            if (!KeyboardLayouts.Set(arguments[0]))
            {
                return new ReturnInfo(this, ReturnCode.ERROR, "This keyboardmap isn't supported, please type: setkeyboardmap /help");
            }

            KeyboardLayouts.Persist();

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Available keyboards map:");
            foreach (string code in KeyboardLayouts.Codes)
            {
                Console.WriteLine("- setkeyboardmap " + code.ToLowerInvariant() + " (" + KeyboardLayouts.GetDisplayName(code) + ")");
            }

            Console.WriteLine("Aliases:");
            foreach (string alias in KeyboardLayouts.Aliases)
            {
                string target = KeyboardLayouts.ResolveCode(alias);
                if (target != null)
                {
                    Console.WriteLine("- setkeyboardmap " + alias + " (= " + target.ToLowerInvariant() + ")");
                }
            }
        }
    }
}
