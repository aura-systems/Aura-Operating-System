using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Threading
{
    public static class PIT
    {
        private const byte PIT_A = 0x40;
        private const byte PIT_B = 0x41;
        private const byte PIT_C = 0x42;
        private const byte CONTROL = 0x43;

        private const byte MASK = 0xFF;
        private const uint SCALE = 1193180;
        private const byte SET = 0x36;

        public static uint IPS = 100;//2048;

        public static void Init()
        {
            IOPort.outb(0x20, 0x11);
            IOPort.outb(0xA0, 0x11);
            IOPort.outb(0x21, 0x20);
            IOPort.outb(0xA1, 0x28);
            IOPort.outb(0x21, 0x04);
            IOPort.outb(0xA1, 0x02);
            IOPort.outb(0x21, 0x01);
            IOPort.outb(0xA1, 0x01);
            IOPort.outb(0x21, 0x0);
            IOPort.outb(0xA1, 0x0);
            timer_phase(IPS);
        }

        private static void timer_phase(uint hz)
        {
            uint divisor = SCALE / hz;
            IOPort.outb(CONTROL, SET);
            IOPort.outb(PIT_A, (byte)(divisor & MASK));
            IOPort.outb(PIT_A, (byte)((divisor >> 8) & MASK));
        }
    }
}
