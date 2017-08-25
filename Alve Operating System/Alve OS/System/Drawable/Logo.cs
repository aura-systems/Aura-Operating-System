/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Logo
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;

namespace Alve_OS.System
{
    class Logo
    {
        /// <summary>
        /// Display Logo of Alve
        /// </summary>
        public static void Print()
        {
            switch (Kernel.langSelected)
            {
                case "fr_FR":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($@"
  █████╗ ██╗    ██╗   ██╗███████╗
 ██╔══██╗██║    ██║   ██║██╔════╝
 ███████║██║    ██║   ██║█████╗   créé par Valentin Charbonnier
 ██╔══██║██║    ╚██╗ ██╔╝██╔══╝         et Alexy DA CRUZ
 ██║  ██║███████╗╚████╔╝ ███████╗
 ╚═╝  ╚═╝╚══════╝ ╚═══╝  ╚══════╝ v{ Kernel.version }
");
                    break;

                case "en_US":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($@"
  █████╗ ██╗    ██╗   ██╗███████╗
 ██╔══██╗██║    ██║   ██║██╔════╝
 ███████║██║    ██║   ██║█████╗   created by Valentin Charbonnier
 ██╔══██║██║    ╚██╗ ██╔╝██╔══╝          and Alexy DA CRUZ
 ██║  ██║███████╗╚████╔╝ ███████╗
 ╚═╝  ╚═╝╚══════╝ ╚═══╝  ╚══════╝ v{ Kernel.version }
");
                    break;
            }
        }
    }
}
