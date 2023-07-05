using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// Converts base data types to an array of bytes, and an array of bytes to base data types.
    /// </summary>
    public static class BitConverter
    {
        // This field indicates the "endianness" of the architecture.
        // The value is set to true if the architecture is
        // little endian; false if it is big endian.
#if BIGENDIAN
        public static readonly bool IsLittleEndian /* = false */;
#else
        public static readonly bool IsLittleEndian = true;
#endif


        /// <summary>
        /// Returns the specified Boolean value as a byte array.
        /// </summary>
        /// <param name="value">A Boolean value.</param>
        /// <returns>A byte array with length 1.</returns>
        public static byte[] GetBytes(bool value)
        {
            byte[] r = new byte[1];
            r[0] = (value ? (byte)1 : (byte)0);
            return r;
        }
        /// <summary>
        /// Returns the specified Unicode character value as a byte array.
        /// </summary>
        /// <param name="value">A Char value.</param>
        /// <returns>An array of bytes with length 2.</returns>
        public static byte[] GetBytes(char value)
        {
            byte[] bytes = new byte[sizeof(char)];
            bytes[0] = (byte)value;
            return bytes;
        }
        /// <summary>
        /// Converts the specified double-precision floating point number to a 64-bit signed integer.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A 64-bit signed integer whose bits are identical to <paramref name="value"/>.</returns>
        public static unsafe long DoubleToInt64Bits(double value) => *((long*)&value);

        /// <summary>
        /// Converts the specified 64-bit signed integer to a double-precision floating point number.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A double-precision floating point number whose bits are identical to <paramref name="value"/>.</returns>
        public static unsafe double Int64BitsToDouble(long value) => *((double*)&value);

        /// <summary>
        /// Converts the specified single-precision floating point number to a 32-bit signed integer.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A 32-bit signed integer whose bits are identical to <paramref name="value"/>.</returns>
        public static unsafe int SingleToInt32Bits(float value) => *((int*)&value);

        /// <summary>
        /// Converts the specified 32-bit signed integer to a single-precision floating point number.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A single-precision floating point number whose bits are identical to <paramref name="value"/>.</returns>
        public static unsafe float Int32BitsToSingle(int value) => *((float*)&value);

        /// <summary>
        /// Converts the specified half-precision floating point number to a 16-bit signed integer.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A 16-bit signed integer whose bits are identical to <paramref name="value"/>.</returns>
      //  [MethodImpl(MethodImplOptions.AggressiveInlining)]
       // public static unsafe short HalfToInt16Bits(Half value) => (short)value._value;

        /// <summary>
        /// Converts the specified 16-bit signed integer to a half-precision floating point number.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A half-precision floating point number whose bits are identical to <paramref name="value"/>.</returns>
       // [MethodImpl(MethodImplOptions.AggressiveInlining)]
      //  public static unsafe Half Int16BitsToHalf(short value) => new Half((ushort)(value));

        /// <summary>
        /// Converts the specified double-precision floating point number to a 64-bit unsigned integer.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A 64-bit unsigned integer whose bits are identical to <paramref name="value"/>.</returns>
        public static unsafe ulong DoubleToUInt64Bits(double value) => *((ulong*)&value);

        /// <summary>
        /// Converts the specified 64-bit unsigned integer to a double-precision floating point number.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A double-precision floating point number whose bits are identical to <paramref name="value"/>.</returns>
        public static unsafe double UInt64BitsToDouble(ulong value) => *((double*)&value);

        /// <summary>
        /// Converts the specified single-precision floating point number to a 32-bit unsigned integer.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A 32-bit unsigned integer whose bits are identical to <paramref name="value"/>.</returns>
        public static unsafe uint SingleToUInt32Bits(float value) => *((uint*)&value);

        /// <summary>
        /// Converts the specified 32-bit unsigned integer to a single-precision floating point number.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A single-precision floating point number whose bits are identical to <paramref name="value"/>.</returns>
        public static unsafe float UInt32BitsToSingle(uint value) => *((float*)&value);

        /// <summary>
        /// Converts the specified half-precision floating point number to a 16-bit unsigned integer.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A 16-bit unsigned integer whose bits are identical to <paramref name="value"/>.</returns>        [MethodImpl(MethodImplOptions.AggressiveInlining)]
       // public static unsafe ushort HalfToUInt16Bits(Half value) => value._value;

        /// <summary>
        /// Converts the specified 16-bit unsigned integer to a half-precision floating point number.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A half-precision floating point number whose bits are identical to <paramref name="value"/>.</returns>
//[MethodImpl(MethodImplOptions.AggressiveInlining)]
       // public static unsafe Half UInt16BitsToHalf(ushort value) => new Half(value);
    }
}
