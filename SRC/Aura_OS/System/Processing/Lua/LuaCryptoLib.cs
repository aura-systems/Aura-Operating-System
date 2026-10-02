/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Lua cosmos.crypto library
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.IO;
using System.Text;
using Cosmos.Executable.Lua;
using Aura_OS.System.Filesystem;

namespace Aura_OS.System.Processing.Lua
{
    /// <summary>
    /// cosmos.crypto: string and file hashes for Lua scripts (port of gen2's LuaCosmosCryptoLib).
    /// Hashes are uppercase hex strings, as in gen2.
    /// </summary>
    internal static class LuaCryptoLib
    {
        public const string LIB_NAME = "cosmos.crypto";

        /// <summary>
        /// Register cosmos.crypto in package.loaded and as the global "cosmos.crypto".
        /// </summary>
        /// <param name="lua">Interpreter whose WorkingDirectory the file functions resolve against.</param>
        public static void Register(LuaInterpreter lua)
        {
            // GEN3-GAP(lua-host): a C# callback only receives ILuaState, so the file functions
            // capture the interpreter to read its WorkingDirectory.
            lua.State.L_RequireF(LIB_NAME, l =>
            {
                NameFuncPair[] define = new NameFuncPair[]
                {
                    new NameFuncPair("strtomd5", CRYPTO_md5),
                    new NameFuncPair("strtosha256", CRYPTO_sha256),
                    new NameFuncPair("strtosha512", CRYPTO_sha512),
                    new NameFuncPair("filetomd5", s => FILE_CRYPTO_md5(s, lua)),
                    new NameFuncPair("filetosha256", s => FILE_CRYPTO_sha256(s, lua)),
                    new NameFuncPair("filetosha512", s => FILE_CRYPTO_sha512(s, lua)),
                };

                l.L_NewLib(define);
                return 1;
            }, true);

            // L_RequireF leaves the library table on the stack
            lua.State.Pop(1);
        }

        private static int CRYPTO_md5(ILuaState lua)
        {
            string input = lua.L_CheckString(1);
            lua.PushString(Aura_OS.System.Security.MD5.hash(input));
            return 1;
        }

        private static int CRYPTO_sha256(ILuaState lua)
        {
            string input = lua.L_CheckString(1);
            lua.PushString(Aura_OS.System.Security.Sha256.hash(Encoding.UTF8.GetBytes(input)));
            return 1;
        }

        private static int CRYPTO_sha512(ILuaState lua)
        {
            string input = lua.L_CheckString(1);
            lua.PushString(Sha512Hex(Encoding.UTF8.GetBytes(input)));
            return 1;
        }

        // GEN3-GAP(finally): file functions read the whole file instead of hashing a stream: a stream left open by an
        // exception is never closed on gen3 (no finally on the exception path, no finalizers).
        // An exception thrown here (FileNotFoundException...) becomes a Lua error that pcall can catch.

        private static int FILE_CRYPTO_md5(ILuaState lua, LuaInterpreter interpreter)
        {
            byte[] file = File.ReadAllBytes(ResolvePath(interpreter, lua.L_CheckString(1)));
            acryptohashnet.MD5 hashAlgorithm = new acryptohashnet.MD5();
            byte[] hashBytes = hashAlgorithm.ComputeHash(file);
            hashAlgorithm.Dispose();
            lua.PushString(Aura_OS.Utils.Conversion.Hex(hashBytes));
            return 1;
        }

        private static int FILE_CRYPTO_sha256(ILuaState lua, LuaInterpreter interpreter)
        {
            byte[] file = File.ReadAllBytes(ResolvePath(interpreter, lua.L_CheckString(1)));
            lua.PushString(Aura_OS.System.Security.Sha256.hash(file));
            return 1;
        }

        private static int FILE_CRYPTO_sha512(ILuaState lua, LuaInterpreter interpreter)
        {
            byte[] file = File.ReadAllBytes(ResolvePath(interpreter, lua.L_CheckString(1)));
            lua.PushString(Sha512Hex(file));
            return 1;
        }

        private static string Sha512Hex(byte[] data)
        {
            acryptohashnet.SHA512 hashAlgorithm = new acryptohashnet.SHA512();
            byte[] hashBytes = hashAlgorithm.ComputeHash(data);
            hashAlgorithm.Dispose();
            return Aura_OS.Utils.Conversion.Hex(hashBytes);
        }

        /// <summary>
        /// Resolve a script-supplied file name as the Lua io library does: relative to the
        /// interpreter's WorkingDirectory. gen2 "0:\..." names are converted first.
        /// </summary>
        private static string ResolvePath(LuaInterpreter interpreter, string path)
        {
            path = AuraPath.FromLegacy(path);

            string workingDirectory = interpreter.WorkingDirectory;

            if (workingDirectory != null && !Path.IsPathRooted(path))
            {
                return Path.Combine(workingDirectory, path);
            }

            return path;
        }
    }
}
