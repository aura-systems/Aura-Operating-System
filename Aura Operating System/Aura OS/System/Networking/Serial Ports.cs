using Cosmos.Core;
using System.Runtime.InteropServices;

namespace Aura_OS.System.Networking
{
    //http://wiki.osdev.org/Serial_Ports
    class Serial_Ports
    {

        [StructLayout(LayoutKind.Explicit)]
        public struct PortAddresses
        {
            public static int COM1 = 0x3F8;
            public static int COM2 = 0x2F8;
            public static int COM3 = 0x3E8;
            public static int COM4 = 0x2E8;

        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InterrputEnableRegister
        {
            public static int DataAvailable = 0;
            public static int TransmitterEmpty = 1;
            public static int Break_Error = 2;
            public static int StatusChange = 3;

            public static int Unused_n4 = 4;
            public static int Unused_n5 = 5;
            public static int Unused_n6 = 6;
        }

        public const ushort PORT = 0x3f8; /* COM1 */

        public void init_serial()
        {
            CDDI.outb(PORT + 1, 0x00);    // Disable all interrupts
            CDDI.outb(PORT + 3, 0x80);    // Enable DLAB (set baud rate divisor)
            CDDI.outb(PORT + 0, 0x03);    // Set divisor to 3 (lo byte) 38400 baud
            CDDI.outb(PORT + 1, 0x00);    //                  (hi byte)
            CDDI.outb(PORT + 3, 0x03);    // 8 bits, no parity, one stop bit
            CDDI.outb(PORT + 2, 0xC7);    // Enable FIFO, clear them, with 14-byte threshold
            CDDI.outb(PORT + 4, 0x0B);    // IRQs enabled, RTS/DSR set
        }

        // Receiving data
        public int serial_received()
        {
            return CDDI.inb(PORT + 5) & 1;
        }

        public char read_serial()
        {
            while (serial_received() == 0) ;

            return (char)CDDI.inb(PORT);
        }

        // Sending data
        public int is_transmit_empty()
        {
            return CDDI.inb(PORT + 5) & 0x20;
        }

        public void write_serial(char a)
        {
            while (is_transmit_empty() == 0) ;

            CDDI.outb(PORT, (byte)a);
        }
    }

}
