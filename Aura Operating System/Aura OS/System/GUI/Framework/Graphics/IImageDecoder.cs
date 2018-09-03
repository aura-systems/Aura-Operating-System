using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.GUI.Framework.Graphics
{
    public interface IImageDecoder
    {
        int[] Map { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        string MagicNumber { get; set; }

        void Load(byte[] raw);

        string ReadMagicNumber(byte[] raw);
    }
}
