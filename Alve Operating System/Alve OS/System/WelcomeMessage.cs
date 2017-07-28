using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

                    Console.WriteLine(" ");
                    Console.WriteLine(" Bienvenue sur Alve " + Kernel.version + " (" + Kernel.revision + ")");
                    Console.WriteLine(" ");
                    Console.WriteLine("   * Documentation: http://alveos.geomtech.fr/docs");
                    if (File.Exists(@"0:\Users\root.usr"))
                    {
                        string RootPassword = File.ReadAllText(@"0:\Users\root.usr");
                        if (RootPassword == "root")
                        {
                            Console.WriteLine(" ");
                            Console.WriteLine("   * Le mot de passe par défaut pour le compte root est 'root'");
                        }
                    }
                    Console.WriteLine(" ");
                    Console.WriteLine("Créer par Valentin CHARBONNIER (valentinbreiz) et Alexy DA CRUZ (GeomTech).");
                    Console.WriteLine(" ");
                    break;

                case "en_US":

                    Console.WriteLine(" ");
                    Console.WriteLine(" Welcome to Alve " + Kernel.version + " (" + Kernel.revision + ")");
                    Console.WriteLine(" ");
                    Console.WriteLine("   * Documentation: http://alveos.geomtech.fr/docs");
                    if (File.Exists("0:\\Users\\root.usr"))
                    {
                        string RootPassword = File.ReadAllText("0:\\Users\\root.usr");
                        if (RootPassword == "root")
                        {
                            Console.WriteLine(" ");
                            Console.WriteLine("   * Default password for root is 'root'");
                        }
                    }
                    Console.WriteLine(" ");
                    Console.WriteLine("Made by Valentin CHARBONNIER (valentinbreiz) and Alexy DA CRUZ (GeomTech).");
                    Console.WriteLine(" ");

                    break;
            }


        }
    }
}
