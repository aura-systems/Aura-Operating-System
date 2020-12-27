/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Clear
* PROGRAMMER(S):      John Welsh <djlw78@gmail.com>
*/

//NOTE: Console conflicted with Console so now it is c_Console. (Still readable)
using System;
namespace Aura_OS.System.Shell.cmdIntr.c_Console
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
