using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Shell.cmdIntr.Util
{
    class CommandAbout : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandAbout(string[] commandvalues) : base(commandvalues)
        {
            Description = "to show informations about Aura Operating System";
        }

        /// <summary>
        /// RebootCommand
        /// </summary>
        public override ReturnInfo Execute()
        {
            Aura_OS.System.Translation.List_Translation.About();
            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}
