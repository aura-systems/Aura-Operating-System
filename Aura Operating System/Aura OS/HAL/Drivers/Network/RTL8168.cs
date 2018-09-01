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
            public uint Command; // bit 31 is OWN, bit 30 is EOR
            public uint vlan;     /* currently unused */
            public uint low_buf;  /* low 32-bits of physical buffer address */
            public uint high_buf; /* high 32-bits of physical buffer address */
        };

        struct RTL8168_networkAdapter_t
        {
            public Descriptor* Rx_Descriptors;
            public Descriptor* Tx_Descriptors;
            public byte* RxBuffers;
            public byte* TxBuffers;
            public ushort TxIndex;
        }

        RTL8168_networkAdapter_t* rAdapter;

        uint GetMacVersion()
        {

            uint reg;

            reg = Ports.ind((ushort)(BaseAddress + 0x40));

            return reg;
        }

        void InitBuffers()
        {

            rAdapter = (RTL8168_networkAdapter_t*)Cosmos.Core.Memory.Old.Heap.MemAlloc((uint)sizeof(RTL8168_networkAdapter_t));

            for (ushort i = 0; i < 32; i++)
            {
                if (i == (32 - 1)) // Last descriptor? if so, set the EOR bit
                {
                    rAdapter->Rx_Descriptors[i].Command = 0x80000000 | 0x40000000 | (2048 & 0x3FFF);
                    rAdapter->Tx_Descriptors[i].Command = 0x40000000;
                }
                else
                {
                    rAdapter->Rx_Descriptors[i].Command = 0x80000000 | (2048 & 0x3FFF);
                    rAdapter->Tx_Descriptors[i].Command = 0;
                }
                rAdapter->Rx_Descriptors[i].vlan = 0;
                rAdapter->Tx_Descriptors[i].vlan = 0;
                rAdapter->Rx_Descriptors[i].low_buf = (uint)&rAdapter->RxBuffers + (uint)(i * 2048);
                Console.WriteLine("0x" + System.Utils.Conversion.DecToHex((int)&rAdapter->RxBuffers + i * 2048));
                rAdapter->Tx_Descriptors[i].low_buf = (uint)&rAdapter->TxBuffers + (uint)(i * 2048);
                Console.WriteLine("0x" + System.Utils.Conversion.DecToHex((int)&rAdapter->TxBuffers + i * 2048));
                rAdapter->Rx_Descriptors[i].high_buf = 0;
                rAdapter->Tx_Descriptors[i].high_buf = 0;
            }

            Console.WriteLine("Descriptors are set up.");
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

            Reset();

            FifoFix();

            Console.WriteLine("Reset done.");

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

            Console.WriteLine("addressrx desc: 0x" + System.Utils.Conversion.DecToHex((int)&rAdapter->Tx_Descriptors));
            Console.WriteLine("addresstx desc: 0x" + System.Utils.Conversion.DecToHex((int)&rAdapter->Rx_Descriptors));

            Ports.outd((ushort)(BaseAddress + 0x20), (uint)&rAdapter->Tx_Descriptors);
            Ports.outd((ushort)(BaseAddress + 0xE4), (uint)&rAdapter->Rx_Descriptors);

            Ports.outw((ushort)(BaseAddress + 0x3C), 0x03FF); //Activating all Interrupts

            Ports.outb((ushort)(BaseAddress + 0x37), 0x0C); // Enabling receive and transmit

            Console.WriteLine("Init done.");

            Console.WriteLine("Netcard version: 0x" + System.Utils.Conversion.DecToHex((int)GetMacVersion() & 0x7cf00000));
            Console.WriteLine("Netcard version: 0x" + System.Utils.Conversion.DecToHex((int)GetMacVersion() & 0x7c800000));

            //byte[] aData = new byte[]
            //{
            //    0x6C, 0x62, 0x6D, 0x93, 0xC1, 0xDA, 0xb8, 0x86, 0x87, 0x24, 0x34, 0xb7, 0x08, 0x00,
            //    0x45, 0x00, 0x00, 0x24, 0x55, 0x1b, 0x00, 0x00, 0x80, 0x11, 0x62, 0x0b, 0xc0, 0xa8, 0x01, 0x46, 0xc0, 0xa8, 0x01, 0x0c,
            //    0x10, 0x92, 0x10, 0x92, 0x00, 0x10, 0x15, 0xf3,
            //    0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x21, 0x21, 0x21
            //
            //};

            //realtek_send_packet(PointerData(aData, aData.Length), aData.Length);

            //Console.WriteLine("Send done.");

        }

        public bool FifoFix()
        {
            Ports.outd((ushort)(BaseAddress + 0xf0), 0x10); /* Send the Reset bit to the Command register */
            Ports.outd((ushort)(BaseAddress + 0xf0), 0x10);
            return true;
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
                Console.WriteLine("0x6C : 0x" + System.Utils.Conversion.DecToHex(Ports.inb((ushort)(BaseAddress + 0x6C))));
                if (Ports.inb((ushort)(BaseAddress + 0x6C)) == 0x02)
                {
                    Console.WriteLine("Link is up with ");
                    if (Ports.inb((ushort)(BaseAddress + 0x6C)) == 0x04) Console.WriteLine("10 Mbps and ");
                    if (Ports.inb((ushort)(BaseAddress + 0x6C)) == 0x08) Console.WriteLine("100 Mbps and ");
                    if (Ports.inb((ushort)(BaseAddress + 0x6C)) == 0x10) Console.WriteLine("1000 Mbps and ");
                    if (Ports.inb((ushort)(BaseAddress + 0x6C)) == 0x01) Console.WriteLine("Full-duplex\n");
                    else Console.WriteLine("Half-duplex\n");
                }
                else if (Ports.inb((ushort)(BaseAddress + 0x6C)) == 0x8B)
                {
                    Console.WriteLine("Link is connected! But not 0x02");
                }
                else if (Ports.inb((ushort)(BaseAddress + 0x6C)) == 0x80)
                {
                    Console.WriteLine("Link is down!");
                }
                else
                {
                    Console.WriteLine("Link is down!? Idk");
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