/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Main entry point for AuraOS.
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Sys = Cosmos.System;

namespace Aura_Boot
{
    /// <summary>
    /// Represents the main kernel of AuraOS, extending the Cosmos.System.Kernel.
    /// This class serves as the entry point for the operating system, handling the initial setup
    /// before the operating system's main loop begins and managing the main loop itself.
    /// </summary>
    public class Kernel : Sys.Kernel
    {
        /// <summary>
        /// Performs initialization tasks before the main AuraOS loop starts.
        /// This method calls the BeforeRun method of the Aura_OS.Kernel to perform
        /// any necessary pre-run initialization tasks.
        /// </summary>
        protected override void BeforeRun()
        {
            Aura_OS.Kernel.BeforeRun();
        }

        /// <summary>
        /// The main loop of AuraOS. This method checks if the Aura_OS.Kernel
        /// indicates that the system should continue running. If not, it stops the kernel.
        /// Otherwise, it calls the Run method of the Aura_OS.Kernel to perform the main
        /// operating system tasks.
        /// </summary>
        protected override void Run()
        {
            if (!Aura_OS.Kernel.Running)
                Stop();
            Aura_OS.Kernel.Run();
        }
    }
}
