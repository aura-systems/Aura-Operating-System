/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Executable runner
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Executable.Lua;
using Aura_OS.System.Processing.Lua;

namespace Aura_OS.System.Processing
{
    public class ExecutableRunner
    {
        /// <summary>
        /// Run once before main.lua with the source lookup function as its argument:
        /// dofile and loadfile serve the executable's own .lua files first, as gen2's LuaFile.VirtualFiles did.
        /// </summary>
        private const string EmbeddedLoaders =
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

        public void Run(Executable executable, List<string> args)
        {
            if (executable == null || executable.LuaSources == null)
            {
                Console.WriteLine("Invalid executable.");
                return;
            }

            if (args == null)
            {
                args = new List<string>();
            }

            LuaInterpreter lua = null;

            try
            {
                // create Lua VM instance (standard libraries, cosmos.crypto, os.execute, os.getenv)
                lua = AuraLua.Create();

                ILuaState state = lua.State;

                // GEN3-GAP(lua-host): Cosmos.Executable.Lua has no virtual-file loader (gen2 LuaFile.VirtualFiles),
                // so every source becomes a package.preload entry, the module name require asks for.
                Dictionary<string, string> sources = new Dictionary<string, string>();

                state.GetGlobal("package");
                state.GetField(-1, "preload");

                foreach (KeyValuePair<string, byte[]> source in executable.LuaSources)
                {
                    // zip directory entries ("lib/") and assets
                    if (source.Key == null || source.Value == null || !source.Key.EndsWith(".lua", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // L_LoadBytes reads one byte per char: decode UTF-8 and drop a BOM so non-ASCII literals survive
                    string code = Encoding.UTF8.GetString(source.Value).TrimStart('\uFEFF');

                    // capture errors
                    if (state.L_LoadBuffer(code, "@" + source.Key) != ThreadStatus.LUA_OK)
                    {
                        throw new LuaException(state.ToString(-1));
                    }

                    // "main.lua" -> "main", "lib/util.lua" -> "lib.util"
                    string module = source.Key.Substring(0, source.Key.Length - 4);
                    string dottedModule = module.Replace('/', '.').Replace('\\', '.');

                    if (dottedModule != module)
                    {
                        // gen2 resolved require "lib/util" straight to the file name: keep that working too
                        state.PushValue(-1);
                        state.SetField(-3, module);
                    }

                    state.SetField(-2, dottedModule);
                    sources[source.Key] = code;
                }

                state.Pop(2);

                // dofile / loadfile on the executable's own files
                if (state.L_LoadBuffer(EmbeddedLoaders, "=cexe") != ThreadStatus.LUA_OK)
                {
                    throw new LuaException(state.ToString(-1));
                }

                state.PushCSharpFunction(l =>
                {
                    string code;

                    if (sources.TryGetValue(l.L_CheckString(1), out code))
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
                    throw new LuaException(state.ToString(-1));
                }

                // args: arg[0] = "main.lua", arg[1..n] = command-line arguments
                state.CreateTable(args.Count, 1);
                state.PushString("main.lua");
                state.RawSetI(-2, 0);

                for (int i = 0; i < args.Count; i++)
                {
                    state.PushString(args[i]);
                    state.RawSetI(-2, i + 1);
                }

                state.SetGlobal("arg");

                // DoString gives the traceback, LuaException and os.exit handling; main.lua also gets the arguments as ...
                lua.DoString("return package.preload['main'](table.unpack(arg))", "=main.lua");
            }
            catch (LuaException e)
            {
                Console.WriteLine(e.Message);

                if (e.LuaStackTrace != null)
                {
                    Console.WriteLine(e.LuaStackTrace);
                }
            }
            catch (LuaExitException e)
            {
                if (e.ExitCode != 0)
                {
                    Console.WriteLine("Exited with code " + e.ExitCode + ".");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            // GEN3-GAP(finally): not a using, gen3 does not run finally/Dispose when an exception unwinds (C7).
            // GEN3-GAP(finalizers): Dispose closes the files the program left open.
            if (lua != null)
            {
                lua.Dispose();
            }
        }
    }
}
