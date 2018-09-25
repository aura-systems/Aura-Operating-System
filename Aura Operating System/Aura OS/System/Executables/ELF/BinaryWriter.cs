using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosELFCore
{
    public class BinaryWriter
    {
        public Stream BaseStream;
        public BinaryWriter(Stream stm)
        {
            BaseStream = stm;
        }
        public void Write(byte data)
        {
            BaseStream.Write(data);
        }
        public void Write(char data)
        {
            BaseStream.Write((byte)data);
        }
        public void WriteBytes(string str)
        {
            for (int i = 0; i < str.Length; i++)
                Write((byte)str[i]);
        }
        public void Write(int data)
        {
            byte[] bits = BitConverter.GetBytes(data);
            foreach (byte b in bits)
            {
                BaseStream.Write(b);
            }
        }
        public void Write(uint data)
        {
            byte[] bits = BitConverter.GetBytes(data);
            foreach (byte b in bits)
            {
                BaseStream.Write(b);
            }
        }
        public void Write(short data)
        {
            byte[] bits = BitConverter.GetBytes(data);
            foreach (byte b in bits)
            {
                BaseStream.Write(b);
            }
        }
        public void Write(ushort data)
        {
            byte[] bits = BitConverter.GetBytes(data);
            foreach (byte b in bits)
            {
                BaseStream.Write(b);
            }
        }
        public void Write(byte[] data)
        {
            foreach (byte b in data)
            {
                BaseStream.Write(b);
            }
        }
        public void Write(string data)
        {
            BaseStream.Write((byte)data.Length);
            foreach (byte b in data)
            {
                BaseStream.Write(b);
            }
        }

    }
}
