/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Background system-stats sampling service.
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.Processing;
using Cosmos.Kernel.Core.Scheduler;
using GarbageCollector = Cosmos.Kernel.Core.Memory.GarbageCollector.GarbageCollector;

namespace Aura_OS.System.Processing.Services
{
    /// <summary>
    /// Samples system-wide statistics (CPU load, live kernel thread count,
    /// committed heap, uptime) once a second on its own kernel thread. It never
    /// touches the framebuffer — it only publishes plain numbers that the UI
    /// and the lsprocess command read. This is the reference threaded service:
    /// it proves the <see cref="ThreadedProcess"/> layer without any locking,
    /// because a torn read of an int/ulong stat is harmless.
    /// </summary>
    public class SystemMonitor : ThreadedProcess
    {
        /// <summary>Percentage of wall-clock the CPU spent busy over the last sample (0-100).</summary>
        public static int CpuUsagePercent;

        /// <summary>Number of live kernel threads at the last sample.</summary>
        public static int ThreadCount;

        /// <summary>Committed managed heap in bytes at the last sample.</summary>
        public static ulong UsedRamBytes;

        /// <summary>Seconds elapsed since the monitor started.</summary>
        public static ulong UptimeSeconds;

        private ulong _lastBusyNs;

        public SystemMonitor() : base("SystemMonitor", ProcessType.KernelComponent)
        {
            IntervalMs = 1000;
        }

        public override void Initialize()
        {
            base.Initialize();

            Kernel.ProcessManager.Register(this);
            Kernel.ProcessManager.Start(this);
        }

        protected override void Tick()
        {
            // Busy time is a monotonic cumulative counter; the per-sample delta
            // over one IntervalMs of wall-clock gives the load percentage.
            ulong nowBusy = SchedulerManager.GetBusyCpuTimeNs();
            ulong deltaBusy = nowBusy >= _lastBusyNs ? nowBusy - _lastBusyNs : 0;
            _lastBusyNs = nowBusy;

            ulong wallNs = (ulong)IntervalMs * 1_000_000UL;
            int pct = wallNs == 0 ? 0 : (int)(deltaBusy * 100UL / wallNs);
            if (pct < 0)
            {
                pct = 0;
            }
            if (pct > 100)
            {
                pct = 100;
            }

            CpuUsagePercent = pct;
            ThreadCount = SchedulerManager.ThreadCount;
            UsedRamBytes = GarbageCollector.GetTotalCommittedBytes();
            UptimeSeconds++;
        }
    }
}
