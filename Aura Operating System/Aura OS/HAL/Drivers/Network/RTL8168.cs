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

        public override string Name => "RTL8168";

        struct Descriptor
        {
            public uint Command; // bit 31 is OWN, bit 30 is EOR
            public uint vlan;     /* currently unused */
            public uint low_buf;  /* low 32-bits of physical buffer address */
            public uint high_buf; /* high 32-bits of physical buffer address */
        };

        ManagedMemoryBlock RxBuffers;
        ManagedMemoryBlock TxBuffers;

        Descriptor[] Rx_Descriptors;
        Descriptor[] Tx_Descriptors;

        uint GetMacVersion()
        {
            return Ports.ind((ushort)(BaseAddress + 0x40));
        }

        void InitBuffers()
        {

            Rx_Descriptors = new Descriptor[32];
            Tx_Descriptors = new Descriptor[32];

            RxBuffers = new ManagedMemoryBlock(2048 * 32 * 2);
            TxBuffers = new ManagedMemoryBlock(RxBuffers.Size + 2048 * 32);

            uint OWN = 0x80000000, EOR = 0x40000000; /* Bit offsets */

            for (int i = 0; i < 32; i++)
            {
                if (i == (32 - 1)) /* Last descriptor? if so, set the EOR bit */
                {
                    Rx_Descriptors[i].Command = OWN | EOR | (2048 & 0x3FFF);
                    Tx_Descriptors[i].Command = EOR;
                }
                else
                {
                    Rx_Descriptors[i].Command = OWN | (2048 & 0x3FFF);
                    Tx_Descriptors[i].Command = 0;
                }

                Rx_Descriptors[i].vlan = 0;
                Tx_Descriptors[i].vlan = 0;

                Rx_Descriptors[i].low_buf = RxBuffers.Offset + (uint)(i * 2048);
                Tx_Descriptors[i].low_buf = TxBuffers.Offset + (uint)(i * 2048);
                Rx_Descriptors[i].high_buf = 0;
                Tx_Descriptors[i].high_buf = 0;
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

            Ports.outb((ushort)(BaseAddress + 0xE0), 0x08);

            Reset();

            // Get the MAC Address
            byte[] eeprom_mac = new byte[6];
            for (uint b = 0; b < 6; b++)
            {
                eeprom_mac[b] = Ports.inb((ushort)(BaseAddress + b));
            }

            this.mac = new MACAddress(eeprom_mac);

            InitBuffers();

            Ports.outd((ushort)(BaseAddress + 0x44), 0x0000E70F); // Enable RX

            Ports.outd((ushort)(BaseAddress + 0x37), 0x04);

            Ports.outd((ushort)(BaseAddress + 0x40), 0x03000700); // Enable TX

            Ports.outd((ushort)(BaseAddress + 0xDA), 2048); // Max rx packet size

            Ports.outb((ushort)(BaseAddress + 0xEC), 0x3F); // No early transmit

            fixed (Descriptor* pbArr = &Tx_Descriptors[0])
            {
                Ports.outd((ushort)(BaseAddress + 0x20), (uint)pbArr);
                Console.WriteLine("addresstx desc: 0x" + System.Utils.Conversion.DecToHex((int)pbArr));
            }

            fixed (Descriptor* pbArr = &Rx_Descriptors[0])
            {
                Ports.outd((ushort)(BaseAddress + 0xE4), (uint)pbArr);
                Console.WriteLine("addressrx desc: 0x" + System.Utils.Conversion.DecToHex((int)pbArr));
            }

            Ports.outw((ushort)(BaseAddress + 0x3C), 0x41BB); //Activating all Interrupts

            Ports.outb((ushort)(BaseAddress + 0x37), 0x0C); // Enabling receive and transmit

            Console.WriteLine("Init done.");

            Console.WriteLine("Netcard version: 0x" + System.Utils.Conversion.DecToHex((int)GetMacVersion() & 0x7cf00000));
            Console.WriteLine("Netcard version: 0x" + System.Utils.Conversion.DecToHex((int)GetMacVersion() & 0x7c800000));

            //ushort[] aData = new ushort[]
            //{
            //    0x6C, 0x62, 0x6D, 0x93, 0xC1, 0xDA, 0xb8, 0x86, 0x87, 0x24, 0x34, 0xb7, 0x08, 0x00,
            //    0x45, 0x00, 0x00, 0x24, 0x55, 0x1b, 0x00, 0x00, 0x80, 0x11, 0x62, 0x0b, 0xc0, 0xa8, 0x01, 0x46, 0xc0, 0xa8, 0x01, 0x0c,
            //    0x10, 0x92, 0x10, 0x92, 0x00, 0x10, 0x15, 0xf3,
            //    0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x21, 0x21, 0x21
            //
            //};

            //rtl8168_send(PointerData(aData, aData.Length), aData.Length);

            Console.WriteLine("Send done.");

        }

        public bool Reset()
        {
            Ports.outb((ushort)(BaseAddress + 0x37), 0x10); /* Send the Reset bit to the Command register */
            while ((Ports.inb((ushort)(BaseAddress + 0x37)) & 0x10) != 0) { } /* Wait for the chip to finish resetting */
            return true;
        }

        public ushort* PointerData(ushort[] data, int length)
        {
            ushort[] safe = new ushort[length];
            for (int i = 0; i < length; i++)
                safe[i] = data[i];

            fixed (ushort* converted = safe)
            {
                // This will update the safe and converted arrays.
                for (int i = 0; i < length; i++)
                    converted[i]++;

                return converted;
            }
        }

        protected void HandleNetworkInterrupt(ref IRQContext aContext)
        {

            ushort status = Ports.inw((ushort)(BaseAddress + 0x3E));

            Console.WriteLine("Status: 0x" + System.Utils.Conversion.DecToHex(status));

            if ((status & 0x0001) != 0)
            {
                Console.WriteLine("WOW PACKET RECEIVED!!!!!");
            }
            if ((status & 0x0002) != 0) Console.WriteLine("Receive error");
            if (((status & 0x0004) != 0) && ((status & 0x0080) != 0))
            {
                Console.WriteLine("Transmit succesfull - descriptor resetted");
            }
            else
            {
                if ((status & 0x0004) != 0) Console.WriteLine("Transmit succesfull - descriptor not resetted");
                if ((status & 0x0080) != 0) Console.WriteLine("Transmit descriptor unavailable");
            }
            if ((status & 0x0008) != 0) Console.WriteLine("Transmit error");
            if ((status & 0x0010) != 0)
            {
                Console.WriteLine("Receive descriptor unavailable");
            }
            if ((status & 0x0020) != 0)
            {
                Console.WriteLine("0x6C : 0x" + System.Utils.Conversion.DecToHex(Ports.inb((ushort)(BaseAddress + 0x6C))));
                if ((Ports.inb((ushort)(BaseAddress + 0x6C)) & 0x02) != 0)
                {
                    Console.WriteLine("Link is up with ");
                    if ((Ports.inb((ushort)(BaseAddress + 0x6C)) & 0x04) != 0) Console.WriteLine("10 Mbps and ");
                    if ((Ports.inb((ushort)(BaseAddress + 0x6C)) & 0x08) != 0) Console.WriteLine("100 Mbps and ");
                    if ((Ports.inb((ushort)(BaseAddress + 0x6C)) & 0x10) != 0) Console.WriteLine("1000 Mbps and ");
                    if ((Ports.inb((ushort)(BaseAddress + 0x6C)) & 0x01) != 0) Console.WriteLine("Full-duplex");
                    else Console.WriteLine("Half-duplex");
                }
                else
                {
                    Console.WriteLine("Link is down!");
                }
            }
            if ((status & 0x0040) != 0)
            {
                Console.WriteLine("RX FIFO overflow!");
                if ((status & 0x0200) != 0)
                {
                    Console.WriteLine("RX FIFO empty");
                }
                else
                {
                    Console.WriteLine("Set 0 to FOVW");
                }
            }
            if ((status & 0x0100) != 0)
            {
                Console.WriteLine("Software Interrupt");
            }
            if ((status & 0x0200) != 0)
            {
                Console.WriteLine("RX FIFO empty");
                Ports.outw((ushort)(BaseAddress + status), 0x0040); //https://groups.google.com/forum/#!topic/fa.linux.kernel/Vo8-9W3LoQs
            } 
            if ((status & 0x0400) != 0) Console.WriteLine("Unknown Status (reserved Bit 11)");
            if ((status & 0x0800) != 0) Console.WriteLine("Unknown Status (reserved Bit 12)");
            if ((status & 0x1000) != 0) Console.WriteLine("Unknown Status (reserved Bit 13)");
            if ((status & 0x2000) != 0) Console.WriteLine("Unknown Status (reserved Bit 14)");
            if ((status & 0x4000) != 0) Console.WriteLine("Timeout");
            if ((status & 0x8000) != 0) Console.WriteLine("Unknown Status (reserved Bit 16)");

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