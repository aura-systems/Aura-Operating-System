using System.Runtime.CompilerServices;

namespace System
{
    public struct Double
    {
        //
        // Public Constants
        //
        public const double MinValue = -1.7976931348623157E+308;
        public const double MaxValue = 1.7976931348623157E+308;

        // Note Epsilon should be a double whose hex representation is 0x1
        // on little endian machines.
        public const double Epsilon = 4.9406564584124654E-324;
        public const double NegativeInfinity = (double)-1.0 / (double)(0.0);
        public const double PositiveInfinity = (double)1.0 / (double)(0.0);
        public const double NaN = (double)0.0 / (double)0.0;

        /// <summary>Represents the additive identity (0).</summary>
        internal const double AdditiveIdentity = 0.0;

        /// <summary>Represents the multiplicative identity (1).</summary>
        internal const double MultiplicativeIdentity = 1.0;

        /// <summary>Represents the number one (1).</summary>
        internal const double One = 1.0;

        /// <summary>Represents the number zero (0).</summary>
        internal const double Zero = 0.0;

        /// <summary>Represents the number negative one (-1).</summary>
        internal const double NegativeOne = -1.0;

        /// <summary>Represents the number negative zero (-0).</summary>
        public const double NegativeZero = -0.0;

        /// <summary>Represents the natural logarithmic base, specified by the constant, e.</summary>
        /// <remarks>Euler's number is approximately 2.7182818284590452354.</remarks>
        //public const double E = Math.E;

        ///// <summary>Represents the ratio of the circumference of a circle to its diameter, specified by the constant, PI.</summary>
        ///// <remarks>Pi is approximately 3.1415926535897932385.</remarks>
        //public const double Pi = Math.PI;

        ///// <summary>Represents the number of radians in one turn, specified by the constant, Tau.</summary>
        ///// <remarks>Tau is approximately 6.2831853071795864769.</remarks>
        //public const double Tau = Math.Tau;

        //
        // Constants for manipulating the private bit-representation
        //

        internal const ulong SignMask = 0x8000_0000_0000_0000;
        internal const int SignShift = 63;
        internal const byte ShiftedSignMask = (byte)(SignMask >> SignShift);

        internal const ulong BiasedExponentMask = 0x7FF0_0000_0000_0000;
        internal const int BiasedExponentShift = 52;
        internal const ushort ShiftedExponentMask = (ushort)(BiasedExponentMask >> BiasedExponentShift);

        internal const ulong TrailingSignificandMask = 0x000F_FFFF_FFFF_FFFF;

        internal const byte MinSign = 0;
        internal const byte MaxSign = 1;

        internal const ushort MinBiasedExponent = 0x0000;
        internal const ushort MaxBiasedExponent = 0x07FF;

        internal const ushort ExponentBias = 1023;

        internal const short MinExponent = -1022;
        internal const short MaxExponent = +1023;

        internal const ulong MinTrailingSignificand = 0x0000_0000_0000_0000;
        internal const ulong MaxTrailingSignificand = 0x000F_FFFF_FFFF_FFFF;

        internal const int TrailingSignificandLength = 52;
        internal const int SignificandLength = TrailingSignificandLength + 1;

        public static unsafe bool IsNegative(double d)
        {
            return false;
            //return BitConverter.DoubleToInt64Bits(d) < 0;
        }

        /// <summary>Determines whether the specified value is negative infinity.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNegativeInfinity(double d)
        {
            return d == double.NegativeInfinity;
        }
        public static bool IsPositiveInfinity(double d)
        {
            return d == double.PositiveInfinity;
        }
        public static unsafe bool IsNaN(double d)
        {
            // A NaN will never equal itself so this is an
            // easy and efficient way to check for NaN.

#pragma warning disable CS1718
            return d != d;
#pragma warning restore CS1718
        }

        internal static ulong ExtractTrailingSignificandFromBits(ulong bits)
        {
            return bits & TrailingSignificandMask;
        }
        internal static ushort ExtractBiasedExponentFromBits(ulong bits)
        {
            return (ushort)((bits >> BiasedExponentShift) & ShiftedExponentMask);
        }
    }
}
