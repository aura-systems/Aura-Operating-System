/*
* PROJECT:          Aura Operating System Development
* CONTENT:          SoundBlaster16 Driver
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
* https://forum.osdev.org/viewtopic.php?f=1&t=32879
* http://soundfile.sapp.org/doc/WaveFormat/
*/

using Aura_OS.Core;
using System;
using System.Text;

namespace Aura_OS.HAL.Drivers.Sound
{
    public unsafe class SoundBlaster16
    {
        static ushort DSP_RESET = 0x226;
        static ushort DSP_READ = 0x22A;
        static ushort DSP_WRITE = 0x22C;
        static ushort DSP_BUFFER = 0x22E;
        static ushort DSP_INTERRUPT = 0x22F;

        static bool sound_blaster = false;
        static int sb16_version_major;
        static int sb16_version_minor;

        public static void Wait()
        {
            int a, b;
            for (int i = 0; i < 1000; i++)
            {
                a = i;
                b = i * 2;
                a = b * i;
            }
        }
        public static void reset_DSP()
        {
            Console.WriteLine("01");
            Ports.outb(DSP_RESET, 1);
            Console.WriteLine("02");
            Wait();
            Ports.outb(DSP_RESET, 0);
            Wait();

            if (Ports.inb(DSP_READ) == 0xAA)
            {
                sound_blaster = true;
            }
        }

        public static void write_DSP(byte value)
        {
            uint status = 0;
            status = Ports.inb(DSP_WRITE);

            Ports.outb(DSP_WRITE, value);
        }

        public static int read_DSP()
        {
            return Ports.inb(DSP_READ);
        }

        static byte INTERRUPT_SETUP = 0x80;

        public static bool init_SB16()
        {

            reset_DSP();

            if (sound_blaster == false)
            {
                return false;
            }

            write_DSP(0xE1);
            sb16_version_major = read_DSP();
            sb16_version_minor = read_DSP();

            status.fd = null;
            status.channel = 1;
            status.mode = 0;
            status.playing = false;

            Console.WriteLine("Searching IRQ...");

            int irq = 5;
            int iss;

            switch (irq)
            {
                case 2:
                    iss = 0x01;
                    break;
                case 5:
                    iss = 0x02;
                    break;
                case 7:
                    iss = 0x04;
                    break;
                case 10:
                    iss = 0x08;
                    break;
                default:
                    iss = readMixer(INTERRUPT_SETUP);
                    break;
            }

            iss = writeMixer(INTERRUPT_SETUP, (byte)(iss &0x0f)) & 0x0f;
            switch (iss)
            {
                case 0x01:
                    irq = 2;
                    break;
                case 0x02:
                    irq = 5;
                    break;
                case 0x04:
                    irq = 7;
                    break;
                case 0x08:
                    irq = 10;
                    break;
                default:
                    return false;
            }
            Console.WriteLine("interruptline=" + irq);
            Cosmos.Core.INTs.SetIrqHandler((byte)irq, sb16_handler);

            return true;

        }

        static byte writeMixer(byte addr, byte val)
        {
            Ports.outb(0x224, addr);
            Ports.outb(0x225, val);
            return Ports.inb(0x225);
        }

        static byte readMixer(byte addr)
        {
            Ports.outb(0x224, addr);
            return Ports.inb(0x225);
        }

        static int RIFF = 0x52494646;
        static int WAVE = 0x57415645;

        static chunk_info wav_read_chunk_header()
        {
            chunk_info info;
            info.chunk_id = 0;
            info.data_size = 0;
            byte[] buf = new byte[13];
            for (int i = 0; i <= 11; i++)
            {
                buf[i] = status.fd[i];
            }
            // read in the chunk header
            //if (status.fd.Length < 8)
            //{
            //    return info;
            //}
            info.chunk_id = (buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3];
            info.data_size = ((buf[7] << 24) | (buf[6] << 16) | (buf[5] << 8) | buf[4]) - 36;
            if (info.chunk_id == RIFF)
            {
                if (((buf[8] << 24) | (buf[9] << 16) | (buf[10] << 8) | buf[11]) != WAVE)
                {
                    // RIFF chunk is invalid
                    info.chunk_id = 0;
                    info.data_size = 0;
                    return info;
                }
                else
                {
                    return info;
                }
            }
            return info;
        }

