/*
 * Rijndael256
 * Copyright (C)2013 2Toad, LLC.
 * licensing@2toad.com
 * 
 * https://github.com/2Toad/Rijndael256
 */

using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Rijndael256
{
    /// <summary>
    /// AES implementation of the Rijndael symmetric-key cipher using
    /// the Encrypt-then-MAC (EtM) mode for Authenticated Encryption.
    /// </summary>
    public class RijndaelEtM : Rijndael
    {
        /// <summary>
        /// Encrypts plaintext using the Encrypt-then-MAC (EtM) mode via the Rijndael cipher in 
        /// CBC mode with a password derived HMAC SHA-512 salt. A random 128-bit Initialization 
        /// Vector is generated for the cipher.
        /// </summary>
        /// <param name="plaintext">The plaintext to encrypt.</param>
        /// <param name="password">The password to encrypt the plaintext with.</param>
        /// <param name="keySize">The cipher key size. 256-bit is stronger, but slower.</param>
        /// <returns>The Base64 encoded EtM ciphertext.</returns>
        public static new string Encrypt(string plaintext, string password, KeySize keySize)
        {
            return Encrypt(Encoding.UTF8.GetBytes(plaintext), password, keySize);
        }

        /// <summary>
        /// Encrypts plaintext using the Encrypt-then-MAC (EtM) mode via the Rijndael cipher in 
        /// CBC mode with a password derived HMAC SHA-512 salt. A random 128-bit Initialization 
        /// Vector is generated for the cipher.
        /// </summary>
        /// <param name="plaintext">The plaintext to encrypt.</param>
        /// <param name="password">The password to encrypt the plaintext with.</param>
        /// <param name="keySize">The cipher key size. 256-bit is stronger, but slower.</param>
        /// <returns>The Base64 encoded EtM ciphertext.</returns>
        public static new string Encrypt(byte[] plaintext, string password, KeySize keySize)
        {
            // Generate a random IV
            var iv = Rng.GenerateRandomBytes(InitializationVectorSize);

            // Encrypt the plaintext
            var etmCiphertext = Encrypt(plaintext, password, iv, keySize);

            // Encode the EtM ciphertext
            return Convert.ToBase64String(etmCiphertext);
        }

        /// <summary>
        /// Encrypts plaintext using the Encrypt-then-MAC (EtM) mode via the Rijndael cipher in 
        /// CBC mode with a password derived HMAC SHA-512 salt.
        /// </summary>
        /// <param name="plaintext">The plaintext to encrypt.</param>
        /// <param name="password">The password to encrypt the plaintext with.</param>
        /// <param name="iv">The initialization vector. Must be 128-bits.</param>
        /// <param name="keySize">The cipher key size. 256-bit is stronger, but slower.</param>
        /// <returns>The EtM ciphertext.</returns>
        public static new byte[] Encrypt(byte[] plaintext, string password, byte[] iv, KeySize keySize)
        {
            // Generate AE keys
            var keyRing = AeKeyRing.Generate(password);

            // Encrypt the plaintext
            var ciphertext = Rijndael.Encrypt(plaintext, keyRing.CipherKey, iv, keySize);

            // Calculate the MAC from the ciphertext
            var mac = CalculateMac(ciphertext, keyRing.MacKey);

            // Append the MAC to the ciphertext
            var etmCiphertext = new byte[ciphertext.Length + mac.Length];
            Buffer.BlockCopy(ciphertext, 0, etmCiphertext, 0, ciphertext.Length);
            Buffer.BlockCopy(mac, 0, etmCiphertext, ciphertext.Length, mac.Length);

            // IV + Cipher + MAC
            return etmCiphertext;
        }

        /// <summary>
        /// Decrypts EtM ciphertext using the Rijndael cipher in CBC mode with a password derived 
        /// HMAC SHA-512 salt.
        /// </summary>
        /// <param name="etmCiphertext">The Base64 encoded EtM ciphertext to decrypt.</param>
        /// <param name="password">The password to decrypt the EtM ciphertext with.</param>
        /// <param name="keySize">The size of the cipher key used to create the EtM ciphertext.</param>
        /// <returns>The plaintext.</returns>
        public static new string Decrypt(string etmCiphertext, string password, KeySize keySize)
        {
            return Decrypt(Convert.FromBase64String(etmCiphertext), password, keySize);
        }

        /// <summary>
        /// Decrypts authenticated ciphertext using the Rijndael cipher in CBC mode with a password derived 
        /// HMAC SHA-512 salt.
        /// </summary>
        /// <param name="etmCiphertext">The EtM ciphertext to decrypt.</param>
        /// <param name="password">The password to decrypt the EtM ciphertext with.</param>
        /// <param name="keySize">The size of the cipher key used to create the EtM ciphertext.</param>
        /// <returns>The plaintext.</returns>
        public static new string Decrypt(byte[] etmCiphertext, string password, KeySize keySize)
        {
            // Generate AE keys
            var keyRing = AeKeyRing.Generate(password);

            // Extract the ciphertext and MAC from the EtM ciphertext
            var mac = new byte[keyRing.MacKey.Length];
            var ciphertext = new byte[etmCiphertext.Length - mac.Length];
            using (var ms = new MemoryStream(etmCiphertext))
            {
                // Extract the ciphertext
                ms.Read(ciphertext, 0, ciphertext.Length);

                // Extract the MAC
                ms.Read(mac, 0, mac.Length);
            }

            // Calculate the MAC from the ciphertext
            var newMac = CalculateMac(ciphertext, keyRing.MacKey);

            // Authenticate ciphertext
            if (!mac.SequenceEqual(newMac)) throw new Exception("Authentication failed!");

            // Decrypt the ciphertext
            return Rijndael.Decrypt(ciphertext, keyRing.CipherKey, keySize);
        }

        /// <summary>
        /// Calculates the MAC for a ciphertext.
        /// </summary>
        /// <param name="ciphertext">The ciphertext.</param>
        /// <param name="key">The key.</param>
        /// <returns>The MAC.</returns>
        public static byte[] CalculateMac(byte[] ciphertext, byte[] key)
        {
            return Hash.Pbkdf2(ciphertext, key, Settings.HashIterations);
        }
    }
}
