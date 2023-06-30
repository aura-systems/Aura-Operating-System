using System.Runtime.CompilerServices;

namespace System
{
    //NOTICE: all of the constructors and constants are required by the compiler
    public readonly partial struct Decimal
    {  
        // Sign mask for the flags field. A value of zero in this bit indicates a
        // positive Decimal value, and a value of one in this bit indicates a
        // negative Decimal value.
        //
        // Look at OleAut's DECIMAL_NEG constant to check for negative values
        // in native code.
        private const int SignMask = unchecked((int)0x80000000);

        // Scale mask for the flags field. This byte in the flags field contains
        // the power of 10 to divide the Decimal value by. The scale byte must
        // contain a value between 0 and 28 inclusive.
        private const int ScaleMask = 0x00FF0000;

        // Number of bits scale is shifted by.
        private const int ScaleShift = 16;

        // Constant representing the Decimal value 0.
        public const decimal Zero = 0m;

        // Constant representing the Decimal value 1.
        public const decimal One = 1m;

        // Constant representing the Decimal value -1.
        public const decimal MinusOne = -1m;

        // Constant representing the largest possible Decimal value. The value of
        // this constant is 79,228,162,514,264,337,593,543,950,335.
        public const decimal MaxValue = 79228162514264337593543950335m;

        // Constant representing the smallest possible Decimal value. The value of
        // this constant is -79,228,162,514,264,337,593,543,950,335.
        public const decimal MinValue = -79228162514264337593543950335m;

        /// <summary>Represents the additive identity (0).</summary>
        private const decimal AdditiveIdentity = 0m;

        /// <summary>Represents the multiplicative identity (1).</summary>
        private const decimal MultiplicativeIdentity = 1m;

        /// <summary>Represents the number negative one (-1).</summary>
        private const decimal NegativeOne = -1m;

        // The lo, mid, hi, and flags fields contain the representation of the
        // Decimal value. The lo, mid, and hi fields contain the 96-bit integer
        // part of the Decimal. Bits 0-15 (the lower word) of the flags field are
        // unused and must be zero; bits 16-23 contain must contain a value between
        // 0 and 28, indicating the power of 10 to divide the 96-bit integer part
        // by to produce the Decimal value; bits 24-30 are unused and must be zero;
        // and finally bit 31 indicates the sign of the Decimal value, 0 meaning
        // positive and 1 meaning negative.
        //
        // NOTE: Do not change the order and types of these fields. The layout has to
        // match Win32 DECIMAL type.
        private readonly int _flags;
        private readonly uint _hi32;
        private readonly ulong _lo64;

        /// <summary>
        /// Gets the scaling factor of the decimal, which is a number from 0 to 28 that represents the number of decimal digits.
        /// </summary>
        public byte Scale => (byte)(_flags >> ScaleShift);

        //    // Constructs a Decimal from a float value.
        //
        public Decimal(float value)
        {
            //DecCalc.VarDecFromR4(value, out AsMutable(ref this));
        }

        // Constructs a Decimal from a double value.
        //
        public Decimal(double value)
        {
           // DecCalc.VarDecFromR8(value, out AsMutable(ref this));
        }
        // Constructs a Decimal from an integer value.
        //
        public Decimal(int value)
        {
            if (value >= 0)
            {
                _flags = 0;
            }
            else
            {
                _flags = SignMask;
                value = -value;
            }
            _lo64 = (uint)value;
            _hi32 = 0;
        }
        //
        // Constructs a Decimal from an unsigned integer value.
        //
        public Decimal(uint value)
        {
            _flags = 0;
            _lo64 = value;
            _hi32 = 0;
        }

        // Constructs a Decimal from a long value.
        //
        public Decimal(long value)
        {
            if (value >= 0)
            {
                _flags = 0;
            }
            else
            {
                _flags = SignMask;
                value = -value;
            }
            _lo64 = (ulong)value;
            _hi32 = 0;
        }

        // Constructs a Decimal from an unsigned long value.
        //
        public Decimal(ulong value)
        {
            _flags = 0;
            _lo64 = value;
            _hi32 = 0;
        }
        private Decimal(in decimal d, int flags)
        {
            this = d;
            _flags = flags;
        }
        public Decimal(int lo, int mid, int hi, bool isNegative, byte scale)
        {
            if (scale > 28)
                throw new Exception("ArgumentOutOfRange_DecimalScale");
            _lo64 = (uint)lo + ((ulong)(uint)mid << 32);
            _hi32 = (uint)hi;
            _flags = ((int)scale) << 16;
            if (isNegative)
                _flags |= SignMask;
        }
        /// <inheritdoc cref="INumberBase{TSelf}.Abs(TSelf)" />
        public static decimal Abs(decimal value)
        {
            return new decimal(in value, value._flags & ~SignMask);
        }
        // Rounds a Decimal to an integer value. The Decimal argument is rounded
        // towards positive infinity.
        public static decimal Ceiling(decimal d)
        {
            int flags = d._flags;
            //if ((flags & ScaleMask) != 0)
            //  DecCalc.InternalRound(ref AsMutable(ref d), (byte)(flags >> ScaleShift), MidpointRounding.ToPositiveInfinity);
            return d;
        }
        public static decimal Max(decimal x, decimal y)
        {
            Console.WriteLine("decimal.max (double,double) not implemted");
            return 0;
            // return DecCalc.VarDecCmp(in x, in y) >= 0 ? x : y;
        }
        private static decimal Round(ref decimal d, int decimals, MidpointRounding mode)
        {
            if ((uint)decimals > 28)
                throw new Exception("decimal round ArgumentOutOfRange_DecimalRound");
            if ((uint)mode > (uint)MidpointRounding.ToPositiveInfinity)
                throw new Exception("decimal round SR.Argument_InvalidEnumValue");

            int scale = d.Scale - decimals;
            // if (scale > 0)
            //   DecCalc.InternalRound(ref AsMutable(ref d), (uint)scale, mode);
            Console.WriteLine("decimal round not implemented");

            return d;
        }
        public static decimal Round(decimal d) => Round(ref d, 0, MidpointRounding.ToEven);
        public static decimal Round(decimal d, int decimals) => Round(ref d, decimals, MidpointRounding.ToEven);
        public static decimal Round(decimal d, MidpointRounding mode) => Round(ref d, 0, mode);
        public static decimal Round(decimal d, int decimals, MidpointRounding mode) => Round(ref d, decimals, mode);

        /// <inheritdoc cref="IFloatingPoint{TSelf}.Round(TSelf)" />
        /// <inheritdoc cref="IFloatingPoint{TSelf}.Round(TSelf)" />
        //public static double Round(double x) => Math.Round(x);

        ///// <inheritdoc cref="IFloatingPoint{TSelf}.Round(TSelf, int)" />
        //public static double Round(double x, int digits) => Math.Round(x, digits);

        ///// <inheritdoc cref="IFloatingPoint{TSelf}.Round(TSelf, MidpointRounding)" />
        //public static double Round(double x, MidpointRounding mode) => Math.Round(x, mode);

        ///// <inheritdoc cref="IFloatingPoint{TSelf}.Round(TSelf, int, MidpointRounding)" />
        //public static double Round(double x, int digits, MidpointRounding mode) => Math.Round(x, digits, mode);
        public static void Truncate(ref decimal d)
        {
            int flags = d._flags;
            //if ((flags & ScaleMask) != 0)
            //   DecCalc.InternalRound(ref AsMutable(ref d), (byte)(flags >> ScaleShift), MidpointRounding.ToZero);
        }

        public static int Sign(decimal d) => (d.Low64 | d.High) == 0 ? 0 : (d._flags >> 31) | 1;

        public static implicit operator decimal(byte value) => new decimal((uint)value);

       // [CLSCompliant(false)]
        public static implicit operator decimal(sbyte value) => new decimal(value);

        public static implicit operator decimal(short value) => new decimal(value);

       // [CLSCompliant(false)]
        public static implicit operator decimal(ushort value) => new decimal((uint)value);

        public static implicit operator decimal(char value) => new decimal((uint)value);

        public static implicit operator decimal(int value) => new decimal(value);

       // [CLSCompliant(false)]
        public static implicit operator decimal(uint value) => new decimal(value);

        public static implicit operator decimal(long value) => new decimal(value);

       // [CLSCompliant(false)]
        public static implicit operator decimal(ulong value) => new decimal(value);

        public static explicit operator decimal(float value) => new decimal(value);

        public static explicit operator decimal(double value) => new decimal(value);

       // public static explicit operator byte(decimal value) => ToByte(value);

       //// [CLSCompliant(false)]
       // public static explicit operator sbyte(decimal value) => ToSByte(value);

       // public static explicit operator char(decimal value)
       // {
       //     ushort temp;
       //     try
       //     {
       //         temp = ToUInt16(value);
       //     }
       //     catch (OverflowException e)
       //     {
       //         throw new OverflowException(SR.Overflow_Char, e);
       //     }
       //     return (char)temp;
       // }

        //public static explicit operator short(decimal value) => ToInt16(value);

        //[CLSCompliant(false)]
        //public static explicit operator ushort(decimal value) => ToUInt16(value);

       // public static explicit operator int(decimal value) => ToInt32(value);

        //[CLSCompliant(false)]
        //public static explicit operator uint(decimal value) => ToUInt32(value);

        //public static explicit operator long(decimal value) => ToInt64(value);

       // [CLSCompliant(false)]
      //  public static explicit operator ulong(decimal value) => ToUInt64(value);

      //  public static explicit operator float(decimal value) => DecCalc.VarR4FromDec(in value);

       // public static explicit operator double(decimal value) => DecCalc.VarR8FromDec(in value);

        public static decimal operator +(decimal d) => d;

        public static decimal operator -(decimal d) => new decimal(in d, d._flags ^ SignMask);

        /// <inheritdoc cref="IIncrementOperators{TSelf}.op_Increment(TSelf)" />
       // public static decimal operator ++(decimal d) => Add(d, One);

        /// <inheritdoc cref="IDecrementOperators{TSelf}.op_Decrement(TSelf)" />
        //public static decimal operator --(decimal d) => Subtract(d, One);

        public static decimal operator +(decimal d1, decimal d2)
        {
           // DecCalc.DecAddSub(ref AsMutable(ref d1), ref AsMutable(ref d2), false);
            return d1;
        }

        public static decimal operator -(decimal d1, decimal d2)
        {
           // DecCalc.DecAddSub(ref AsMutable(ref d1), ref AsMutable(ref d2), true);
            return d1;
        }

        /// <inheritdoc cref="IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
        public static decimal operator *(decimal d1, decimal d2)
        {
            //DecCalc.VarDecMul(ref AsMutable(ref d1), ref AsMutable(ref d2));
            return d1;
        }

        /// <inheritdoc cref="IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
        public static decimal operator /(decimal d1, decimal d2)
        {
         //   DecCalc.VarDecDiv(ref AsMutable(ref d1), ref AsMutable(ref d2));
            return d1;
        }

        /// <inheritdoc cref="IModulusOperators{TSelf, TOther, TResult}.op_Modulus(TSelf, TOther)" />
        public static decimal operator %(decimal d1, decimal d2)
        {
           // DecCalc.VarDecMod(ref AsMutable(ref d1), ref AsMutable(ref d2));
            return d1;
        }

        /// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
        public static bool operator ==(decimal d1, decimal d2) => DecCalc.VarDecCmp(in d1, in d2) == 0;

        /// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
        public static bool operator !=(decimal d1, decimal d2) => DecCalc.VarDecCmp(in d1, in d2) != 0;

        /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
        public static bool operator <(decimal d1, decimal d2) => DecCalc.VarDecCmp(in d1, in d2) < 0;

        /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
        public static bool operator <=(decimal d1, decimal d2) => DecCalc.VarDecCmp(in d1, in d2) <= 0;

        /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
        public static bool operator >(decimal d1, decimal d2) => DecCalc.VarDecCmp(in d1, in d2) > 0;

        /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
        public static bool operator >=(decimal d1, decimal d2) => DecCalc.VarDecCmp(in d1, in d2) >= 0;

    }
}
