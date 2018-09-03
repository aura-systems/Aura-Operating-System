using System;
using System.Collections.Generic;
using System.Text;
using Aura_OS.System.GUI.Graphics;
 namespace Aura_OS.System.GUI.Imaging.Formats
{
    public class CIF : IImage
    {
         public Image Read(byte[] raw)
        {
            var reader = new BinaryReader(raw);
             int Width = reader.ReadInt32();
            int Height = reader.ReadInt32();
            int[] Map = new int[Width * Height];
             var re = new Image(Width, Height);
             for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    re.SetPixel(x, y, new Color(
                        reader.GetUint8(),
                        reader.GetUint8(),
                        reader.GetUint8()
                    ));
             return re;
        }
     }
} 