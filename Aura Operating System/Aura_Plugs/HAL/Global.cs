/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Plug of Cosmos.HAL.Global
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using IL2CPU.API.Attribs;
using Cosmos.HAL;
using Cosmos.Core;
using Aura_OS.System;

namespace Aura_Plugs.HAL
{

    [Plug(Target = typeof(Cosmos.HAL.Global))]
    public static class Global
    {

        static public PIT PIT = new PIT();

        static public void Init(TextScreenBase textScreen)
        {

            Cosmos.Core.Global.Init();

            Console.WriteLine("[Aura Operating System v" + Aura_OS.Kernel.version + " - Made by valentinbreiz and geomtech]");
            CustomConsole.WriteLineInfo("Starting Cosmos kernel...");

            PCI.Setup();
            CustomConsole.WriteLineOK("PCI Devices Scan");

            ACPI.Start();
            CustomConsole.WriteLineOK("ACPI Initialization");

           // Cosmos.HAL.BlockDevice.IDE.InitDriver();
            CustomConsole.WriteLineOK("IDE Driver Initialization");

            //Cosmos.HAL.BlockDevice.AHCI.InitDriver();
            CustomConsole.WriteLineOK("AHCI Driver Initialization");

            CustomConsole.WriteLineOK("Kernel successfully initialized!");

        }
    }
}
