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
                            Console.WriteLine("Ce répertoire n'éxiste pas !");
                            break;
                        case "typename":
                            Console.WriteLine("Type\t Nom");
                            break;
                        case "doesnotexist":
                            Console.WriteLine(arg + " n'existe pas !");
                            break;
                        case "alreadyexist":
                            Console.WriteLine(arg + " existe deja !");
                            break;
                        case "NameSizeParent":
                            Console.WriteLine("Nom\tTaille\tParent");
                            break;
                        case "OSName":
                            Console.WriteLine("Nom du système d'exploitation: Alve");
                            break;
                        case "OSVersion":
                            Console.WriteLine("Version du système:            " + Kernel.version);
                            break;
                        case "OSRevision":
                            Console.WriteLine("Révision du système:           " + Kernel.revision);
                            break;
                        case "UnknownCommand":
                            Console.WriteLine("Commande inconnue.");
                            break;
                        case "unknownlanguage":
                            Console.WriteLine("Langue inconnue.");
                            break;
                        case "availablelanguage":
                            Console.WriteLine("Langues disponibles: en_US fr_FR");
                            break;
                        case "unknowncolor":
                            Console.WriteLine("Couleur inconnue.");
                            break;
                        case "logged":
                            Console.WriteLine("Vous êtes connecté en tant que " + arg + ".");
                            break;
                        case "unknownuser":
                            Console.WriteLine("Utilisateur inconnu.");
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
                        case "unknownlanguage":
                            Console.WriteLine("Unknown language.");
                            break;
                        case "availablelanguage":
                            Console.WriteLine("Available languages: en_US fr_FR");
                            break;
                        case "unknowncolor":
                            Console.WriteLine("Unknown colour.");
                            break;
                        case "logged":
                            Console.WriteLine("You are logged in " + arg + ".");
                            break;
                        case "unknownuser":
                            Console.WriteLine("Unknown user.");
                            break;
                    }
                    break;
            }
        }
    }
}
