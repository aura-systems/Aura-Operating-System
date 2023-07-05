﻿/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - CommandManager
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.System.Shell.cmdIntr.c_Console;
using Aura_OS.System.Shell.cmdIntr.Network;
using Aura_OS.System.Shell.cmdIntr.Power;
using Aura_OS.System.Shell.cmdIntr.Util;
using Aura_OS.System.Utils;
using Cosmos.System.Network;
using Aura_OS;
using Aura_OS.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aura_OS.System.Shell.cmdIntr.SystemInfomation;
using Aura_OS.System.Shell.cmdIntr;
using Aura_OS.Interpreter.Commands.Util;
using Aura_OS.Interpreter.Commands.Filesystem;
using LibDotNetParser.CILApi;
using libDotNetClr;
using LibDotNetParser;

namespace Aura_OS.Interpreter
{
    public class CommandManager : Process
    {
        public List<ICommand> CMDs = new List<ICommand>();

        public CommandManager() : base("cmdManager", ProcessType.KernelComponent)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            RegisterAllCommands();

            Kernel.ProcessManager.Register(this);
            Kernel.ProcessManager.Start(this);
        }

        private static int NumbOfSuccesssTests = 0;
        private static int NumbOfFailedTests = 0;
        private static DotNetClr clr;
        private static DotNetFile fl;

        public void RegisterAllCommands()
        {
            CMDs.Add(new CommandReboot(new string[] { "reboot", "rb" }));
            CMDs.Add(new CommandShutdown(new string[] { "shutdown", "sd" }));

            CMDs.Add(new CommandClear(new string[] { "clear", "clr" }));
            CMDs.Add(new CommandKeyboardMap(new string[] { "setkeyboardmap", "setkeyboard" }));
            CMDs.Add(new CommandEnv(new string[] { "export", "set" }));
            CMDs.Add(new CommandEcho(new string[] { "echo" }));

            CMDs.Add(new CommandIPConfig(new string[] { "ipconfig", "ifconfig", "netconf" }));
            CMDs.Add(new CommandPing(new string[] { "ping" }));
            CMDs.Add(new CommandUdp(new string[] { "udp" }));
            CMDs.Add(new CommandDns(new string[] { "dns" }));
            CMDs.Add(new CommandWget(new string[] { "wget" }));
            CMDs.Add(new CommandFtp(new string[] { "ftp" }));
            CMDs.Add(new CommandHttpServer(new string[] { "httpserver" }));

            CMDs.Add(new CommandVersion(new string[] { "version", "ver", "about" }));
            CMDs.Add(new CommandSystemInfo(new string[] { "systeminfo", "sysinfo" }));
            CMDs.Add(new CommandTime(new string[] { "time", "date" }));
            CMDs.Add(new CommandHelp(new string[] { "help" }));

            CMDs.Add(new CommandChangeRes(new string[] { "changeres", "cr" }));
            CMDs.Add(new CommandLsprocess(new string[] { "lsprocess" }));
            CMDs.Add(new CommandLspci(new string[] { "lspci" }));
            //CMDs.Add(new CommandCrash(new string[] { "crash" }));

            CMDs.Add(new CommandVol(new string[] { "vol" }));
            CMDs.Add(new CommandDir(new string[] { "dir", "ls", "l" }));
            CMDs.Add(new CommandMkdir(new string[] { "mkdir", "md" }));
            CMDs.Add(new CommandRmdir(new string[] { "rmdir", "rmd" }));
            CMDs.Add(new CommandCat(new string[] { "cat" }));
            CMDs.Add(new CommandCD(new string[] { "cd" }));
            CMDs.Add(new CommandMkfil(new string[] { "touch", "mkfil", "mf" }));
            CMDs.Add(new CommandRmfil(new string[] { "rmfil", "rmf" }));
            CMDs.Add(new CommandHex(new string[] { "hex" }));
            CMDs.Add(new CommandTree(new string[] { "tree" }));
            CMDs.Add(new CommandRun(new string[] { "run" }));

            /*
            CMDs.Add(new CommandPCName(new string[] { "pcn" }));

            CMDs.Add(new CommandMIV(new string[] { "miv", "edit" }));*/

            CMDs.Add(new CommandAction(new string[] { "beep" }, () =>
            {
                Cosmos.System.PCSpeaker.Beep();
            }));
            CMDs.Add(new CommandAction(new string[] { "crash" }, () =>
            {
                throw new Exception("Exception test");
            }));
            CMDs.Add(new CommandAction(new string[] { "crashn" }, () =>
            {
                string[] test =
                {
                    "test1",
                    "tert2"
                };
                test[2] = "test3"; //Should make a Null reference exception
            }));

            CMDs.Add(new CommandAction(new string[] { "dotnet" }, () =>
            {
                Console.WriteLine("Parsing files...");

                var fl = new DotNetFile(Files.TestApp);
                var clr = new DotNetClr(fl);

                Console.WriteLine("CLR created!");

                //Register our internal methods
                clr.RegisterResolveCallBack(AssemblyCallback);
                clr.RegisterCustomInternalMethod("TestsComplete", TestsComplete);
                clr.RegisterCustomInternalMethod("TestSuccess", TestSuccess);
                clr.RegisterCustomInternalMethod("TestFail", TestFail);
                clr.RegisterCustomInternalMethod("TestsRxObject", TestRxObject);

                Console.WriteLine("Methods registered!");

                Console.WriteLine("Starting tests...");

                clr.Start();

                Console.WriteLine("Tests done.");
            }));

            CMDs.Add(new CommandAction(new string[] { "calc" }, () =>
            {
                var fl = new DotNetFile(Files.CalculatorApp);
                var clr = new DotNetClr(fl);

                clr.RegisterResolveCallBack(AssemblyCallback);
                clr.Start();
            }));
        }

