/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Init networking
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;

namespace Aura_OS.System
{
    class NetworkInit
    {
        public static void Init()
        {
            CustomConsole.WriteLineInfo("Finding PCNETII nic...");

            HAL.Drivers.Network.AMDPCNetII xNic;

            Cosmos.HAL.PCIDevice xNicDev = Cosmos.HAL.PCI.GetDevice(Cosmos.HAL.VendorID.AMD, Cosmos.HAL.DeviceID.PCNETII);
            if (xNicDev == null)
            {
                CustomConsole.WriteLineError("PCNETII not found!!");
                return;
            }

            CustomConsole.WriteLineOK("Found AMD PCNetII NIC on PCI " + xNicDev.bus + ":" + xNicDev.slot + ":" + xNicDev.function);
            CustomConsole.WriteLineInfo("NIC IRQ: " + xNicDev.InterruptLine);

            xNic = new HAL.Drivers.Network.AMDPCNetII(xNicDev);

            CustomConsole.WriteLineInfo("NIC MAC Address: " + xNic.MACAddress.ToString());

            Network.NetworkStack.Init();
            xNic.Enable();
            
            CustomConsole.WriteLineOK("Network initialization done!");
        }
    }
}
