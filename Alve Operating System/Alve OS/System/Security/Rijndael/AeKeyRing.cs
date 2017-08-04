/*
 * Rijndael256
 * Copyright (C)2013 2Toad, LLC.
 * licensing@2toad.com
 * 
 * https://github.com/2Toad/Rijndael256
 */

using System.Text;

namespace Rijndael256
{
    /// <summary>
    /// Authenticated Encryption keys.
    /// </summary>
    public class AeKeyRing
    {
        // The generated hash is 512-bit (128 chars)
        // We split that into two 256-bit keys (64 chars each)
        private const int KeyLength = 64;

        /// <summary>
        /// The key used by the cipher.
        /// </summary>
        public string CipherKey { get; set; }

        /// <summary>
        /// The key used by the MAC.
        /// </summary>
        public byte[] MacKey { get; set; }

        /// <summary>
        /// Generates a SHA-512 hash from the provided password, and derives two
        /// 256-bit keys from the hash.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>A pair of 256-bit keys.</returns>
        public static AeKeyRing Generate(string password)
        {
            // Generate 512-bit hash from password
            var hash = Hash.Sha512(password);

            // Split hash into two 256-bit keys
            return new AeKeyRing {
                CipherKey = hash.Substring(0, KeyLength),
                MacKey = Encoding.UTF8.GetBytes(hash.Substring(KeyLength, KeyLength))
            };
        }
    }
}
