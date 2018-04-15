/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Logo
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;

namespace Aura_OS.System
{
    class Logo
    {

        /// <summary>
        /// Display Logo of Aura
        /// </summary>
        public static void Print()
        {
            switch (Kernel.langSelected)
            {
                case "fr_FR":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($@"
  █████╗ ██╗   ██╗██████╗  █████╗
 ██╔══██╗██║   ██║██╔══██╗██╔══██╗
 ███████║██║   ██║██████╔╝███████║
 ██╔══██║██║   ██║██╔══██╗██╔══██║ 
 ██║  ██║╚██████╔╝██║  ██║██║  ██║ créé par Aura Team
 ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝ v{ Kernel.version }
");
                    break;

                case "en_US":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($@"
  █████╗ ██╗   ██╗██████╗  █████╗
 ██╔══██╗██║   ██║██╔══██╗██╔══██╗
 ███████║██║   ██║██████╔╝███████║ 
 ██╔══██║██║   ██║██╔══██╗██╔══██║ 
 ██║  ██║╚██████╔╝██║  ██║██║  ██║ created by Aura Team
 ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝ v{ Kernel.version }
");
                    break;

                case "nl_NL":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($@"
  █████╗ ██╗   ██╗██████╗  █████╗
 ██╔══██╗██║   ██║██╔══██╗██╔══██╗
 ███████║██║   ██║██████╔╝███████║ 
 ██╔══██║██║   ██║██╔══██╗██╔══██║ 
 ██║  ██║╚██████╔╝██║  ██║██║  ██║ ontwikkeld door Aura Team
 ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝ v{ Kernel.version }
");
                    break;
            }
        }

    }
}
