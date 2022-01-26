using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Shell.cmdIntr
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
