using System.Runtime.InteropServices;

namespace System
{
    public readonly partial struct Decimal
    {
        // Low level accessors used by a DecCalc and formatting
        internal uint High => _hi32;
        internal uint Low => (uint)_lo64;
        internal uint Mid => (uint)(_lo64 >> 32);

        internal ulong Low64 => _lo64;

        // private static ref DecCalc AsMutable(ref decimal d) => ref Unsafe.As<decimal, DecCalc>(ref d);

        #region APIs need by number formatting.



        #endregion

        // Rounds a Decimal to an integer value. The Decimal argument is rounded
        // towards negative infinity.
        //
        public static decimal Floor(decimal d)
        {
            //int flags = d._flags;
            // if ((flags & ScaleMask) != 0)
            //    DecCalc.InternalRound(ref AsMutable(ref d), (byte)(flags >> ScaleShift), MidpointRounding.ToNegativeInfinity);
            Console.WriteLine("Decimal.floor is not implemented");
            
            return d;
        }

        /// <summary>
        /// Class that contains all the mathematical calculations for decimal. Most of which have been ported from oleaut32.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private struct DecCalc
        {
            private const uint SignMask = 0x80000000;
            private const uint ScaleMask = 0x00FF0000;

            private const int DEC_SCALE_MAX = 28;

            private const uint TenToPowerNine = 1000000000;
            private const ulong TenToPowerEighteen = 1000000000000000000;

            // The maximum power of 10 that a 32 bit integer can store
            private const int MaxInt32Scale = 9;
            // The maximum power of 10 that a 64 bit integer can store
            private const int MaxInt64Scale = 19;

            // Fast access for 10^n where n is 0-9
            private static readonly uint[] s_powers10 = new uint[] {
                1,
                10,
                100,
                1000,
                10000,
                100000,
                1000000,
                10000000,
                100000000,
                1000000000
            };

            // Fast access for 10^n where n is 1-19
            private static readonly ulong[] s_ulongPowers10 = new ulong[] {
                10,
                100,
                1000,
                10000,
                100000,
                1000000,
                10000000,
                100000000,
                1000000000,
                10000000000,
                100000000000,
                1000000000000,
                10000000000000,
                100000000000000,
                1000000000000000,
                10000000000000000,
                100000000000000000,
                1000000000000000000,
                10000000000000000000,
            };

            private static readonly double[] s_doublePowers10 = new double[] {
                1, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9,
                1e10, 1e11, 1e12, 1e13, 1e14, 1e15, 1e16, 1e17, 1e18, 1e19,
                1e20, 1e21, 1e22, 1e23, 1e24, 1e25, 1e26, 1e27, 1e28, 1e29,
                1e30, 1e31, 1e32, 1e33, 1e34, 1e35, 1e36, 1e37, 1e38, 1e39,
                1e40, 1e41, 1e42, 1e43, 1e44, 1e45, 1e46, 1e47, 1e48, 1e49,
                1e50, 1e51, 1e52, 1e53, 1e54, 1e55, 1e56, 1e57, 1e58, 1e59,
                1e60, 1e61, 1e62, 1e63, 1e64, 1e65, 1e66, 1e67, 1e68, 1e69,
                1e70, 1e71, 1e72, 1e73, 1e74, 1e75, 1e76, 1e77, 1e78, 1e79,
                1e80
            };

            /// <summary>
            /// Decimal Compare updated to return values similar to ICompareTo
            /// </summary>
            internal static int VarDecCmp(in decimal d1, in decimal d2)
            {
                if ((d2.Low64 | d2.High) == 0)
                {
                    if ((d1.Low64 | d1.High) == 0)
                        return 0;
                    return (d1._flags >> 31) | 1;
                }
                if ((d1.Low64 | d1.High) == 0)
                    return -((d2._flags >> 31) | 1);

                int sign = (d1._flags >> 31) - (d2._flags >> 31);
                if (sign != 0)
                    return sign;
                return VarDecCmpSub(in d1, in d2);
            }
            private static ulong UInt32x32To64(uint a, uint b)
            {
                return (ulong)a * (ulong)b;
            }
            private static int VarDecCmpSub(in decimal d1, in decimal d2)
            {
                int flags = d2._flags;
                int sign = (flags >> 31) | 1;
                int scale = flags - d1._flags;

                ulong low64 = d1.Low64;
                uint high = d1.High;

                ulong d2Low64 = d2.Low64;
                uint d2High = d2.High;

                if (scale != 0)
                {
                    scale >>= ScaleShift;

                    // Scale factors are not equal. Assume that a larger scale factor (more decimal places) is likely to mean that number is smaller.
                    // Start by guessing that the right operand has the larger scale factor.
                    if (scale < 0)
                    {
                        // Guessed scale factor wrong. Swap operands.
                        scale = -scale;
                        sign = -sign;

                        ulong tmp64 = low64;
                        low64 = d2Low64;
                        d2Low64 = tmp64;

                        uint tmp = high;
                        high = d2High;
                        d2High = tmp;
                    }

                    // d1 will need to be multiplied by 10^scale so it will have the same scale as d2.
                    // Scaling loop, up to 10^9 at a time.
                    do
                    {
                        uint power = scale >= MaxInt32Scale ? TenToPowerNine : s_powers10[scale];
                        ulong tmpLow = UInt32x32To64((uint)low64, power);
                        ulong tmp = UInt32x32To64((uint)(low64 >> 32), power) + (tmpLow >> 32);
                        low64 = (uint)tmpLow + (tmp << 32);
                        tmp >>= 32;
                        tmp += UInt32x32To64(high, power);
                        // If the scaled value has more than 96 significant bits then it's greater than d2
                        if (tmp > uint.MaxValue)
                            return sign;
                        high = (uint)tmp;
                    } while ((scale -= MaxInt32Scale) > 0);
                }

                uint cmpHigh = high - d2High;
                if (cmpHigh != 0)
                {
                    // check for overflow
                    if (cmpHigh > high)
                        sign = -sign;
                    return sign;
                }

                ulong cmpLow64 = low64 - d2Low64;
                if (cmpLow64 == 0)
                    sign = 0;
                // check for overflow
                else if (cmpLow64 > low64)
                    sign = -sign;
                return sign;
            }

        }
    }
}
