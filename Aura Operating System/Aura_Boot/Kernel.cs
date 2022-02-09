using Sys = Cosmos.System;

namespace Aura_Boot
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Aura_OS.Kernel.BeforeRun();
        }

        protected override void Run()
        {
            if (!Aura_OS.Kernel.Running)
                Stop();
            Aura_OS.Kernel.Run();
        }
    }
}
