using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Threading
{
    public static unsafe class Console
    {
        private static byte* vram = (byte*)0xB8000;
        public static SpinLock mutex = new SpinLock();
        public static int X;
        public static int Y;
        public static int width = 80;
        public static int height = 25;
        public static TextColour Background = TextColour.Black;
        public static TextColour Foreground = TextColour.Gray;
        public static char[] hchars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        public static string nchars = "0123456789";

        private static byte CombinedColour
        {
            get
            {
                return (byte)((byte)Foreground | (byte)(Background) << 4);
            }
        }

        public static void Clear()
        {
            X = 0;
            Y = 0;
            UpdateCursor();
            Utils.memset(vram, 0, (uint)((width * height) * 2));
        }

        private static void UpdateCursor()
        {
            IOPort.outb(0x3d4, 0xa);
            IOPort.outb(0x3d5, 14);
            ushort position = (ushort)((Y * width) + X - 1);
            IOPort.outb(0x3D4, 0x0F);
            IOPort.outb(0x3D5, (byte)(position & 0xFF));
            IOPort.outb(0x3D4, 0x0E);
            IOPort.outb(0x3D5, (byte)((position >> 8) & 0xFF));
        }

        public static void WriteLine()
        {
            NewLine();
        }

        public static void WriteLine(char source)
        {
            Write(source);
            NewLine();
        }

        public static void Write(char source)
        {
            PlotChar((byte)source);
        }

        public static void WriteLine(string source)
        {
            Write(source);
            NewLine();
        }

        public static void Write(string source)
        {
            for (int index = 0; index < source.Length; index++)
            {
                PlotChar((byte)source[index]);
            }
        }

        public static void WriteLine(byte* source)
        {
            Write(source);
            NewLine();
        }

        public static void Write(byte* source)
        {
            byte* data = source;
            while (*data != 0)
            {
                Write((char)*data);
                data++;
            }
        }

        public static void WriteLine(int value, char format = 't', bool skipHeader = false)
        {
            WriteLine((long)value, format, skipHeader);
        }

        public static void Write(int value, char format = 't', bool skipHeader = false)
        {
            Write((long)value, format, skipHeader);
        }

        public static void WriteLine(long value, char format = 't', bool skipHeader = false)
        {
            Write(value, format, skipHeader);
            NewLine();
        }

        public static void Write(long value, char format = 't', bool skipHeader = false)
        {
            if (format == 't')
            {
                if (value < 0)
                {
                    Write('-');
                    Write(-value, format, true);
                }
                else
                {
                    long n = value / 10;
                    long r = value % 10;
                    if (r < 0)
                    {
                        r += 10;
                        n--;
                    }
                    if (value >= 10)
                    {
                        Write(n, format, true);
                    }
                    PlotChar((byte)nchars[(int)r]);
                }
            }
            else if (format == 'h')
            {
                if (value <= 0)
                {
                    Write("0x00");
                }
                else
                {
                    if (!skipHeader)
                    {
                        Write("0x");
                    }
                    long n = value / 16;
                    long r = value % 16;
                    if (r < 0)
                    {
                        r += 16;
                        n--;
                    }
                    if (value >= 16)
                    {
                        Write(n, format, true);
                    }
                    r += '0';
                    if (r > '9')
                    {
                        r -= '9';
                        r += '@';
                    }
                    PlotChar((byte)r);
                }
            }
        }

        public static void NewLine()
        {
            Y++;
            X = 0;
            if (Y >= height)
            {
                byte* ptr = vram;
                int size = width / 2;
                Utils.memcpy((uint*)ptr, (uint*)ptr + size, (int)(width * height) / 2);
                Y = height - 1;
            }
        }

        private static void PlotChar(byte aChar)
        {
            if (aChar == '\n')
            {
                NewLine();
            }
            else if (aChar == '\b')
            {
                if (X == 0)
                {
                    X = width;
                    Y--;
                }
                X--;
                PlotChar((byte)' ');
                X--;
            }
            else if (aChar == '\r')
            {
                X = 0;
            }
            else if (aChar == '\t')
            {
                Write("    ");
            }
            else
            {
                if (aChar >= 0x20 && aChar <= 0x7F)
                {
                    uint xScreenOffset = (uint)(((Y * width) + X) * 2);
                    vram[xScreenOffset] = aChar;
                    vram[xScreenOffset + 1] = CombinedColour;
                    X++;
                }
            }
            if (X >= width)
            {
                NewLine();
            }
            UpdateCursor();
        }

        private static byte* input = null;
        private static int position;
        private static int max;

        public static byte* ReadLine(byte* output, int length)
        {
            mutex.Lock();
            max = length;
            position = 0;
            input = output;
            while (max != -1)
            {

            }
            max = -1;
            position = 0;
            input = null;
            mutex.Unlock();
            return null;
        }

        public static byte* ReadLine()
        {
            var content = (byte*)Heap.alloc(256);
            ReadLine(content, 256);
            return content;
        }

        public static void RecieveKey(char aChar)
        {
            if (input != null)
            {
                if (aChar == 0x0A)
                {
                    max = -1;
                    NewLine();
                }
                else
                {
                    if (aChar == '\b')
                    {
                        if (position > 0)
                        {
                            position--;
                            input[position] = 0;
                            Write(aChar);
                        }
                    }
                    else if (aChar == '\t')
                    {
                        RecieveKey(' ');
                        RecieveKey(' ');
                        RecieveKey(' ');
                        RecieveKey(' ');
                    }
                    else
                    {
                        if (position <= max)
                        {
                            Write(aChar);
                            input[position] = (byte)aChar;
                            position++;
                        }
                        else
                        {
                            RecieveKey((char)0x0A);
                        }
                    }
                    if (position == max)
                    {
                        RecieveKey((char)0x0A);
                    }
                }
            }
        }
    }
}
