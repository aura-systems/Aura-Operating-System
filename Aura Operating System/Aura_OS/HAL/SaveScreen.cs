/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Save video ram and reload it.
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

//using Aura_OS.HAL.Drivers;

namespace Aura_OS.HAL
{
    /*class SaveScreen
    {

        static uint[] lastbuffer;
        static int lastX;
        static int lastY;

        public static void SaveCurrentScreen()
        {
            lastbuffer = ManagedVBE.LinearFrameBuffer.ToArray();
            lastX = Global.AConsole.X;
            lastY = Global.AConsole.Y;
        }

        public static void PushLastScreen()
        {
            Global.AConsole.X = lastX;
            Global.AConsole.Y = lastY;
            lastX = 0;
            lastY = 0;
            ManagedVBE.LinearFrameBuffer.Copy(lastbuffer);
            lastbuffer = null;
        }

    }*/
}
