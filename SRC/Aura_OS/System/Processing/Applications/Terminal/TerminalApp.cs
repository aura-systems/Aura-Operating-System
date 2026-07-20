/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Graphical terminal application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Users;
using Cosmos.Kernel.System.Keyboard;
using System;
using System.Collections.Generic;
using Aura_OS.Processing;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Processing.Interpreter.Commands;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Processing.Processes;

namespace Aura_OS.System.Processing.Applications.Terminal
{
    public class TerminalApp : Application
    {
        private static string ApplicationName = "Terminal";

        public Graphics.UI.GUI.Components.Console Console;

        private CommandManager _commandManager;
        private List<string> _commands;
        private int _commandIndex;
        private string _command;

        private bool _redirect = false;
        private TerminalTextWriter _writer;

        // Commands run on the app's Work() thread so a slow one (long output,
        // network, compute) no longer freezes the desktop. Update() (UI thread)
        // enqueues the command and marks the terminal busy; Work() drains the
        // queue and executes. While busy the terminal ignores further command
        // input, but the compositor and every other app stay responsive.
        private readonly Queue<string> _pending = new Queue<string>();
        private readonly SpinGuard _pendingGuard = new SpinGuard();
        private volatile bool _busy = false;

        public TerminalApp(int width, int height, int x = 0, int y = 0) : base(ApplicationName, width, height, x, y)
        {
            RunsWorker = true;
            Window.Icon = Kernel.ResourceManager.GetIcon("16-terminal.bmp");

            Console = new(4, Window.TopBar.Height + 6, width - 7, height - Window.TopBar.Height - 9);
            AddChild(Console);

            _commandManager = new CommandManager(this);
            _commandManager.Initialize();
            _commandIndex = 0;
            _commands = new List<string>();
            _command = string.Empty;
            _writer = new TerminalTextWriter(this);

            BeforeCommand();

            MarkDirty();
        }

        public override void Update()
        {
            base.Update();

            if (Focused)
            {
                ActivateRedirection();

                KeyEvent keyEvent = null;

                // While a command runs on the Work() thread, don't process command
                // input (leave keys queued, exactly as when the old synchronous
                // Execute blocked the UI). The desktop stays responsive because the
                // command is off the UI thread.
                if (_busy)
                {
                    return;
                }

                if (Input.KeyboardManager.TryGetKey(out keyEvent))
                {
                    MarkDirty();

                    switch (keyEvent.Key)
                    {
                        case ConsoleKeyEx.Enter:
                            if (Console.ScrollMode)
                            {
                                break;
                            }
                            if (_command.Length > 0)
                            {
                                Console.mX -= _command.Length;

                                Console.ScrollMode = true;

                                global::System.Console.WriteLine(_command);

                                // History is UI-thread-only; add here so Work()
                                // never touches _commands concurrently with the
                                // Up/Down history navigation below.
                                _commands.Add(_command);
                                _commandIndex = _commands.Count - 1;

                                // Hand the command to the Work() thread and mark
                                // busy; Work() prints the next prompt when it's done.
                                _pendingGuard.Enter();
                                try
                                {
                                    _pending.Enqueue(_command);
                                }
                                finally
                                {
                                    _pendingGuard.Exit();
                                }
                                _busy = true;

                                _command = string.Empty;

                                MarkDirty();

                                break;
                            }
                            else
                            {
                                global::System.Console.WriteLine();
                                global::System.Console.WriteLine();
                            }

                            BeforeCommand();

                            MarkDirty();

                            break;
                        case ConsoleKeyEx.Backspace:
                            if (Console.ScrollMode)
                            {
                                break;
                            }
                            if (_command.Length > 0)
                            {
                                _command = _command.Remove(_command.Length - 1);
                                Console.mX--;
                            }
                            break;
                        case ConsoleKeyEx.UpArrow:
                            if (KeyboardManager.ControlPressed)
                            {
                                Console.ScrollUp();
                            }
                            else
                            {
                                if (_commandIndex >= 0)
                                {
                                    Console.mX -= _command.Length;
                                    _command = _commands[_commandIndex];
                                    _commandIndex--;
                                    Console.mX += _command.Length;
                                }
                            }
                            break;
                        case ConsoleKeyEx.DownArrow:
                            if (KeyboardManager.ControlPressed)
                            {
                                Console.ScrollDown();
                            }
                            else
                            {
                                if (_commandIndex < _commands.Count - 1)
                                {
                                    Console.mX -= _command.Length;
                                    _commandIndex++;
                                    _command = _commands[_commandIndex];
                                    Console.mX += _command.Length;
                                }
                            }
                            break;
                        default:
                            if (Console.ScrollMode)
                            {
                                break;
                            }
                            if (char.IsLetterOrDigit(keyEvent.KeyChar) || char.IsPunctuation(keyEvent.KeyChar) || char.IsSymbol(keyEvent.KeyChar) || keyEvent.KeyChar == ' ')
                            {
                                _command += keyEvent.KeyChar;
                                Console.mX++;
                            }
                            break;
                    }
                }
            }
            else
            {
                DeactivateRedirection();
            }
        }

