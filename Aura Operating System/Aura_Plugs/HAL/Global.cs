/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Plug of Cosmos.HAL.Global
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using IL2CPU.API.Attribs;
using Cosmos.HAL;
using Cosmos.Core;
using Cosmos.HAL.Network;
using Aura_OS;
using Cosmos.HAL.Drivers;

namespace Aura_Plugs.HAL
{

    [Plug(Target = typeof(Cosmos.HAL.Global))]
    public static class Global
    {

        static public void Init(TextScreenBase textScreen, bool InitScrollWheel, bool InitPS2, bool InitNetwork, bool IDEInit)
        {
            PCI.Setup();
            Aura_OS.System.CustomConsole.WriteLineOK("PCI Devices Scan");

            var _SVGAIIDevice = PCI.GetDevice(VendorID.VMWare, DeviceID.SVGAIIAdapter);

            //Check if VMWare SVGAII graphic card exists without VBE enabled from multiboot
            //This is useful to detect if VBE graphic mode is enabled from the bootloader.
            /*if (_SVGAIIDevice != null && PCI.Exists(_SVGAIIDevice) && VBE.IsAvailable() == false)
            {
                Global.AConsole = new Aura_OS.System.AConsole.GraphicalConsole();
            }*/
            if (VBEAvailable())
            {
                Aura_OS.Global.AConsole = new Aura_OS.System.AConsole.GraphicalConsole();
            }
            else
            {
                Aura_OS.Global.AConsole = new Aura_OS.System.AConsole.VGAConsole(textScreen);
            }

            Console.WriteLine("[Aura Operating System v" + Aura_OS.Global.version + " - Made by valentinbreiz and geomtech]");
            Aura_OS.System.CustomConsole.WriteLineInfo("Starting Cosmos kernel...");

            ACPI.Start();
            Aura_OS.System.CustomConsole.WriteLineOK("ACPI Initialization");

            Cosmos.HAL.Global.PS2Controller.Initialize(false);
            Aura_OS.System.CustomConsole.WriteLineOK("PS/2 Controller Initialization");

            Cosmos.HAL.BlockDevice.IDE.InitDriver();
            Aura_OS.System.CustomConsole.WriteLineOK("IDE Driver Initialization");

            Cosmos.HAL.BlockDevice.AHCI.InitDriver();
            Aura_OS.System.CustomConsole.WriteLineOK("AHCI Driver Initialization");

            Cosmos.HAL.Network.NetworkInit.Init();
            Aura_OS.System.CustomConsole.WriteLineOK("Network Devices Initialization");

            Aura_OS.System.CustomConsole.WriteLineOK("Kernel successfully initialized!");

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
        private static bool BGAExists()
        {
            return VBEDriver.ISAModeAvailable();
        }
    }
}
