using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleFileSystem
{
    public class StructStream
    {
        public List<byte> _buffer = new List<byte>();
        public int _offset = 0;

        byte[] buffer { get; set; }

        public StructStream()
        {

        }

        public StructStream(byte[] abuffer)
        {
            buffer = abuffer;
        }

        public void Flip()
        {
            _buffer.Reverse();
        }

        public byte ReadByte()
        {
            var b = buffer[_offset];
            _offset += 1;
            return b;
        }

        public byte[] Read(int length)
        {
            var data = new byte[length];
            Array.Copy(buffer, _offset, data, 0, length);
            _offset += length;
            return data;
        }

    


        public long ReadLong()
        {
            byte[] b = new byte[8];
            b[0] = ReadByte();
            b[1] = ReadByte();
            b[2] = ReadByte();
            b[3] = ReadByte();

            b[4] = ReadByte();
            b[5] = ReadByte();
            b[6] = ReadByte();
            b[7] = ReadByte();

            return BitConverter.ToInt64(b, 0);
        }

        public short ReadShort()
        {
            byte[] b = new byte[2];
            b[0] = ReadByte();
            b[1] = ReadByte();
            return BitConverter.ToInt16(b, 0);
        }

        public float ReadFloat()
        {
            byte[] b = new byte[4];
            b[0] = ReadByte();
            b[1] = ReadByte();
            b[2] = ReadByte();
            b[3] = ReadByte();
            return BitConverter.ToSingle(b, 0);
        }

        public ushort ReadUShort()
        {
            byte[] b = new byte[2];
            b[0] = ReadByte();
            b[1] = ReadByte();
            return BitConverter.ToUInt16(b, 0);
        }

        public string ReadString(int length)
        {
            var data = Read(length);
            return Encoding.UTF8.GetString(data);
        }


       

      
        public void WriteShort(short value)
        {
            _buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void WriteUShort(ushort value)
        {
            _buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void WriteInt(int value)
        {
            _buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void WriteFloat(float value)
        {
            _buffer.AddRange(BitConverter.GetBytes(value));
        }

        public ulong ReadUInt64()
        {
            return unchecked((((ulong)ReadByte() << 56) |
                   ((ulong)ReadByte() << 48) |
                   ((ulong)ReadByte() << 40) |
                   ((ulong)ReadByte() << 32) |
                   ((ulong)ReadByte() << 24) |
                   ((ulong)ReadByte() << 16) |
                   ((ulong)ReadByte() << 8) |
                    ReadByte()));
        }

        public uint ReadUInt32()
        {
            return (uint)(
                (ReadByte() << 24) |
                (ReadByte() << 16) |
                (ReadByte() << 8) |
                 ReadByte());
        }

        public unsafe float ReadSingle()
        {
            uint value = ReadUInt32();
            return *(float*)&value;
        }

        public unsafe double ReadDouble()
        {
            ulong value = ReadUInt64();
            return *(double*)&value;
        }

        public void WriteSingle(Single value)
        {
            _buffer.AddRange(BitConverter.GetBytes(value));
        }

        public unsafe void WriteDouble(double value)
        {
            WriteUInt64(*(ulong*)&value);
        }

        public void WriteUInt64(ulong value)
        {
            _buffer.Add((byte)((value & 0xFF00000000000000) >> 56));
            _buffer.Add((byte)((value & 0xFF000000000000) >> 48));
            _buffer.Add((byte)((value & 0xFF0000000000) >> 40));
            _buffer.Add((byte)((value & 0xFF00000000) >> 32));
            _buffer.Add((byte)((value & 0xFF000000) >> 24));
            _buffer.Add((byte)((value & 0xFF0000) >> 16));
            _buffer.Add((byte)((value & 0xFF00) >> 8));
            _buffer.Add((byte)(value & 0xFF));

        }

        public void WriteInt64(Int64 value)
        {
            _buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void WriteByte(byte value)
        {
            _buffer.Add(BitConverter.GetBytes(value)[0]);
        }
        public void WriteSByte(sbyte value)
        {
            _buffer.Add(BitConverter.GetBytes(value)[0]);
        }

        public void WriteLong(long value)
        {
            _buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void WriteString(string data, bool length = true)
        {
            var abuffer = Encoding.UTF8.GetBytes(data);
            if (length)
            {
                WriteInt(abuffer.Length);
            }
            _buffer.AddRange(abuffer);
        }
    }
}