        public static int playwav(byte[] file)
        {
            if (status.playing || file == null)
            {
                return -1;
            }
            chunk_info info;
            int bytes_read;
            status.fd = file;
            info = wav_read_chunk_header();
            Console.WriteLine("chunk_id= 0x" + System.Utils.Conversion.DecToHex((int)info.chunk_id));
            Console.WriteLine("data_size= " + info.data_size);
            if (info.chunk_id != RIFF)
            {
                return -1;
            }
            int bits;
            bool is_signed;
            int sample_rate;

            bytes_read = status.fd.Length;
            if (bytes_read < info.data_size)
            {
                Console.WriteLine("bytes_read < info.data_size");
                return -1;
            }
            if (status.fd[20] != 1)
            {
                Console.WriteLine("not uncompressed PCM format");
                return -1;
            }
            if (status.fd[22] != 1)
            {
                Console.WriteLine("not mono sound");
                return -1;
            }
            bits = status.fd[34];
            Console.WriteLine("bits=" + bits);
            if (bits == 16)
            {
                is_signed = true;
            }
            else
            {
                is_signed = false;
            }
            sample_rate = (status.fd[25] << 8) | (status.fd[24]);

            Console.WriteLine("sample_rate=" + sample_rate);

            dma_buffer = new ManagedMemoryBlock((uint)info.data_size);

            return play_file(status.fd, bits, is_signed, sample_rate);
        }

        static byte STATUS_8BIT = (1 << 1);
        static byte STATUS_SIGNED = (1 << 2);

        static ManagedMemoryBlock dma_buffer;

        static int play_file(byte[] fd, int bits, bool is_signed, int sample_rate)
        {
            if (status.playing)
            {
                return -1;
            }
            if (bits == 8)
            {
                status.mode |= STATUS_8BIT;
                status.channel = 1;
            }
            else if (bits != 16)
            {
                Console.WriteLine("can only play 8 or 16bit audio");
                return -1;
            }
            else
            {
                status.channel = 5;
            }

            status.fd = fd;
            status.playing = true;
            if (is_signed)
            {
                status.mode |= STATUS_SIGNED;
            }

            // set sampling rate
            sb16_set_sample_rate(sample_rate);

            Console.WriteLine("sampling rate set!");

            uint bytes_read = (uint)status.fd.Length;
            for (int i = 0; i < dma_buffer.Size; i++)
            {
                dma_buffer[(uint)i] = status.fd[44 + i];
            }

            dma_start(status.channel, dma_buffer.Offset, dma_buffer.Size, (byte)DMA.DMA_MODE_AI);

            Console.WriteLine("dma_start done.");

            sb16_start_playback(dma_buffer.Size);
            if (bytes_read < dma_buffer.Size)
            {
                Console.WriteLine("bytes_read < dma_buffer.Size");
                sb16_stop_playback_after();
            }

            return 0;
        }

        static void sb16_stop_playback_after()
        {
            if ((status.mode & STATUS_8BIT) != 0)
            {
                sb16_dsp_write((byte)DSPCMD.DSP_STOP_AFTER_8BIT);
            }
            else
            {
                sb16_dsp_write((byte)DSPCMD.DSP_STOP_AFTER_16BIT);
            }
        }

        // DSP commands
        enum DSPCMD{
            DSP_STOP_8BIT = 0xD0,
            DSP_STOP_16BIT = 0xD5,
            DSP_STOP_AFTER_8BIT = 0xDA,
            DSP_STOP_AFTER_16BIT = 0xD9,
            DSP_PLAY_8BIT = 0xC0,
            DSP_PLAY_16BIT = 0xB0,
            DSP_PAUSE_8BIT = 0xD0,
            DSP_RESUME_8BIT = 0xD4,
            DSP_PAUSE_16BIT = 0xD5,
            DSP_RESUME_16BIT = 0xD6,

            // DSP mode flags
            DSP_PLAY_AI = 0x06,
            DSP_PLAY_UNSIGNED = 0x00,
            DSP_PLAY_SIGNED = 0x10,
            DSP_PLAY_MONO = 0x00,
            DSP_PLAY_STEREO = 0x20,
        };

        static byte STATUS_PLAYING = (1 << 0);
        static byte STATUS_MONO = (1 << 3);

        static void sb16_start_playback(uint size)
        {
            ushort block_size = (ushort)size;
            // write the I/O command, transfer mode and block size to DSP
            byte command = 0;
            if ((status.mode & STATUS_8BIT) != 0)
            {
                command |= (byte)DSPCMD.DSP_PLAY_8BIT;
            }
            else
            {
                command |= (byte)DSPCMD.DSP_PLAY_16BIT;
            }
            command |= (byte)DSPCMD.DSP_PLAY_AI;
            sb16_dsp_write(command);
            byte mode = 0;
            if ((status.mode & STATUS_MONO) != 0)
            {
                mode |= (byte)DSPCMD.DSP_PLAY_MONO;
            }
            if ((status.mode & STATUS_SIGNED) != 0)
            {
                mode |= (byte)DSPCMD.DSP_PLAY_SIGNED;
            }
            sb16_dsp_write(mode);
            block_size--;
            sb16_dsp_write((byte)LO_BYTE(block_size));
            sb16_dsp_write((byte)HI_BYTE(block_size));
        }

