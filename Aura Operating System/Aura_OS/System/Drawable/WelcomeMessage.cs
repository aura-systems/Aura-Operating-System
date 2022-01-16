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
            if (Global.AConsole.Type == AConsole.ConsoleType.Graphical)
            {
                Logo.Print();
            }
            else
            {
                Logo.PrintText();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            switch (Global.langSelected)
            {
                case "fr_FR":
                    Console.WriteLine(" * Documentation: github.com/aura-systems");
                    break;

                case "en_US":
                    Console.WriteLine(" * Documentation: github.com/aura-systems");
                    break;

                case "nl_NL":
                    Console.WriteLine(" * Documentatie: github.com/aura-systems");
                    break;

                case "it_IT":
                    Console.WriteLine(" * Documentazione: github.com/aura-systems");
                    break;
                
                case "pl_PL":
                    Console.WriteLine(" * Dokumentacja: github.com/aura-systems");
                    break;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" ");
        }

    }
}
