/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Save video ram and reload it.
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.Core;
using Aura_OS.System.Shell.VESAVBE;
using Cosmos.Core.Memory.Old;

namespace Aura_OS.HAL
{
    unsafe class SaveScreen
    {

        static uint* lastbuffer;
        static int lastX;
        static int lastY;

        public static void SaveCurrentScreen()
        {
            lastbuffer = (uint*)Heap.MemAlloc((uint)(Drivers.VBE.len * 4));
            Memory.Memcpy(lastbuffer, Drivers.VBE._buffer, Drivers.VBE.len);
            lastX = Kernel.AConsole.X;
            lastY = Kernel.AConsole.Y;
        }

        public static void PushLastScreen()
        {
            Kernel.AConsole.X = lastX;
            Kernel.AConsole.Y = lastY;
            lastX = 0;
            lastY = 0;
            Memory.Memcpy(Drivers.VBE._buffer, lastbuffer, Drivers.VBE.len);
            Drivers.VBE.WriteToScreen();
            lastbuffer = null;
        }

    }
}
