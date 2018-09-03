using System;
using System.Collections.Generic;
using System.Text;
using Aura_OS.System.GUI.Imaging;
 namespace Aura_OS.System.GUI
{
    public class SdfChar
    {
        public SdfChar(int id, int x, int y, int width, int height, int xoffset, int yoffset, int xadvance)
        {
            Id = id;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Xoffset = xoffset;
            Yoffset = yoffset;
            Xadvance = xadvance;
        }
         public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Xoffset { get; set; }
        public int Yoffset { get; set; }
        public int Xadvance { get; set; }
         public SdfChar(string s)
        {
            var segs = s.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
             Id = int.Parse(segs[1].Split('=')[1].Trim());
            X = int.Parse(segs[2].Split('=')[1].Trim());
            Y = int.Parse(segs[3].Split('=')[1].Trim());
            Width = int.Parse(segs[4].Split('=')[1].Trim());
            Height = int.Parse(segs[5].Split('=')[1].Trim());
            Xoffset = int.Parse(segs[6].Split('=')[1].Trim());
            Yoffset = int.Parse(segs[7].Split('=')[1].Trim());
            Xadvance = int.Parse(segs[8].Split('=')[1].Trim());
        }
         public SdfChar Clone()
        {
            return new SdfChar(Id, X, Y, Width, Height, Xoffset, Yoffset, Xadvance);
        }
    }
     public class SdfFont
    {
        public Image AtlasImage { get; set; }
        public List<SdfChar> Chars { get; set; } = new List<SdfChar>();
         public int FontSize { get; set; }
         public SdfFont(string fnt, Image atlasImage)
        {
            AtlasImage = atlasImage;
            foreach (var line in fnt.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith("info"))
                {
                    var segs = line.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);
                    FontSize = int.Parse(segs[2].Split(' ')[0]);
                }
                else if (!line.StartsWith("chars") && line.StartsWith("char"))
                {
                    Chars.Add(new SdfChar(line));
                }
            }
        }
         public SdfChar GetChar(char c)
        {
            foreach (var chr in Chars)
            {
                if (chr.Id == (byte) c)
                {
                    return chr;
                }
            }
             return null;
        }
    }
} 