/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Save video ram and reload it.
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.Core;
using Aura_OS.System.Shell.VESAVBE;

namespace Aura_OS.HAL
{
    unsafe class SaveScreen
    {

        static uint* lastbuffer;

        public static void SaveCurrentScreen()
        {
            Memory.Memcpy(lastbuffer, Drivers.VBE._buffer, Drivers.VBE.len);
        }

        public static void PushLastScreen()
        {
            Memory.Memcpy((uint*)Graphics.vga_mem, lastbuffer, Drivers.VBE.len);
            lastbuffer = null;
        }

    }
}