        private static byte[] AssemblyCallback(string dll)
        {
            if (dll == "System.Private.CoreLib")
            {
                return Files.Framework;
            }
            else
            {
                return null;
            }
        }
        private static void TestSuccess(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var testName = (string)Stack[Stack.Length - 1].value;

            PrintWithColor("Test Success: " + testName, ConsoleColor.Green);
            NumbOfSuccesssTests++;
        }

        private static void TestsComplete(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            Console.WriteLine();
            PrintWithColor("All Tests Completed.", ConsoleColor.DarkYellow);
            Console.WriteLine();
            PrintWithColor("Passed tests: " + NumbOfSuccesssTests, ConsoleColor.Green);
            PrintWithColor("Failed tests: " + NumbOfFailedTests, ConsoleColor.Red);
        }

        private static void TestFail(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            var testName = (string)Stack[Stack.Length - 1].value;

            PrintWithColor("Test Failure: " + testName, ConsoleColor.Red);
            NumbOfFailedTests++;
        }
        private static void TestRxObject(MethodArgStack[] Stack, ref MethodArgStack returnValue, DotNetMethod method)
        {
            // var cctor = fl.GetMethod("TestApp.Tests", "TestObject", ".ctor");
            // if (cctor == null)
            //    throw new NullReferenceException();
            // var s = new CustomList<MethodArgStack>();
            // s.Add(MethodArgStack.String("value"));
            // returnValue = clr.CreateObject(cctor, s);
        }
        private static void PrintWithColor(string text, ConsoleColor fg)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = fg;
            Console.WriteLine(text);
            Console.ForegroundColor = old;
        }

        /// <summary>
        /// Shell Interpreter
        /// </summary>
        /// <param name="cmd">Command</param>
        public void Execute(string cmd)
        {
            //CommandsHistory.Add(cmd); //adding last command to the commands history

            if (cmd.Length <= 0)
            {
                Kernel.console.WriteLine();
                return;
            }

            #region Parse command

            List<string> arguments = Misc.ParseCommandLine(cmd);

            string firstarg = arguments[0]; //command name

            if (arguments.Count > 0)
            {
                arguments.RemoveAt(0); //get only arguments
            }

            #endregion

            foreach (var command in CMDs)
            {
                if (command.ContainsCommand(firstarg))
                {
                    ReturnInfo result;

                    if (arguments.Count > 0 && (arguments[0] == "/help" || arguments[0] == "/h"))
                    {
                        ShowHelp(command);
                        result = new ReturnInfo(command, ReturnCode.OK);
                    }
                    else
                    {
                        result = CheckCommand(command);

                        if (result.Code == ReturnCode.OK)
                        {
                            if (arguments.Count == 0)
                            {
                                result = command.Execute();
                            }
                            else
                            {
                                result = command.Execute(arguments);
                            }
                        }
                    }

                    ProcessCommandResult(result);

                    return;
                }
            }

            Kernel.console.Foreground = ConsoleColor.DarkRed;
            Kernel.console.WriteLine("Unknown command.");
            Kernel.console.Foreground = ConsoleColor.White;

            Kernel.console.WriteLine();
        }

        /// <summary>
        /// Show command description
        /// </summary>
        /// <param name="command">Command</param>
        private void ShowHelp(ICommand command)
        {
            Kernel.console.WriteLine("Description: " + command.Description + ".");
            Kernel.console.WriteLine();
            if (command.CommandValues.Length > 1)
            {
                Kernel.console.Write("Aliases: ");
                for (int i = 0; i < command.CommandValues.Length; i++)
                {
                    if (i != command.CommandValues.Length - 1)
                    {
                        Kernel.console.Write(command.CommandValues[i] + ", ");
                    }
                    else
                    {
                        Kernel.console.Write(command.CommandValues[i]);
                    }
                }
                Kernel.console.WriteLine();
                Kernel.console.WriteLine();
            }
            command.PrintHelp();
        }

        /// <summary>
        /// Check command availability to avoid unwanted behavior.
        /// </summary>
        /// <param name="command">Command</param>
        private ReturnInfo CheckCommand(ICommand command)
        {
            if (command.Type == CommandType.Filesystem)
            {
                if (Kernel.VirtualFileSystem == null || Kernel.VirtualFileSystem.GetVolumes().Count == 0)
                {
                    return new ReturnInfo(command, ReturnCode.ERROR, "No volume detected!");
                }
            }
            if (command.Type == CommandType.Network)
            {
                if (NetworkStack.ConfigEmpty())
                {
                    return new ReturnInfo(command, ReturnCode.ERROR, "No network configuration detected! Use ipconfig /set.");
                }
            }
            return new ReturnInfo(command, ReturnCode.OK);
        }

        /// <summary>
        /// Process result info of the command
        /// </summary>
        /// <param name="result">Result information</param>
        private void ProcessCommandResult(ReturnInfo result)
        {
            if (result.Code == ReturnCode.ERROR_ARG)
            {
                Kernel.console.Foreground = ConsoleColor.DarkRed;
                Kernel.console.WriteLine("Command arguments are incorrectly formatted.");
                Kernel.console.Foreground = ConsoleColor.White;
            }
            else if (result.Code == ReturnCode.ERROR)
            {
                Kernel.console.Foreground = ConsoleColor.DarkRed;
                Kernel.console.WriteLine("Error: " + result.Info);
                Kernel.console.Foreground = ConsoleColor.White;
            }

            Kernel.console.WriteLine();
        }

    }
}
