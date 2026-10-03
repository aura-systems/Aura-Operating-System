/*
* PROJECT:          Aura Operating System Development
* CONTENT:          A package's Lua code in an interpreter
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Executable.Lua;
using Aura_OS.System.Processing.Applications;

namespace Aura_OS.System.Processing.Lua
{
    /// <summary>
    /// Puts a package's Lua files in an interpreter, for a console program (PackageRunner) and an
    /// app (PackageApp) alike.
    /// </summary>
    internal static class LuaPackage
    {
        /// <summary>
        /// Run once before the main file with the source lookup function as its argument:
        /// dofile and loadfile serve the package's own .lua files first, as gen2's LuaFile.VirtualFiles did.
        /// </summary>
        private const string PackageLoaders =
            "local source, rawloadfile, rawdofile, load = ..., loadfile, dofile, load\n" +
            "function loadfile(name, ...)\n" +
            "  local code = type(name) == 'string' and source(name)\n" +
            "  if code then return load(code, '@' .. name, ...) end\n" +
            "  return rawloadfile(name, ...)\n" +
            "end\n" +
            "function dofile(name)\n" +
            "  local code = type(name) == 'string' and source(name)\n" +
            "  if code then return assert(load(code, '@' .. name))() end\n" +
            "  return rawdofile(name)\n" +
            "end\n";

        /// <summary>
        /// Registers the aura library, and makes the package's .lua files what require, dofile and
        /// loadfile find first.
        /// </summary>
        /// <param name="app">The app's window for aura.app, null for a console program.</param>
        /// <exception cref="LuaException">A file has a syntax error.</exception>
        public static void Load(LuaInterpreter lua, Package package, PackageApp app)
        {
            LuaAuraLib.Register(lua, package, app);

            ILuaState state = lua.State;

            // GEN3-GAP(lua-host): Cosmos.Executable.Lua has no virtual-file loader (gen2 LuaFile.VirtualFiles),
            // so every source becomes a package.preload entry, the module name require asks for.
            // File name -> its code as a Lua string (its bytes, one character each).
            Dictionary<string, string> sources = new Dictionary<string, string>();

            state.GetGlobal("package");
            state.GetField(-1, "preload");

            foreach (KeyValuePair<string, byte[]> file in package.Files)
            {
                // layouts, images, package.xml
                if (file.Value == null || !file.Key.EndsWith(".lua", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // The code is loaded as its bytes, UTF-8 literals included, without a BOM.
                byte[] code = file.Value;
                int bom = code.Length >= 3 && code[0] == 0xEF && code[1] == 0xBB && code[2] == 0xBF ? 3 : 0;

                if (bom > 0)
                {
                    byte[] trimmed = new byte[code.Length - bom];
                    Array.Copy(code, bom, trimmed, 0, trimmed.Length);
                    code = trimmed;
                }

                if (state.L_LoadBytes(code, LuaText.Encode("@" + file.Key)) != ThreadStatus.LUA_OK)
                {
                    string error = LuaText.Decode(state.ToString(-1));
                    state.Pop(3);
                    throw new LuaException(error);
                }

                // "main.lua" -> "main", "lib/util.lua" -> "lib.util"
                string module = file.Key.Substring(0, file.Key.Length - 4);
                string dottedModule = module.Replace('/', '.');

                if (dottedModule != module)
                {
                    // gen2 resolved require "lib/util" straight to the file name: keep that working too
                    state.PushValue(-1);
                    state.SetField(-3, LuaText.Encode(module));
                }

                state.SetField(-2, LuaText.Encode(dottedModule));
                sources[file.Key] = Encoding.Latin1.GetString(code);
            }

            state.Pop(2);

            // dofile / loadfile on the package's own files
            if (state.L_LoadBuffer(PackageLoaders, LuaText.Encode("=" + package.Name + Package.Extension)) != ThreadStatus.LUA_OK)
            {
                string error = LuaText.Decode(state.ToString(-1));
                state.Pop(1);
                throw new LuaException(error);
            }

            state.PushCSharpFunction(l =>
            {
                string code;

                if (sources.TryGetValue(LuaText.Decode(l.L_CheckString(1)), out code))
                {
                    l.PushString(code);
                }
                else
                {
                    l.PushNil();
                }

                return 1;
            });

            if (state.PCall(1, 0, 0) != ThreadStatus.LUA_OK)
            {
                string error = LuaText.Decode(state.ToString(-1));
                state.Pop(1);
                throw new LuaException(error);
            }
        }

        /// <summary>
        /// Runs the package's main file: arg[0] is its name, arg[1..n] and ... the arguments.
        /// </summary>
        /// <exception cref="LuaException">The file raised an error it did not catch.</exception>
        /// <exception cref="LuaExitException">The file called os.exit.</exception>
        public static void RunMain(LuaInterpreter lua, Package package, List<string> args)
        {
            ILuaState state = lua.State;

            state.CreateTable(args.Count, 1);
            state.PushString(LuaText.Encode(package.Main));
            state.RawSetI(-2, 0);

            for (int i = 0; i < args.Count; i++)
            {
                state.PushString(LuaText.Encode(args[i] ?? ""));
                state.RawSetI(-2, i + 1);
            }

            state.SetGlobal("arg");

            // "main.lua" -> "main"
            string module = package.Main.Substring(0, package.Main.Length - 4).Replace('/', '.');

            // DoString gives the traceback, LuaException and os.exit handling
            lua.DoString("return package.preload[" + Quote(module) + "](table.unpack(arg))", "=" + package.Main);
        }

        /// <summary>
        /// A Lua string literal.
        /// </summary>
        private static string Quote(string text)
        {
            return "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") + "\"";
        }
    }
}
