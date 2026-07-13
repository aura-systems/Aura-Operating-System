/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Main entry point for AuraOS (Cosmos gen3).
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Sys = Cosmos.Kernel.System;

namespace Aura_Boot
{
    /// <summary>
    /// Represents the main kernel of AuraOS, extending the Cosmos gen3 kernel.
    /// This class serves as the entry point for the operating system, handling the initial setup
    /// before the operating system's main loop begins and managing the main loop itself.
    /// </summary>
    public class Kernel : Sys.Kernel
    {
        protected override void OnBoot()
        {
            // Default OnBoot brings up the graphical KernelConsole; hardware (timer, keyboard,
            // mouse, storage, NIC) is initialized automatically by the gen3 library initializers.
            base.OnBoot();

            Sys.Network.NetworkStack.Initialize();
        }

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
