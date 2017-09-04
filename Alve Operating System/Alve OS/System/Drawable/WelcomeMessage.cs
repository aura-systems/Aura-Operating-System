/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Welcome Message
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using Alve_OS.System.Security;
using Alve_OS.System.Computer;
using Alve_OS.System.Translation;
using Alve_OS.System.Users;

namespace Alve_OS.System
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
                    Console.WriteLine(" * Documentation: alve-os.github.io");
                    Console.ForegroundColor = ConsoleColor.White;

                    //redo that bullshit code
                    if ((Kernel.RootContent == MD5.hash("root") + "|admin") || (Kernel.ComputerName == "Alve-PC"))
                    {
                        Console.WriteLine();

                        Console.ForegroundColor = ConsoleColor.Green;
                        Text.Display("tips");

                        if (Kernel.RootContent == MD5.hash("root") + "|admin")
                        {
                            Console.WriteLine(" ");
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("   - Le mot de passe par défaut pour le compte root est 'root'");
                            Console.ForegroundColor = ConsoleColor.White;
                        }



                        if (Kernel.ComputerName == "Alve-PC")
                        {
                            Console.WriteLine(" ");
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("   - Le nom de l'ordinateur est 'Alve-PC', pensez à le changer.");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    Console.WriteLine(" ");
                    break;

                case "en_US":
                    Logo.Print();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" * Documentation: alve-os.github.io");
                    Console.ForegroundColor = ConsoleColor.White;

                    //redo that bullshit code
                    if ((Kernel.RootContent == MD5.hash("root") + "|admin") || (Kernel.ComputerName == "Alve-PC"))
                    {
                        Console.WriteLine();

                        Console.ForegroundColor = ConsoleColor.Green;
                        Text.Display("tips");

                        if (UserLevel.TypeUser() == UserLevel.Administrator())
                        {
                            if (Kernel.RootContent == MD5.hash("root") + "|admin")
                            {
                                Console.WriteLine(" ");
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.WriteLine("   * Default password for root is 'root'");
                                Console.ForegroundColor = ConsoleColor.White;
                            }

                        }

                        if (Kernel.ComputerName == "Alve-PC")
                        {
                            Console.WriteLine(" ");
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("   - Computer name is 'Alve-PC', think to change it.");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    Console.WriteLine(" ");
                    break;
            }
        }

    }
}
