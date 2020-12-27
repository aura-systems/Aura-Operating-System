using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace Aura_OS.System.Shell.cmdIntr.c_Console
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
                    Sys.KeyboardManager.SetKeyLayout(new Sys.ScanMaps.FR_Standard());
                    break;

                case "qwerty":
                    Sys.KeyboardManager.SetKeyLayout(new Sys.ScanMaps.US_Standard());
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
