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
        protected MACAddress mac;

        uint BaseAddress;

        public override CardType CardType => CardType.Ethernet;

        int realtek_next_tx = 0;

        public override string Name => "RTL8168";

        struct Descriptor
        {
            public ushort Command; // bit 31 is OWN, bit 30 is EOR
            public ushort FrameLen;
            public uint vlan;     /* currently unused */
            public uint low_buf;  /* low 32-bits of physical buffer address */
            public uint high_buf; /* high 32-bits of physical buffer address */
        };

        Descriptor* Rx_Descriptors = (Descriptor*)0x100000; /* 1MB Base Address of Rx Descriptors */
        Descriptor* Tx_Descriptors = (Descriptor*)0x200000; /* 1MB Base Address of Rx Descriptors */

        static byte[][] rx_buffer = new byte[10][];
        static byte[][] tx_buffer = new byte[10][];

        int num_of_rx_descriptors = 10, num_of_tx_descriptors = 10;

        int rx_buffer_len = 1024;

        void init_buffers()
        {

            for (int i = 0; i < rx_buffer.Length; i++)
            {
                rx_buffer[i] = new byte[1024];
            }

            //Setup RX

            /* rx_buffer_len is the size (in bytes) that is reserved for incoming packets */
            uint OWN = (1 << 15), EOR = (1 << 14); /* bit offsets */
            for (int i = 0; i < num_of_rx_descriptors; i++) /* num_of_rx_descriptors can be up to 1024 */
            {
                if (i == (num_of_rx_descriptors - 1)) /* Last descriptor? if so, set the EOR bit */
                    Rx_Descriptors[i].Command = (ushort)(OWN | EOR | (rx_buffer_len & 0x3FFF));
                else
                    Rx_Descriptors[i].Command = (ushort)(OWN | (rx_buffer_len & 0x3FFF));

                /** packet_buffer_address is the *physical* address for the buffer */
                fixed (byte* ptr = &rx_buffer[i][0])
                {
                    Rx_Descriptors[i].low_buf = (uint)ptr;
                }

                Rx_Descriptors[i].high_buf = 0;
            /* If you are programming for a 64-bit OS, put the high memory location in the 'high_buf' descriptor area */
            }


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

            // Enable the card
            pciCard.EnableDevice();

            SetIrqHandler(device.InterruptLine, HandleNetworkInterrupt);

            // Get the MAC Address
            byte[] eeprom_mac = new byte[6];
            for (uint b = 0; b < 6; b++)
            {
                eeprom_mac[b] = Ports.inb((ushort)(BaseAddress + b));
            }

            this.mac = new MACAddress(eeprom_mac);

            Reset();

            Console.WriteLine("Reset done.");

            Ports.outw((ushort)(BaseAddress + 0x3E), Ports.inw((ushort)(BaseAddress + 0x3E))); //Status zurücksetzen

            init_buffers();

            Ports.outd((ushort)(BaseAddress + 0xE0), 0x002B);

            //Ports.outd((ushort)(BaseAddress + 0x50), 0xC0);

            Ports.outd((ushort)(BaseAddress + 0x44), 0x0000E70F); // Enable RX
            Ports.outb((ushort)(BaseAddress + 0x37), 0x04); // Enable TX
            Ports.outd((ushort)(BaseAddress + 0x40), 0x03000700);
            Ports.outw((ushort)(BaseAddress + 0xDA), 0x0FFF); // Maximal 8kb-Pakete
            Ports.outb((ushort)(BaseAddress + 0xEC), 0x3F); // No early transmit

            Console.WriteLine("addressrx desc: 0x" + System.Utils.Conversion.DecToHex((int)Rx_Descriptors));
            Console.WriteLine("addresstx desc: 0x" + System.Utils.Conversion.DecToHex((int)Tx_Descriptors));

            Ports.outd((ushort)(BaseAddress + 0x20), (uint)Rx_Descriptors);
            Ports.outd((ushort)(BaseAddress + 0xE4), (uint)Tx_Descriptors);

            Console.WriteLine("0x20 : " + Ports.inb((ushort)(BaseAddress + 0x20)) + " " + Ports.inb((ushort)(BaseAddress + 0x24)));
            Console.WriteLine("0xE4 : " + Ports.inb((ushort)(BaseAddress + 0xE4)) + " " + Ports.inb((ushort)(BaseAddress + 0xE8)));

            Ports.outw((ushort)(BaseAddress + 0x3C), 0x03FF); //Activating all Interrupts
            Ports.outb((ushort)(BaseAddress + 0x37), 0x0C); // Enabling receive and transmit

            //Ports.outd((ushort)(BaseAddress + 0x50), 0x00); /* Lock config registers */

            Console.WriteLine("Init done.");

            byte[] aData = new byte[]
            {
                0x6C, 0x62, 0x6D, 0x93, 0xC1, 0xDA, 0xb8, 0x86, 0x87, 0x24, 0x34, 0xb7, 0x08, 0x00,
                0x45, 0x00, 0x00, 0x24, 0x55, 0x1b, 0x00, 0x00, 0x80, 0x11, 0x62, 0x0b, 0xc0, 0xa8, 0x01, 0x46, 0xc0, 0xa8, 0x01, 0x0c,
                0x10, 0x92, 0x10, 0x92, 0x00, 0x10, 0x15, 0xf3,
                0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x21, 0x21, 0x21
            
            };

            //realtek_send_packet(PointerData(aData, aData.Length), aData.Length);

            Console.WriteLine("Send done.");

        }

        public bool Reset()
        {
            Ports.outb((ushort)(BaseAddress + 0x37), 0x10); /* Send the Reset bit to the Command register */
            while ((Ports.inb((ushort)(BaseAddress + 0x37)) & 0x10) != 0) { } /* Wait for the chip to finish resetting */
            return true;
        }

        public byte* PointerData(byte[] data, int length)
        {
            byte[] safe = new byte[length];
            for (int i = 0; i < length; i++)
                safe[i] = data[i];

            fixed (byte* converted = safe)
            {
                // This will update the safe and converted arrays.
                for (int i = 0; i < length; i++)
                    converted[i]++;

                return converted;
            }
        }

        bool printed = false;
        bool printed1 = false;

        protected void HandleNetworkInterrupt(ref IRQContext aContext)
        {
            ushort status = Ports.inw((ushort)(BaseAddress + 0x3E));

            Console.WriteLine("Status: 0x" + System.Utils.Conversion.DecToHex(status));

            if (status == 0x0020)
            {
                Console.WriteLine("0x6C : " + Ports.inb((ushort)(BaseAddress + 0x6C)));
                if (Ports.inb((ushort)(BaseAddress + 0x6C)) == 0x02)
                {
                    Console.WriteLine("Link is up with ");
                    if (Ports.inb((ushort)(BaseAddress + 0x6C)) == 0x04) Console.WriteLine("10 Mbps and ");
                    if (Ports.inb((ushort)(BaseAddress + 0x6C)) == 0x08) Console.WriteLine("100 Mbps and ");
                    if (Ports.inb((ushort)(BaseAddress + 0x6C)) == 0x10) Console.WriteLine("1000 Mbps and ");
                    if (Ports.inb((ushort)(BaseAddress + 0x6C)) == 0x01) Console.WriteLine("Full-duplex\n");
                    else Console.WriteLine("Half-duplex\n");
                }
                else
                {
                    Console.WriteLine("Link is down\n");
                }
            }
            else if (status == 0x0040)
            {
                Console.WriteLine("Receive FIFO overflow!");
                Reset();
            }

            Ports.outw((ushort)(BaseAddress + 0x3E), Ports.inw((ushort)(BaseAddress + 0x3E)));

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