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
            Description = "to run a program: an installed package by name, or a .bat, .lua or .pkg file";
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
                    Package package = Kernel.PackageManager != null ? Kernel.PackageManager.Find(arguments[0]) : null;

                    if (package == null)
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR, "This package does not exist.");
                    }

                    return RunPackage(package, args);
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
                        case ".pkg":
                            return RunPackage(new Package(File.ReadAllBytes(filePath)), args);
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

        /// <summary>
        /// An app's package opens its window; a console program's runs here until it returns. Both get
        /// the arguments.
        /// </summary>
        private ReturnInfo RunPackage(Package package, List<string> args)
        {
            try
            {
                if (package.IsApp)
                {
                    Kernel.ApplicationManager.StartPackage(package, args);
                }
                else
                {
                    PackageRunner runner = new();
                    runner.Run(package, args);
                }

                return new ReturnInfo(this, ReturnCode.OK);
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.Message);
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

                // arg[0] = script path, arg[1..n] = arguments (as lua script.lua args...), as Lua strings (UTF-8 bytes)
                state.CreateTable(args.Count, 1);
                state.PushString(LuaText.Encode(filePath));
                state.RawSetI(-2, 0);

                for (int i = 0; i < args.Count; i++)
                {
                    state.PushString(LuaText.Encode(args[i] ?? ""));
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
            catch (Exception e)
            {
                // One clause: gen3 kernels up to 3.0.89 enter the first typed catch whatever the type.
                if (e is LuaExitException exit)
                {
                    if (exit.ExitCode != 0)
                    {
                        result = new ReturnInfo(this, ReturnCode.ERROR, "Exited with code " + exit.ExitCode + ".");
                    }
                }
                else if (e is LuaException error)
                {
                    result = new ReturnInfo(this, ReturnCode.ERROR, error.Message + (error.LuaStackTrace != null ? "\n" + error.LuaStackTrace : ""));
                }
                else
                {
                    result = new ReturnInfo(this, ReturnCode.ERROR, e.ToString());
                }
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
            Console.WriteLine(" - run {package} [args]");
            Console.WriteLine(" - run {file.bat|file.lua|file.pkg} [args]");
        }
    }
}