        enum DMA {
            // playback
            DMA_MODE_SC = 0x48,
            DMA_MODE_AI = 0x48 | 0x10,
            // recording
            DMA_MODE_RECORD_SC = 0x44,
            DMA_MODE_RECORD_AI = 0x44 | 0x10,
        };

        static void dma_start(byte channel, uint addr, uint size, byte mode)
        {
            if (channel >= 8)
            {
                return;
            }
            // Disable the sound card DMA channel by setting the appropriate mask bit 
            disable_dma(channel);
            // Clear the byte pointer flip-flop 
            clear_dma_ff(channel);
            // Write the DMA mode for the transfer (48h is single-cycle playback)
            set_dma_mode(channel, mode);
            // Write the offset of the buffer, low byte followed by high byte.
            set_dma_addr(channel, addr);
            // Write the transfer length, low byte followed by high byte.
            set_dma_count(channel, size);
            // Enable the sound card DMA channel by clearing the appropriate mask bit
            enable_dma(channel);
        }

        static void enable_dma(uint dmanr)
        {
            if (dmanr <= 3)
                dma_outb((byte)dmanr, (ushort)DMAPorts.DMA1Mask);
            else
                dma_outb((byte)(dmanr & 3), (ushort)DMAPorts.DMA2Mask);
        }

        static void set_dma_count(uint dmanr, uint count)
        {
            count--;
            if (dmanr <= 3)
            {
                dma_outb((byte)(LO_BYTE(count)), (ushort)(((dmanr & 3) << 1) + 1 + (ushort)DMAPorts.DMA1Base));
                dma_outb((byte)(HI_BYTE(count)),
                        (ushort)(((dmanr & 3) << 1) + 1 + (ushort)DMAPorts.DMA1Base));
            }
            else
            {
                dma_outb((byte)(LO_BYTE(count / 2)),
                        (ushort)(((dmanr & 3) << 2) + 2 + (ushort)DMAPorts.DMA2Base));
                dma_outb((byte)(HI_BYTE(count / 2)),
                        (ushort)(((dmanr & 3) << 2) + 2 + (ushort)DMAPorts.DMA2Base));
            }
        }

        // page ports (index with channel)
        static ushort[] page_port = 
        {
           0x87, 0x83, 0x81, 0x82, // channel 0-3
           0x8F, // channel 4 is apparently unusable
           0x8B, 0x89, 0x8A // channel 5-7
        };

        static void set_dma_page(uint dmanr, byte pagenr)
        {
            if (dmanr <= 3)
                dma_outb(pagenr, page_port[dmanr]);
            else
                dma_outb((byte)(pagenr & 0xFE), page_port[dmanr]);
        }

        static void set_dma_addr(uint dmanr, uint addr)
        {
            set_dma_page(dmanr, (byte)(addr >> 16));
            if (dmanr <= 3)
            {
                dma_outb((byte)LO_BYTE(addr), (ushort)(((dmanr & 3) << 1) + (ushort)DMAPorts.DMA1Base));
                dma_outb((byte)HI_BYTE(addr), (ushort)(((dmanr & 3) << 1) + (ushort)DMAPorts.DMA1Base));
            }
            else
            {
                dma_outb((byte)LO_BYTE(addr / 2), (ushort)(((dmanr & 3) << 2) + (ushort)DMAPorts.DMA2Base));
                dma_outb((byte)HI_BYTE(addr / 2), (ushort)(((dmanr & 3) << 2) + (ushort)DMAPorts.DMA2Base));
            }
        }

        static void set_dma_mode(uint dmanr, byte mode)
        {
            if (dmanr <= 3)
                dma_outb((byte)(mode | dmanr), (ushort)DMAPorts.DMA1Mode);
            else
                dma_outb((byte)(mode | (dmanr & 3)), (ushort)DMAPorts.DMA2Mode);
        }

        static void clear_dma_ff(uint dmanr)
        {
            if (dmanr <= 3)
                dma_outb(0, (ushort)DMAPorts.DMA1ClrBytePtr);
            else
                dma_outb(0, (ushort)DMAPorts.DMA2ClrBytePtr);
        }

