/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Init networking
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.HAL;
using System;
using System.Collections.Generic;

namespace Aura_OS.System
{
    class NetworkInit
    {

        public static void Init()
        {
            CustomConsole.WriteLineInfo("Finding nic...");

            PCIDevice RTL8168 = PCI.GetDevice((VendorID)0x10EC, (DeviceID)0x8168);
            if (RTL8168 != null)
            {
                HAL.Drivers.Network.RTL8168 xNic;

                CustomConsole.WriteLineOK("Found RTL8168 NIC on PCI " + RTL8168.bus + ":" + RTL8168.slot + ":" + RTL8168.function);
                CustomConsole.WriteLineInfo("NIC IRQ: " + RTL8168.InterruptLine);

                xNic = new HAL.Drivers.Network.RTL8168(RTL8168);

                CustomConsole.WriteLineInfo("NIC MAC Address: " + xNic.MACAddress.ToString());

                Network.NetworkStack.Init();
                xNic.Enable();

                Network.NetworkStack.ConfigIP(xNic, new Network.IPV4.Config(new Network.IPV4.Address(192, 168, 1, 70), new Network.IPV4.Address(255, 255, 255, 0)));
            }
            PCIDevice AMDPCNETII = PCI.GetDevice(VendorID.AMD, DeviceID.PCNETII);
            if (AMDPCNETII != null)
            {
                HAL.Drivers.Network.AMDPCNetII xNic;

                CustomConsole.WriteLineOK("Found AMDPCNETII NIC on PCI " + AMDPCNETII.bus + ":" + AMDPCNETII.slot + ":" + AMDPCNETII.function);
                CustomConsole.WriteLineInfo("NIC IRQ: " + RTL8168.InterruptLine);

                xNic = new HAL.Drivers.Network.AMDPCNetII(AMDPCNETII);

                CustomConsole.WriteLineInfo("NIC MAC Address: " + xNic.MACAddress.ToString());

                Network.NetworkStack.Init();
                xNic.Enable();

                Network.NetworkStack.ConfigIP(xNic, new Network.IPV4.Config(new Network.IPV4.Address(192, 168, 1, 70), new Network.IPV4.Address(255, 255, 255, 0)));
            }
            if (RTL8168 == null && AMDPCNETII == null)
            {
                CustomConsole.WriteLineError("No supported network card found!!");
                return;
            }

            CustomConsole.WriteLineOK("Network initialization done!");
        }
    }
}
