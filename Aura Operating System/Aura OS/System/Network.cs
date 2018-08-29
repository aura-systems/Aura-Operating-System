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
            CustomConsole.WriteLineInfo("Finding RTL8168 nic...");

            HAL.Drivers.Network.RTL8168 xNic;

            Cosmos.HAL.PCIDevice xNicDev = Cosmos.HAL.PCI.GetDevice((Cosmos.HAL.VendorID)0x10EC, (Cosmos.HAL.DeviceID)0x8168);
            if (xNicDev == null)
            {
                CustomConsole.WriteLineError("RTL8168 not found!!");
                return;
            }

            CustomConsole.WriteLineOK("Found RTL8168 NIC on PCI " + xNicDev.bus + ":" + xNicDev.slot + ":" + xNicDev.function);
            CustomConsole.WriteLineInfo("NIC IRQ: " + xNicDev.InterruptLine);

            xNic = new HAL.Drivers.Network.RTL8168(xNicDev);

            CustomConsole.WriteLineInfo("NIC MAC Address: " + xNic.MACAddress.ToString());

            Network.NetworkStack.Init();
            xNic.Enable();

            Network.NetworkStack.ConfigIP(xNic, new Network.IPV4.Config(new Network.IPV4.Address(192, 168, 1, 70), new Network.IPV4.Address(255, 255, 255, 0)));
            CustomConsole.WriteLineOK("Network initialization done!");
        }
    }
}
