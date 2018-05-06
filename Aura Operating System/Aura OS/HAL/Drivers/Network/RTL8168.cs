/*
* PROJECT:          Aura Operating System Development
* CONTENT:          RTL8168 driver
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.Core;
using Aura_OS.System.Utils;
using Cosmos.HAL;
using System;

namespace Aura_OS.HAL.Drivers.Network
{
    class RTL8168
    {

        static uint rl8168_io_base_addr = 0;
        static uint RL_MAC_OFFSET = 0x00;

        static byte[] device_mac = new byte[6];

        public static void Init(PCIDevice device)
        {
            if (device == null)
            {
                throw new ArgumentException("PCI Device is null. Unable to get RTL8168 card");
            }

            rl8168_io_base_addr = device.BaseAddressBar[0].BaseAddress;

            Read_Mac();

            Console.WriteLine("Network Card MAC Address: %02x:%02x:%02x:%02x:%02x:%02x",
               Conversion.DecToHex(device_mac[0]), Conversion.DecToHex(device_mac[1]), Conversion.DecToHex(device_mac[2]),
               Conversion.DecToHex(device_mac[3]), Conversion.DecToHex(device_mac[4]), Conversion.DecToHex(device_mac[5]));

            return;
        }

        static void Read_Mac()
        {
            for (int i = 0; i < 6; i++)
                device_mac[i] = Ports.inb((ushort)(rl8168_io_base_addr + RL_MAC_OFFSET + i));
            return;
        }

    }
}
