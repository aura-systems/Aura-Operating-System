using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Aura_OS.System.Sound
{
    class PCSpeaker
    {
        static IOPort io = new IOPort(0);
        static int PP = 0, D = 0;
        static void Outb(ushort port, byte data)
        {
            if (io.Port != port)
                io = new IOPort(port);
            io.Byte = data;
            PP = port;
            D = data;

        }
        static byte Inb(ushort port)
        {
            if (io.Port != port)
                io = new IOPort(port);
            return io.Byte;

        }

        //Play sound using built in speaker
        static void play_sound(UInt32 nFrequence)
        {
            UInt32 Div;
            uint tmp;

            //Set the PIT to the desired frequency
            Div = 1193180 / nFrequence;
            Outb(0x43, 0xb6);
            Outb(0x42, (byte)(Div));
            Outb(0x42, (byte)(Div >> 8));

            //And play the sound using the PC speaker
            tmp = Inb(0x61);
            if (tmp != (tmp | 3))
            {
                Outb(0x61, (byte)(tmp | 3));
            }
        }

        static void nosound()
        {
            int tmp = Inb(0x61) & 0xFC;
            Outb(0x61, (byte)tmp);
        }

        public static void beep()
        {
            play_sound(1000);
            Thread.Sleep(10);
            nosound();
        }
    }
}
