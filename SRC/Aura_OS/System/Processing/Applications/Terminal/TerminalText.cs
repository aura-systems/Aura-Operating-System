/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Terminal redirections
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Processing.Processes;
using System;
using System.IO;
using System.Text;

namespace Aura_OS.System.Processing.Applications.Terminal
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

                _terminal.Console.Draw();
                _terminal.Draw();
                Explorer.Screen.DrawImage(_terminal.Window.GetBuffer(), _terminal.Window.X, _terminal.Window.Y);
                Kernel.Canvas.DrawImage(Explorer.Screen.Bitmap, 0, 0);
                Kernel.Canvas.Display();
            }
        }

        public override void Write(string value)
        {
            if (_isEnabled)
            {
                _terminal.Console.Foreground = Console.ForegroundColor;
                _terminal.Console.Background = Console.BackgroundColor;

                _terminal.Console.Write(value);

                _terminal.Console.Draw();
                _terminal.Draw();
                Explorer.Screen.DrawImage(_terminal.Window.GetBuffer(), _terminal.Window.X, _terminal.Window.Y);
                Kernel.Canvas.DrawImage(Explorer.Screen.Bitmap, 0, 0);
                Kernel.Canvas.Display();
            }
        }

        public override void Write(char value)
        {
            if (_isEnabled)
            {
                _terminal.Console.Foreground = Console.ForegroundColor;
                _terminal.Console.Background = Console.BackgroundColor;

                _terminal.Console.Write(value.ToString());

                _terminal.Console.Draw();
                _terminal.Draw();
                Explorer.Screen.DrawImage(_terminal.Window.GetBuffer(), _terminal.Window.X, _terminal.Window.Y);
                Kernel.Canvas.DrawImage(Explorer.Screen.Bitmap, 0, 0);
                Kernel.Canvas.Display();
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
