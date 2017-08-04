/*
 * Rijndael256
 * Copyright (C)2013 2Toad, LLC.
 * licensing@2toad.com
 * 
 * https://github.com/2Toad/Rijndael256
 */

using System.Security.Cryptography;

namespace Rijndael256
{
    public static class Rng
    {
        static Rng()
        {
            Random = RandomNumberGenerator.Create();
        }

        /// <summary>
        /// Generates an array of bytes using a cryptographically strong sequence
        /// of random values.
        /// </summary>
        /// <param name="size">The size of the array.</param>
        /// <returns>The array of bytes.</returns>
        public static byte[] GenerateRandomBytes(int size)
        {
            var bytes = new byte[size];
            Random.GetBytes(bytes);
            return bytes;
        }

        private static readonly RandomNumberGenerator Random;
    }
}
