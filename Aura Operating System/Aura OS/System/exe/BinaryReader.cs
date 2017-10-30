using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.exe
{
    public class BinaryReader
    {
        public IOS BaseStream;

        public BinaryReader(IOS stream)
        {
            stream.Position = 0;
            this.BaseStream = stream;
        }

        public byte ReadByte()
        {
            this.BaseStream.Position++;
            return this.BaseStream.Data[this.BaseStream.Position - 1];
        }

        public int ReadInt32()
        {
            int result = BitConverter.ToInt32(this.BaseStream.Data, this.BaseStream.Position);
            this.BaseStream.Position += 4;
            return result;
        }

        public uint ReadUInt32()
        {
            uint result = BitConverter.ToUInt32(this.BaseStream.Data, this.BaseStream.Position);
            this.BaseStream.Position += 4;
            return result;
        }

        public string ReadAllText()
        {
            string text = "";
            while (this.BaseStream.Position < this.BaseStream.Data.Length)
            {
                text += ((char)this.BaseStream.Read()).ToString();
            }
            return text;
        }

        public void Close()
        {
            this.BaseStream.Close();
        }

        public string ReadString()
        {
            byte b = this.BaseStream.Read();
            string text = "";
            for (int i = 0; i < (int)b; i++)
            {
                text += ((char)this.BaseStream.Read()).ToString();
            }
            return text;
        }
    }
}
