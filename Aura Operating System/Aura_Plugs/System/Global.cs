/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Plug of Cosmos.System.Global
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using IL2CPU.API.Attribs;
using Cosmos.HAL;
using Aura_OS;
using Cosmos.Core;
using Cosmos.HAL.Drivers;

namespace Aura_OS.System.Plugs
{

    [Plug(Target = typeof(Cosmos.System.Global))]
    public static class Global
    {
        public static void Init(TextScreenBase textScreen)
        {
            Cosmos.System.Global.mDebugger.Send("Creating Console");

            Aura_Plugs.HAL.Global.Init(textScreen);

            /*if (_SVGAIIDevice != null && PCI.Exists(_SVGAIIDevice))
            {
                return new SVGAIICanvas();
            }
            */
            if (VBEAvailable())
            {
                Kernel.AConsole = new AConsole.VESAVBE.VESAVBEConsole();
            }
            else
            {
                Kernel.AConsole = new AConsole.VGA.VGAConsole(textScreen);
            }

            Cosmos.System.Global.mDebugger.Send("HW Init");

            Cosmos.System.Network.NetworkStack.Init();
            Cosmos.System.Global.mDebugger.Send("Network Stack Init");

            Cosmos.System.Global.NumLock = false;
            Cosmos.System.Global.CapsLock = false;
            Cosmos.System.Global.ScrollLock = false;
        }

        /// <summary>
        /// Checks is VBE is supported exists
        /// </summary>
        /// <returns></returns>
        private static bool VBEAvailable()
        {
            if (BGAExists())
            {
                return true;
            }
            else if (PCI.Exists(VendorID.VirtualBox, DeviceID.VBVGA))
            {
                return true;
            }
            else if (PCI.Exists(VendorID.Bochs, DeviceID.BGA))
            {
                return true;
            }
            else if (VBE.IsAvailable())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether the Bochs Graphics Adapter exists (not limited to Bochs)
        /// </summary>
        /// <returns></returns>
        public static bool BGAExists()
        {
            return VBEDriver.ISAModeAvailable();
        }

    }
}
