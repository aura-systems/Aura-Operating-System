using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.exe
{
    public class BinaryWriter
    {
        public IOS BaseStream;

        public void Write(byte data)
        {
            this.BaseStream.Write(data);
        }

        public void Write(char data)
        {
            this.BaseStream.Write((byte)data);
        }

        public void WriteBytes(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                this.Write((byte)str[i]);
            }
        }

        public void Write(int data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            byte[] array = bytes;
            for (int i = 0; i < array.Length; i++)
            {
                byte i2 = array[i];
                this.BaseStream.Write(i2);
            }
        }

        public void Write(uint data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            byte[] array = bytes;
            for (int i = 0; i < array.Length; i++)
            {
                byte i2 = array[i];
                this.BaseStream.Write(i2);
            }
        }

        public void Write(short data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            byte[] array = bytes;
            for (int i = 0; i < array.Length; i++)
            {
                byte i2 = array[i];
                this.BaseStream.Write(i2);
            }
        }

        public void Write(ushort data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            byte[] array = bytes;
            for (int i = 0; i < array.Length; i++)
            {
                byte i2 = array[i];
                this.BaseStream.Write(i2);
            }
        }

        public void Write(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                byte i2 = data[i];
                this.BaseStream.Write(i2);
            }
        }

        public void Write(string data)
        {
            this.BaseStream.Write((byte)data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                byte i2 = (byte)data[i];
                this.BaseStream.Write(i2);
            }
        }

        public BinaryWriter(IOS file)
        {
            this.BaseStream = file;
        }
    }
}
