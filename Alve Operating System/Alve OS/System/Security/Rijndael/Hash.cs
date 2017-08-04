/*
 * Rijndael256
 * Copyright (C)2013 2Toad, LLC.
 * licensing@2toad.com
 * 
 * https://github.com/2Toad/Rijndael256
 */

using System;
using System.Security.Cryptography;
using System.Text;

namespace Rijndael256
{
    /// <summary>
    /// Cryptographic hash functions.
    /// </summary>
    public static class Hash
    {
        /// <summary>
        /// Generates a SHA-512 hash from the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The hash.</returns>
        public static string Sha512(string data)
        {
            var hash = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "");
        }
        
        /// <summary>
        /// Generates a PBKDF2 hash from the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="iterations">The number of iterations to derive the hash.</param>
        /// <param name="size">The size of the hash.</param>
        /// <returns>The hash.</returns>
        public static byte[] Pbkdf2(string data, string salt, int iterations, int size = 64)
        {
            return Pbkdf2(data, Encoding.UTF8.GetBytes(salt), iterations, size);
        }

        /// <summary>
        /// Generates a PBKDF2 hash from the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="iterations">The number of iterations to derive the hash.</param>
        /// <param name="size">The size of the hash.</param>
        /// <returns>The hash.</returns>
        public static byte[] Pbkdf2(string data, byte[] salt, int iterations, int size = 64)
        {
            return Pbkdf2(Encoding.UTF8.GetBytes(data), salt, iterations, size);
        }

        /// <summary>
        /// Generates a PBKDF2 hash from the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="iterations">The number of iterations to derive the hash.</param>
        /// <param name="size">The size of the hash.</param>
        /// <returns>The hash.</returns>
        internal static byte[] Pbkdf2(byte[] data, byte[] salt, int iterations, int size = 64)
        {
            return (new Rfc2898DeriveBytes(data, salt, iterations)).GetBytes(size);
        }
    }
}