/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Clear
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

namespace Aura_OS.System.Processing.Interpreter.Commands.c_Console
{
    class CommandClear : ICommand
    {
        private IShellHost _host;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandClear(string[] commandvalues, IShellHost host) : base(commandvalues)
        {
            Description = "to clear the console";
            _host = host;
        }

        /// <summary>
        /// CommandClear
        /// </summary>
        public override ReturnInfo Execute()
        {
            if (_host != null)
            {
                _host.ClearConsole();
            }
            else
            {
                AuraPower.Shutdown();
            }

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}
