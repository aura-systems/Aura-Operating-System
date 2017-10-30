using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.exe
{
    public unsafe class MemoryStream : IOS
    {
        private unsafe byte* a = null;

        public override void Close()
        {
        }

        public unsafe override void Write(byte i)
        {
            if (this.a == null)
            {
                base.Write(i);
            }
            else
            {
                this.a[this.Position] = i;
                this.Position++;
            }
        }

        public unsafe override byte Read()
        {
            byte result;
            if (this.a == null)
            {
                result = base.Read();
            }
            else
            {
                this.Position++;
                result = this.a[this.Position - 1];
            }
            return result;
        }

        public MemoryStream(int size)
        {
            this.a = null;
            base.init(size);
        }

        public MemoryStream(byte[] dat)
        {
            this.a = null;
            this.Data = dat;
        }

        public unsafe MemoryStream(byte* ptr)
        {
            this.a = ptr;
        }
    }
}
