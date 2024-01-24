/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Terminal redirections
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using System.Text;

namespace Aura_OS.System.Processing.Application.Terminal
{
    public class TerminalTextWriter : TextWriter
    {
        private TerminalApp _terminal;
        private bool _isEnabled;

        public TerminalTextWriter(TerminalApp terminal)
        {
            _terminal = terminal;
            _isEnabled = true;
        }

        public override void WriteLine(string value)
        {
            if (_isEnabled)
            {
                _terminal.Console.Foreground = Console.ForegroundColor;
                _terminal.Console.Background = Console.BackgroundColor;

                _terminal.Console.WriteLine(value);
            }
        }

        public override void Write(char value)
        {
            if (_isEnabled)
            {
                _terminal.Console.Foreground = Console.ForegroundColor;
                _terminal.Console.Background = Console.BackgroundColor;

                _terminal.Console.Write(value.ToString());
            }
        }

        public override Encoding Encoding => Encoding.ASCII;

        public void Enable()
        {
            _isEnabled = true;
        }

        public void Disable()
        {
            _isEnabled = false;
        }
    }
}
