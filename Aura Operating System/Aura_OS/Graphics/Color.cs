using System;

namespace WaveOS.Graphics
{
    public struct Color : IDisposable
    {
        public Color(byte A, byte R, byte G, byte B)
        {
            this.A = A;
            this.R = R;
            this.G = G;
            this.B = B;
        }
        public Color(byte R, byte G, byte B)
        {
            A = 255;
            this.R = R;
            this.G = G;
            this.B = B;
        }
        public Color(Color Color, byte A)
        {
            this.A = A;
            R = Color.R;
            G = Color.G;
            B = Color.B;
        }
        public Color(int ARGB)
        {
            A = (byte)((ARGB & 0xFF000000) >> 24);
            R = (byte)((ARGB & 0x00FF0000) >> 16);
            G = (byte)((ARGB & 0x0000FF00) >> 8);
            B = (byte)((ARGB & 0x000000FF) >> 0);
        }

        public int ARGB
        {
            get
            {
                return ((R & 0x0ff) << 16) | ((G & 0x0ff) << 8) | (B & 0x0ff);
            }
            set
            {
                // Does not work
                byte[] ARGB = BitConverter.GetBytes(value);
                A = ARGB[0];
                R = ARGB[1];
                G = ARGB[2];
                B = ARGB[3];
            }
        }
        public byte Brightness
        {
            get
            {
                return (byte)((R / 3) + (G / 3) + (B / 3));
            }
        }
        public byte A, R, G, B;

        public static Color AlphaBlend(Color NewColor, Color BackColor)
        {
            byte R = (byte)(((NewColor.A * NewColor.R) + ((256 - NewColor.A) * BackColor.R)) >> 8);
            byte G = (byte)(((NewColor.A * NewColor.G) + ((256 - NewColor.A) * BackColor.G)) >> 8);
            byte B = (byte)(((NewColor.A * NewColor.B) + ((256 - NewColor.A) * BackColor.B)) >> 8);
            return new(R, G, B);
        }
        public static Color ColorBlend(Color[] Colors)
        {
            Color BaseColor = new(0, 0, 0, 0);
            for (int I = 0; I < Colors.Length; I++)
            {
                BaseColor.A += (byte)(Colors[I].A / Colors.Length);
                BaseColor.R += (byte)(Colors[I].R / Colors.Length);
                BaseColor.G += (byte)(Colors[I].G / Colors.Length);
                BaseColor.B += (byte)(Colors[I].B / Colors.Length);
            }
            return BaseColor;
        }
        public void Dispose()
        {
            Cosmos.Core.GCImplementation.Free(this);
        }

        #region Colors
        public static readonly Color White = new(255, 255, 255, 255);
        public static readonly Color Black = new(255, 0, 0, 0);
        public static readonly Color Red = new(255, 255, 0, 0);
        public static readonly Color Green = new(255, 0, 255, 0);
        public static readonly Color Blue = new(255, 0, 0, 255);
        public static readonly Color CoolGreen = new(255, 54, 94, 53);
        public static readonly Color HotPink = new(255, 230, 62, 109);
        public static readonly Color UbuntuPurple = new(255, 66, 5, 22);
        public static readonly Color GoogleBlue = new(255, 66, 133, 244);
        public static readonly Color GoogleGreen = new(255, 52, 168, 83);
        public static readonly Color GoogleYellow = new(255, 251, 188, 5);
        public static readonly Color GoogleRed = new(255, 234, 67, 53);
        public static readonly Color DeepOrange = new(255, 255, 64, 0);
        public static readonly Color RubyRed = new(255, 204, 52, 45);
        public static readonly Color Transparent = new(0, 0, 0, 0);
        public static readonly Color StackOverflowOrange = new(255, 244, 128, 36);
        public static readonly Color StackOverflowBlack = new(255, 34, 36, 38);
        public static readonly Color StackOverflowWhite = new(255, 188, 187, 187);
        public static readonly Color DeepGray = new(255, 25, 25, 25);
        public static readonly Color LightGray = new(255, 125, 125, 125);
        public static readonly Color SuperOrange = new(255, 255, 99, 71);
        public static readonly Color FakeGrassGreen = new(255, 60, 179, 113);
        public static readonly Color DeepBlue = new(255, 51, 47, 208);
        public static readonly Color BloodOrange = new(255, 255, 123, 0);
        #endregion

        public static class SystemColors
        {
            public static Color BackGround = StackOverflowBlack;
            public static Color ForeGround = GoogleBlue;
            public static Color ContentText = White;
            public static Color TitleText = Black;
            public static Color Button = GoogleBlue;
            public static Color ButtonHighlight = new(255, 77, 144, 255);
            public static Color ButtonClick = DeepBlue;
        }
    }
}