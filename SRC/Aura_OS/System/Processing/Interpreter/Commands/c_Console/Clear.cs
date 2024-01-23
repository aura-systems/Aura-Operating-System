/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Clear
* PROGRAMMER(S):      John Welsh <djlw78@gmail.com>
*/

using System;

namespace Aura_OS.System.Processing.Interpreter.Commands.c_Console
{
    class CommandClear : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandClear(string[] commandvalues) : base(commandvalues)
        {
            Description = "to clear the console";
        }

        /// <summary>
        /// CommandClear
        /// </summary>
        public override ReturnInfo Execute()
        {
            Console.Clear();
            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}
