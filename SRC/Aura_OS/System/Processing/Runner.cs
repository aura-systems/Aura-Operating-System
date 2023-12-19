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
using System.Diagnostics;
using Cosmos.Core;

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
                    if (source == "main.lua")
                    {
                        continue;
                    }

                    LoadLuaFile(source);
                }

                LoadLuaFile("main.lua");

                void LoadLuaFile(string fileName)
                {
                    var status = Lua.L_LoadBytes(executable.LuaSources[fileName], fileName);

                    // capture errors
                    if (status != ThreadStatus.LUA_OK)
                    {
                        throw new Exception(Lua.ToString(-1));
                    }
                }

                /*
                foreach (var arg in args)
                {
                    Console.WriteLine("arg=" + arg);
                    Console.ReadKey();

                    Lua.PushString(arg);
                }

                Console.WriteLine("Args pushed");
                Console.WriteLine("args.Count=" + args.Count);
                Console.ReadKey();*/

                var status = Lua.PCall(0, LuaDef.LUA_MULTRET, 0);
                if (status != ThreadStatus.LUA_OK)
                {
                    throw new Exception(Lua.ToString(-1));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
