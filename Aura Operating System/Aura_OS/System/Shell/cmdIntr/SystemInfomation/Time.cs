/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Time
* PROGRAMMER(S):    Alexy DA CRUZ <dacruzalexy@gmail.com>
*/


using L = Aura_OS.System.Translation;

namespace Aura_OS.System.Shell.cmdIntr.SystemInfomation
{
    class CommandTime : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandTime(string[] commandvalues) : base(commandvalues)
        {
            Description = "to get time and date";
        }

        /// <summary>
        /// RebootCommand
        /// </summary>
        public override ReturnInfo Execute()
        {
            L.Text.Display("time");
            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}
