/*
* PROJECT:          Aura Operating System Development
* CONTENT:          PCSpeaker
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*                   https://wiki.osdev.org/Sound
*                   https://github.com/ApolloHub/AIC-Framework for notes.
*/

using Cosmos.Core;
using Cosmos.HAL;
using System;

namespace Aura_OS.System.Sound
{
    public class PCSpeaker
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

        PIT pit = new PIT();

        public void beep()
        {
            play_sound(Notes.A4);
            pit.Wait(10);
            nosound();
        }

        public struct Notes
        {
            public static uint A0 = 28; // Exactly 27.500
            public static uint AS0 = 29;
            public static uint B0 = 31;

            public static uint C1 = 33;
            public static uint CS1 = 35;
            public static uint D1 = 37;
            public static uint DS1 = 39;
            public static uint E1 = 41;
            public static uint F1 = 44;
            public static uint FS1 = 46;
            public static uint G1 = 49;
            public static uint GS1 = 52;
            public static uint A1 = 55; // Exactly 55.000hz
            public static uint AS1 = 58;
            public static uint B1 = 62;

            public static uint C2 = 65;
            public static uint CS2 = 69;
            public static uint D2 = 73;
            public static uint DS2 = 78;
            public static uint E2 = 82;
            public static uint F2 = 87;
            public static uint FS2 = 92;
            public static uint G2 = 98;
            public static uint GS2 = 104;
            public static uint A2 = 110; // Exactly 110.000hz
            public static uint AS2 = 117;
            public static uint B2 = 123;

            public static uint C3 = 131;
            public static uint CS3 = 139;
            public static uint D3 = 147;
            public static uint DS3 = 156;
            public static uint E3 = 165;
            public static uint F3 = 175;
            public static uint FS3 = 185;
            public static uint G3 = 196;
            public static uint GS3 = 208;
            public static uint A3 = 220; // Exactly 220.000hz
            public static uint AS3 = 233;
            public static uint B3 = 247;

            public static uint C4 = 262;
            public static uint CS4 = 277;
            public static uint D4 = 294;
            public static uint DS4 = 311;
            public static uint E4 = 330;
            public static uint F4 = 349;
            public static uint FS4 = 370;
            public static uint G4 = 392;
            public static uint GS4 = 415;
            public static uint A4 = 440; // Exactly 440.000hz | Concert Pitch
            public static uint AS4 = 466;
            public static uint B4 = 494;
        }
    }
}
