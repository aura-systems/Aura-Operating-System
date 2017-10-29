using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
namespace Aura_OS.System.Threading
{
    public static unsafe class ISR
    {
        public static uint old_esp;
        public static IRQContext Registers;
        private static uint[] isr_routines = new uint[0xFF];
        private static string[] system_exceptions =
        {
            "Division by zero",
            "Debug",
            "Non-maskable interrupt",
            "Breakpoint",
            "Detected overflow",
            "Out-of-bounds",
            "Invalid opcode",
            "No coprocessor",
            "Double fault",
            "Coprocessor segment overrun",
            "Bad TSS",
            "Segment not present",
            "Stack fault",
            "General protection fault",
            "Page fault",
            "Unknown interrupt",
            "Coprocessor fault",
            "Alignment check",
            "Machine check",
            "Reserved",
            "Reserved",
            "Reserved",
            "Reserved",
            "Reserved",
            "Reserved",
            "Reserved",
            "Reserved",
            "Reserved",
            "Reserved",
            "Reserved",
            "Reserved",
            "Reserved"
        };
        private static bool[] other_interupts_flags = new bool[16];
        private static string[] other_interupts =
        {
            "Programmable Interrupt Timer",
            "PS2 Keyboard",
            "Cascade",
            "COM2",
            "Com1",
            "LPT2",
            "Floppy Disk",
            "LPT1",
            "CMOS",
            "Legacy SCSI / NIC",
            "SCSI / NIC",
            "SCSI / NIC",
            "PS2 Mouse",
            "Coprocessor",
            "Primary ATA",
            "Secondary ATA"
        };

        public static void Init()
        {
            for (int i = 0; i < isr_routines.Length; i++)
            {
                isr_routines[0] = 0;
            }
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
        }

        public static void CommonISRHandler(ref IRQContext r)
        {
            IrqAck(r.Interrupt);
            if (r.Interrupt < 32)
            {
                HandleException(ref r);
            }
            else if (r.Interrupt == 0x80)
            {
                API.Handle(ref r);
            }
            else if (isr_routines[r.Interrupt] != 0)
            {
                Caller.CallCode(isr_routines[r.Interrupt], null);
            }
            else if (!other_interupts_flags[r.Interrupt])
            {
                other_interupts_flags[r.Interrupt] = true;
                Console.Write("Unhandled INT ");
                if (r.Interrupt - 0x20 < other_interupts.Length)
                {
                    Console.Write(other_interupts[r.Interrupt - 0x20]);
                }
                else
                {
                    Console.Write((int)r.Interrupt - 0x20);
                }
                Console.NewLine();
            }
        }

        private static void HandleException(ref IRQContext r)
        {
            if (r.Interrupt == 0x06)
            {
                Console.Write("Thread ");
                Console.Write(Scheduler.CurrentThread.name);
                Console.WriteLine(" died");
                Scheduler.CurrentThread.state = Thread.THREAD_STATES.DEAD;
                //for(int i = 0; i < Scheduler.ActiveThreads.Count; i++)
                //{
                //    if(Scheduler.ActiveThreads[i] == Scheduler.CurrentThread)
                //    {
                //        Scheduler.ActiveThreads.RemoveAt(i);
                //        break;
                //    }
                //}
                r.EIP = Utils.getMethodHandler(Scheduler.Idle);
            }
            else
            {
                Console.Write("System Error : ");
                Console.Write(system_exceptions[r.Interrupt]);
                Console.WriteLine(" : Locking Up");
                CPU.DisableInts();
                while (true) { }
            }
        }

        private static void IrqAck(uint irq_no)
        {
            if (irq_no > 8)
            {
                IOPort.outb(0xA0, 0x20);
            }
            IOPort.outb(0x20, 0x20);
        }

        public static void SetIntHandler(byte aIntNo, aMethod aHandler)
        {
            SetRealHandler((byte)(0x20 + aIntNo), aHandler);
        }

        public static void SetRealHandler(byte aIntNo, aMethod aHandler)
        {
            isr_routines[aIntNo] = Utils.getMethodHandler(aHandler);
        }

        public static void SetMask(byte IRQline)
        {
            ushort port;
            byte value;

            if (IRQline < 8)
            {
                port = 0x20 + 1;
            }
            else
            {
                port = 0xA0 + 1;
                IRQline -= 8;
            }
            value = (byte)(IOPort.inb(port) | (1 << IRQline));
            IOPort.outb(port, value);
        }

        public static void ClearMask(byte IRQline)
        {
            ushort port;
            byte value;

            if (IRQline < 8)
            {
                port = 0x20 + 1;
            }
            else
            {
                port = 0xA0 + 1;
                IRQline -= 8;
            }
            value = (byte)(IOPort.inb(port) & ~(1 << IRQline));
            IOPort.outb(port, value);
        }


        [StructLayout(LayoutKind.Explicit, Size = 80)]
        public struct IRQContext
        {
            [FieldOffset(0)]
            public unsafe uint* MMXContext;

            [FieldOffset(4)]
            public uint EDI;

            [FieldOffset(8)]
            public uint ESI;

            [FieldOffset(12)]
            public uint EBP;

            [FieldOffset(16)]
            public uint ESP;

            [FieldOffset(20)]
            public uint EBX;

            [FieldOffset(24)]
            public uint EDX;

            [FieldOffset(28)]
            public uint ECX;

            [FieldOffset(32)]
            public uint EAX;

            [FieldOffset(36)]
            public uint Interrupt;

            [FieldOffset(40)]
            public uint Param;

            [FieldOffset(44)]
            public uint EIP;

            [FieldOffset(48)]
            public uint CS;

            [FieldOffset(52)]
            public uint EFlags;

            [FieldOffset(56)]
            public uint UserESP;
        }
    }
}
