/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Kernel
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Alve_OS.System
{
    class Strings
    {

        public static void Display(string lang, string ToTranslate)
        {
            switch (lang)
            {
                case "fr_FR":

                    switch (ToTranslate)
                    {
                        case "shutdown":
                            Console.WriteLine("Extinction en cours...");
                            break;
                    }

                    break;

                case "en_US":

                    switch (ToTranslate)
                    {
                        case "shutdown":
                            Console.WriteLine("Shutting Down...");
                            break;
                    }

                    break;
            }
        }

    }
}