        /// <summary>
        /// Work() thread: execute one queued command off the UI thread, then print
        /// the next prompt and clear the busy flag. Command output reaches this
        /// terminal's Console through the global Console redirection set by
        /// ActivateRedirection while this terminal is focused.
        /// </summary>
        protected override void Work()
        {
            string command = null;

            _pendingGuard.Enter();
            try
            {
                if (_pending.Count > 0)
                {
                    command = _pending.Dequeue();
                }
            }
            finally
            {
                _pendingGuard.Exit();
            }

            if (command == null)
            {
                return;
            }

            _commandManager.Execute(command);

            Console.ScrollMode = false;

            BeforeCommand();

            MarkDirty();

            _busy = false;
        }

        public override void Draw()
        {
            base.Draw();

            Console.Draw();

            if (!Console.ScrollMode)
            {
                if (_command.Length > 0)
                {
                    int baseX = Console.mX - _command.Length;

                    for (int i = 0; i < _command.Length; i++)
                    {
                        Console.WriteByte(_command[i],  (baseX + i) * Kernel.font.Width, Console.mY * Kernel.font.Height, (uint)Console.ForegroundColor.ToArgb());
                    }
                }

                Console.DrawCursor();
            }

            Console.DrawInParent();
        }

        public override void ResizeWindow(int width, int height)
        {
            base.ResizeWindow(width, height);

            int newWidth = width - 7;
            int newHeight = height - Window.TopBar.Height - 9;

            Console.Resize(newWidth, newHeight);
            Console.InitConsole(newWidth, newHeight);
            BeforeCommand();
        }

        public override void MarkDirty()
        {
            base.MarkDirty();
            Console.MarkDirty();
        }

        public void ActivateRedirection()
        {
            if (_redirect == false)
            {
                global::System.Console.SetOut(_writer);
                _writer.Enable();
                _redirect = true;
            }
        }

        public void DeactivateRedirection()
        {
            if (_redirect == true)
            {
                // global::System.Console.SetOut(global::System.Console.Out);
                _writer.Disable();
                _redirect = false;
            }
        }

        #region BeforeCommand

        /// <summary>
        /// Display the line before the user input and set the console color.
        /// </summary>
        public void BeforeCommand()
        {
            Console.Foreground = ConsoleColor.Blue;
            Console.Write(UserLevel.TypeUser);

            Console.Foreground = ConsoleColor.Yellow;
            Console.Write(Kernel.userLogged);

            Console.Foreground = ConsoleColor.DarkGray;
            Console.Write("@");

            Console.Foreground = ConsoleColor.Blue;
            Console.Write(Kernel.ComputerName);

            Console.Foreground = ConsoleColor.Gray;
            Console.Write("> ");

            Console.Foreground = ConsoleColor.DarkGray;
            Console.Write(Kernel.CurrentDirectory + "~ ");

            Console.Foreground = ConsoleColor.White;
        }

        #endregion
    }
}
