/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Interlocked spinlock for cross-thread critical sections.
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Threading;

namespace Aura_OS.Processing
{
    /// <summary>
    /// A tiny spinlock built on <see cref="Interlocked"/>. Used instead of
    /// <c>lock</c>/<see cref="System.Threading.Monitor"/> because the kernel's
    /// contended-monitor path (LowLevelMonitor, GC-handle backed) faults under
    /// concurrency (see WorkerThread / project-multithreading notes). Interlocked
    /// lowers to a raw <c>cmpxchg</c> — no monitor, no GC handle — and the kernel's
    /// own SpinLock uses the same primitive.
    ///
    /// Only for brief critical sections (a list Add/Remove, a snapshot copy).
    /// Not reentrant. Safe here because it is never taken from interrupt context;
    /// a holder that gets preempted is simply rescheduled and releases.
    /// </summary>
    public sealed class SpinGuard
    {
        private int _held; // 0 = free, 1 = held

        public void Enter()
        {
            while (Interlocked.CompareExchange(ref _held, 1, 0) != 0)
            {
                // Spin. The preemptive scheduler will run the holder, which then
                // releases; on a single CPU this just burns the rest of the quantum.
            }
        }

        public void Exit()
        {
            Interlocked.Exchange(ref _held, 0);
        }
    }
}
