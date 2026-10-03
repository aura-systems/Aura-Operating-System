/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Console.Out of a shell session
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Processing.Processes;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Aura_OS.System.Processing.Interpreter
{
    /// <summary>
    /// Console.Out of a focused shell session (ShellSession): writes into its Console control, and
    /// draws its app's window while a command runs. Installed with Console.SetOut, so no member may
    /// throw: gen3 does not run finally/lock release on the exception path and the SyncTextWriter
    /// monitor would stay held (C7). Every body is wrapped in try/catch.
    /// </summary>
    public class ShellWriter : TextWriter
    {
        private readonly Graphics.UI.GUI.Components.Console _console;
        private readonly Application _app;
        private bool _isEnabled;

        /// <summary>
        /// Minimum Stopwatch ticks between two presents (~16 ms).
        /// </summary>
        private long _presentInterval;

        /// <summary>
        /// Stopwatch timestamp of the last present.
        /// </summary>
        private long _lastPresent;

        /// <summary>
        /// True while the terminal is being drawn (a re-entrant write only appends text).
        /// </summary>
        private bool _drawing;

        public ShellWriter(Graphics.UI.GUI.Components.Console console, Application app)
        {
            _console = console;
            _app = app;
            _isEnabled = true;
            _presentInterval = Stopwatch.Frequency / 60;
            _lastPresent = 0;
            _drawing = false;
        }

        public override void WriteLine(string value)
        {
            try
            {
                if (_isEnabled)
                {
                    _console.Foreground = Console.ForegroundColor;
                    _console.Background = Console.BackgroundColor;

                    _console.WriteLine(value);

                    Refresh();
                }
            }
            catch (Exception)
            {
            }
        }

        public override void Write(string value)
        {
            try
            {
                if (_isEnabled && value != null)
                {
                    _console.Foreground = Console.ForegroundColor;
                    _console.Background = Console.BackgroundColor;

                    _console.Write(value);

                    Refresh();
                }
            }
            catch (Exception)
            {
            }
        }

        public override void Write(char value)
        {
            try
            {
                if (_isEnabled)
                {
                    _console.Foreground = Console.ForegroundColor;
                    _console.Background = Console.BackgroundColor;

                    _console.Write(value.ToString());

                    Refresh();
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Used by WriteLine() and char[] writes (otherwise split into one Write(char) per char).
        /// </summary>
        public override void Write(char[] buffer, int index, int count)
        {
            try
            {
                if (_isEnabled && buffer != null && count > 0)
                {
                    _console.Foreground = Console.ForegroundColor;
                    _console.Background = Console.BackgroundColor;

                    _console.Write(new string(buffer, index, count));

                    Refresh();
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Used by span writes (otherwise split into one Write(char) per char).
        /// </summary>
        public override void Write(ReadOnlySpan<char> buffer)
        {
            try
            {
                if (_isEnabled && buffer.Length > 0)
                {
                    _console.Foreground = Console.ForegroundColor;
                    _console.Background = Console.BackgroundColor;

                    _console.Write(new string(buffer));

                    Refresh();
                }
            }
            catch (Exception)
            {
            }
        }

        public override Encoding Encoding => Encoding.UTF8;

        public void Enable()
        {
            _isEnabled = true;
        }

        public void Disable()
        {
            _isEnabled = false;
        }

        /// <summary>
        /// Draw the app's window on screen and present it, at most once every ~16 ms.
        /// </summary>
        private void Refresh()
        {
            // A skipped refresh is repainted by the main loop on its next frame.
            _console.MarkDirty();

            if (_drawing)
            {
                return;
            }

            // GEN3-GAP(present): no partial Canvas.Display(rect), every present copies the whole frame,
            // so a burst of writes presents at most once per ~16 ms (the main loop presents each frame anyway).
            long now = Stopwatch.GetTimestamp();
            if (now - _lastPresent < _presentInterval)
            {
                return;
            }

            _lastPresent = now;
            _drawing = true;

            try
            {
                // The console into the window (the rest of the window does not change).
                _console.Draw(_app.Window);

                if (Explorer.Screen != null)
                {
                    Explorer.Screen.DrawCanvas(_app.Window.GetBuffer(), _app.Window.X, _app.Window.Y);
                }

                Kernel.Present();
            }
            catch (Exception)
            {
            }

            _drawing = false;
        }
    }
}
