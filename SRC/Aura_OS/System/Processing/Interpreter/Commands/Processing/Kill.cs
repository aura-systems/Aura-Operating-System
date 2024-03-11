/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Kill process command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Aura_OS.Processing;
using Aura_OS.System.Graphics.UI.GUI;

namespace Aura_OS.System.Processing.Interpreter.Commands.Util
{
    class CommandKill : ICommand
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CommandKill(string[] commandvalues) : base(commandvalues)
        {
            Description = "to stop a process";
        }

        /// <summary>
        /// CommandLsprocess
        /// </summary>
        public override ReturnInfo Execute(List<string> arguments)
        {
            try
            {
                if (arguments.Count > 1)
                {
                    if (arguments[0] == "/f")
                    {
                        uint pid = uint.Parse(arguments[1]);
                        Process process = Kernel.ProcessManager.GetProcessByPid(pid);

                        if (process == null)
                        {
                            return new ReturnInfo(this, ReturnCode.ERROR, "Failed to get process " + pid + ".");
                        }

                        if (process is Application)
                        {
                            Application application = process as Application;
                            application.Window.Visible = false;
                        }

                        Kernel.ProcessManager.Stop(process);
                        Kernel.ProcessManager.Unregister(process);

                        Console.WriteLine("Process " + process.Name + " (pid " + pid + ") killed.");

                        return new ReturnInfo(this, ReturnCode.OK);
                    }
                    else
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                    }
                }
                else if (arguments.Count == 1)
                {
                    uint pid = uint.Parse(arguments[0]);
                    Process process = Kernel.ProcessManager.GetProcessByPid(pid);

                    if (process == null)
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR, "Failed to get process " + pid + ".");
                    }

                    if (process is Application)
                    {
                        Application application = process as Application;
                        application.Window.Visible = false;
                    }

                    Kernel.ProcessManager.Stop(process);

                    Console.WriteLine("Process " + process.Name + " (pid " + pid + ") stopped.");

                    return new ReturnInfo(this, ReturnCode.OK);
                }
                else
                {
                    return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                }
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR, ex.ToString());
            } 
        }

        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - kill {pid}         to stop a process.");
            Console.WriteLine(" - kill /f {pid}      to kill a process.");
        }
    }
}
