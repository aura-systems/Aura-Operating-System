using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SshDotNet
{
    public static class CryptoExtensions
    {
        public static byte[] Encrypt(this SymmetricAlgorithm algorithm, byte[] input)
        {
            // Create memory stream to which to write encrypted data.
            using (var memoryStream = new MemoryStream())
            {
                //// Ensure that crypto operations are thread-safe.
                //lock (algorithm)
                //{
                using (var transform = algorithm.CreateEncryptor())
                {
                    var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);

                    // Write input data to crypto stream.
                    cryptoStream.Write(input, 0, input.Length);
                    cryptoStream.FlushFinalBlock();

                    // Return encrypted data.
                    return memoryStream.ToArray();
                }
                //}
            }
        }

        public static byte[] Decrypt(this SymmetricAlgorithm algorithm, byte[] input)
        {
            // Create memory stream to which from read decrypted data.
            using (var memoryStream = new MemoryStream(input))
            {
                //// Ensure that crypto operations are thread-safe.
                //lock (algorithm)
                //{
                using (var transform = algorithm.CreateDecryptor())
                {
                    var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);

                    // Read output data to crypto stream.
                    byte[] output = new byte[input.Length];
                    int outputLength = cryptoStream.Read(output, 0, output.Length);

                    Array.Resize(ref output, outputLength);

                    // Return decrypted data.
                    return output;
                }
                //}
            }
        }

        public static int GetNumber(this RNGCryptoServiceProvider rng, int min, int max)
        {
            // Return random integer between min and max (excluding max).
            return (int)(min + rng.GetNumber() * (max - min));
        }

        public static double GetNumber(this RNGCryptoServiceProvider rng)
        {
            // Return random double between 0 and 1.
            byte[] randBytes = new byte[8];
            rng.GetBytes(randBytes);
            return (double)BitConverter.ToUInt32(randBytes, 0) / uint.MaxValue;
        }
    }
}
