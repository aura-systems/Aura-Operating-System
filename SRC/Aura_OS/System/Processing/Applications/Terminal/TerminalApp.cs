/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Graphical terminal application. The window is Resources/UI/Layouts/Terminal.xml.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Users;
using Cosmos.Kernel.System.Keyboard;
using System;
using System.Collections.Generic;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Processing.Interpreter.Commands;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Graphics.UI.GUI.Layout;
using Aura_OS.System.Processing.Processes;

namespace Aura_OS.System.Processing.Applications.Terminal
{
    public class TerminalApp : Application
    {
        public Graphics.UI.GUI.Components.Console Console;

        private CommandManager _commandManager;
        private List<string> _commands;
        private int _commandIndex;
        private string _command;

        private bool _redirect = false;
        private TerminalTextWriter _writer;

        /// <summary>
        /// Terminal whose writer is currently installed as Console.Out (null: Kernel.GuiSink).
        /// Console.SetOut is process-global, so with several terminals only the owner may restore the sink.
        /// </summary>
        private static TerminalApp _redirectOwner;

        public TerminalApp(int x = 0, int y = 0) : base(AppLayout.Load("Terminal"), x, y)
        {
            Console = Find<Graphics.UI.GUI.Components.Console>("console");

            _commandManager = new CommandManager(this);
            _commandManager.Initialize();
            _commandIndex = 0;
            _commands = new List<string>();
            _command = string.Empty;
            _writer = new TerminalTextWriter(this);

            BeforeCommand();
        }

        public override void Update()
        {
            base.Update();

            if (Focused)
            {
                ActivateRedirection();

                KeyEvent keyEvent = null;

                if (Input.KeyboardManager.TryGetKey(out keyEvent))
                {
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

                                _commandManager.Execute(_command);

                                Console.ScrollMode = false;

                                _commands.Add(_command);
                                _commandIndex = _commands.Count - 1;

                                _command = string.Empty;
                            }
                            else
                            {
                                global::System.Console.WriteLine();
                                global::System.Console.WriteLine();
                            }

                            BeforeCommand();

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
                                // Count check: Up on an empty history indexed _commands[0] (crash screen, C6).
                                if (_commandIndex >= 0 && _commandIndex < _commands.Count)
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

                    // Redraws the console only (the window manager draws it into the window).
                    Console.Input = _command;
                }
            }
            else
            {
                DeactivateRedirection();
            }
        }

        /// <summary>
        /// The console fills the resized window now: it starts over empty, with the prompt and
        /// the command being typed.
        /// </summary>
        public override void ResizeWindow(int width, int height)
        {
            base.ResizeWindow(width, height);

            int consoleWidth = Console.Width;
            int consoleHeight = Console.Height;

            Layout.Arrange();

            if (Console.Width != consoleWidth || Console.Height != consoleHeight)
            {
                BeforeCommand();
                Console.mX += _command.Length;
                Console.Input = _command;
            }
        }

        public override void Stop()
        {
            // Minimized or closed: Update() no longer runs, give Console.Out back to the GUI sink now.
            DeactivateRedirection();

            base.Stop();
        }

        public void ActivateRedirection()
        {
            if (_redirect == false)
            {
                global::System.Console.SetOut(_writer);
                _writer.Enable();
                _redirectOwner = this;
                _redirect = true;
            }
        }

        public void DeactivateRedirection()
        {
            if (_redirect == true)
            {
                _writer.Disable();

                // GEN3-GAP(console-global): Console.SetOut is process-global. Restore Aura's serial sink, never
                // Console.Out (SetOut(Console.Out) is a no-op) and never the KernelConsole, which would paint
                // over the desktop. Skip it when another terminal already took Console.Out.
                if (_redirectOwner == this)
                {
                    if (Kernel.GuiSink != null)
                    {
                        global::System.Console.SetOut(Kernel.GuiSink);
                    }
                    else
                    {
                        global::System.Console.SetOut(global::System.IO.TextWriter.Null);
                    }

                    _redirectOwner = null;
                }

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
