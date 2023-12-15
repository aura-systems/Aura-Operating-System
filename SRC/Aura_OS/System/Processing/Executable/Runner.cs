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

namespace Aura_OS.System.Processing.Executable
{
    public class ExecutableRunner
    {
        public static void Run(Executable executable)
        {
            try
            {
                // create Lua VM instance
                var Lua = LuaAPI.NewState();

                // load base libraries
                Lua.L_OpenLibs();

                var status = Lua.L_DoByteArray(executable.LuaScript, "main.lua");

                // capture errors
                if (status != ThreadStatus.LUA_OK)
                {
                    throw new Exception(Lua.ToString(-1));
                }

                // ensuare the value returned by 'framework/main.lua' is a Lua table
                if (!Lua.IsTable(-1))
                {
                    throw new Exception(
                          "start's return value is not a table");
                }

                var AwakeRef = StoreMethod("main");

                Lua.Pop(1);

                CallMethod(AwakeRef);

                int StoreMethod(string name)
                {
                    Lua.GetField(-1, name);
                    if (!Lua.IsFunction(-1))
                    {
                        throw new Exception(string.Format(
                            "method {0} not found!", name));
                    }
                    return Lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);
                }

                void CallMethod(int funcRef)
                {
                    Lua.RawGetI(LuaDef.LUA_REGISTRYINDEX, funcRef);
                    var status = Lua.PCall(0, 0, 0);
                    if (status != ThreadStatus.LUA_OK)
                    {
                        Console.WriteLine(Lua.ToString(-1));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
