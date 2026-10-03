/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - what clear and exit act on
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

namespace Aura_OS.System.Processing.Interpreter.Commands
{
    /// <summary>
    /// The console a shell's commands write to: the Terminal's (ShellSession). clear and exit act on it.
    /// </summary>
    public interface IShellHost
    {
        /// <summary>
        /// Empties the console.
        /// </summary>
        void ClearConsole();

        /// <summary>
        /// Closes the app the shell runs in, once the command line returns.
        /// </summary>
        void Exit();
    }
}
