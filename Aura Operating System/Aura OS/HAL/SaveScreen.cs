/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Save video ram and reload it.
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

namespace Aura_OS.HAL
{
    class SaveScreen
    {

        static int[] lastbuffer;
        static int lastX;
        static int lastY;

        public static void SaveCurrentScreen()
        {
            lastbuffer = Drivers.ManagedVBE.LinearFrameBuffer.Copy(0, 0, Drivers.ManagedVBE.len * 4);
            lastX = Kernel.AConsole.X;
            lastY = Kernel.AConsole.Y;
        }

        public static void PushLastScreen()
        {
            Kernel.AConsole.X = lastX;
            Kernel.AConsole.Y = lastY;
            lastX = 0;
            lastY = 0;
            Drivers.ManagedVBE.LinearFrameBuffer.Copy(lastbuffer);
            lastbuffer = null;
        }

    }
}
