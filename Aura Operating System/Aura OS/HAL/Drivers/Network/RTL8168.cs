/*
* PROJECT:          Aura Operating System Development
* CONTENT:          RTL8168 driver
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.Core;
using Aura_OS.System.Utils;
using Cosmos.Core;
using Cosmos.HAL;
using Cosmos.HAL.Network;
using System;
using System.Collections.Generic;
using static Cosmos.Core.INTs;

namespace Aura_OS.HAL.Drivers.Network
{
    unsafe class RTL8168
    {

        protected PCIDevice pciCard;
        public static MACAddress mac;

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

            device.Claimed = true;
            device.EnableDevice();

            Read_Mac();

            mac = new MACAddress(device_mac);
            Console.WriteLine("MAC OK!");


            SetIrqHandler(device.InterruptLine, HandleNetworkInterrupt);
        }

        static void HandleNetworkInterrupt(ref IRQContext aContext)
        {
            Console.WriteLine("RTL8168 IRQ raised!");
        }

        static void Read_Mac()
        {
            for (int i = 0; i < 6; i++)
                device_mac[i] = Ports.inb((ushort)(rl8168_io_base_addr + RL_MAC_OFFSET + i));
            return;
        }

    }
}
