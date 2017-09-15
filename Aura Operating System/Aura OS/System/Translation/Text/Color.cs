/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Help colours
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;

namespace Aura_OS.System.Translation
{
    class Color
    {

        /// <summary>
        /// Display help on color
        /// </summary>
        public static void Display()
        {
            switch (Kernel.langSelected)
            {
                case "fr_FR":
                    Console.WriteLine("Liste des couleurs disponible pour la console.");
                    Console.WriteLine("0 = Noir");
                    Console.WriteLine("1 = Bleu");
                    Console.WriteLine("2 = Vert");
                    Console.WriteLine("3 = Bleu foncé");
                    Console.WriteLine("4 = Rouge");
                    Console.WriteLine("5 = Violet");
                    Console.WriteLine("6 = Jaune");
                    Console.WriteLine("7 = Blanc");
                    break;

                case "en_US":
                    Console.WriteLine("List of the available colors for the console.");
                    Console.WriteLine("0 = Black");
                    Console.WriteLine("1 = Blue");
                    Console.WriteLine("2 = Green");
                    Console.WriteLine("3 = Dark blue");
                    Console.WriteLine("4 = Red");
                    Console.WriteLine("5 = Purple");
                    Console.WriteLine("6 = Yellow");
                    Console.WriteLine("7 = White");
                    break;
            }
        }

    }
}
