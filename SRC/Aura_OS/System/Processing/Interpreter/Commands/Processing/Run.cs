/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Run Script
* PROGRAMMER(S):    DA CRUZ Alexy <dacruzalexy@gmail.com>
*/

using Aura_OS.Processing;
using Aura_OS.System.Processing.Processes;
using System;
using System.Collections.Generic;
using System.IO;
using UniLua;

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
                string filePath = Path.Combine(Kernel.CurrentDirectory, arguments[0]);

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
                    foreach (var package in Kernel.PackageManager.Packages)
                    {
                        if (package.Name == arguments[0])
                        {
                            return RunCexe(package.Executable, args);
                        }
                    }

                    string installedPath = "0:\\System\\Programs\\" + arguments[0] + ".cexe";

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
                            Batch.Execute(filePath);
                            break;
                        case ".cexe":
                            byte[] executableBytes = File.ReadAllBytes(filePath);
                            Executable executable = new(executableBytes);
                            return RunCexe(executable, args);
                        case ".lua":
                            return RunLua(filePath);
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

        private ReturnInfo RunLua(string filePath)
        {
            LuaProcess process = new LuaProcess(Path.GetFileName(filePath), filePath);
            process.Initialize();
 
            Kernel.ProcessManager.Register(process);
            Kernel.ProcessManager.Start(process);

            return new ReturnInfo(this, ReturnCode.OK);
        }

        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - run {file}");
        }
    }
}
