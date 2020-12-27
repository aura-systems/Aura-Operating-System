/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Version
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.SystemInfomation
{
    class CommandVersion : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandVersion(string[] commandvalues) : base(commandvalues)
        {
            Description = "to display system version";
        }

        /// <summary>
        /// RebootCommand
        /// </summary>
        public override ReturnInfo Execute()
        {
            L.Text.Display("about");
            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}
