/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Plug of Cosmos.System.Global
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using IL2CPU.API.Attribs;
using Cosmos.HAL;

namespace Aura_OS.System.Plugs
{

    [Plug(Target = typeof(Cosmos.System.Global))]
    public static class Global
    {
        public static void Init(TextScreenBase textScreen)
        {
            Kernel.AConsole = new Shell.VESAVBE.VESAVBEConsole();
            HAL.Plugs.Global.Init(textScreen);
            Cosmos.System.Global.NumLock = false;
            Cosmos.System.Global.CapsLock = false;
            Cosmos.System.Global.ScrollLock = false;
            //Network.NetworkStack.Init();
        }
    }
}
