/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Lsprocess command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS;
using Aura_OS.Processing;
using Aura_OS.System.Processing.Interpreter;
using Aura_OS.System.Processing.Services;
using Cosmos.Kernel.Core.Scheduler;
using System;
using System.Text;

namespace Aura_OS.System.Processing.Interpreter.Commands.Util
{
    class CommandLsprocess : ICommand
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CommandLsprocess(string[] commandvalues) : base(commandvalues)
        {
            Description = "to list all registered processes";
        }

        /// <summary>
        /// CommandLsprocess
        /// </summary>
        public override ReturnInfo Execute()
        {
            StringBuilder sb = new StringBuilder();

            // TID/THREAD reflect the gen3 preemptive scheduler: a threaded
            // process (or a GUI app with a compute worker) reports its kernel
            // thread and that thread's scheduler state; a purely cooperative
            // process shows "-" (it runs on the main UI thread).
            sb.Append("ID".PadRight(6));
            sb.Append("TYPE".PadRight(17));
            sb.Append("STATE".PadRight(6));
            sb.Append("TID".PadRight(6));
            sb.Append("THREAD".PadRight(14));
            sb.AppendLine("NAME");

            for (int i = 0; i < Kernel.ProcessManager.Processes.Count; i++)
            {
                Process process = Kernel.ProcessManager.Processes[i];
                uint tid = process.ThreadId;

                sb.Append(process.ID.ToString().PadRight(6));
                sb.Append(Process.GetServiceTypeString(process.Type).PadRight(17));
                sb.Append((process.Running ? "run" : "stop").PadRight(6));
                sb.Append((tid == 0 ? "-" : tid.ToString()).PadRight(6));
                sb.Append(ThreadStateName(tid).PadRight(14));
                sb.AppendLine(process.Name);
            }

            sb.AppendLine();
            sb.Append("Live kernel threads: ");
            sb.Append(SchedulerManager.ThreadCount.ToString());
            sb.Append("   CPU: ");
            sb.Append(SystemMonitor.CpuUsagePercent.ToString());
            sb.Append("%   Heap: ");
            sb.Append((SystemMonitor.UsedRamBytes / 1024).ToString());
            sb.Append("KB   Uptime: ");
            sb.Append(SystemMonitor.UptimeSeconds.ToString());
            sb.AppendLine("s");

            Console.WriteLine(sb.ToString());

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Resolve a process's backing kernel thread to a scheduler-state label.
        /// Cooperative processes (tid 0) run on the main UI thread; a tid that
        /// is no longer in the registry has exited.
        /// </summary>
        private static string ThreadStateName(uint tid)
        {
            if (tid == 0)
            {
                return "cooperative";
            }

            Thread[] threads = SchedulerManager.Threads;
            if (threads != null)
            {
                for (int i = 0; i < threads.Length; i++)
                {
                    Thread thread = threads[i];
                    if (thread != null && thread.Id == tid)
                    {
                        return StateLabel(thread.State);
                    }
                }
            }

            return "exited";
        }

        // Manual map (enum.ToString relies on reflection metadata under NativeAOT).
        private static string StateLabel(ThreadState state)
        {
            switch (state)
            {
                case ThreadState.Created:
                    return "Created";
                case ThreadState.Ready:
                    return "Ready";
                case ThreadState.Running:
                    return "Running";
                case ThreadState.Blocked:
                    return "Blocked";
                case ThreadState.Sleeping:
                    return "Sleeping";
                case ThreadState.Dead:
                    return "Dead";
                default:
                    return "?";
            }
        }
    }
}
