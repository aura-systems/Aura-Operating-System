/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Translation system
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System;

namespace Alve_OS.System.Translation
{
    class Text
    {

        /*
         * ## Utilisation des mots clés:
         * 
         * Les mots clés remplace une phrase
         * par leur traduction correspondant
         * à la langue du système. Vous devez donc
         * associer un mot clé à une phrase.
         */

        public static void Display(string ToTranslate, string arg = "")
        {
            switch (Kernel.langSelected)
            {
                case "fr_FR":

                    switch (ToTranslate)
                    {
                        case "shutdown": //Ceci est un mot clé
                            Console.WriteLine("Extinction en cours..."); //Ceci est une phrase traduite
                            break;
                        case "keyboard":
                            Console.WriteLine("Initialisation clavier FR...");
                            break;
                        case "restart": 
                            Console.WriteLine("Redémarrage en cours..."); 
                            break;
                        case "directorydoesntexist":
                            Console.WriteLine("Ce répertoire n'existe pas !");
                            break;
                        case "typename":
                            Console.WriteLine("Type\t Nom");
                            break;
                        case "doesnotexist":
                            Console.WriteLine(arg + " n'existe pas !");
                            break;
                        case "alreadyexist":
                            Console.WriteLine(arg + " existe déjà !");
                            break;
                        case "NameSizeParent":
                            Console.WriteLine("Nom\tTaille\tParent");
                            break;
                        case "OSName":
                            Console.Write("Nom du système d'exploitation:     Alve");
                            break;
                        case "OSVersion":
                            Console.Write("Version du système:                " + Kernel.version);
                            break;
                        case "OSRevision":
                            Console.Write("Révision du système:               " + Kernel.revision);
                            break;
                        case "UnknownCommand":
                            Console.WriteLine("Commande inconnue.");
                            break;
                    }
                    break;

                case "en_US":

                    switch (ToTranslate)
                    {
                        case "shutdown":
                            Console.WriteLine("Shutting Down...");
                            break;
                        case "keyboard":
                            Console.WriteLine("Keyboard initialisation EN...");
                            break;
                        case "restart":
                            Console.WriteLine("Restarting...");
                            break;
                        case "directorydoesntexist":
                            Console.WriteLine("This directory doesn't exist!");
                            break;
                        case "typename":
                            Console.WriteLine("Type\t Name");
                            break;
                        case "doesnotexist":
                            Console.WriteLine(arg + " does not exist!");
                            break;
                        case "alreadyexist":
                            Console.WriteLine(arg + " already exist!");
                            break;
                        case "NameSizeParent":
                            Console.WriteLine("Name\tSize\tParent");
                            break;
                        case "OSName":
                            Console.WriteLine("Operating system name:     Alve");
                            break;
                        case "OSVersion":
                            Console.WriteLine("Operating system version:  " + Kernel.version);
                            break;
                        case "OSRevision":
                            Console.WriteLine("Operating system revision: " + Kernel.revision);
                            break;
                        case "UnknownCommand":
                            Console.WriteLine("Unknown command.");
                            break;
                    }
                    break;
            }
        }
    }
}
