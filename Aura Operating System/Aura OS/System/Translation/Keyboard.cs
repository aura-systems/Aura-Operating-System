/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Keyboard initialization
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System;
using Sys = Cosmos.System;

namespace Aura_OS.System.Translation
{
    class Keyboard
    {

        /// <summary>
        /// Init the keyboard with the user language
        /// </summary>
        public static void Init()
        {
            try
            {
                switch (Kernel.langSelected)
                {
                    case "fr_FR":
                        Sys.KeyboardManager.SetKeyLayout(new Sys.ScanMaps.FR_Standard());
                        break;

                    case "en_US":
                        Sys.KeyboardManager.SetKeyLayout(new Sys.ScanMaps.US_Standard());
                        break;

                    case "nl_NL":
                        Sys.KeyboardManager.SetKeyLayout(new Sys.ScanMaps.US_Standard());
                        break;
                }
            }
            catch
            {
                Console.WriteLine("[ERROR]");
            }  
        }

    }
}
