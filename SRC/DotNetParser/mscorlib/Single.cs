using System.Runtime.CompilerServices;

namespace System
{
    public readonly struct Single
    {  //
        // Public constants
        //
        public const float MinValue = (float)-3.40282346638528859e+38;
        public const float MaxValue = (float)3.40282346638528859e+38;

        // Note Epsilon should be a float whose hex representation is 0x1
        // on little endian machines.
        public const float Epsilon = (float)1.4e-45;
        public const float NegativeInfinity = (float)-1.0 / (float)0.0;
        public const float PositiveInfinity = (float)1.0 / (float)0.0;
        public const float NaN = (float)0.0 / (float)0.0;

     
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool IsNegative(float f)
        {
            return BitConverter.SingleToInt32Bits(f) < 0;
        }

        /// <summary>Determines whether the specified value is negative infinity.</summary>
       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool IsNegativeInfinity(float f)
        {
            return f == float.NegativeInfinity;
        }
        public static unsafe bool IsNaN(float f)
        {
            // A NaN will never equal itself so this is an
            // easy and efficient way to check for NaN.

#pragma warning disable CS1718
            return f != f;
#pragma warning restore CS1718
        }
    }
}
