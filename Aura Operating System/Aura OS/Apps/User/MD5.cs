/*
* PROJECT:          Aura Operating System Development
* CONTENT:          MD5 Hash
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System;
using Security = Aura_OS.System.Security;
using Aura_OS.System.Translation;

namespace Aura_OS.Apps.User
{
    class MD5
    {
        public static void Hash(string tohash)
        {
            string Hash = Security.MD5.hash(tohash);
            Console.WriteLine();
            Text.Display("md5");
            Console.WriteLine(" - " + Hash);
        }
        
    }
}
