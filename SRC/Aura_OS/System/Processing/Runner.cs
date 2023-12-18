/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Executable runner
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using Aura_OS.Interpreter;
using UniLua;
using System.Text;
using System.Xml.Linq;

namespace Aura_OS.System.Processing
{
    public class ExecutableRunner
    {
        public static void Run(Executable executable, List<string> args)
        {
            try
            {
                // create Lua VM instance
                var Lua = LuaAPI.NewState();

                // load base libraries
                Lua.L_OpenLibs();

                foreach (var source in executable.LuaSources.Keys)
                {
                    var status = Lua.L_LoadBytes(executable.LuaSources[source], source);

                    Console.WriteLine(Encoding.ASCII.GetString(executable.LuaSources[source]));
                    Console.WriteLine(source + " added.");
                    Console.ReadKey();

                    // capture errors
                    if (status != ThreadStatus.LUA_OK)
                    {
                        throw new Exception(Lua.ToString(-1));
                    }
                }

                Console.WriteLine("Files added");
                Console.ReadKey();

                Lua.GetGlobal("main");

                if (!Lua.IsFunction(-1))
                {
                    throw new Exception(string.Format(
                        "method {0} not found!", "main"));
                }

                foreach (var arg in args)
                {
                    Console.WriteLine("arg=" + arg);
                    Console.ReadKey();

                    Lua.PushString(arg);
                }

                Console.WriteLine("Args pushed");
                Console.WriteLine("args.Count=" + args.Count);
                Console.ReadKey();

                Lua.Call(args.Count, 0);

                Console.WriteLine("called");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
