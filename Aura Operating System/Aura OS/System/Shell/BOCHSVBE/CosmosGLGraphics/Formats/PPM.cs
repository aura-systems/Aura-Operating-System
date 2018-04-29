using Aura_OS.System.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Shell.VBE.CosmosGLGraphics.Formats
{
    public class PPM : IImage
    {
        public Image Read(byte[] bytes)
        {
            var reader = new BinaryReader(bytes);
            if (reader.GetChar() != 'P' || reader.GetChar() != '6')
                return null;
            reader.GetChar(); //Eat newline
            string widths = "", heights = "";
            char temp;

            while ((temp = reader.GetChar()) != ' ')
            {
                if (temp == '#')
                {
                    while ((temp = reader.GetChar()) != '\n') ;
                }
                else
                {
                    widths += temp;
                }

            }
            while ((temp = reader.GetChar()) >= '0' && temp <= '9')
            {
                heights += temp;
            }
            if (reader.GetChar() != '2' || reader.GetChar() != '5' || reader.GetChar() != '5')
                return null;
            reader.GetChar(); //Eat the last newline
            int width = int.Parse(widths),
                height = int.Parse(heights);
            var re = new Image(width, height);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    re.SetPixel(x, y, new Color(
                        reader.GetUint8(),
                        reader.GetUint8(),
                        reader.GetUint8()
                    ));

            return re;
        }
    }
}
