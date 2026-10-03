/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Console program runner (.pkg without a layout)
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Cosmos.Executable.Lua;
using Aura_OS.System.Processing.Lua;

namespace Aura_OS.System.Processing
{
    /// <summary>
    /// Runs a console program's package in the calling Terminal, until its main file returns.
    /// An app's package opens a window instead (PackageApp).
    /// </summary>
    public class PackageRunner
    {
        public void Run(Package package, List<string> args)
        {
            if (package == null)
            {
                Console.WriteLine("Invalid package.");
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

                LuaPackage.Load(lua, package, null);
                LuaPackage.RunMain(lua, package, args);
            }
            catch (Exception e)
            {
                // One clause: gen3 kernels up to 3.0.89 enter the first typed catch whatever the type.
                if (e is LuaExitException exit)
                {
                    if (exit.ExitCode != 0)
                    {
                        Console.WriteLine("Exited with code " + exit.ExitCode + ".");
                    }
                }
                else if (e is LuaException error)
                {
                    Console.WriteLine(error.Message);

                    if (error.LuaStackTrace != null)
                    {
                        Console.WriteLine(error.LuaStackTrace);
                    }
                }
                else
                {
                    Console.WriteLine(e.ToString());
                }
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
