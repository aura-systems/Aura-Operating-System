/*
 * Rijndael256
 * Copyright (C)2013 2Toad, LLC.
 * licensing@2toad.com
 * 
 * https://github.com/2Toad/Rijndael256
 */

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Rijndael256
{
    /// <summary>
    /// AES implementation of the Rijndael symmetric-key cipher.
    /// </summary>
    public class Rijndael
    {
        internal const int InitializationVectorSize = 16;
        internal const CipherMode BlockCipherMode = CipherMode.CBC;

        /// <summary>
        /// Encrypts plaintext using the Rijndael cipher in CBC mode with a password derived HMAC SHA-512 salt.
        /// A random 128-bit Initialization Vector is generated for the cipher.
        /// </summary>
        /// <param name="plaintext">The plaintext to encrypt.</param>
        /// <param name="password">The password to encrypt the plaintext with.</param>
        /// <param name="keySize">The cipher key size. 256-bit is stronger, but slower.</param>
        /// <returns>The Base64 encoded ciphertext.</returns>
        public static string Encrypt(string plaintext, string password, KeySize keySize)
        {
            return Encrypt(Encoding.UTF8.GetBytes(plaintext), password, keySize);
        }

        /// <summary>
        /// Encrypts plaintext using the Rijndael cipher in CBC mode with a password derived HMAC SHA-512 salt.
        /// A random 128-bit Initialization Vector is generated for the cipher.
        /// </summary>
        /// <param name="plaintext">The plaintext to encrypt.</param>
        /// <param name="password">The password to encrypt the plaintext with.</param>
        /// <param name="keySize">The cipher key size. 256-bit is stronger, but slower.</param>
        /// <returns>The Base64 encoded ciphertext.</returns>
        public static string Encrypt(byte[] plaintext, string password, KeySize keySize)
        {
            // Generate a random IV
            var iv = Rng.GenerateRandomBytes(InitializationVectorSize);

            // Encrypt the plaintext
            var ciphertext = Encrypt(plaintext, password, iv, keySize);

            // Encode the ciphertext
            return Convert.ToBase64String(ciphertext);
        }

        /// <summary>
        /// Encrypts plaintext using the Rijndael cipher in CBC mode with a password derived HMAC SHA-512 salt.
        /// </summary>
        /// <param name="plaintext">The plaintext to encrypt.</param>
        /// <param name="password">The password to encrypt the plaintext with.</param>
        /// <param name="iv">The initialization vector. Must be 128-bits.</param>
        /// <param name="keySize">The cipher key size. 256-bit is stronger, but slower.</param>
        /// <returns>The ciphertext.</returns>
        public static byte[] Encrypt(byte[] plaintext, string password, byte[] iv, KeySize keySize)
        {
            if (iv.Length != InitializationVectorSize) throw new ArgumentOutOfRangeException(nameof(iv), "AES requires an Initialization Vector of 128-bits.");

            byte[] ciphertext;
            using (var ms = new MemoryStream())
            {
                // Insert IV at beginning of ciphertext
                ms.Write(iv, 0, iv.Length);

                // Create a CryptoStream to encrypt the plaintext
                using (var cs = new CryptoStream(ms, CreateEncryptor(password, iv, keySize), CryptoStreamMode.Write))
                {
                    // Encrypt the plaintext
                    cs.Write(plaintext, 0, plaintext.Length);
                    cs.FlushFinalBlock();
                }

                ciphertext = ms.ToArray();
            }

            // IV + Cipher
            return ciphertext;
        }

        /// <summary>
        /// Encrypts a plaintext file using the Rijndael cipher in CBC mode with a password derived HMAC SHA-512 salt.
        /// A random 128-bit Initialization Vector is generated for the cipher.
        /// </summary>
        /// <param name="plaintextFile">The plaintext file to encrypt.</param>
        /// <param name="ciphertextFile">The resulting ciphertext file.</param>
        /// <param name="password">The password to encrypt the plaintext file with.</param>
        /// <param name="keySize">The cipher key size. 256-bit is stronger, but slower.</param>
        public static void Encrypt(string plaintextFile, string ciphertextFile, string password, KeySize keySize)
        {
            // Create a new ciphertext file to write the ciphertext to
            using (var fsc = new FileStream(ciphertextFile, FileMode.Create, FileAccess.Write))
            {
                // Store the IV at the beginning of the ciphertext file
                var iv = Rng.GenerateRandomBytes(InitializationVectorSize);
                fsc.Write(iv, 0, iv.Length);

                // Create a CryptoStream to encrypt the plaintext
                using (var cs = new CryptoStream(fsc, CreateEncryptor(password, iv, keySize), CryptoStreamMode.Write))
                {
                    // Open the plaintext file
                    using (var fsp = new FileStream(plaintextFile, FileMode.Open, FileAccess.Read))
                    {
                        // Create a buffer to process the plaintext file in chunks
                        // Reading the whole file into memory can cause 
                        // Out of Memory exceptions if the file is large
                        var buffer = new byte[4096];

                        // Read a chunk from the plaintext file
                        int bytesRead;
                        while ((bytesRead = fsp.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            // Encrypt the plaintext and write it to the ciphertext file
                            cs.Write(buffer, 0, bytesRead);
                        }

                        // Finalize encryption
                        cs.FlushFinalBlock();
                    }
                }
            }
        }

        /// <summary>
        /// Decrypts ciphertext using the Rijndael cipher in CBC mode with a password derived HMAC SHA-512 salt.
        /// </summary>
        /// <param name="ciphertext">The Base64 encoded ciphertext to decrypt.</param>
        /// <param name="password">The password to decrypt the ciphertext with.</param>
        /// <param name="keySize">The size of the cipher key used to create the ciphertext.</param>
        /// <returns>The plaintext.</returns>
        public static string Decrypt(string ciphertext, string password, KeySize keySize)
        {
            return Decrypt(Convert.FromBase64String(ciphertext), password, keySize);
        }

        /// <summary>
        /// Decrypts ciphertext using the Rijndael cipher in CBC mode with a password derived HMAC SHA-512 salt.
        /// </summary>
        /// <param name="ciphertext">The ciphertext to decrypt.</param>
        /// <param name="password">The password to decrypt the ciphertext with.</param>
        /// <param name="keySize">The size of the cipher key used to create the ciphertext.</param>
        /// <returns>The plaintext.</returns>
        public static string Decrypt(byte[] ciphertext, string password, KeySize keySize)
        {
            using (var ms = new MemoryStream(ciphertext))
            {
                // Extract the IV from the ciphertext
                var iv = new byte[InitializationVectorSize];
                ms.Read(iv, 0, iv.Length);

                // Create a CryptoStream to decrypt the ciphertext
                using (var cs = new CryptoStream(ms, CreateDecryptor(password, iv, keySize), CryptoStreamMode.Read))
                {
                    // Decrypt the ciphertext
                    using (var sr = new StreamReader(cs, Encoding.UTF8)) return sr.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Decrypts ciphertext using the Rijndael cipher in CBC mode with a password derived HMAC SHA-512 salt.
        /// </summary>
        /// <param name="ciphertextFile">The ciphertext file to decrypt.</param>
        /// <param name="plaintextFile">The resulting plaintext file.</param>
        /// <param name="password">The password to decrypt the ciphertext file with.</param>
        /// <param name="keySize">The size of the cipher key used to create the ciphertext file.</param>
        public static void Decrypt(string ciphertextFile, string plaintextFile, string password, KeySize keySize)
        {
            // Open the ciphertext file
            using (var fsc = new FileStream(ciphertextFile, FileMode.Open, FileAccess.Read))
            {
                // Read the IV from the beginning of the ciphertext file
                var iv = new byte[InitializationVectorSize];
                fsc.Read(iv, 0, iv.Length);

                // Create a new plaintext file to write the plaintext to
                using (var fsp = new FileStream(plaintextFile, FileMode.Create, FileAccess.Write))
                {
                    // Create a CryptoStream to decrypt the ciphertext
                    using (var cs = new CryptoStream(fsp, CreateDecryptor(password, iv, keySize), CryptoStreamMode.Write))
                    {
                        // Create a buffer to process the plaintext file in chunks
                        // Reading the whole file into memory can cause 
                        // Out of Memory exceptions if the file is large
                        var buffer = new byte[4096];

                        // Read a chunk from the ciphertext file
                        int bytesRead;
                        while ((bytesRead = fsc.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            // Decrypt the ciphertext and write it to the plaintext file
                            cs.Write(buffer, 0, bytesRead);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generates a cryptographic key from a password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="keySize">The cipher key size. 256-bit is stronger, but slower.</param>
        /// <returns>The cryptographic key.</returns>
        public static byte[] GenerateKey(string password, KeySize keySize)
        {
            // Create a salt to help prevent rainbow table attacks
            var salt = Hash.Pbkdf2(password, Hash.Sha512(password + password.Length), Settings.HashIterations);

            // Generate a key from the password and salt
            return Hash.Pbkdf2(password, salt, Settings.HashIterations, (int)keySize / 8);
        }

        /// <summary>
        /// Creates a symmetric Rijndael encryptor.
        /// </summary>
        /// <param name="password">The password to encrypt the plaintext with.</param>
        /// <param name="iv">The initialization vector. Must be 128-bits.</param>
        /// <param name="keySize">The cipher key size. 256-bit is stronger, but slower.</param>
        /// <returns>The symmetric encryptor.</returns>
        public static ICryptoTransform CreateEncryptor(string password, byte[] iv, KeySize keySize)
        {
#if NET452
            var rijndael = new RijndaelManaged { Mode = BlockCipherMode };
#else
            var rijndael = Aes.Create();
            rijndael.Mode = BlockCipherMode;
#endif
            return rijndael.CreateEncryptor(GenerateKey(password, keySize), iv);
        }

        /// <summary>
        /// Creates a symmetric Rijndael decryptor.
        /// </summary>
        /// <param name="password">The password to decrypt the ciphertext with.</param>
        /// <param name="iv">The initialization vector. Must be 128-bits.</param>
        /// <param name="keySize">The cipher key size.</param>
        /// <returns>The symmetric decryptor.</returns>
        public static ICryptoTransform CreateDecryptor(string password, byte[] iv, KeySize keySize)
        {
#if NET452
            var rijndael = new RijndaelManaged { Mode = BlockCipherMode };
#else
            var rijndael = Aes.Create();
            rijndael.Mode = BlockCipherMode;
#endif
            return rijndael.CreateDecryptor(GenerateKey(password, keySize), iv);
        }
    }
}
