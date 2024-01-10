namespace DoomSharp.Core;

public readonly struct Angle
{
    private readonly uint _value;

    // Binary Angle Measument, BAM.
    public static readonly Angle Angle0 = new(0);
    public static readonly Angle Angle45 = new(0x20000000);
    public static readonly Angle Angle90 = new(0x40000000);
    public static readonly Angle Angle180 = new(0x80000000);
    public static readonly Angle Angle270 = new(0xc0000000);
                               
    public static readonly Angle TraceAngle = new(0xc000000);

    public Angle(uint value)
    {
        _value = value;
    }

    public Angle(int value)
    {
        _value = (uint)value;
    }

    public uint Value => _value;

    public double ToDegrees()
    {
        return 360 * ((double)_value / 0x100000000);
    }

    public static Angle Abs(Angle angle)
    {
        var value = (int)angle._value;
        return value < 0 ? new Angle(-value) : angle;
    }

    public static Angle operator +(Angle angle)
    {
        return angle;
    }

    public static Angle operator -(Angle angle)
    {
        var inverseAngle = -(int)angle._value;
        return new Angle(inverseAngle);
    }

    public static Angle operator +(Angle a, Angle b)
    {
        return new Angle(a._value + b._value);
    }

    public static Angle operator -(Angle a, Angle b)
    {
        return new Angle(a._value - b._value);
    }

    public static Angle operator *(uint a, Angle b)
    {
        return new Angle(a * b._value);
    }

    public static Angle operator *(Angle a, uint b)
    {
        return new Angle(a._value * b);
    }

    public static Angle operator /(Angle a, uint b)
    {
        return new Angle(a._value / b);
    }

    public static bool operator ==(Angle a, Angle b)
    {
        return a._value == b._value;
    }

    public static bool operator !=(Angle a, Angle b)
    {
        return a._value != b._value;
    }

    public static bool operator <(Angle a, Angle b)
    {
        return a._value < b._value;
    }

    public static bool operator >(Angle a, Angle b)
    {
        return a._value > b._value;
    }

    public static bool operator <=(Angle a, Angle b)
    {
        return a._value <= b._value;
    }

    public static bool operator >=(Angle a, Angle b)
    {
        return a._value >= b._value;
    }

    public override bool Equals(object? obj)
    {
        if (obj is Angle b)
        {
            return _value == b._value;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return ToDegrees().ToString();
    }
}