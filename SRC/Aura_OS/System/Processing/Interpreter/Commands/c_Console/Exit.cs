/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Exit
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

namespace Aura_OS.System.Processing.Interpreter.Commands.c_Console
{
    class CommandExit : ICommand
    {
        private IShellHost _host;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandExit(string[] commandvalues, IShellHost host) : base(commandvalues)
        {
            Description = "to exit the console";
            _host = host;
        }

        /// <summary>
        /// CommandClear
        /// </summary>
        public override ReturnInfo Execute()
        {
            if (_host != null)
            {
                _host.Exit();
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}
