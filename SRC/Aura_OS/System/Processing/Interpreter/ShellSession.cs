/*
* PROJECT:          Aura Operating System Development
* CONTENT:          A shell in a Console control (the Terminal's)
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Processing.Interpreter.Commands;

namespace Aura_OS.System.Processing.Interpreter
{
    /// <summary>
    /// A shell writing into a Console control: it runs command lines with its own commands, and while
    /// its app is focused, Console.Out writes into the console (Activate), output of a command or of
    /// anything else. clear empties the console, exit closes the app.
    /// </summary>
    public class ShellSession : IShellHost
    {
        public readonly Graphics.UI.GUI.Components.Console Console;

        private readonly CommandManager _commands;
        private readonly ShellWriter _writer;
        private readonly Action _exit;
        private bool _active;

        /// <summary>
        /// Session whose writer is Console.Out (null: Kernel.GuiSink). Console.SetOut is process-global,
        /// so with several sessions only the owner may give it back.
        /// </summary>
        private static ShellSession _owner;

        /// <param name="exit">Closes the app once the command line returns (the exit command).</param>
        public ShellSession(Application app, Graphics.UI.GUI.Components.Console console, Action exit)
        {
            Console = console;
            _exit = exit;
            _writer = new ShellWriter(console, app);

            // The constructor replaces the static command table: these clear and exit act on this session.
            _commands = new CommandManager(this);
            _commands.Initialize();
        }

        /// <summary>
        /// Runs a command line; its output goes to the console.
        /// </summary>
        public void Execute(string line)
        {
            Activate();
            _commands.Execute(line);
        }

        /// <summary>
        /// Console.Out writes into the console, until Deactivate.
        /// </summary>
        public void Activate()
        {
            if (!_active)
            {
                global::System.Console.SetOut(_writer);
                _writer.Enable();
                _owner = this;
                _active = true;
            }
        }

        /// <summary>
        /// Console.Out goes back to Aura's serial sink.
        /// </summary>
        public void Deactivate()
        {
            if (_active)
            {
                _writer.Disable();

                // GEN3-GAP(console-global): Console.SetOut is process-global. Restore Aura's serial sink, never
                // Console.Out (SetOut(Console.Out) is a no-op) and never the KernelConsole, which would paint
                // over the desktop. Skip it when another session already took Console.Out.
                if (_owner == this)
                {
                    if (Kernel.GuiSink != null)
                    {
                        global::System.Console.SetOut(Kernel.GuiSink);
                    }
                    else
                    {
                        global::System.Console.SetOut(global::System.IO.TextWriter.Null);
                    }

                    _owner = null;
                }

                _active = false;
            }
        }

        void IShellHost.ClearConsole()
        {
            Console.ClearText();
        }

        void IShellHost.Exit()
        {
            _exit();
        }
    }
}
