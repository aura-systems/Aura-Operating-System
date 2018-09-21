/*
* PROJECT:          Aura Operating System Development
* CONTENT:          AC97 Driver
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/


using Aura_OS.Core;
using Cosmos.HAL;
using System;
using System.Collections.Generic;
using System.Text;
using static Cosmos.Core.INTs;

namespace Aura_OS.HAL.Drivers.Sound
{
    public unsafe class AC97
    {

        public int NAM_RESET = 0x0000;
        public int NAM_MASTER_VOLUME = 0x0002;
        public int NAM_MONO_VOLUME = 0x0006;
        public int NAM_PC_BEEP = 0x000A;
        public int NAM_PCM_VOLUME = 0x0018;
        public int NAM_EXT_AUDIO_ID = 0x0028;
        public int NAM_EXT_AUDIO_STC = 0x002A;
        public int NAM_FRONT_SPLRATE = 0x002C;
        public int NAM_LR_SPLRATE = 0x0032;
        public int NABM_POBDBAR = 0x0010;
        public int NABM_POCIV = 0x0014;
        public int NABM_POLVI = 0x0015;
        public int NABM_PISTATUS = 0x0006;
        public int NABM_POSTATUS = 0x0016;
        public int NABM_MCSTATUS = 0x0026;
        public int NABM_POPICB = 0x0018;
        public int NABM_PICONTROL = 0x000B;
        public int NABM_POCONTROL = 0x001B;
        public int NABM_MCCONTROL = 0x002B;
        public int NABM_GLB_STAT = 0x0030;

        public int FLAG_BUP = (1 << 14);
        public int FLAG_IOC = (1 << 15);

        // A structure describing an AC97 card
        struct ac97_device
        {
            // The two I/O spaces
            ushort nambar, nabmbar;

            // The buffer's virtual addresses
            void** vbd;

            // Pointer to the output stream
            cdi_audio_stream output;
        };


        protected PCIDevice pciCard;
        uint BaseAddress;

        public cdi_audio_device AC97card;
        public cdi_audio_stream AC97Stream;

        public uint SAMPLES_PER_BUFFER = 0xFFFE;

        private unsafe struct BDL_Entry
        {
            public void* pointer;
            public int cl;
        }

        //private static BDL_Entry[] m_bdls;
        private static List<ManagedMemoryBlock> m_bdls;
        private static List<ManagedMemoryBlock> m_bufs;

        public AC97(PCIDevice device)
        {
            pciCard = device;

            // We are handling this device
            pciCard.Claimed = true;

            BaseAddress = this.pciCard.BaseAddressBar[0].BaseAddress;

            // Enable the card
            pciCard.EnableDevice();

            // Enable all interrupts
            Ports.outb((ushort)(BaseAddress + 0x1B), (1 << 3) | (1 << 4) | (1 << 2));

            SetIrqHandler(device.InterruptLine, HandleInterrupt);
            AC97card.record = 0;
            AC97card.streams = new List<cdi_audio_stream>();

            AC97Stream.num_buffers = 32;
            AC97Stream.buffer_size = SAMPLES_PER_BUFFER;
            AC97Stream.sample_format = cdi_audio_sample_format_t.CDI_AUDIO_16SI;

            AC97Stream.device = device;

            // Reset the card
            Ports.outw((ushort)(BaseAddress + NAM_RESET), 42);

            Wait();

            // And still something...
            Ports.outw((ushort)(BaseAddress + NABM_PICONTROL), 0x02);
            Ports.outw((ushort)(BaseAddress + NABM_POCONTROL), 0x02);
            Ports.outw((ushort)(BaseAddress + NABM_MCCONTROL), 0x02);

            Wait();

            // Set volume to 100 %
            Ports.outw((ushort)(BaseAddress + NAM_MASTER_VOLUME), 0x0000);
            Ports.outw((ushort)(BaseAddress + NAM_MONO_VOLUME), 0x00);
            Ports.outw((ushort)(BaseAddress + NAM_PC_BEEP), 0x00);
            Ports.outw((ushort)(BaseAddress + NAM_PCM_VOLUME), 0x0000);

            Wait();

            // Test if sample rate is adjustable
            if ((Ports.inw((ushort)(BaseAddress + NAM_EXT_AUDIO_ID)) & 1) == 0)
            {
                // No it isn't; it's fixed to 48 kHz
                AC97Stream.fixed_sample_rate = 48000;
            }
            else
            {
                // Yes, it is
                AC97Stream.fixed_sample_rate = 0;

                // Use that capability (Variable Audio Rate)
                Ports.outw((ushort)(BaseAddress + NAM_EXT_AUDIO_STC),
                    Ports.inw((ushort)((BaseAddress + NAM_EXT_AUDIO_STC) | 1)));

                Wait();

                // Set the sample rate to 44.1 kHz
                Ports.outw((ushort)(BaseAddress + NAM_FRONT_SPLRATE), 44100);
                Ports.outw((ushort)(BaseAddress + NAM_LR_SPLRATE), 44100);
            }

            m_bdls = new List<ManagedMemoryBlock>();
            //m_bdls = new BDL_Entry[32];
            m_bufs = new List<ManagedMemoryBlock>();

            for (int i = 0; i < 32; i++)
            {
                //(uint)(32 * sizeof(BDL_Entry)))

                int Offset = i * 32 * sizeof(BDL_Entry);

                // Allocate each buffer
                m_bufs[i] = new ManagedMemoryBlock(4096); ;

                
                m_bdls[i].Write32((uint)Offset + 0, (byte)m_bufs[i].Offset);
                

                m_bdls[i].Write32((uint)Offset + 8, 4096 & 0xFFFF);

                //for (int h = 0; h < SAMPLES_PER_BUFFER * 4; h++)
                //{
                //    buffer[(uint)h] = 0;
                //}

            }

            
            Console.WriteLine("0x" + System.Utils.Conversion.DecToHex((byte)m_bdls[0].Offset));
            Ports.outb((ushort)(BaseAddress + NABM_POBDBAR), (byte)m_bdls[0].Offset);
            

            int m_lvi = 3;
            Ports.outb((ushort)(BaseAddress + 0x15), (byte)m_lvi);

            // Set audio to playing
            Ports.outb((ushort)(BaseAddress + 0x1B), (byte)(Ports.inb((ushort)(BaseAddress + 0x1B)) | (1 << 0)));

        }

        public void Wait()
        {
            int a = 4000;
            for (int i = 0; i < a; i++)
            {
                int v = i * 5;
            }
        }

        public static void FindAll()
        {
            Console.WriteLine("Scanning for AC97 cards...");
            PCIDevice INTELAC97 = PCI.GetDevice((VendorID)0x8086, (DeviceID)0x2415);
            if (INTELAC97 != null)
            {
                AC97 nic = new AC97(INTELAC97);
                Console.WriteLine("Found AC97 on PCI " + INTELAC97.bus + ":" + INTELAC97.slot + ":" + INTELAC97.function);
                Console.WriteLine("NIC IRQ: " + INTELAC97.InterruptLine);
                return;
            }
            PCIDevice device = PCI.GetDevice((VendorID)0x2425, (DeviceID)0x2445);
            if (device != null)
            {
                AC97 nic = new AC97(device);
                Console.WriteLine("Found AC97 on PCI " + device.bus + ":" + device.slot + ":" + device.function);
                Console.WriteLine("NIC IRQ: " + device.InterruptLine);
                return;
            }
            if (INTELAC97 == null && device == null)
            {
                Console.WriteLine("No AC97 card found!");
            }
        }

        protected void HandleInterrupt(ref IRQContext aContext)
        {
            Console.WriteLine("HandleInterrupt");
        }

        public struct cdi_audio_device
        {
            public int record;

            public List<cdi_audio_stream> streams;
        };

        public struct cdi_audio_stream
        {

            public PCIDevice device;

            public uint num_buffers;

            public uint buffer_size;

            public int fixed_sample_rate;

            public cdi_audio_sample_format_t sample_format;
        };

        public enum cdi_audio_sample_format_t
        {
            // 16 bit signed integer
            CDI_AUDIO_16SI = 0,
            // 8 bit signed integer
            CDI_AUDIO_8SI,
            // 32 bit signed integer
            CDI_AUDIO_32SI
        };

    }
}
