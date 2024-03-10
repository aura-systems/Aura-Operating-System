/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Lua process
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI;
using System;
using UniLua;

namespace Aura_OS.Processing
{
    public class LuaProcess : Process
    {
        Application Application;
        private string _filePath;

        public LuaProcess(string name, string filePath) : base(name, ProcessType.Program)
        {
            _filePath = filePath;
        }

        public override void Start()
        {
            try
            {
                base.Start();

                // create Lua VM instance
                var Lua = LuaAPI.NewState((int)ID);

                // load base libraries
                Lua.L_OpenLibs();

                // load and run Lua script file
                var LuaScriptFile = _filePath;
                var status = Lua.L_DoFile(LuaScriptFile);

                // capture errors
                if (status != ThreadStatus.LUA_OK)
                {
                    throw new Exception(Lua.ToString(-1));
                }

                // ensuare the value returned by 'main.lua' is a Lua table
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
                // return new ReturnInfo(this, ReturnCode.ERROR, e.ToString());
                Console.WriteLine(e.ToString());
            }

            Stop();
        }
    }
}
