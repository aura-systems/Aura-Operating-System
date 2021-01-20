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

namespace Aura_Plugs.HAL
{

    [Plug(Target = typeof(Cosmos.HAL.Global))]
    public static class Global
    {

        static public void Init(TextScreenBase textScreen)
        {

            Console.WriteLine("[Aura Operating System v" + Aura_OS.Kernel.version + " - Made by valentinbreiz and geomtech]");
            Aura_OS.System.CustomConsole.WriteLineInfo("Starting Cosmos kernel...");

            PCI.Setup();
            Aura_OS.System.CustomConsole.WriteLineOK("PCI Devices Scan");

            ACPI.Start();
            Aura_OS.System.CustomConsole.WriteLineOK("ACPI Initialization");

            Cosmos.HAL.Global.PS2Controller.Initialize();
            Aura_OS.System.CustomConsole.WriteLineOK("PS/2 Controller Initialization");

            Cosmos.HAL.BlockDevice.IDE.InitDriver();
            Aura_OS.System.CustomConsole.WriteLineOK("IDE Driver Initialization");

            Cosmos.HAL.BlockDevice.AHCI.InitDriver();
            Aura_OS.System.CustomConsole.WriteLineOK("AHCI Driver Initialization");

            Cosmos.HAL.Network.NetworkInit.Init();
            Aura_OS.System.CustomConsole.WriteLineOK("Network Devices Initialization");

            Aura_OS.System.CustomConsole.WriteLineOK("Kernel successfully initialized!");

        }
    }
}
