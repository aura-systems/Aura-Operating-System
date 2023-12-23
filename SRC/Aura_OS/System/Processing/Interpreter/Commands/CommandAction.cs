/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Base class for classless commands
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;

namespace Aura_OS.System.Processing.Interpreter.Commands
{
    class CommandAction : ICommand
    {

        private Action _action;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandAction(string[] commandvalues, Action action) : base(commandvalues)
        {
            _action = action;
        }

        /// <summary>
        /// RebootCommand
        /// </summary>
        public override ReturnInfo Execute()
        {
            _action();
            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}
