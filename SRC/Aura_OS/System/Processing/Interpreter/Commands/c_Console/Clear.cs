/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Clear
* PROGRAMMER(S):      John Welsh <djlw78@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Processing.Applications.Terminal;

namespace Aura_OS.System.Processing.Interpreter.Commands.c_Console
{
    class CommandClear : ICommand
    {
        private TerminalApp _terminal;
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandClear(string[] commandvalues, Application terminal) : base(commandvalues)
        {
            Description = "to clear the console";
            _terminal = terminal as TerminalApp;
        }

        /// <summary>
        /// CommandClear
        /// </summary>
        public override ReturnInfo Execute()
        {
            if (_terminal != null)
            {
                _terminal.Console.ClearText();
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}
