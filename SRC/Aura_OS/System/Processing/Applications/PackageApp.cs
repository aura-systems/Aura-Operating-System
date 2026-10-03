/*
* PROJECT:          Aura Operating System Development
* CONTENT:          App from a package (.pkg): a layout file and Lua code
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.Executable.Lua;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Processing.Lua;

namespace Aura_OS.System.Processing.Applications
{
    /// <summary>
    /// An app whose window is its package's layout file and whose behaviour is its package's Lua:
    /// the main file runs once when the app opens, then the handlers it gave with app:on run on the
    /// layout's events and its timers (app:every), on the UI thread. os.exit in a handler closes the
    /// app. SRC/Packages/README.md describes the Lua side.
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

        private bool _disposed;

        // A handler called os.exit: the app closes after its update.
        private bool _exited;

        /// <exception cref="InvalidDataException">The layout file is wrong.</exception>
        /// <exception cref="InvalidOperationException">The main file failed.</exception>
        public PackageApp(Package package, int x = 0, int y = 0) : base(package.LoadLayout(), x, y)
        {
            Package = package;

            Exception error = null;

            try
            {
                _lua = AuraLua.Create();

                LuaPackage.Load(_lua, package, this);
                LuaPackage.RunMain(_lua, package, new List<string>());
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
        /// Runs a handler. Its error is logged with the Lua traceback, and the app goes on.
        /// </summary>
        /// <returns>False when the handler raised an error.</returns>
        private bool Call(string name, int reference)
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

            try
            {
                if (state.PCall(0, 0, top + 1) != ThreadStatus.LUA_OK)
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

            // Closed here rather than in the handler, which runs in the middle of the controls' updates.
            if (_exited && !_disposed)
            {
                Dispose();
            }
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
