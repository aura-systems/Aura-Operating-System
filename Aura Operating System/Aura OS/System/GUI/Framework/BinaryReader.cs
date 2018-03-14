using System.Collections.Generic;

namespace Aura_OS.System.GUI
{
    public class BinaryReader
    {
        private int _pos = 0;
        private byte[] _data;

        public List<byte> _buffer = new List<byte>();
        public int _index = -1;

        public BinaryReader(byte[] data)
        {
            _data = data;
        }

        public int Seek(int pos)
        {
            var oldPos = _pos;
            _pos = pos;
            return oldPos;
        }

        public int Tell()
        {
            return _pos;
        }

        public byte GetUint8()
        {
            return _data[_pos++];
        }

        public ushort GetUint16()
        {
            return (ushort)(((GetUint8() << 8) | GetUint8()) >> 0);
        }

        public uint GetUint32()
        {
            return (uint)GetInt32();
        }

        public short GetInt16()
        {
            //            var result = GetUint16();
            //             if ((result & 0x8000) == 1)
            //             {
            //                 result -= (1 << 16);
            //             }
            //             return result;

            return (short)GetUint16();
        }

        public int GetInt32()
        {
            return ((GetUint8() << 24) |
                    (GetUint8() << 16) |
                    (GetUint8() << 8) |
                    (GetUint8()));
        }

        public short GetFword()
        {
            return GetInt16();
        }

        public int Get2Dot14()
        {
            return GetInt16() / (1 << 14);
        }

        public int GetFixed()
        {
            return GetInt32() / (1 << 16);
        }

        public string GetString(int length)
        {
            var result = "";
            for (var i = 0; i < length; i++)
            {
                result += (char)GetUint8();
            }
            return result;
        }

         public void GetDate()
         {
             GetUint32();
             GetUint32();
             /* var macTime = GetUint32() * 0x100000000 + GetUint32();
             return new DateTime(macTime, DateTimeKind.Utc);*/
         }

        public char GetChar()
        {
            return (char) GetUint8();
        }

        public int ReadInt32()
        {
            return unchecked(
                   ((int)ReadByte() << 24) |
                   ((int)ReadByte() << 16) |
                   ((int)ReadByte() << 8) |
                    (int)ReadByte());
        }

        public byte ReadByte()
        {
            _index++;
            return _buffer[_index];
        }
    }
}