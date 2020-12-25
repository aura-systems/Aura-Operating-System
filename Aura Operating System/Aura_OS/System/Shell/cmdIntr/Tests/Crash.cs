/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Crash
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using System;

namespace Aura_OS.System.Shell.cmdIntr.Tests
{
    class CommandCrash : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandCrash(string[] commandvalues) : base(commandvalues)
        {
            Description = "make a test crash";
        }

        /// <summary>
        /// RebootCommand
        /// </summary>
        public override ReturnInfo Execute()
        {
            throw new Exception("Crash test");
        }
    }
}
