/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Save video ram and reload it.
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

namespace Aura_OS.HAL
{
    class SaveScreen
    {

        static uint[] lastbuffer;
        static int lastX;
        static int lastY;

        public static void SaveCurrentScreen()
        {
            lastbuffer = new uint[Drivers.ManagedVBE.len * 4];
            for (int i = 0; i < Drivers.ManagedVBE.LinearFrameBuffer.Size; i++)
            {
                lastbuffer[i] = Drivers.ManagedVBE.LinearFrameBuffer.Bytes[(uint)i];
            }
            lastX = Kernel.AConsole.X;
            lastY = Kernel.AConsole.Y;
        }

        public static void PushLastScreen()
        {
            Kernel.AConsole.X = lastX;
            Kernel.AConsole.Y = lastY;
            lastX = 0;
            lastY = 0;
            for (int i = 0; i < Drivers.ManagedVBE.LinearFrameBuffer.Size; i++)
            {
                Drivers.ManagedVBE.LinearFrameBuffer.Bytes[(uint)i] = (byte)lastbuffer[i];
            }
            lastbuffer = null;
        }

    }
}
