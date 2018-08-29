/*
* PROJECT:          Aura Operating System Development
* CONTENT:          RTL8168 Driver
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Aura_OS.Core;
using Cosmos.Core;
using Cosmos.Core.IOGroup.Network;
using Cosmos.HAL;
using static Cosmos.Core.INTs;

namespace Aura_OS.HAL.Drivers.Network
{
    public unsafe class RTL8168 : NetworkDevice
    {
        protected PCIDevice pciCard;
        protected AMDPCNetIIIOGroup io;
        protected MACAddress mac;
        protected bool mInitDone;

        protected ManagedMemoryBlock rxBuffer;
        protected int rxBufferOffset;
        protected UInt16 capr;

        protected Queue<byte[]> mRecvBuffer;
        protected Queue<byte[]> mTransmitBuffer;
        private int mNextTXDesc;

        const UInt16 RxBufferSize = 32768;
        uint BaseAddress;

        public override CardType CardType => CardType.Ethernet;

        public struct rx_desc
        {
            public ushort buffer_size;
            public ushort reserved;
            public int eor; // if set end of ring
            public int own; // if 1 owned by nic / else owned by host
            public uint vlan;
            public uint addr_low;
            public uint addr_high;
        }

        public struct rx_desc_status
        {
            public ushort frame_length; // if own = 0 and ls = 1 -> packet length incl. crc in bytes
            public int tcpf; // if set -> tcp checksum failure
            public int udpf; // if set -> udp checksum failure
            public int ipf; // if set -> ip checksum failure
            public int pid; // protocol-ID:
                            /*
                            00 = IP
                            01 = TCP/IP
                            10 = UDP/IP
                            11 = IP
                            */
            public int crc; // if set -> crc-error
            public int runt; // packet smaller than 64 bytes
            public int res; // if set and ls=1 -> error (crc,runt,rwt,fae)
            public int rwt; // packet bigger than 8192 bytes
            public int reserved; // always 0x01
            public int bar; // broadcast packet received
            public int pam; // physical packet received
            public int mar; // multicast packet received
            public int ls; // if set this is the last segment of packet
            public int fs; // if set this is the first segment of packet
            public int eor; // if set end of ring
            public int own; // if 1 owned by nic / else owned by host
            public uint vlan;
            public uint addr_low;
            public uint addr_high;
        }

        struct tx_desc
        {
            public ushort frame_length; // if own = 0 and ls = 1 -> packet length incl. crc in bytes
            public int tcpcs; // if set -> auto checksum
            public int udpcs; // if set -> auto checksum
            public int ipcs; // if set -> auto checksum
            public int reserved; // Reserved
            public int lgsen; // Large-send
            public int ls; // if set this is the last segment of packet
            public int fs; // if set this is the first segment of packet
            public int eor; // if set end of ring
            public int own; // if 1 owned by nic / else owned by host
            public uint vlan;
            public uint addr_low;
            public uint addr_high;
        }

        rx_desc* rx_descs;
        tx_desc* tx_descs;

        byte*[] rx_buf = new byte*[10];
        byte*[] tx_buf = new byte*[10];

        int realtek_next_tx = 0;

        public override string Name => "RTL8168";

        public static class Size
        {
            public static int Of(uint x)
            {
                return sizeof(uint);
            }

        }

        void init_buffers()
        {
            rx_descs = (rx_desc*)Cosmos.Core.Memory.Old.Heap.MemAlloc((uint)rx_descs);
            tx_descs = (tx_desc*)Cosmos.Core.Memory.Old.Heap.MemAlloc((uint)tx_descs);
            for (int i = 0; i < 10; i++)
            {
                rx_buf[i] = (byte*)0;
                tx_buf[i] = (byte*)0;

                rx_descs[i].own = 1;
                rx_descs[i].eor = 0;
                rx_descs[i].buffer_size = 0x0FFF;
                rx_descs[i].addr_low = (uint)rx_buf[i];
                rx_descs[i].addr_high = 0;

                tx_descs[i].own = 0;
                tx_descs[i].eor = 0;
                tx_descs[i].fs = 0;
                tx_descs[i].ls = 0;
                tx_descs[i].lgsen = 0;
                tx_descs[i].ipcs = 0;
                tx_descs[i].udpcs = 0;
                tx_descs[i].tcpcs = 0;
                tx_descs[i].frame_length = 0x0FFF;
                tx_descs[i].addr_low = (uint)tx_buf[i];
                tx_descs[i].addr_high = 0;
            }
            rx_descs[9].eor = 1;
            tx_descs[9].eor = 1;
        }

        public RTL8168(PCIDevice device)
        {
            if (device == null)
            {
                throw new ArgumentException("PCI Device is null. Unable to get Realtek 8168 card");
            }
            pciCard = device;

            // We are handling this device
            pciCard.Claimed = true;

            BaseAddress = this.pciCard.BaseAddressBar[0].BaseAddress;

            SetIrqHandler(device.InterruptLine, HandleNetworkInterrupt);

            // Enable the card
            pciCard.EnableDevice();

            // Get the MAC Address
            byte[] eeprom_mac = new byte[6];
            for (uint b = 0; b < 6; b++)
            {
                eeprom_mac[b] = Ports.inb((ushort)(BaseAddress + b));
            }

            this.mac = new MACAddress(eeprom_mac);

            Ports.outw((ushort)(BaseAddress + 0x3E), Ports.inw((ushort)(BaseAddress + 0x3E))); //Status zurücksetzen

            init_buffers();

            Ports.outd((ushort)(BaseAddress + 0x44), 0x0000E70F);
            Ports.outb((ushort)(BaseAddress + 0x37), 0x04); // Enable TX
            Ports.outd((ushort)(BaseAddress + 0x40), 0x03000700);
            Ports.outw((ushort)(BaseAddress + 0xDA), 0x0FFF); // Maximal 8kb-Pakete
            Ports.outb((ushort)(BaseAddress + 0xEC), 0x3F); // No early transmit

            Ports.outd((ushort)(BaseAddress + 0x20), (uint)tx_descs);
            Ports.outd((ushort)(BaseAddress + 0xE4), (uint)rx_descs);

            Ports.outw((ushort)(BaseAddress + 0x3C), 0x03FF); //Activating all Interrupts
            Ports.outb((ushort)(BaseAddress + 0x37), 0x0C); // Enabling receive and transmit

        }

        protected void HandleNetworkInterrupt(ref IRQContext aContext)
        {
            Console.WriteLine("RTL8168 IRQ raised!");
        }


        #region Network Device Implementation
        public override MACAddress MACAddress
        {
            get { return this.mac; }
        }

        public override bool Enable()
        {
            return true;
        }

        public override bool Ready
        {
            get { return true; }
        }

        public override bool QueueBytes(byte[] buffer, int offset, int length)
        {
            return true;
        }

        public override bool ReceiveBytes(byte[] buffer, int offset, int max)
        {
            throw new NotImplementedException();
        }

        public override byte[] ReceivePacket()
        {
            return new byte[] { };
        }

        public override int BytesAvailable()
        {
            return 0;
        }

        public override bool IsSendBufferFull()
        {
            return false;
        }

        public override bool IsReceiveBufferFull()
        {
            return false;
        }

        #endregion

    }
}