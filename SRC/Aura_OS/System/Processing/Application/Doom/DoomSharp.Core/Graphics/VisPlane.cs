using System;

namespace DoomSharp.Core.Graphics;

public class VisPlane
{
    public VisPlane()
    {
        Height = default;
        PicNum = 0;
        LightLevel = 0;
        MinX = 0;
        MaxX = 0;
        Pad1 = 0;
        Pad2 = 0;
        Pad3 = 0;
        Pad4 = 0;

        FillTop(0xff);
    }

    public Fixed Height { get; set; }
    public int PicNum { get; set; }
    public int LightLevel { get; set; }
    public int MinX { get; set; }
    public int MaxX { get; set; }

    // leave pads for [minx-1]/[maxx+1]

    private byte Pad1 { get; set; }
    // Here lies the rub for all
    //  dynamic resize/change of resolution.
    private byte[] Top { get; } = new byte[Constants.ScreenWidth];
    private byte Pad2 { get; set; }
    private byte Pad3 { get; set; }
    // See above.
    private byte[] Bottom { get; } = new byte[Constants.ScreenWidth];
    private byte Pad4 { get; set; }

    public byte ReadTop(int i)
    {
        return i switch
        {
            < 0 => Pad1,
            >= Constants.ScreenWidth => Pad2,
            _ => Top[i]
        };
    }

    public void WriteTop(int i, byte value)
    {
        switch (i)
        {
            case < 0:
                Pad1 = value;
                break;
            case >= Constants.ScreenWidth:
                Pad2 = value;
                break;
            default:
                Top[i] = value;
                break;
        }
    }

    public byte ReadBottom(int i)
    {
        return i switch
        {
            < 0 => Pad3,
            >= Constants.ScreenWidth => Pad4,
            _ => Bottom[i]
        };
    }

    public void WriteBottom(int i, byte value)
    {
        switch (i)
        {
            case < 0:
                Pad3 = value;
                break;
            case >= Constants.ScreenWidth:
                Pad4 = value;
                break;
            default:
                Bottom[i] = value;
                break;
        }
    }

    private void FillTop(byte value)
    {
        Pad1 = value;
        Pad2 = value;
        Array.Fill(Top, value);
    }
}