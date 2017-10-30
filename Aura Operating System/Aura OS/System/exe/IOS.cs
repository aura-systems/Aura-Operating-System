using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.exe
{
    public abstract class IOS
    {
        public int Position;

        public byte[] Data;

        private bool a = true;

        public virtual void Write(byte i)
        {
            if (this.Data.Length + 1 < this.Position)
            {
                byte[] array = new byte[this.Data.Length + 1000];
                for (int j = 0; j < this.Data.Length; j++)
                {
                    array[j] = i;
                }
                this.Data = array;
            }
            this.Data[this.Position] = i;
            this.Position++;
        }

        public virtual byte Read()
        {
            this.Position++;
            return this.Data[this.Position - 1];
        }

        public virtual void Flush()
        {
            this.Data = null;
            this.Position = 0;
        }

        public virtual void Close()
        {
        }

        public void init(int size)
        {
            this.a = false;
            this.Data = new byte[size];
        }
    }
}
