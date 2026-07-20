/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Reusable gen3 kernel-thread run loop.
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Threading;
using SchedulerManager = Cosmos.Kernel.Core.Scheduler.SchedulerManager;
using PerCpuState = Cosmos.Kernel.Core.Scheduler.PerCpuState;

namespace Aura_OS.Processing
{
    /// <summary>
    /// Wraps a gen3 kernel thread running a repeating work loop. AuraOS boots
    /// with the preemptive scheduler enabled (Cosmos default), so a plain
    /// <see cref="System.Threading.Thread"/> maps onto a real kernel thread.
    /// This helper standardises the start/stop lifecycle, swallows per-iteration
    /// exceptions so one faulting tick cannot silently kill the thread, and
    /// records the kernel thread id so lsprocess can report it.
    ///
    /// Used both by background services (<see cref="ThreadedProcess"/>) and by
    /// cooperative GUI apps that offload heavy compute (Cube, GameBoy) while the
    /// main thread keeps compositing.
    /// </summary>
    public class WorkerThread
    {
        private readonly string _name;
        private readonly Action _iteration;
        private readonly int _intervalMs;
        private Thread _thread;

        /// <summary>Set false to ask the loop to exit at the next iteration boundary.</summary>
        public volatile bool Running;

        /// <summary>Kernel scheduler thread id, captured once the loop starts (0 until then).</summary>
        public uint ThreadId { get; private set; }

        /// <param name="name">Diagnostic label for the worker.</param>
        /// <param name="iteration">One unit of work; called repeatedly until stopped.</param>
        /// <param name="intervalMs">Delay between iterations. 0 spins (still preemptible).</param>
        public WorkerThread(string name, Action iteration, int intervalMs)
        {
            _name = name;
            _iteration = iteration;
            _intervalMs = intervalMs;
        }

        public void Start()
        {
            if (Running)
            {
                return;
            }
            Running = true;
            _thread = new Thread(Loop);
            _thread.Start();
        }

        /// <summary>Signals the loop to exit; it stops after the current iteration.</summary>
        public void Stop()
        {
            Running = false;
        }

        private void Loop()
        {
            ThreadId = CurrentThreadId();

            while (Running)
            {
                try
                {
                    _iteration();
                }
                catch
                {
                    // Keep the worker alive across a faulting iteration; a service
                    // that throws every tick just spins harmlessly until stopped.
                }

                if (_intervalMs > 0)
                {
                    // Sleep via the scheduler directly, NOT System.Threading.Thread.Sleep.
                    // The BCL Thread.Sleep path routes through CoreLib's WaitSubsystem ->
                    // LowLevelMonitor, a GC-handle-backed kernel Monitor that faults (#GP,
                    // non-canonical pointer) under concurrent use by several worker threads.
                    // SchedulerManager.Sleep blocks this thread with a plain timer wakeup —
                    // no monitor, no GC handle — the same primitive the scheduler wakes from
                    // reliably. See project-multithreading notes on the kernel Thread.Sleep bug.
                    SchedulerManager.Sleep((uint)_intervalMs);
                }
            }
        }

        /// <summary>Id of the kernel thread this loop is currently running on.</summary>
        private static uint CurrentThreadId()
        {
            if (!SchedulerManager.IsReady)
            {
                return 0;
            }

            PerCpuState cpu = SchedulerManager.GetCpuState(SchedulerManager.GetCurrentCpuId());
            return cpu != null && cpu.CurrentThread != null ? cpu.CurrentThread.Id : 0;
        }
    }
}