        static void disable_dma(uint dmanr)
        {
            if (dmanr <= 3)
                dma_outb((byte)(dmanr | 4), (ushort)DMAPorts.DMA1Mask);
            else
                dma_outb((byte)((dmanr & 3) | 4), (ushort)DMAPorts.DMA2Mask);
        }

        static void dma_outb(byte data, ushort port)
        {
            Ports.outb(port, data);
        }

        static int SB_SET_RATE = 0x41;
        static int LO_BYTE(uint data) { return ((int)data) & 0xFF; }
        static int HI_BYTE(uint data) { return (((int)data) >> 8) & 0xFF; }

        static void sb16_set_sample_rate(int hz)
        {
            sb16_dsp_write((byte)SB_SET_RATE);
            sb16_dsp_write((byte)HI_BYTE((uint)hz));
            sb16_dsp_write((byte)LO_BYTE((uint)hz));
        }

        // DMA ports (1 is for 8-bit operations and 2 is for 16-bit)
        enum DMAPorts {
            DMA1Mask = 0xA,
            DMA1Mode = 0xB,
            DMA1ClrBytePtr = 0xC,
            DMA2Mask = 0xD4,
            DMA2Mode = 0xD6,
            DMA2ClrBytePtr = 0xD8,
            DMA1Base = 0x00,
            DMA2Base = 0xC0,
        };

        enum DSP {
            DSPReset = 0x6,
            DSPRead = 0xA,
            DSPWrite = 0xC,
            DSPWriteStatus = 0xC,
            DSPReadStatus = 0xE,
            DSPIntAck = 0xF,
        };

        static byte READ_STATUS = (1 << 7);
        static byte WRITE_STATUS = (1 << 7);

        static void sb16_dsp_write(byte bytee)
        {
            // Read the write-buffer status port (2xC) until bit 7 is cleared
            while ((sb16_inb((byte)DSP.DSPWriteStatus) & WRITE_STATUS) != 0) ;
            // Write the value to the write port (2xC)
            sb16_outb(bytee, (ushort)DSP.DSPWrite);
        }

        static byte sb16_inb(byte offset)
        {
            return Ports.inb((ushort)(0x220 + offset));
        }

        static void sb16_outb(byte data, ushort offset)
        {
            Ports.outb((ushort)(0x220 + offset), data);
        }


        public struct status
        {
            public static byte[] fd;
            public static byte channel;
            public static byte mode;
            public static bool playing;
        }

        public struct chunk_info
        {
            public long chunk_id;
            public long data_size;
        }

        static ManagedMemoryBlock first_block;
        //static ManagedMemoryBlock second_block = new ManagedMemoryBlock(dma_buffer.Size + dma_buffer.Size);
        static ManagedMemoryBlock current_block;


        static void reset_playback()
        {
            first_block = dma_buffer;
            current_block = dma_buffer;

            status.playing = false;
            current_block = first_block;
            int i;
            for (i = 0; i < dma_buffer.Size; i++)
            {
                dma_buffer[(uint)i] = 0;
            }
            status.fd = null;
        }

        static void sb16_stop_playback()
        {
            if ((status.mode & STATUS_8BIT) != 0)
            {
                sb16_dsp_write((byte)DSPCMD.DSP_STOP_8BIT);
            }
            else
            {
                sb16_dsp_write((byte)DSPCMD.DSP_STOP_16BIT);
            }
            reset_playback();
        }

        static void sb16_handler(ref Cosmos.Core.INTs.IRQContext context)
        {

            Console.WriteLine("sb16_handler");

            //save all
            //registers_t regs;
            //save_regs(regs);

            if (status.playing)
            {
                //uint flags;
                //block_interrupts(&flags);
                uint bytes_read = dma_buffer.Size;
                for (int i = 0; i < dma_buffer.Size; i++)
                {
                    current_block[(uint)i] = dma_buffer[(uint)i];
                }
                //restore_interrupts(flags);
                if (bytes_read > 0)
                {
                    ushort block_size = (ushort)bytes_read;
                    // there are no more chunks after this one; stop playback after the
                    // current block
                    if (bytes_read < dma_buffer.Size)
                    {
                        sb16_stop_playback_after();
                        reset_playback();
                    }
                    if (current_block == first_block)
                    {
                        //current_block = second_block;
                    }
                    else
                    {
                        current_block = first_block;
                    }

                    sb16_start_playback(block_size);
                }
                else
                {
                    // file has no more bytes left; we're done playing
                    reset_playback();
                    sb16_stop_playback();
                }
            }

            sb16_inb((byte)DSP.DSPReadStatus);
            sb16_inb((byte)DSP.DSPIntAck);


            }

    }
}
