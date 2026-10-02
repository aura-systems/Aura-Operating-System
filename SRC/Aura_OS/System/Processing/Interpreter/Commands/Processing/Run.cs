/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Run Script
* PROGRAMMER(S):    DA CRUZ Alexy <dacruzalexy@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.Executable.Lua;
using Aura_OS.System.Filesystem;
using Aura_OS.System.Processing.Lua;

namespace Aura_OS.System.Processing.Interpreter.Commands.Processing
{
    class CommandRun : ICommand
    {
        public CommandRun(string[] commandvalues) : base(commandvalues, CommandType.Filesystem)
        {
            Description = "to run a program (supports .bat and .lua .cexe files)";
        }

        public override ReturnInfo Execute(List<string> arguments)
        {
            try
            {
                if (arguments.Count == 0)
                {
                    return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                }

                string filePath = AuraPath.Resolve(arguments[0]);

                string fileExtension = Path.GetExtension(filePath);

                List<string> args = new List<string>();
                if (arguments.Count > 0)
                {
                    for (int i = 1; i < arguments.Count; i++)
                    {
                        args.Add(arguments[i]);
                    }
                }

                if (fileExtension == string.Empty)
                {
                    if (Kernel.PackageManager != null && Kernel.PackageManager.Packages != null)
                    {
                        foreach (var package in Kernel.PackageManager.Packages)
                        {
                            if (package != null && package.Name == arguments[0])
                            {
                                return RunCexe(package.Executable, args);
                            }
                        }
                    }

                    string installedPath = AuraPaths.ProgramsDir + arguments[0] + ".cexe";

                    if (File.Exists(installedPath))
                    {
                        return RunCexe(new Executable(File.ReadAllBytes(installedPath)), args);
                    }

                    return new ReturnInfo(this, ReturnCode.ERROR, "This package does not exist.");
                }
                else
                {
                    if (!File.Exists(filePath))
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR, "This file does not exist.");
                    }

                    switch (fileExtension.ToLower())
                    {
                        case ".bat":
                            // Batch builds a fresh CommandManager per line, which resets the static command table:
                            // put the caller's (the Terminal's, whose clear/exit are bound to it) back afterwards.
                            List<ICommand> callerCommands = CommandManager.GetCommands();
                            Batch.Execute(filePath);
                            if (callerCommands != null)
                            {
                                CommandManager._commands = callerCommands;
                            }
                            break;
                        case ".cexe":
                            byte[] executableBytes = File.ReadAllBytes(filePath);
                            Executable executable = new(executableBytes);
                            return RunCexe(executable, args);
                        case ".lua":
                            return RunLua(filePath, args);
                        default:
                            return new ReturnInfo(this, ReturnCode.ERROR, "Unsupported file type.");
                    }
                }

                return new ReturnInfo(this, ReturnCode.OK);
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.ToString());
            }
        }

        private ReturnInfo RunCexe(Executable executable, List<string> args)
        {
            try
            {
                ExecutableRunner runner = new();
                runner.Run(executable, args);

                return new ReturnInfo(this, ReturnCode.OK);
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.ToString());
            }
        }

        /// <summary>
        /// Run a Lua script with its arguments (arg table and ...). A script that returns a table
        /// with a main function (gen2 convention) gets main() called; a plain script just runs.
        /// </summary>
        private ReturnInfo RunLua(string filePath, List<string> args)
        {
            ReturnInfo result = new ReturnInfo(this, ReturnCode.OK);
            LuaInterpreter lua = null;

            try
            {
                lua = AuraLua.Create();

                ILuaState state = lua.State;

                // arg[0] = script path, arg[1..n] = arguments (as lua script.lua args...)
                state.CreateTable(args.Count, 1);
                state.PushString(filePath);
                state.RawSetI(-2, 0);

                for (int i = 0; i < args.Count; i++)
                {
                    state.PushString(args[i]);
                    state.RawSetI(-2, i + 1);
                }

                state.SetGlobal("arg");

                // DoString gives the traceback, LuaException and os.exit handling; filePath is absolute
                lua.DoString(
                    "local f = assert(loadfile(arg[0])) " +
                    "local m = f(table.unpack(arg)) " +
                    "if type(m) == 'table' and type(m.main) == 'function' then m.main() end",
                    "=run");
            }
            catch (LuaException e)
            {
                result = new ReturnInfo(this, ReturnCode.ERROR, e.Message + (e.LuaStackTrace != null ? "\n" + e.LuaStackTrace : ""));
            }
            catch (LuaExitException e)
            {
                if (e.ExitCode != 0)
                {
                    result = new ReturnInfo(this, ReturnCode.ERROR, "Exited with code " + e.ExitCode + ".");
                }
            }
            catch (Exception e)
            {
                result = new ReturnInfo(this, ReturnCode.ERROR, e.ToString());
            }

            // GEN3-GAP(finally): not a using, gen3 does not run finally/Dispose when an exception unwinds (C7).
            // GEN3-GAP(finalizers): Dispose closes the files the script left open.
            if (lua != null)
            {
                lua.Dispose();
            }

            return result;
        }

        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - run {file}");
        }
    }
}
