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
            Console.WriteLine("Finding PCNETII nic...");

            HAL.Drivers.Network.AMDPCNetII xNic;

            Cosmos.HAL.PCIDevice xNicDev = Cosmos.HAL.PCI.GetDevice(Cosmos.HAL.VendorID.AMD, Cosmos.HAL.DeviceID.PCNETII);
            if (xNicDev == null)
            {
                Console.WriteLine("PCNETII not found!!");
                return;
            }

            Console.WriteLine("Found AMD PCNetII NIC on PCI " + xNicDev.bus + ":" + xNicDev.slot + ":" + xNicDev.function);
            Console.WriteLine("NIC IRQ: " + xNicDev.InterruptLine);

            xNic = new HAL.Drivers.Network.AMDPCNetII(xNicDev);

            Console.WriteLine("NIC MAC Address: " + xNic.MACAddress.ToString());

            Network.NetworkStack.Init();
            xNic.Enable();

            Network.NetworkStack.ConfigIP(xNic, new Network.IPV4.Config(new Network.IPV4.Address(192, 168, 1, 70), new Network.IPV4.Address(255, 255, 255, 0)));
            Console.WriteLine("Network initialization done!");
        }
    }
}
