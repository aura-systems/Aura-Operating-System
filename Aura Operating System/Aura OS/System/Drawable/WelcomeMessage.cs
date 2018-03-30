/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Welcome Message
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;

namespace Aura_OS.System
{
    class WelcomeMessage
    {

        /// <summary>
        /// Display the welcome message
        /// </summary>
        public static void Display()
        {
            switch (Kernel.langSelected)
            {
                case "fr_FR":
                    Logo.Print();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" * Documentation: aura-team.com");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(" ");
                    break;

                case "en_US":
                    Logo.Print();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" * Documentation: aura-team.com");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(" ");
                    break;

                case "nl_NL":
                    Logo.Print();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" * Documentatie: aura-team.com");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(" ");
                    break;
            }
        }

    }
}
