/*
 * Rijndael256
 * Copyright (C)2013 2Toad, LLC.
 * licensing@2toad.com
 * 
 * https://github.com/2Toad/Rijndael256
 */

namespace Rijndael256
{
    /// <summary>
    /// A collection of mutable defaults
    /// </summary>
    public static class Settings
    {
        static Settings()
        {
            // Set defaults during initialization
            Reset();
        }

        /// <summary>
        /// Resets all the settings to their default values
        /// </summary>
        public static void Reset()
        {
            HashIterations = _hashIterations;
        }

        /// <summary>
        /// The number of iterations used to derive hashes.
        /// Default is 10000.
        /// </summary>
        public static int HashIterations;

        private const int _hashIterations = 10000;
    }
}
