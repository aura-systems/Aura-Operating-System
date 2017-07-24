/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Help colours
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;

namespace Alve_OS.System.Translation
{
    class Color
    {
        public static void Display()
        {
            switch (Kernel.langSelected)
            {
                case "fr_FR":
                    Console.WriteLine("Change les couleurs par d" + ASCII.eaigu + "faut du premier plan de la console.");
                    Console.WriteLine("0 = Noir");
                    Console.WriteLine("1 = Bleu");
                    Console.WriteLine("2 = Vert");
                    Console.WriteLine("3 = Bleu fonc" + ASCII.eaigu);
                    Console.WriteLine("4 = Rouge");
                    Console.WriteLine("5 = Violet");
                    Console.WriteLine("6 = Jaune");
                    Console.WriteLine("7 = Blanc");
                    break;

                case "en_US":
                    Console.WriteLine("Changes the default colors of the foreground of the console.");
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
