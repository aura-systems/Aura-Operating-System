/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Translation system
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
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
                        case "AmountRAM":
                            Console.WriteLine("Montant de la RAM:             " + Cosmos.Core.CPU.GetAmountOfRAM() + "MB");
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
                        case "languageask":
                            Console.WriteLine("Choisissez votre langue:");
                            break;
                        case "chooseyourusername":
                            Console.WriteLine("Choisissez votre nom d'utilisateur pour votre compte Alve:");
                            break;
                        case "alreadyuser":
                            Console.WriteLine("Cet utilisateur existe déjà !");
                            break;
                        case "passuser":
                            Console.WriteLine("Choisissez un mot de passe pour " + arg);
                            break;
                        case "setupcmd":
                            Console.WriteLine("Voulez vous réellement réinitialiser votre ordinateur ? Tous les fichiers vont être effacés. [o/n]");
                            break;
                        case "user":
                            Console.Write("Utilisateur > ");
                            break;
                        case "passwd":
                            Console.Write("Mot de passe > ");
                            break;
                        case "charmin":
                            Console.Write("Le nom d'utilisateur doit être constitué de caractères alphanumériques et doit être entre 4 à 20 caractères.");
                            break;
                        case "pswcharmin":
                            Console.Write("Le mot de passe doit être constitué de caractères alphanumériques et doit être entre 6 à 40 caractères.");
                            break;
                        case "errorwhileusercreating":
                            Console.Write("Une erreur s'est produite lors de la création du compte utilisateur.");
                            break;
                        case "whattypeuser":
                            Console.Write("Quel est le niveau de l'utilisateur ?");
                            break;
                        case "groupsavailable":
                            Console.Write("Groupes disponibles: 'admin'; 'standard'");
                            break;
                        case "mkfil":
                            Console.WriteLine("Entrez le nom du fichier (mkfil fichier.txt).");
                            break;
                        case "doesnotexit":
                            Console.WriteLine("Ce fichier n'existe pas.");
                            break;
                        case "wrongpassword":
                            Console.WriteLine("Mauvais mot de passe.");
                            break;
                        case "askcomputername":
                            Console.WriteLine("Choisissez le nom pour votre ordinateur :");
                            break;
                        case "computernameincorrect":
                            Console.WriteLine("Le nom de l'ordinateur est incorrect, il doit être composé au maximum de 15 caractères.");
                            break;
                        case "computernamename":
                            Console.Write("Nom de l'ordinateur > ");
                            break;
                        case "computernamesuccess":
                            Console.Write("Le nouveau nom de l'ordinateur a été appliqué ! \n\nRedémarrez l'ordinateur pour que le changement prenne effet.");
                            break;
                        case "tips":
                            Console.WriteLine(" * Conseil(s) :");
                            break;
                        case "foldercontent":
                            Console.WriteLine("Contenu du dossier '" + arg + "':");
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
                        case "AmountRAM":
                            Console.WriteLine("Amount of RAM:             " + Cosmos.Core.CPU.GetAmountOfRAM() + "MB");
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
                        case "languageask":
                            Console.WriteLine("Choose your language:");
                            break;
                        case "chooseyourusername":
                            Console.WriteLine("Choose a user name for your Alve Account:");
                            break;
                        case "alreadyuser":
                            Console.WriteLine("This user exist already!");
                            break;
                        case "passuser":
                            Console.WriteLine("Choose a password for " + arg);
                            break;
                        case "setupcmd":
                            Console.WriteLine("Do you really want to reinitialize the computer ? All files will be erased. [o/n]");
                            break;
                        case "user":
                            Console.Write("Login > ");
                            break;
                        case "passwd":
                            Console.Write("Password > ");
                            break;
                        case "charmin":
                            Console.Write("Username length should be 4-20 characters and contain alphanumeric text.");
                            break;
                        case "pswcharmin":
                            Console.Write("Password length should be 6-40 characters and contain alphanumeric text.");
                            break;
                        case "errorwhileusercreating":
                            Console.Write("An error occurred while creating the user account.");
                            break;
                        case "whattypeuser":
                            Console.Write("What will be the user level ?");
                            break;
                        case "mkfil":
                            Console.WriteLine("Enter the file name (mkfil file.txt).");
                            break;
                        case "doesnotexit":
                            Console.WriteLine("This file does not exist.");
                            break;
                        case "wrongpassword":
                            Console.WriteLine("Wrong Password.");
                            break;
                        case "askcomputername":
                            Console.WriteLine("Choose your computer name :");
                            break;
                        case "computernameincorrect":
                            Console.WriteLine("The computer name is incorrect, computer name length should be 1-20 characters.");
                            break;
                        case "computernamename":
                            Console.Write("Computer name > ");
                            break;
                        case "computernamesuccess":
                            Console.Write("The new computer name has been applied! \n\nReboot the computer for the changing name take effect.");
                            break;
                        case "tips":
                            Console.WriteLine(" * Tips :");
                            break;
                        case "foldercontent":
                            Console.WriteLine("Folder's content of '" + arg + "':");
                            break;
                    }
                    break;
            }
        }

        public static string Menu(string ToTranslate)
        {
            switch (Kernel.langSelected)
            {
                case "fr_FR":

                    switch (ToTranslate)
                    {
                        case "setup":
                            string text = Drawable.Menu.DispLoginForm("Création d'un compte Alve.");
                            return text;
                        case "alreadyuser":
                            Drawable.Menu.DispErrorDialog("Cet utilisateur existe déjà !");
                            break;
                        case "error1":
                            Drawable.Menu.DispErrorDialog("Erreur pendant la création de l'utilisateur!");
                            break;
                        case "error2":
                            Drawable.Menu.DispErrorDialog("Ce mot de passe est trop court !");
                            break;
                        case "error3":
                            Drawable.Menu.DispErrorDialog("Ce pseudo est trop court !");
                            break;
                        case "errorcomputer":
                            Drawable.Menu.DispErrorDialog("Computer name length must be 1-20 characters.");
                            break;
                        case "computernamedialog":
                            string text1 = Drawable.Menu.DispCompuernameDialog("║ Choisissez un nom pour votre PC :", "║ Nom du PC : ");
                            return text1;
                    }
                    break;

                case "en_US":

                    switch (ToTranslate)
                    {
                        case "setup":
                            string text = Drawable.Menu.DispLoginForm("Alve account creation.");
                            return text;
                        case "alreadyuser":
                            Drawable.Menu.DispErrorDialog("This user already exists!");
                            break;
                        case "error1":
                            Drawable.Menu.DispErrorDialog("Error while creating the user!");
                            break;
                        case "error2":
                            Drawable.Menu.DispErrorDialog("This password is too short!");
                            break;
                        case "error3":
                            Drawable.Menu.DispErrorDialog("This nickname is too short!");
                            break;
                        case "errorcomputer":
                            Drawable.Menu.DispErrorDialog("Computer name length must be 1-20 characters.");
                            break;
                        case "computernamedialog":
                            string text2 = Drawable.Menu.DispCompuernameDialog("║ Choose your computer name:", "║ Computer name: ");
                            return text2;
                    }
                    break;
            }
            return "";
        }
    }
}
