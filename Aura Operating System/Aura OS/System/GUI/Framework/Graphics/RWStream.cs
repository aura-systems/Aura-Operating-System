using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.GUI.Framework.Graphics
{
    public class RWStream
    {
        public List<byte> _buffer = new List<byte>();
        public int _index = -1;

        public RWStream(byte[] raw)
        {
            for (int i = 0; i < raw.Length; i++)
            {
                _buffer.Add(raw[i]);
            }

        }

        public RWStream(byte[] raw, int offset)
        {
            for (int i = 0; i < raw.Length; i++)
            {
                _buffer.Add(raw[i]);
            }
            _index = offset - 1;
        }

        public RWStream()
        {

        }

        public ulong ReadUInt64()
        {
            return unchecked(
                   ((ulong)ReadByte() << 56) |
                   ((ulong)ReadByte() << 48) |
                   ((ulong)ReadByte() << 40) |
                   ((ulong)ReadByte() << 32) |
                   ((ulong)ReadByte() << 24) |
                   ((ulong)ReadByte() << 16) |
                   ((ulong)ReadByte() << 8) |
                    (ulong)ReadByte());
        }

        public void WriteUInt64(ulong value)
        {
            _index++;
            _buffer[_index] = ((byte)((value & 0xFF00000000000000) >> 56));
            _index++;
            _buffer[_index] = ((byte)((value & 0xFF000000000000) >> 48));
            _index++;
            _buffer[_index] = ((byte)((value & 0xFF0000000000) >> 40));
            _index++;
            _buffer[_index] = ((byte)((value & 0xFF00000000) >> 32));
            _index++;
            _buffer[_index] = ((byte)((value & 0xFF000000) >> 24));
            _index++;
            _buffer[_index] = ((byte)((value & 0xFF0000) >> 16));
            _index++;
            _buffer[_index] = ((byte)((value & 0xFF00) >> 8));
            _index++;
            _buffer[_index] = ((byte)(value & 0xFF));

        }

        public int ReadInt32()
        {
            return unchecked(
                   ((int)ReadByte() << 24) |
                   ((int)ReadByte() << 16) |
                   ((int)ReadByte() << 8) |
                    (int)ReadByte());
        }

        public void WriteInt32(int value)
        {

            _buffer.Add((byte)((value & 0xFF000000) >> 24));

            _buffer.Add((byte)((value & 0xFF0000) >> 16));

            _buffer.Add((byte)((value & 0xFF00) >> 8));

            _buffer.Add((byte)(value & 0xFF));

        }

        public void WriteByte(byte b)
        {
            _index++;
            _buffer.Add(b);
        }

        public byte ReadByte()
        {
            _index++;
            return _buffer[_index];
        }

        public byte ReadByte(int addr)
        {
            return _buffer[addr];
        }


        public void SetIndex(int index)
        {
            _index = index - 1;
        }

        public void WriteString(string name)
        {
            foreach (var i in name)
            {
                WriteByte((byte)i);
            }
            WriteByte(0);
        }

        public string ReadString()
        {
            byte x = 1;
            string re = "";
            while (x != 0)
            {
                x = ReadByte();
                if (x != 0)
                {
                    re += (char)x;
                }

            }
            return re;
        }

    }
}
