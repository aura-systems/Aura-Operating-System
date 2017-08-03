/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Welcome Message
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Alve_OS.System.Security;
using Alve_OS.System.Computer;

namespace Alve_OS.System
{
    class WelcomeMessage
    {

        /// <summary>
        /// Affiche le message de bienvenue
        /// </summary>
        public static void Display()
        {
            switch (Kernel.langSelected)
            {
                case "fr_FR":

                    Console.WriteLine(" Bienvenue sur Alve " + Kernel.version + " (" + Info.getComputerName() + ")");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" * Documentation:    http://alve.geomtech.fr/docs");
                    Console.WriteLine(Info.getAmountOfRAM());
                    Console.ForegroundColor = ConsoleColor.White;
                    if (File.Exists(@"0:\Users\root.usr"))
                    {
                        string RootPassword = File.ReadAllText(@"0:\System\Users\root.usr");
                        if (RootPassword == MD5.hash("root"))
                        {
                            Console.WriteLine(" ");
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine(" * Le mot de passe par défaut pour le compte root est 'root'");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    Console.WriteLine(" Créé par Valentin CHARBONNIER (valentinbreiz) et Alexy DA CRUZ (GeomTech).");
                    Console.WriteLine(" ");
                    break;
                     
                case "en_US":

                    Console.WriteLine(" Welcome to Alve " + Kernel.version + " (" + Info.getComputerName() + ")");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" * Documentation:    http://alve.geomtech.fr/docs");
                    Console.ForegroundColor = ConsoleColor.White;
                    if (File.Exists("0:\\Users\\root.usr"))
                    {
                        string RootPassword = File.ReadAllText("0:\\Users\\root.usr");
                        if (RootPassword == "root")
                        {
                            Console.WriteLine(" ");
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine(" * Default password for root is 'root'");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    Console.WriteLine(" Made by Valentin CHARBONNIER (valentinbreiz) and Alexy DA CRUZ (GeomTech).");
                    Console.WriteLine(" ");
                    break;
            }
        }
    }
}
