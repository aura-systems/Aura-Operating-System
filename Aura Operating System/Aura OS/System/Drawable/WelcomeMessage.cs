/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Welcome Message
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using Aura_OS.System.Security;
using Aura_OS.System.Computer;
using Aura_OS.System.Translation;
using Aura_OS.System.Users;

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

                    //redo that bullshit code
                    //if ((Kernel.RootContent == MD5.hash("root") + "|admin") || (Kernel.ComputerName == "aura-pc"))
                    //{
                    //    Console.WriteLine();
                    //
                    //    Console.ForegroundColor = ConsoleColor.Green;
                    //    Text.Display("tips");
                    //
                     //   if (Kernel.RootContent == MD5.hash("root") + "|admin")
                     //   {
                     //       Console.WriteLine(" ");
                     //       Console.ForegroundColor = ConsoleColor.Blue;
                     //       Console.WriteLine("   - Le mot de passe par défaut pour le compte root est 'root'");
                     //       Console.ForegroundColor = ConsoleColor.White;
                     //   }
                     //
                     //   if (Kernel.ComputerName == "aura-pc")
                     //   {
                     //       Console.WriteLine(" ");
                      //      Console.ForegroundColor = ConsoleColor.Blue;
                      //      Console.WriteLine("   - Le nom de l'ordinateur est 'aura-pc', pensez à le changer.");
                      //      Console.ForegroundColor = ConsoleColor.White;
                      //  }
                    //}
                    Console.WriteLine(" ");
                    break;

                case "en_US":
                    Logo.Print();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" * Documentation: aura-team.com");
                    Console.ForegroundColor = ConsoleColor.White;

                    //redo that bullshit code
                    //if ((Kernel.RootContent == MD5.hash("root") + "|admin") || (Kernel.ComputerName == "Aura-PC"))
                    //{
                    //    Console.WriteLine();
                     //
                    //    Console.ForegroundColor = ConsoleColor.Green;
                     //   Text.Display("tips");
                     //
                     //   if (Kernel.RootContent == MD5.hash("root") + "|admin")
                     //   {
                     //       Console.WriteLine(" ");
                     //       Console.ForegroundColor = ConsoleColor.Blue;
                     //       Console.WriteLine("   * Default password for root is 'root'");
                     //       Console.ForegroundColor = ConsoleColor.White;
                      //  }
                      //
                     //   if (Kernel.ComputerName == "aura-pc")
                      //  {
                      //      Console.WriteLine(" ");
                      //      Console.ForegroundColor = ConsoleColor.Blue;
                      //      Console.WriteLine("   - Computer name is 'aura-pc', think to change it.");
                      //      Console.ForegroundColor = ConsoleColor.White;
                      //  }
                     // }
                    Console.WriteLine(" ");
                    break;
            }
        }

    }
}
