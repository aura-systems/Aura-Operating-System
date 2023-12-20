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
        // TODO: continue adding exceptions to the list, as HAL and Core would be documented.
        /// <summary>
        /// Initializes the console, screen and keyboard.
        /// </summary>
        /// <param name="textScreen">A screen device.</param>
        public static void Init(TextScreenBase textScreen, bool initScrollWheel = true, bool initPS2 = true, bool initNetwork = true, bool ideInit = true)
        {
            // We must init Console before calling Inits.
            // This is part of the "minimal" boot to allow output.
            Kernel.aConsole = new Graphics.UI.CUI.Console(textScreen);

            CustomConsole.WriteLineInfo("Initializing the Hardware Abstraction Layer (HAL)...");
            Cosmos.HAL.Global.Init(textScreen, initScrollWheel, initPS2, initNetwork, ideInit);

            CustomConsole.WriteLineInfo("Starting Aura Operating System v" + Kernel.Version + "-" + Kernel.Revision);

            // TODO: @ascpixi: The end-user should have an option to exclude parts of
            //       Cosmos, such as the network stack, when they are not needed. As of
            //       now, these modules will *always* be included, as they're referenced
            //       by the initialization code.
            CustomConsole.WriteLineInfo("Initializing the network stack...");
            Cosmos.System.Network.NetworkStack.Initialize();

            Cosmos.System.Global.NumLock = false;
            Cosmos.System.Global.CapsLock = false;
            Cosmos.System.Global.ScrollLock = false;
        }
    }
}