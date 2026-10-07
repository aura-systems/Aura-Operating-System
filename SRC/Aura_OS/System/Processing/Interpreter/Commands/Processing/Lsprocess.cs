/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Lsprocess command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS;
using Aura_OS.System.Processing.Interpreter;
using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Kernel.System.Diagnostics;

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

            sb.AppendLine("ID      TYPE    STATE    NAME");

            for (int i = 0; i < Kernel.ProcessManager.Processes.Count; i++)
            {
                sb.Append(Kernel.ProcessManager.Processes[i].ID.ToString().PadRight(8, ' '));
                sb.Append(((int)Kernel.ProcessManager.Processes[i].Type).ToString().PadRight(8, ' '));
                sb.Append((Kernel.ProcessManager.Processes[i].Running ? 1 : 0).ToString().PadRight(9, ' '));
                sb.Append(Kernel.ProcessManager.Processes[i].Name.ToString().PadRight(24, ' '));

                sb.AppendLine();
            }

            AppendKernelThreads(sb);

            Console.WriteLine(sb.ToString());

            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// Kernel scheduler threads (Aura processes all run on the main loop thread).
        /// GEN3-GAP(idle-thread): the main loop is the scheduler's idle thread, so no CPU% is shown.
        /// </summary>
        private static void AppendKernelThreads(StringBuilder sb)
        {
            if (!SchedulerDiagnostics.IsInitialized)
            {
                return;
            }

            sb.AppendLine();
            sb.AppendLine("TID     STATE     RUNTIME");

            int slotCount = SchedulerDiagnostics.ThreadSlotCount;
            for (int slot = 0; slot < slotCount; slot++)
            {
                if (!SchedulerDiagnostics.TryGetThreadInSlot(slot, out KernelThreadInfo thread) || thread.State == KernelThreadState.Dead)
                {
                    continue;
                }

                sb.Append(thread.Id.ToString().PadRight(8, ' '));
                sb.Append(GetStateName(thread.State).PadRight(10, ' '));
                sb.Append((thread.TotalRuntimeNs / SchedulerDiagnostics.NanosecondsPerMillisecond).ToString() + "ms");
                if (thread.IsIdle)
                {
                    sb.Append(" (main loop)");
                }

                sb.AppendLine();
            }
        }

        // No enum ToString() under NativeAOT: map the state by hand.
        private static string GetStateName(KernelThreadState state)
        {
            switch (state)
            {
                case KernelThreadState.Created:
                    return "Created";
                case KernelThreadState.Ready:
                    return "Ready";
                case KernelThreadState.Running:
                    return "Running";
                case KernelThreadState.Blocked:
                    return "Blocked";
                case KernelThreadState.Sleeping:
                    return "Sleeping";
                case KernelThreadState.Dead:
                    return "Dead";
                default:
                    return "?";
            }
        }
    }
}
