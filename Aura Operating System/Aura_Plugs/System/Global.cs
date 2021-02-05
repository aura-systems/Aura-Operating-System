/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Plug of Cosmos.System.Global
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using IL2CPU.API.Attribs;
using Cosmos.HAL;
using Aura_OS;

namespace Aura_OS.System.Plugs
{

    [Plug(Target = typeof(Cosmos.System.Global))]
    public static class Global
    {
        public static void Init(TextScreenBase textScreen)
        {
            Cosmos.System.Global.mDebugger.Send("Creating Console");

            Aura_Plugs.HAL.Global.Init(textScreen);

            Kernel.AConsole = new AConsole.VESAVBE.VESAVBEConsole();

            Cosmos.System.Global.mDebugger.Send("HW Init");

            Cosmos.System.Network.NetworkStack.Init();
            Cosmos.System.Global.mDebugger.Send("Network Stack Init");

            Cosmos.System.Global.NumLock = false;
            Cosmos.System.Global.CapsLock = false;
            Cosmos.System.Global.ScrollLock = false;
        }
    }
}
