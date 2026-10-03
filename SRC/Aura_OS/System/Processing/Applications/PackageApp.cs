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
    /// layout's events, on the UI thread. SRC/Packages/README.md describes the Lua side.
    /// </summary>
    public class PackageApp : Application
    {
        public readonly Package Package;

        private readonly LuaInterpreter _lua;

        // Event name -> handler function (a registry reference).
        private readonly Dictionary<string, int> _handlers = new Dictionary<string, int>();

        private bool _disposed;

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
                string message = luaError != null && luaError.LuaStackTrace != null ? luaError.Message + "\n" + luaError.LuaStackTrace : error.Message;

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
        /// Runs a handler. Its error is logged with the Lua traceback, and the app goes on.
        /// </summary>
        private void Call(string name, int reference)
        {
            if (_disposed)
            {
                return;
            }

            ILuaState state = _lua.State;
            int top = state.GetTop();

            state.PushCSharpFunction(Traceback);
            state.RawGetI(LuaDef.LUA_REGISTRYINDEX, reference);

            if (state.PCall(0, 0, top + 1) != ThreadStatus.LUA_OK)
            {
                Logs.DoOSLog("[Error] " + Package.Name + ": " + name + ": " + state.ToString(-1));
            }

            state.SetTop(top);

            // The handler changed controls: draw the window again.
            MarkDirty();
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
