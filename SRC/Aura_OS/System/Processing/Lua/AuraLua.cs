/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Lua interpreter factory (Cosmos.Executable.Lua)
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.Executable.Lua;
using Aura_OS.System.Processing.Interpreter.Commands;

namespace Aura_OS.System.Processing.Lua
{
    /// <summary>
    /// Creates the Lua interpreters Aura runs scripts with (run file.lua, .pkg programs and apps).
    /// </summary>
    internal static class AuraLua
    {
        /// <summary>
        /// Create a Lua 5.2 interpreter with the standard libraries, cosmos.crypto,
        /// Aura's environment variables and os.execute wired to the shell.
        /// The caller must Dispose() it (after its try/catch, not in a using).
        /// </summary>
        /// <param name="shell">The Terminal's CommandManager when the caller has it; null uses a Batch-style one.</param>
        /// <returns>The interpreter.</returns>
        internal static LuaInterpreter Create(CommandManager shell = null)
        {
            LuaInterpreter lua = new LuaInterpreter();

            // Relative names for dofile, require, loadfile, io.open, os.remove... (Unix VFS path, ends with '/')
            lua.WorkingDirectory = Kernel.CurrentDirectory;

            // GEN3-GAP(console-input): Console.In is the KernelConsole line editor, which would block the UI
            // thread, paint over the GUI and drain tty1. io.read gives nil instead.
            lua.Input = TextReader.Null;

            // Output stays null: print and io.write follow Console.Out, the Terminal's writer while it is focused.

            lua.ExecuteCommand = command => ExecuteCommand(lua, shell, command);

            // GEN3-GAP(env): Environment.GetEnvironmentVariable is always null on gen3,
            // so os.getenv reads Aura's variables (set / export).
            ILuaState state = lua.State;
            state.GetGlobal("os");
            state.PushCSharpFunction(GetEnv);
            state.SetField(-2, "getenv");
            state.Pop(1);

            LuaCryptoLib.Register(lua);

            return lua;
        }

        /// <summary>
        /// os.execute(command): run one shell line, then follow a cd it made.
        /// </summary>
        private static int ExecuteCommand(LuaInterpreter lua, CommandManager shell, string command)
        {
            int status = 0;
            CommandManager manager = shell;
            List<ICommand> callerCommands = null;

            try
            {
                if (manager == null)
                {
                    // The CommandManager constructor resets the static command table (Batch.cs pattern).
                    // Keep the caller's table (the Terminal's, whose clear/exit are bound to it) and put it back afterwards.
                    callerCommands = CommandManager.GetCommands();
                    manager = new CommandManager();

                    if (callerCommands != null && callerCommands.Count > 0)
                    {
                        CommandManager._commands = callerCommands;
                    }
                    else
                    {
                        callerCommands = null;
                        manager.Initialize();
                    }
                }

                manager.Execute(command);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                status = 1;
            }

            // GEN3-GAP(finally): not in a finally, gen3 does not run finally blocks when an exception unwinds (C7).
            if (callerCommands != null)
            {
                CommandManager._commands = callerCommands;
            }

            lua.WorkingDirectory = Kernel.CurrentDirectory;

            return status;
        }

        /// <summary>
        /// os.getenv(name): the value from Kernel.EnvironmentVariables, nil when unset.
        /// </summary>
        private static int GetEnv(ILuaState lua)
        {
            string name = lua.L_CheckString(1);
            string value;

            if (Kernel.EnvironmentVariables != null && Kernel.EnvironmentVariables.TryGetValue(name, out value) && value != null)
            {
                lua.PushString(value);
            }
            else
            {
                lua.PushNil();
            }

            return 1;
        }
    }
}
