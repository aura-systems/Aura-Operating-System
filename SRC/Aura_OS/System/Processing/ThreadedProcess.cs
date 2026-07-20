/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Process that self-drives on a dedicated kernel thread.
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

namespace Aura_OS.Processing
{
    /// <summary>
    /// A <see cref="Process"/> that runs itself on a dedicated gen3 kernel
    /// thread instead of being ticked cooperatively by ProcessManager.Update().
    /// Use it for background services (stats sampling, polling loops) that need
    /// their own cadence and never touch the shared framebuffer.
    ///
    /// GUI work stays on the cooperative main thread — the window manager and
    /// every app share one framebuffer and a pile of mutable statics, so the
    /// compositor must remain single-threaded. Apps that need parallelism spawn
    /// a <see cref="WorkerThread"/> for their compute and hand results back to
    /// the main thread by atomic reference publish (see CubeApp / GameBoyApp),
    /// rather than deriving from this class.
    /// </summary>
    public abstract class ThreadedProcess : Process
    {
        private WorkerThread _worker;

        /// <summary>Delay between <see cref="Tick"/> calls, in milliseconds.</summary>
        protected int IntervalMs = 100;

        protected ThreadedProcess(string name, ProcessType type) : base(name, type)
        {
        }

        public override bool IsThreaded => true;

        public override uint ThreadId => _worker != null ? _worker.ThreadId : 0;

        public override void Start()
        {
            if (Running)
            {
                return;
            }
            base.Start();
            _worker = new WorkerThread(Name, Tick, IntervalMs);
            _worker.Start();
        }

        public override void Stop()
        {
            if (!Running)
            {
                return;
            }
            if (_worker != null)
            {
                _worker.Stop();
            }
            base.Stop();
        }

        /// <summary>One iteration of the service loop, run on the worker thread.</summary>
        protected abstract void Tick();
    }
}
