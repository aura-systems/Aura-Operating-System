/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Main entry point for AuraOS.
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Aura_OS.System;
using Sys = Cosmos.Kernel.System;

namespace Aura_Boot
{
    /// <summary>
    /// Represents the main kernel of AuraOS, extending Cosmos.Kernel.System.Kernel.
    /// This class serves as the entry point for the operating system (selected by
    /// CosmosKernelClass in Aura_OS.csproj), handling the initial setup before the
    /// operating system's main loop begins and managing the main loop itself.
    /// </summary>
    public class Kernel : Sys.Kernel
    {
        /// <summary>
        /// Called once during boot, with interrupts enabled and the drivers started.
        /// base.OnBoot() brings up the KernelConsole, which makes Console.* draw.
        /// Prints the boot banner the gen2 Global plug used to print.
        /// </summary>
        protected override void OnBoot()
        {
            base.OnBoot();

            CustomConsole.WriteLineInfo("Starting Aura Operating System v" + Aura_OS.Kernel.Version + "-" + Aura_OS.Kernel.Revision);
            CustomConsole.WriteLineInfo("Cosmos gen3 " + VersionString);
        }

        /// <summary>
        /// Performs initialization tasks before the main AuraOS loop starts.
        /// This method calls the BeforeRun method of the Aura_OS.Kernel to perform
        /// any necessary pre-run initialization tasks.
        /// </summary>
        protected override void BeforeRun()
        {
            try
            {
                Aura_OS.Kernel.BeforeRun();
            }
            catch (Exception ex)
            {
                // gen3: an exception escaping BeforeRun is not caught by Cosmos.Kernel.System.Kernel.Start().
                // StopKernel falls back to text when the canvas/assets do not exist yet.
                Crash.StopKernel("Boot failed", ex.Message, "", "0");
            }
        }

        /// <summary>
        /// The main loop of AuraOS. Loops here while Aura_OS.Kernel indicates that the
        /// system should continue running (Cosmos.Kernel.System.Kernel.Start() logs two
        /// serial lines around every Run() call), then stops the kernel.
        /// </summary>
        protected override void Run()
        {
            while (Aura_OS.Kernel.Running && !Stopped)
            {
                Aura_OS.Kernel.Run();
            }

            Stop();
        }
    }
}
