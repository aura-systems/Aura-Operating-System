namespace Aura_OS.System.GUI.Graphics
{
    public class Color
    {
        //public byte R { get; set; }
        // public byte G { get; set; }
        // public byte B { get; set; }
        public byte A { get; set; }
         private byte _r;
         public byte R
        {
            get { return _r; }
            set
            {
                _r = value;
                Hex = ((R << 16) | (G << 8) | B);
            }
        }
         private byte _g;
         public byte G
        {
            get { return _g; }
            set
            {
                _g = value;
                Hex = ((R << 16) | (G << 8) | B);
            }
        }
         private byte _b;
         public byte B
        {
            get { return _b; }
            set
            {
                _b = value;
                Hex = ((R << 16) | (G << 8) | B);
            }
        }
         private int Hex { get; set; }
         public Color(byte r, byte g, byte b)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            A = 255;
        }
         public Color(byte r, byte g, byte b, byte a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            A = a;
        }
         public Color(Color c, byte a)
        {
            this.R = c.R;
            this.G = c.G;
            this.B = c.B;
            A = a;
        }
         public Color(int hex)
        {
            R = ((byte) (hex >> 16));
            G = ((byte) (hex >> 8));
            B = ((byte) (hex >> 0));
            A = 255;
        }
         public Color()
        {
        }
         public int ToHex() => Hex;
         public static implicit operator int(Color c)
        {
            return c.ToHex();
        }
    }
} 