/*
* PROJECT:          Aura Operating System Development
* CONTENT:          RTL8139 Driver
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
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
    public class RTL8139 : NetworkDevice
    {
        protected PCIDevice pciCard;
        protected MACAddress mac;
        protected bool mInitDone;

        protected ManagedMemoryBlock rxBuffer;
        protected int rxBufferOffset;
        protected ushort capr;

        protected Queue<byte[]> mRecvBuffer;
        protected Queue<byte[]> mTransmitBuffer;
        private int mNextTXDesc;

        const ushort RxBufferSize = 32768;
        uint BaseAddress;

        public override CardType CardType => CardType.Ethernet;

        public override string Name => "RTL8139";

        public RTL8139(PCIDevice device)
        {
            if (device == null) {
                throw new ArgumentException("PCI Device is null. Unable to get Realtek 8139 card");
            }
            pciCard = device;

            // We are handling this device
            pciCard.Claimed = true;

            BaseAddress = this.pciCard.BaseAddressBar[0].BaseAddress;

            SetIrqHandler(device.InterruptLine, HandleNetworkInterrupt);

            // Enable the card
            pciCard.EnableDevice();

            // Turn on the card
            Ports.outb((ushort)(BaseAddress + 0x52), 0x01);

            //Do a software reset
            SoftwareReset();

            // Get the MAC Address
            byte[] eeprom_mac = new byte[6];
            for (uint b = 0; b < 6; b++)
            {
                eeprom_mac[b] = Ports.inb((ushort)(BaseAddress + b));
            }

            this.mac = new MACAddress(eeprom_mac);

            // Get a receive buffer and assign it to the card
            rxBuffer = new ManagedMemoryBlock(RxBufferSize + 2048 + 16, 4);

            RBStartRegister = rxBuffer.Offset;

            // Setup receive Configuration
            RecvConfigRegister = 0xF381;
            // Setup Transmit Configuration
            TransmitConfigRegister = 0x3000300;

            // Setup Interrupts
            IntMaskRegister = 0x7F;
            IntStatusRegister = 0xFFFF;

            // Setup our Receive and Transmit Queues
            mRecvBuffer = new Queue<byte[]>();
            mTransmitBuffer = new Queue<byte[]>();
        }

        protected void HandleNetworkInterrupt(ref IRQContext aContext)
        {
            ushort cur_status = IntStatusRegister;

            Console.WriteLine("RTL8139 IRQ raised!");
            if ((cur_status & 0x01) != 0)
            {
                while ((CommandRegister & 0x01) == 0)
                {
                    //uint packetHeader = BitConverter.Touint(rxBuffer, rxBufferOffset + capr);
                    uint packetHeader = rxBuffer.Read32(capr);
                    ushort packetLen = (ushort)(packetHeader >> 16);
                    if ((packetHeader & 0x3E) != 0x00)
                    {
                        CommandRegister = 0x04; // TX Only;
                        capr = CurBufferAddressRegister;
                        CommandRegister = 0x0C; // RX and TX Enabled
                    }
                    else if ((packetHeader & 0x01) == 0x01)
                    {
                        ReadRawData(packetLen);
                    }

                    CurAddressPointerReadRegister = (ushort)(capr - 0x10);
                }
            }
            if ((cur_status & 0x10) != 0)
            {
                CurAddressPointerReadRegister = (ushort)(CurBufferAddressRegister - 0x10);
                cur_status = (ushort)(cur_status | 0x01);
            }

            IntStatusRegister = cur_status;
        }

        #region Register Access
        protected uint RBStartRegister
        {
            get { return Ports.ind((ushort)(BaseAddress + 0x30)); }
            set { Ports.outd((ushort)(BaseAddress + 0x30), (byte)value); }
        }
        internal uint RecvConfigRegister
        {
            get { return Ports.ind((ushort)(BaseAddress + 0x44)); }
            set { Ports.outd((ushort)(BaseAddress + 0x44), (byte)value); }
        }
        internal ushort CurAddressPointerReadRegister
        {
            get { return Ports.inw((ushort)(BaseAddress + 0x38)); }
            set { Ports.outw((ushort)(BaseAddress + 0x38), value); }
        }
        internal ushort CurBufferAddressRegister
        {
            get { return Ports.inw((ushort)(BaseAddress + 0x3A)); }
            set { Ports.outw((ushort)(BaseAddress + 0x3A), value); }
        }

        internal ushort IntMaskRegister
        {
            get { return Ports.inw((ushort)(BaseAddress + 0x3C)); }
            set { Ports.outw((ushort)(BaseAddress + 0x3C), value); }
        }
        internal ushort IntStatusRegister
        {
            get { return Ports.inw((ushort)(BaseAddress + 0x3E)); }
            set { Ports.outw((ushort)(BaseAddress + 0x3E), value); }
        }

        internal byte CommandRegister
        {
            get { return Ports.inb((ushort)(BaseAddress + 0x37)); }
            set { Ports.outb((ushort)(BaseAddress + 0x37), value); }
        }
        protected byte MediaStatusRegister
        {
            get { return Ports.inb((ushort)(BaseAddress + 0x58)); }
            set { Ports.outb((ushort)(BaseAddress + 0x58), value); }
        }

        protected byte Config1Register
        {
            get { return Ports.inb((ushort)(BaseAddress + 0x52)); }
            set { Ports.outb((ushort)(BaseAddress + 0x52), value); }
        }

        internal uint TransmitConfigRegister
        {
            get { return Ports.ind((ushort)(BaseAddress + 0x40)); }
            set { Ports.outd((ushort)(BaseAddress + 0x40), (byte)value); }
        }

        internal uint TransmitAddress1Register
        {
            get { return Ports.ind((ushort)(BaseAddress + 0x20)); }
            set { Ports.outd((ushort)(BaseAddress + 0x20), (byte)value); }
        }
        internal uint TransmitAddress2Register
        {
            get { return Ports.ind((ushort)(BaseAddress + 0x24)); }
            set { Ports.outd((ushort)(BaseAddress + 0x24), (byte)value); }
        }
        internal uint TransmitAddress3Register
        {
            get { return Ports.ind((ushort)(BaseAddress + 0x28)); }
            set { Ports.outd((ushort)(BaseAddress + 0x28), (byte)value); }
        }
        internal uint TransmitAddress4Register
        {
            get { return Ports.ind((ushort)(BaseAddress + 0x2C)); }
            set { Ports.outd((ushort)(BaseAddress + 0x2C), (byte)value); }
        }
        internal uint TransmitDescriptor1Register
        {
            get { return Ports.ind((ushort)(BaseAddress + 0x10)); }
            set { Ports.outd((ushort)(BaseAddress + 0x10), (byte)value); }
        }
        internal uint TransmitDescriptor2Register
        {
            get { return Ports.ind((ushort)(BaseAddress + 0x14)); }
            set { Ports.outd((ushort)(BaseAddress + 0x14), (byte)value); }
        }
        internal uint TransmitDescriptor3Register
        {
            get { return Ports.ind((ushort)(BaseAddress + 0x18)); }
            set { Ports.outd((ushort)(BaseAddress + 0x18), (byte)value); }
        }
        internal uint TransmitDescriptor4Register
        {
            get { return Ports.ind((ushort)(BaseAddress + 0x1C)); }
            set { Ports.outd((ushort)(BaseAddress + 0x1C), (byte)value); }
        }
        #endregion

        protected bool CmdBufferEmpty
        {
            get { return ((CommandRegister & 0x01) == 0x01); }
        }

        #region Network Device Implementation
        public override MACAddress MACAddress
        {
            get { return this.mac; }
        }

        public override bool Enable()
        {
            // Enable Receiving and Transmitting of data
            CommandRegister = 0x0C;

            while (this.Ready == false)
            { }

            return true;
        }

        public override bool Ready
        {
            get { return ((Config1Register & 0x20) == 0); }
        }

        public override bool QueueBytes(byte[] buffer, int offset, int length)
        {
            byte[] data = new byte[length];
            for (int b = 0; b < length; b++)
            {
                data[b] = buffer[b + offset];
            }

            Console.WriteLine("Try sending");

            if (SendBytes(ref data) == false)
            {
                Console.WriteLine("Queuing");
                mTransmitBuffer.Enqueue(data);
            }

            return true;
        }

        public override bool ReceiveBytes(byte[] buffer, int offset, int max)
        {
            throw new NotImplementedException();
        }

        public override byte[] ReceivePacket()
        {
            if (mRecvBuffer.Count < 1)
            {
                return null;
            }

            byte[] data = mRecvBuffer.Dequeue();
            return data;
        }

        public override int BytesAvailable()
        {
            if (mRecvBuffer.Count < 1)
            {
                return 0;
            }

            return mRecvBuffer.Peek().Length;
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

        #region Helper Functions
        private void ReadRawData(ushort packetLen)
        {
            int recv_size = packetLen - 4;
            byte[] recv_data = new byte[recv_size];
            for (uint b = 0; b < recv_size; b++)
            {
                recv_data[b] = rxBuffer[(uint)(capr + 4 + b)];
            }
            if (DataReceived != null)
            {
                DataReceived(recv_data);
            }
            else
            {
                if (mRecvBuffer == null)
                {
                }

                mRecvBuffer.Enqueue(recv_data);
            }

            capr += (ushort)((packetLen + 4 + 3) & 0xFFFFFFFC);
            if (capr > RxBufferSize)
            {
                capr -= RxBufferSize;
            }
        }

        protected void SoftwareReset()
        {
            CommandRegister = 0x10;
            while ((CommandRegister & 0x10) != 0)
            { }
        }

        protected bool SendBytes(ref byte[] aData)
        {
            int txd = mNextTXDesc++;
            if (mNextTXDesc >= 4)
            {
                mNextTXDesc = 0;
            }

            ManagedMemoryBlock txBuffer;
            if (aData.Length < 64)
            {
                txBuffer = new ManagedMemoryBlock(64);
                for (uint b = 0; b < aData.Length; b++)
                {
                    txBuffer[b] = aData[b];
                }
            }
            else
            {
                txBuffer = new ManagedMemoryBlock((uint)aData.Length);
                for (uint b = 0; b < aData.Length; b++)
                {
                    txBuffer[b] = aData[b];
                }
            }

            switch (txd)
            {
                case 0:
                    TransmitAddress1Register = txBuffer.Offset;
                    TransmitDescriptor1Register = txBuffer.Size;
                    break;
                case 1:
                    TransmitAddress2Register = txBuffer.Offset;
                    TransmitDescriptor2Register = txBuffer.Size;
                    break;
                case 2:
                    TransmitAddress3Register = txBuffer.Offset;
                    TransmitDescriptor3Register = txBuffer.Size;
                    break;
                case 3:
                    TransmitAddress4Register = txBuffer.Offset;
                    TransmitDescriptor4Register = txBuffer.Size;
                    break;
                default:
                    return false;
            }

            return true;
        }

        #endregion
    }
}