/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Live system monitor / task manager window.
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Processing.Services;
using Cosmos.Kernel.Core.Scheduler;

namespace Aura_OS.System.Processing.Applications
{
    /// <summary>
    /// GUI counterpart to the lsprocess command: shows the aggregate stats
    /// published by the <see cref="SystemMonitor"/> background service plus the
    /// live kernel-thread table straight from the scheduler. Open Cube or the
    /// GameBoy emulator and their compute worker threads appear here in real time.
    /// </summary>
    public class SystemMonitorApp : Application
    {
        public static string ApplicationName = "System Monitor";

        public SystemMonitorApp(int width, int height, int x = 0, int y = 0) : base(ApplicationName, width, height, x, y)
        {
            // Refresh every frame so the stats and thread states stay live.
            ForceDirty = true;
        }

        public override void Draw()
        {
            base.Draw();

            int h = Kernel.font.Height;
            int line = 0;

            DrawString("CPU usage    : " + SystemMonitor.CpuUsagePercent + " %", 0, line++ * h);
            DrawString("Live threads : " + SystemMonitor.ThreadCount, 0, line++ * h);
            DrawString("Heap used    : " + (SystemMonitor.UsedRamBytes / 1024) + " KB", 0, line++ * h);
            DrawString("Uptime       : " + SystemMonitor.UptimeSeconds + " s", 0, line++ * h);

            line++; // spacer
            DrawString("TID   STATE       CPU(ms)", 0, line++ * h);

            Thread[] threads = SchedulerManager.Threads;
            if (threads != null)
            {
                for (int i = 0; i < threads.Length; i++)
                {
                    Thread thread = threads[i];
                    if (thread == null || thread.State == ThreadState.Dead)
                    {
                        continue;
                    }

                    string row = thread.Id.ToString().PadRight(6)
                        + StateLabel(thread.State).PadRight(12)
                        + (thread.TotalRuntime / 1_000_000UL).ToString();
                    DrawString(row, 0, line++ * h);
                }
            }
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
                default:
                    return "?";
            }
        }
    }
}
