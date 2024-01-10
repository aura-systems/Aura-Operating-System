using System;

namespace SFML.Graphics
{
    // sfml byte[] format is rgba https://en.sfml-dev.org/forums/index.php?topic=2476.0
    public class Texture
    {
        public Texture(uint width, uint height)
        {
            Width = width;
            Height = height;
            byteRgbaData = new ushort[this.Width * this.Height * 4];
        }

        public uint Width { get; }
        public uint Height { get; }
        public ushort[] byteRgbaData { get; private set; }


        internal void Update(byte[] sfmlTextureData, uint height, uint width, uint x, uint y)
        {
            Array.Copy(sfmlTextureData, byteRgbaData, sfmlTextureData.Length);
            /*for (uint i = 0; i < height; i++)
            {
                uint destinationY = y + i;
                // convert array of byte to array of uint for simpler manipulation
                uint[] data = new uint[width * height];
                Buffer.BlockCopy(sfmlTextureData, 0, data, 0, (int)(width * height));
                // copy data to correct location in texture
                Array.Copy(data, i * width, Pixels, destinationY * this.Width + x, width);
            }*/
            //sfmlTextureData.CopyTo(pixels, );
        }

        internal void Dispose()
        {
            //Array.Clear(this.byteRgbaData, 0, this.byteRgbaData.Length);
        }
    }

}