namespace DoomSharp.Core.Graphics;

public class Sky
{
    public const string FlatName = "F_SKY1";
    public const int AngleToSkyShift = 22;

    public int FlatNum { get; set; }
    public int Texture { get; set; }
    public Fixed TextureMid { get; set; }

    public void InitSkyMap()
    {
        TextureMid = Fixed.FromInt(100);
    }
}