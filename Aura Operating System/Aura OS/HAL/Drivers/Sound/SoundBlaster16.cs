/*
* PROJECT:          Aura Operating System Development
* CONTENT:          SoundBlaster16 Driver
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
* https://forum.osdev.org/viewtopic.php?f=1&t=32879
*/

using Aura_OS.Core;
using System;
using System.Text;

namespace Aura_OS.HAL.Drivers.Sound
{
    public class SoundBlaster16
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
            Ports.outb(DSP_RESET, 1);
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
            status.playing = 0;

            return true;

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
            info.data_size = (buf[7] << 24) | (buf[6] << 16) | (buf[5] << 8) | buf[4];
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
            if (status.playing == 1 || file == null)
            {
                return -1;
            }
            chunk_info info;
            int bytes_read;
            status.fd = file;
            info = wav_read_chunk_header();
            Console.WriteLine("chunk_id= 0x" + System.Utils.Conversion.DecToHex((int)info.chunk_id));
            Console.WriteLine("data_size= 0x" + info.data_size);
            return 0;
        }

        public struct status
        {
            public static byte[] fd;
            public static byte channel;
            public static byte mode;
            public static byte playing;
        }

        public struct chunk_info
        {
            public long chunk_id;
            public long data_size;
        }

    }
}
