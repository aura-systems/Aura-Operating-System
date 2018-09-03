using Cosmos.Core.Memory.Old;

 namespace Aura_OS.System.GUI.Graphics
{
    public unsafe class Canvas : ICanvas
    {
        public int Width { get; set; }
        public int Height { get; set; }
         private uint* _buffer;
         public Canvas(int width, int height)
        {
            Width = width;
            Height = height;
             _buffer = (uint*) Heap.MemAlloc((uint) (Width * Height * 4));
        }
         public void SetPixel(int x, int y, Color c)
        {
            SetPixel(x, y, (uint)c.ToHex());
        }
         public void SetPixel(int x, int y, uint c)
        {
            _buffer[x + (y * Width)] = c;
        }
         public Color GetPixel(int x, int y)
        {
            return new Color((int)_buffer[x + (y * Width)]);
        }
         public void WriteToScreen()
        {
            Cosmos.Core.MemoryOperations.Copy((uint*)Shell.VESAVBE.Graphics.ModeInfo.framebuffer, _buffer, Width * Height * 4);
        }
         public uint Blit(int x0, int y0, int w, int h)
        {
            int y;
            uint target = 0;
            for (y = 0; y < h; y++)
            {
                int yoff = ((y + y0) * w * 4);
                Cosmos.Core.MemoryOperations.Copy((byte*)target, (byte*)_buffer + (yoff) + (x0 * 4), w);
            }
            return target;
        }
         public void SetScanLine(int offset, int length, uint color)
        {
            Cosmos.Core.MemoryOperations.Fill((byte*)(offset + _buffer), (int)color, length);
        }
         public void Clear(uint c)
        {
            Cosmos.Core.MemoryOperations.Fill((byte*)_buffer, (int)c, Width * Height * 4);
        }
    }
} 