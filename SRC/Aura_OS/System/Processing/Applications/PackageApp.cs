/*
* PROJECT:          Aura Operating System Development
* CONTENT:          App from a package (.pkg): a layout file and Lua code
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.Executable.Lua;
using Cosmos.Kernel.System.Keyboard;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Processing.Interpreter;
using Aura_OS.System.Processing.Lua;

namespace Aura_OS.System.Processing.Applications
{
    /// <summary>
    /// An app whose window is its package's layout file and whose behaviour is its package's Lua:
    /// the main file runs once when the app opens, then the handlers it gave run on the layout's
    /// events (app:on), its timers (app:every), the keys typed while it is focused (app:onKey) and
    /// its resizes (app:onResize), on the UI thread. os.exit in a handler closes the app.
    /// SRC/Packages/README.md describes the Lua side.
    /// </summary>
    public class PackageApp : Application
    {
        public readonly Package Package;

        private readonly LuaInterpreter _lua;

        // Event name -> handler function (a registry reference).
        private readonly Dictionary<string, int> _handlers = new Dictionary<string, int>();

        /// <summary>
        /// A handler app:every calls every Interval milliseconds.
        /// </summary>
        private sealed class Timer
        {
            public int Reference;
            public long Interval;
            public long Next;
        }

        private readonly List<Timer> _timers = new List<Timer>();

        // app:onKey and app:onResize handlers (registry references), LuaConstants.LUA_NOREF for none.
        private int _keyHandler = LuaConstants.LUA_NOREF;
        private int _resizeHandler = LuaConstants.LUA_NOREF;

        /// <summary>
        /// The app's shell (aura.shell.open), null without one: Console.Out writes into its console
        /// while the app is focused.
        /// </summary>
        internal ShellSession Shell { get; private set; }

        private bool _disposed;

        // A handler called os.exit: the app closes after its update.
        private bool _exited;

        /// <param name="args">The main file's arguments (arg[1]... and ...), as a file to open; null for none.</param>
        /// <exception cref="InvalidDataException">The layout file is wrong.</exception>
        /// <exception cref="InvalidOperationException">The main file failed.</exception>
        public PackageApp(Package package, int x = 0, int y = 0, List<string> args = null) : base(package.LoadLayout(), x, y)
        {
            Package = package;

            Exception error = null;

            try
            {
                _lua = AuraLua.Create();

                LuaPackage.Load(_lua, package, this);
                LuaPackage.RunMain(_lua, package, args ?? new List<string>());
            }
            catch (Exception ex)
            {
                // One clause: gen3 kernels up to 3.0.89 enter the first typed catch whatever the type.
                error = ex;
            }

            if (error != null)
            {
                if (_lua != null)
                {
                    _lua.Dispose();
                }

                LuaException luaError = error as LuaException;
                LuaExitException exit = error as LuaExitException;
                string message;

                if (exit != null)
                {
                    message = "exited with code " + exit.ExitCode + " before its window opened.";
                }
                else if (luaError != null && luaError.LuaStackTrace != null)
                {
                    message = luaError.Message + "\n" + luaError.LuaStackTrace;
                }
                else
                {
                    message = error.Message;
                }

                throw new InvalidOperationException(package.Name + ": " + message, error);
            }
        }

        /// <summary>
        /// Calls the Lua function (a registry reference) on the layout's event of that name.
        /// </summary>
        internal void SetHandler(string name, int reference)
        {
            int previous;

            if (_handlers.TryGetValue(name, out previous))
            {
                _lua.State.L_Unref(LuaDef.LUA_REGISTRYINDEX, previous);
            }

            _handlers[name] = reference;
            On(name, () => Call(name, reference));
        }

        /// <summary>
        /// Calls the Lua function (a registry reference) with each key typed while the app is focused.
        /// </summary>
        internal void SetKeyHandler(int reference)
        {
            Unref(_keyHandler);
            _keyHandler = reference;
        }

        /// <summary>
        /// Calls the Lua function (a registry reference) once the window was resized and its controls
        /// placed again.
        /// </summary>
        internal void SetResizeHandler(int reference)
        {
            Unref(_resizeHandler);
            _resizeHandler = reference;
        }

        /// <summary>
        /// Opens the app's shell in one of its Console controls.
        /// </summary>
        /// <exception cref="InvalidOperationException">The app already has one.</exception>
        internal ShellSession OpenShell(Graphics.UI.GUI.Components.Console console)
        {
            if (Shell != null)
            {
                throw new InvalidOperationException("the app already has a shell");
            }

            Shell = new ShellSession(this, console, RequestExit);
            return Shell;
        }

        /// <summary>
        /// Closes the app once its update is over: safe from a handler, unlike Dispose.
        /// </summary>
        internal void RequestExit()
        {
            _exited = true;
        }

        private void Unref(int reference)
        {
            if (reference != LuaConstants.LUA_NOREF && !_disposed)
            {
                _lua.State.L_Unref(LuaDef.LUA_REGISTRYINDEX, reference);
            }
        }

        /// <summary>
        /// Calls the Lua function (a registry reference) every interval milliseconds, the first time
        /// one interval from now.
        /// </summary>
        internal void AddTimer(long interval, int reference)
        {
            _timers.Add(new Timer
            {
                Reference = reference,
                Interval = interval,
                Next = Environment.TickCount64 + interval,
            });
        }

        /// <summary>
        /// Calls the timers that are due. One whose handler fails stops: it would fail again every time.
        /// </summary>
        private void RunTimers()
        {
            long now = Environment.TickCount64;

            // By index: a handler can add a timer.
            for (int i = 0; i < _timers.Count && !_disposed && !_exited; i++)
            {
                Timer timer = _timers[i];

                if (now < timer.Next)
                {
                    continue;
                }

                // From now rather than from the last due time: a slow frame does not queue up calls.
                timer.Next = now + timer.Interval;

                if (!Call("every " + timer.Interval + " ms", timer.Reference))
                {
                    _timers.RemoveAt(i);
                    _lua.State.L_Unref(LuaDef.LUA_REGISTRYINDEX, timer.Reference);
                    i--;
                }
            }
        }

        /// <summary>
        /// Passes the typed keys to the key handler, one call each.
        /// </summary>
        private void ReadKeys()
        {
            KeyEvent key;

            while (_keyHandler != LuaConstants.LUA_NOREF && !_disposed && !_exited && Input.KeyboardManager.TryGetKey(out key))
            {
                // GEN3-GAP(null-deref): a null event would halt the kernel.
                if (key != null)
                {
                    Call("key", _keyHandler, lua => PushKey(lua, key));
                }
            }
        }

        /// <summary>
        /// A key as the handler gets it: { name = "enter", char = "a", ctrl = , shift = , alt = }.
        /// </summary>
        private static int PushKey(ILuaState lua, KeyEvent key)
        {
            lua.CreateTable(0, 5);

            string name = KeyName(key.Key);
            if (name != null)
            {
                lua.PushString(name);
                lua.SetField(-2, "name");
            }

            char c = key.KeyChar;
            if (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsSymbol(c) || c == ' ')
            {
                lua.PushString(LuaText.Encode(c.ToString()));
                lua.SetField(-2, "char");
            }

            lua.PushBoolean(KeyboardManager.ControlPressed || (key.Modifiers & ConsoleModifiers.Control) != 0);
            lua.SetField(-2, "ctrl");
            lua.PushBoolean(KeyboardManager.ShiftPressed || (key.Modifiers & ConsoleModifiers.Shift) != 0);
            lua.SetField(-2, "shift");
            lua.PushBoolean(KeyboardManager.AltPressed || (key.Modifiers & ConsoleModifiers.Alt) != 0);
            lua.SetField(-2, "alt");
            return 1;
        }

        /// <summary>
        /// The name of a key that types no character, null for the others. A switch, not
        /// Enum.ToString: NativeAOT keeps no enum names.
        /// </summary>
        private static string KeyName(ConsoleKeyEx key)
        {
            switch (key)
            {
                case ConsoleKeyEx.Enter:
                    return "enter";
                case ConsoleKeyEx.Backspace:
                    return "backspace";
                case ConsoleKeyEx.Tab:
                    return "tab";
                case ConsoleKeyEx.Escape:
                    return "escape";
                case ConsoleKeyEx.UpArrow:
                    return "up";
                case ConsoleKeyEx.DownArrow:
                    return "down";
                case ConsoleKeyEx.LeftArrow:
                    return "left";
                case ConsoleKeyEx.RightArrow:
                    return "right";
                case ConsoleKeyEx.Home:
                    return "home";
                case ConsoleKeyEx.End:
                    return "end";
                case ConsoleKeyEx.PageUp:
                    return "pageUp";
                case ConsoleKeyEx.PageDown:
                    return "pageDown";
                case ConsoleKeyEx.Insert:
                    return "insert";
                case ConsoleKeyEx.Delete:
                    return "delete";
            }

            return null;
        }

        /// <summary>
        /// Runs a handler. Its error is logged with the Lua traceback, and the app goes on.
        /// </summary>
        /// <param name="pushArguments">Pushes the handler's arguments and returns how many, null for none.</param>
        /// <returns>False when the handler raised an error.</returns>
        private bool Call(string name, int reference, Func<ILuaState, int> pushArguments = null)
        {
            if (_disposed || _exited)
            {
                return true;
            }

            bool succeeded = true;

            ILuaState state = _lua.State;
            int top = state.GetTop();

            state.PushCSharpFunction(Traceback);
            state.RawGetI(LuaDef.LUA_REGISTRYINDEX, reference);
            int arguments = pushArguments != null ? pushArguments(state) : 0;

            try
            {
                if (state.PCall(arguments, 0, top + 1) != ThreadStatus.LUA_OK)
                {
                    Logs.DoOSLog("[Error] " + Package.Name + ": " + name + ": " + LuaText.Decode(state.ToString(-1) ?? "(error object is not a string)"));
                    succeeded = false;
                }
            }
            catch (Exception ex)
            {
                // One clause: gen3 kernels up to 3.0.89 enter the first typed catch whatever the type.
                // os.exit is the one error no protected call keeps: it reaches here as LuaExitException.
                if (ex is LuaExitException)
                {
                    _exited = true;
                }
                else
                {
                    Logs.DoOSLog("[Error] " + Package.Name + ": " + name + ": " + ex.Message);
                    succeeded = false;
                }
            }

            state.SetTop(top);

            // The handler changed controls: draw the window again.
            MarkDirty();
            return succeeded;
        }

        public override void Update()
        {
            // Before the base update, which places the controls again when a text got longer or shorter.
            RunTimers();

            base.Update();

            if (Focused && !_disposed)
            {
                if (Shell != null)
                {
                    Shell.Activate();
                }

                ReadKeys();
            }
            else if (Shell != null)
            {
                Shell.Deactivate();
            }

            // Closed here rather than in the handler, which runs in the middle of the controls' updates.
            if (_exited && !_disposed)
            {
                Dispose();
            }
        }

        public override void ResizeWindow(int width, int height)
        {
            base.ResizeWindow(width, height);

            // Placed now rather than at the next draw: the handler sees the controls' new sizes.
            Layout.Arrange();

            if (_resizeHandler != LuaConstants.LUA_NOREF)
            {
                Call("resize", _resizeHandler);
            }
        }

        public override void Stop()
        {
            // Minimized or closed: Update no longer runs, give Console.Out back now.
            if (Shell != null)
            {
                Shell.Deactivate();
            }

            base.Stop();
        }

        /// <summary>
        /// Message handler of the event calls: the error with the stack traceback.
        /// </summary>
        private static int Traceback(ILuaState lua)
        {
            lua.L_Traceback(lua, lua.ToString(1) ?? "(error object is not a string)", 1);
            return 1;
        }

        public override void Dispose()
        {
            base.Dispose();

            // GEN3-GAP(finalizers): Dispose closes the files the app left open.
            if (!_disposed)
            {
                _disposed = true;
                _lua.Dispose();
            }
        }
    }
}
