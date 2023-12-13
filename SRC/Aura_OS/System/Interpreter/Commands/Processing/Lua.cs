/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Lua
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Shell.cmdIntr;
using System;
using System.Collections.Generic;
using UniLua;

namespace Aura_OS.Interpreter.Commands.Util
{
    class CommandLua : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandLua(string[] commandvalues) : base(commandvalues)
        {
            Description = "to execute a Lua script";
        }

        /// <summary>
        /// CommandLua
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count == 0)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            try
            {
                // create Lua VM instance
                var Lua = LuaAPI.NewState();

                // load base libraries
                Lua.L_OpenLibs();

                // load and run Lua script file
                var LuaScriptFile = Kernel.CurrentDirectory + arguments[0];
                var status = Lua.L_DoFile(LuaScriptFile);

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
                return new ReturnInfo(this, ReturnCode.ERROR, e.ToString());
            }
            
            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}