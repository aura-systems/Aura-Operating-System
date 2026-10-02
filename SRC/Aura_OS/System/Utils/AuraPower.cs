/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Reboot / shutdown with a filesystem flush
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Aura_OS.System.Filesystem;
using Cosmos.Kernel.System.Diagnostics;

namespace Aura_OS.System
{
    /// <summary>
    /// Every reboot/shutdown goes through here (inside ...Commands.Power a bare `Power` is that namespace).
    /// Unmounts (flushes) every volume, then calls Cosmos.Kernel.System.Power. Does not return.
    /// </summary>
    public static class AuraPower
    {
        public static void Reboot()
        {
            Log.WriteString("[Aura] Rebooting...\n");
            UnmountVolumes();
            Cosmos.Kernel.System.Power.Reboot();
        }

        public static void Shutdown()
        {
            Log.WriteString("[Aura] Shutting down...\n");
            UnmountVolumes();
            Cosmos.Kernel.System.Power.Shutdown();
        }

        private static void UnmountVolumes()
        {
            try
            {
                Volumes.UnmountAll();
            }
            catch (Exception)
            {
                // Power off anyway.
            }
        }
    }
}
