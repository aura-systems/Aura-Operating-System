/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Welcome Message
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using Alve_OS.System.Security;

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
                    Logo.Print();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" * Documentation: github.com/Alve-OS/Alve-Operating-System/wiki");
                    Console.ForegroundColor = ConsoleColor.White;
                    if (File.Exists(@"0:\Users\root.usr"))
                    {
                        string RootPassword = File.ReadAllText(@"0:\System\Users\root.usr");
                        if (RootPassword == MD5.hash("root"))
                        {
                            Console.WriteLine(" ");
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("* Le mot de passe par défaut pour le compte root est 'root'");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    Console.WriteLine(" ");
                    break;
                     
                case "en_US":
                    Logo.Print();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" * Documentation: github.com/Alve-OS/Alve-Operating-System/wiki");
                    Console.ForegroundColor = ConsoleColor.White;
                    if (File.Exists("0:\\Users\\root.usr"))
                    {
                        string RootPassword = File.ReadAllText("0:\\Users\\root.usr");
                        if (RootPassword == "root")
                        {
                            Console.WriteLine(" ");
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("   * Default password for root is 'root'");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    Console.WriteLine(" ");
                    break;
            }
        }
    }
}
