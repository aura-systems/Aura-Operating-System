/*
* PROJECT:          Aura Operating System Development
* CONTENT:          MD5 Hash
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System;
using Security = Aura_OS.System.Security;
using Aura_OS.System.Translation;
using System.Collections.ObjectModel;
using Aura_OS.System.Security;

namespace Aura_OS.Apps.User
{
    class CryptoTool
    {
        public static void HashMD5(string tohash)
        {
            string Hash = Security.MD5.hash(tohash);
            Console.WriteLine();
            Text.Display("md5");
            Console.WriteLine(" - " + Hash);
        }

        public static void HashSHA256(string tohash)
        {
            string Hash = Security.Sha256.hash(tohash);
            Console.WriteLine();
            Text.Display("SHA256");
            Console.WriteLine(" - " + Hash);
        }

    }
}
