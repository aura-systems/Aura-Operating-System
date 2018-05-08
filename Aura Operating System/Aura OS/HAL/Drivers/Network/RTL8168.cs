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
using static Cosmos.Core.Memory.Old.Heap;

namespace Aura_OS.HAL.Drivers.Network
{
    unsafe class RTL8168
    {

        protected PCIDevice pciCard;
        public static MACAddress mac;

        static uint BaseAddress = 0;
        static uint RL_MAC_OFFSET = 0x00;

        static byte[] device_mac = new byte[6];

        public struct cdi_mem_area
        {
            public uint size;
            public void* vaddr;
            public cdi_mem_sg_list paddr;

            public cdi_mem_osdep osdep;
        };

        public struct cdi_mem_sg_list
        {
            public uint num;
            public cdi_mem_sg_item* items;
        };

        public struct cdi_mem_sg_item
        {
            public ushort start;
            public uint size;
        }

        public struct cdi_mem_osdep
        {
        }


        public struct rtl8168b_device
        {

            public int tx_index;
            public int rx_index;

            public cdi_mem_area* tx_buffer_area;
            public cdi_mem_area* rx_buffer_area;

            public rtl8168b_descriptor* tx_buffer;
            public rtl8168b_descriptor* rx_buffer;

            public uint tx_buffer_phys;
            public uint rx_buffer_phys;

            public cdi_mem_area* rx_area;
        }

        public struct rtl8168b_descriptor
        {
            public uint command;
            public uint vlan;
            public ulong address;
        }


        public static void Init(PCIDevice device)
        {
            if (device == null)
            {
                throw new ArgumentException("PCI Device is null. Unable to get RTL8168 card");
            }

            SetIrqHandler(device.InterruptLine, HandleNetworkInterrupt);

            BaseAddress = device.BaseAddressBar[0].BaseAddress;

            device.Claimed = true;
            device.EnableDevice();

        }

        static void HandleNetworkInterrupt(ref IRQContext aContext)
        {
            Console.WriteLine("RTL8168 IRQ raised!");
        }

        static void Read_Mac()
        {
            for (int i = 0; i < 6; i++)
                device_mac[i] = Ports.inb((ushort)(BaseAddress + RL_MAC_OFFSET + i));
            return;
        }

    }
}
