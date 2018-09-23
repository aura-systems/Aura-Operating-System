using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleFileSystem
{
    public unsafe class BlockBuffer
    {
        public long Offset { get; set; }

        public BlockBuffer(IBlockDevice blockDevice)
        {
            BlockDevice = blockDevice;
            _buffer = BlockDevice.ReadBlock(0);
            _readBuffer = BlockDevice.ReadBlock(0);
        }

        public IBlockDevice BlockDevice { get; set; }

        private byte[] _buffer { get; set; }
        private long _bufferOffset { get; set; }

        private byte BlockReadByte(long offset)
        {
            var block = offset / BlockDevice.BlockSize;

            if (_bufferOffset != block)
            {
                _buffer = BlockDevice.ReadBlock(block);
                _bufferOffset = block;
            }

            var localOffset = offset - (block * BlockDevice.BlockSize);

            return _buffer[localOffset];
        }

        private byte[] BlockReadBytes(long offset, long size)
        {
//            var block = offset / BlockDevice.BlockSize;
//
//            if (_bufferOffset != block)
//            {
//                _buffer = BlockDevice.ReadBlock(block);
//                _bufferOffset = block;
//            }
//
//            var localOffset = offset - (block * BlockDevice.BlockSize);
//
//            var data = new byte[size];
//            Array.Copy(_buffer, localOffset, data, 0, size);

            var data = new List<byte>();

            for (long off = offset; off < offset + size; off++)
            {
                data.Add(BlockReadByte(off));
            }

            Offset += size;
            return data.ToArray();
        }


        private byte[] _readBuffer { get; set; }
        private long _readBufferOffset { get; set; }

        private void BlockWriteOffsetByte(long offset, byte b)
        {
            var block = offset / BlockDevice.BlockSize;

            if (_readBufferOffset != block)
            {
                _readBuffer = BlockDevice.ReadBlock(block);
                _readBufferOffset = block;
            }

            var localOffset = offset - (block * BlockDevice.BlockSize);

            _readBuffer[localOffset] = b;

            BlockDevice.WriteBlock(block, _readBuffer);
        }

        private void BlockWriteOffsetBytes(long offset, byte[] b)
        {
            var block = offset / BlockDevice.BlockSize;

            if (_readBufferOffset != block)
            {
                _readBuffer = BlockDevice.ReadBlock(block);
                _readBufferOffset = block;
            }

            var localOffset = offset - (block * BlockDevice.BlockSize);

            foreach (var b1 in b)
            {
                _readBuffer[localOffset++] = b1;
            }

            BlockDevice.WriteBlock(block, _readBuffer);
        }

        public byte ReadByte()
        {
            var b = BlockReadByte(Offset);
            Offset += 1;
            return b;
        }

        public byte[] Read(long length) => BlockReadBytes(Offset, length);

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

        public string ReadString()
        {
            var length = ReadLong();
            var data = Read(length);
            return Encoding.UTF8.GetString(data);
        }

        public ulong ReadUInt64()
        {
            return unchecked((((ulong) ReadByte() << 56) |
                              ((ulong) ReadByte() << 48) |
                              ((ulong) ReadByte() << 40) |
                              ((ulong) ReadByte() << 32) |
                              ((ulong) ReadByte() << 24) |
                              ((ulong) ReadByte() << 16) |
                              ((ulong) ReadByte() << 8) |
                              ReadByte()));
        }

        public uint ReadUInt32()
        {
            return (uint) (
                (ReadByte() << 24) |
                (ReadByte() << 16) |
                (ReadByte() << 8) |
                ReadByte());
        }

        public float ReadSingle()
        {
            uint value = ReadUInt32();
            return *(float*) &value;
        }

        public double ReadDouble()
        {
            ulong value = ReadUInt64();
            return *(double*) &value;
        }


        private void WriteAddRange(byte[] b)
        {
            foreach (var b1 in b)
            {
                WriteByte(b1);
            }
            

            /* if (b.Length <= BlockDevice.BlockSize)
             {
                 BlockWriteOffsetBytes(Offset, b);
                 Offset += b.Length;
                 return;
             }
 
             var segsCount = b.Length / BlockDevice.BlockSize + 1;
 
             var buf = new byte[BlockDevice.BlockSize];
 
             for (int i = 0; i < segsCount; i++)
             {
 
                 var l = BlockDevice.BlockSize;
 
                 if (i + 1 == segsCount)
                 {
                     var x = (i + 1) * BlockDevice.BlockSize;
                     var y = b.Length - x;
                     l = y <= BlockDevice.BlockSize ? y : BlockDevice.BlockSize;
                 }
 
                 Array.Copy(b, buf, l);
                 BlockWriteOffsetBytes(Offset, buf);
                 Offset += l;
             }
             */

        }

        private void WriteAdd(byte b)
        {
            BlockWriteOffsetByte(Offset, b);
            Offset += 1;
        }

        public void WriteShort(short value)
        {
            WriteAddRange(BitConverter.GetBytes(value));
        }

        public void WriteUShort(ushort value)
        {
            WriteAddRange(BitConverter.GetBytes(value));
        }

        public void WriteInt(int value)
        {
            WriteAddRange(BitConverter.GetBytes(value));
        }

        public void WriteFloat(float value)
        {
            WriteAddRange(BitConverter.GetBytes(value));
        }

        public void WriteSingle(Single value)
        {
            WriteAddRange(BitConverter.GetBytes(value));
        }


        public unsafe void WriteDouble(double value)
        {
            WriteUInt64(*(ulong*) &value);
        }

        public void WriteUInt64(ulong value)
        {
            WriteAdd((byte) ((value & 0xFF00000000000000) >> 56));
            WriteAdd((byte) ((value & 0xFF000000000000) >> 48));
            WriteAdd((byte) ((value & 0xFF0000000000) >> 40));
            WriteAdd((byte) ((value & 0xFF00000000) >> 32));
            WriteAdd((byte) ((value & 0xFF000000) >> 24));
            WriteAdd((byte) ((value & 0xFF0000) >> 16));
            WriteAdd((byte) ((value & 0xFF00) >> 8));
            WriteAdd((byte) (value & 0xFF));
        }

        public void WriteUInt32(uint value)
        {
            WriteAdd((byte) ((value & 0xFF000000) >> 24));
            WriteAdd((byte) ((value & 0xFF0000) >> 16));
            WriteAdd((byte) ((value & 0xFF00) >> 8));
            WriteAdd((byte) (value & 0xFF));
        }

        public void WriteInt64(Int64 value)
        {
            WriteAddRange(BitConverter.GetBytes(value));
        }

        public void WriteByte(byte value)
        {
            WriteAdd(BitConverter.GetBytes(value)[0]);
        }

        public void WriteSByte(sbyte value)
        {
            WriteAdd(BitConverter.GetBytes(value)[0]);
        }

        public void WriteLong(long value)
        {
            WriteAddRange(BitConverter.GetBytes(value));
        }

        public void WriteString(string data, bool length = true)
        {
            var abuffer = Encoding.UTF8.GetBytes(data);
            if (length)
            {
                WriteLong(abuffer.Length);
            }

            WriteAddRange(abuffer);
        }
    }
}