/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Translation system
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Computer;
using System;

namespace Aura_OS.System.Translation
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

        // HELP

        /*
        * ## How to use keywords:
        * 
        * Keywords replace a sentence
        * by their translation that correspond to the keyword
        * to the system language. so you have
        * a keyword to a sentence.
        */

        /// <summary>
        /// Display text in user language
        /// </summary>
        /// <param name="ToTranslate">keyword</param>
        /// <param name="arg">dynamic string</param>
        /// <param name="arg2">dynamic string</param>
        public static void Display(string ToTranslate, string arg = "", string arg2 = "")
        {
            switch (Kernel.langSelected)
            {
                case "fr_FR":

                    switch (ToTranslate)
                    {
                        case "shutdown": //This is a keyword
                            Console.WriteLine("Extinction en cours..."); //That is a translated sentence.
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
                            Console.WriteLine(arg + " existe déjà !");
                            break;
                        case "Computername":
                            Console.WriteLine("Nom du PC:                     " + Kernel.ComputerName);
                            break;
                        case "OSName":
                            Console.WriteLine("Nom du système d'exploitation: Aura");
                            break;
                        case "OSVersion":
                            Console.WriteLine("Version du système:            " + Kernel.version);
                            break;
                        case "OSRevision":
                            Console.WriteLine("Révision du système:           " + Kernel.revision);
                            break;
                        case "AmountRAM":
                            Console.WriteLine("Montant de la RAM:             " + Info.GetAmountRAM());
                            break;
                        case "UnknownCommand":
                            Console.WriteLine("Commande inconnue.");
                            break;
                        case "unknownlanguage":
                            Console.WriteLine("Langue inconnue.");
                            break;
                        case "availablelanguage":
                            Console.WriteLine("Langues disponibles: en_US fr_FR nl_NL");
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
                            Console.WriteLine("Choisissez votre nom d'utilisateur pour votre compte Aura:");
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
                        case "liquideditor":
                            Console.WriteLine("Liquid Editor v" + arg + " par TheCool1James & valentinbreiz                            ");
                            break;
                        case "filename":
                            Console.Write("Nom du fichier : ");
                            break;
                        case "saved":
                            Console.WriteLine("'" + arg + "' a bien été sauvegardé dans '" + arg2 + "' !");
                            break;
                        case "menuliquideditor":
                            Console.Write("[F1]Sauvegarder  [F2]Nouveau  [ESC]Quitter\n");
                            break;
                        case "list":
                            Console.WriteLine(" Liste: " + arg + "\n");
                            break;
                        case "line":
                            Console.WriteLine(" Ligne: " + arg + "\n");
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
                            Console.Write("Le nouveau nom de l'ordinateur a été appliqué !");
                            break;
                        case "tips":
                            Console.WriteLine(" * Conseil(s) :");
                            break;
                        case "mkdir":
                            Console.WriteLine("Entrez le nom du dossier (mkdir dossier).");
                            break;
                        case "time":
                            Console.WriteLine("Date et heure:                 " + Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));
                            break;
                        case "mkdirfilealreadyexist":
                            Console.WriteLine("Le dossier existait déjà, le répertoire \"" + arg + "\" a donc été créé.");
                            break;
                        case "mkdirunsupporteddot":
                            Console.WriteLine("Vous ne pouvez pas mettre de point(s) dans le nom de votre répertoire.");
                            break;
                        case "invalidargument":
                            Console.WriteLine("Cet argument est invalide.");
                            break;
                        case "filecopied":
                            Console.WriteLine("Fichier copié.");
                            break;
                        case "filealreadyexist":
                            Console.WriteLine("Le fichier existe déjà.");
                            break;
                        case "docpoover":
                            Console.WriteLine("Faites 'cp -o sourceFile destinationFile' pour remplacer le fichier.");
                            break;
                        case "sourcefiledoesntexist":
                            Console.WriteLine("Le fichier source n'existe pas.");
                            break;
                        case "usagecp":
                            Console.WriteLine("Utilisation: cp {args} fichierSource fichierDestination");
                            break;
                        case "emptyarg":
                            Console.WriteLine("Argument inconnu.");
                            break;
                        case "about":
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Aura [version " + Kernel.version + "-" + Kernel.revision + "]");
                            Console.WriteLine("créé par Alexy DA CRUZ et Valentin CHARBONNIER.");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Site Web: aura-team.com");
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        case "errorthisisafile":
                            Console.WriteLine("Erreur: Ceci est un fichier.");
                            break;
                        case "volCommand":
                            Console.WriteLine();
                            Console.WriteLine("  Volume ###\tFormat\tSize");
                            Console.WriteLine("  ----------\t------\t--------");
                            break;
                        case "NetworkInProgress":
                            Console.WriteLine("Nous travaillons sur le réseau actuellement, de ce fait,");
                            Console.WriteLine("cette fonctionnalité est encore en Alpha.");
                            Console.WriteLine("Vous pouvez visiter la roadmap aura-team.com pour");
                            Console.WriteLine("voir la progression du projet sur ce domaine.");
                            Console.WriteLine();
                            break;
                        case "MAC":
                            Console.WriteLine("Addresse Physique (MAC): " + Core.Network.PhysicalAddress());
                            break;
                        case "notavalidscript":
                            Console.WriteLine("Ce fichier n'est pas un script valide.");
                            break;
                        case "md5":
                            Console.WriteLine("Voici le résultat en MD5.");
                            break;
                        case "user:existalready":
                            Console.WriteLine($"{arg} existe déjà !");
                            break;
                        case "user:hasbeencreated":
                            Console.WriteLine($"{arg} a bien été créé !");
                            break;
                        case "user:personaldirectories":
                            Console.WriteLine($"Les dossiers personnelles ont bien été créés !");
                            break;
                        case "user:doesntexist":
                            Console.WriteLine($"{arg} n'existe pas !");
                            break;
                        case "user:hasbeenremoved":
                            Console.WriteLine($"{arg} a bien été supprimé !");
                            break;
                        case "user:passwordhasbeenchanged":
                            Console.WriteLine($"Le mot de passe a bien été changé !");
                            break;
                        case "passwd:newpass":
                            Console.Write($"Entrer un nouveau mot de passe: ");
                            break;
                        case "passwd:retype":
                            Console.Write($"Confirmer le nouveau mot de passe: ");
                            break;
                        case "passwd:updated":
                            Console.WriteLine($"passwd: Le mot de passe a bien été changé.");
                            break;
                        case "adduser":
                            Console.WriteLine($"Essayez: settings adduser user pass");
                            break;
                        case "_passuser":
                            Console.WriteLine($"Essayez: settings passuser user pass");
                            break;
                        case "remuser":
                            Console.WriteLine($"Essayez: settings remuser user");
                            break;
                        case "nofilesystem":
                            Console.WriteLine("Aucun volume détecté, le sytème de fichiers est désactivé!");
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
                            Console.WriteLine(arg + " already exists!");
                            break;
                        case "Computername":
                            Console.WriteLine("Computer name:             " + Kernel.ComputerName);
                            break;
                        case "OSName":
                            Console.WriteLine("Operating system name:     Aura");
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
                            Console.WriteLine("Available languages: en-US fr-FR nl-NL");
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
                            Console.WriteLine("Choose a user name for your Aura account:");
                            break;
                        case "alreadyuser":
                            Console.WriteLine("This user already exists!");
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
                        case "liquideditor":
                            Console.WriteLine("Liquid Editor v" + arg + " by TheCool1James & valentinbreiz                             ");
                            break;
                        case "filename":
                            Console.Write("File name : ");
                            break;
                        case "saved":
                            Console.WriteLine("'" + arg + "' has been saved in '" + arg2 + "' !");
                            break;
                        case "menuliquideditor":
                            Console.Write("[F1]Save  [F2]New  [ESC]Exit\n");
                            break;
                        case "list":
                            Console.WriteLine(" List: " + arg + "\n");
                            break;
                        case "line":
                            Console.WriteLine(" Line: " + arg + "\n");
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
                            Console.Write("The new computer name has been applied!");
                            break;
                        case "tips":
                            Console.WriteLine(" * Tips :");
                            break;
                        case "mkdir":
                            Console.WriteLine("Enter the directory name (mkdir directory).");
                            break;
                        case "time":
                            Console.WriteLine("Date and time:             " + Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));
                            break;
                        case "mkdirfilealreadyexist":
                            Console.WriteLine("That folder existed already, directory \"" + arg + "\" has been created.");
                            break;
                        case "mkdirunsupporteddot":
                            Console.WriteLine("You can't have a dot in your directory name.");
                            break;
                        case "invalidargument":
                            Console.WriteLine("This argument is invalid.");
                            break;
                        case "filecopied":
                            Console.WriteLine("Copied File.");
                            break;
                        case "filealreadyexist":
                            Console.WriteLine("This file already exist.");
                            break;
                        case "docpoover":
                            Console.WriteLine("Do 'cp -o sourceFile destinationFile' to overwrite the file.");
                            break;
                        case "sourcefiledoesntexist":
                            Console.WriteLine("Source file doesn't not exist.");
                            break;
                        case "usagecp":
                            Console.WriteLine("Usage: cp {args} sourceFile destinationFile");
                            break;
                        case "emptyarg":
                            Console.WriteLine("Unknow Argument.");
                            break;
                        case "about":
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Aura [version " + Kernel.version + "-" + Kernel.revision + "]");
                            Console.WriteLine("Created by Alexy DA CRUZ and Valentin CHARBONNIER.");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Website: aura-team.com");
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        case "errorthisisafile":
                            Console.WriteLine("Error: This is a file.");
                            break;
                        case "volCommand":
                            Console.WriteLine();
                            Console.WriteLine("  Volume ###\tFormat\tSize");
                            Console.WriteLine("  ----------\t------\t--------");
                            break;
                        case "NetworkInProgress":
                            Console.WriteLine("We are working on network, so");
                            Console.WriteLine("this feature is in alpha version.");
                            Console.WriteLine("You can visit the roadmap here aura-team.com to");
                            Console.WriteLine("see the progression on this part.");
                            Console.WriteLine();
                            break;
                        case "MAC":
                            Console.WriteLine("MAC Address: " + Core.Network.PhysicalAddress());
                            break;
                        case "notavalidscript":
                            Console.WriteLine("This file is not a valid script.");
                            break;
                        case "md5":
                            Console.WriteLine("This is the result in MD5.");
                            break;
                        case "user:existalready":
                            Console.WriteLine($"{arg} exist already !");
                            break;
                        case "user:hasbeencreated":
                            Console.WriteLine($"{arg} has been created !");
                            break;
                        case "user:personaldirectories":
                            Console.WriteLine($"{arg} Personal directories has been created ! !");
                            break;
                        case "user:doesntexist":
                            Console.WriteLine($"{arg} doesn't exist !");
                            break;
                        case "user:hasbeenremoved":
                            Console.WriteLine($"{arg} has been removed !");
                            break;
                        case "user:passwordhasbeenchanged":
                            Console.WriteLine($"Password has been changed !");
                            break;
                        case "passwd:newpass":
                            Console.Write($"Enter new password: ");
                            break;
                        case "passwd:retype":
                            Console.Write($"Retype new password: ");
                            break;
                        case "passwd:updated":
                            Console.WriteLine($"passwd: password updated successfully.");
                            break;
                        case "adduser":
                            Console.WriteLine($"Try: settings adduser user pass");
                            break;
                        case "_passuser":
                            Console.WriteLine($"Try: settings passuser user pass");
                            break;
                        case "remuser":
                            Console.WriteLine($"Try: settings remuser user");
                            break;
                        case "nofilesystem":
                            Console.WriteLine("No volumes are detected, filesystem is disabled!");
                            break;
                    }
                    break;

                case "nl_NL":

                    switch (ToTranslate)
                    {

                        case "shutdown":
                            Console.WriteLine("Afsluiten...");
                            break;
                        case "keyboard":
                            Console.WriteLine("Initialisatie van het toetsenbord NL...");
                            break;
                        case "restart":
                            Console.WriteLine("Opnieuw opstarten...");
                            break;
                        case "directorydoesntexist":
                            Console.WriteLine("Deze map bestaat niet!");
                            break;
                        case "doesnotexist":
                            Console.WriteLine(arg + " bestaat niet!");
                            break;
                        case "alreadyexist":
                            Console.WriteLine(arg + " bestaat al!");
                            break;
                        case "Computername":
                            Console.WriteLine("Naam computer:             " + Kernel.ComputerName);
                            break;
                        case "OSName":
                            Console.WriteLine("Naam besturingssysteem:    Aura");
                            break;
                        case "OSVersion":
                            Console.WriteLine("Versie besturingssysteem:  " + Kernel.version);
                            break;
                        case "OSRevision":
                            Console.WriteLine("Revisie besturingssysteem: " + Kernel.revision);
                            break;
                        case "AmountRAM":
                            Console.WriteLine("Hoeveelheid RAM:           " + Cosmos.Core.CPU.GetAmountOfRAM() + "MB");
                            break;
                        case "UnknownCommand":
                            Console.WriteLine("Onbekend commando.");
                            break;
                        case "unknownlanguage":
                            Console.WriteLine("Onbekende taal.");
                            break;
                        case "availablelanguage":
                            Console.WriteLine("Beschikbare talen: en-US fr-FR nl-NL");
                            break;
                        case "unknowncolor":
                            Console.WriteLine("Onbekende kleur.");
                            break;
                        case "logged":
                            Console.WriteLine("Je bent ingelogd " + arg + ".");
                            break;
                        case "unknownuser":
                            Console.WriteLine("Onbekende gebruiker.");
                            break;
                        case "languageask":
                            Console.WriteLine("Kies je taal:");
                            break;
                        case "chooseyourusername":
                            Console.WriteLine("Kies een gebruikersnaam voor je Aura account:");
                            break;
                        case "alreadyuser":
                            Console.WriteLine("Deze gebruiker bestaat al!");
                            break;
                        case "passuser":
                            Console.WriteLine("Kies een wachtwoord voor " + arg);
                            break;
                        case "setupcmd":
                            Console.WriteLine("Wilt u de computer echt opnieuw initialiseren ? Alle bestanden worden gewist. [o/n]");
                            break;
                        case "user":
                            Console.Write("Gebruiker > ");
                            break;
                        case "passwd":
                            Console.Write("Wachtwoord > ");
                            break;
                        case "charmin":
                            Console.Write("De lengte van de gebruikersnaam moet 4-20 tekens lang zijn en kan alleen bestaan uit letters en cijfers.");
                            break;
                        case "pswcharmin":
                            Console.Write("De lengte van het wachtwoord moet 6-40 tekens lang zijn en kan alleen bestaan uit letters en cijfers.");
                            break;
                        case "errorwhileusercreating":
                            Console.Write("Er is een fout opgetreden bij het maken van het gebruikersaccount.");
                            break;
                        case "whattypeuser":
                            Console.Write("Wat is het gebruikersniveau ?");
                            break;
                        case "mkfil":
                            Console.WriteLine("Voer de bestandsnaam in (mkfil file.txt).");
                            break;
                        case "doesnotexit":
                            Console.WriteLine("Dit bestand bestaat niet.");
                            break;
                        case "wrongpassword":
                            Console.WriteLine("Verkeerd wachtwoord.");
                            break;
                        case "liquideditor":
                            Console.WriteLine("Liquid Editor v" + arg + " door TheCool1James & valentinbreiz                             ");
                            break;
                        case "filename":
                            Console.Write("Bestandsnaam : ");
                            break;
                        case "saved":
                            Console.WriteLine("'" + arg + "' is opgeslagen in '" + arg2 + "' !");
                            break;
                        case "menuliquideditor":
                            Console.Write("[F1]Opslaan  [F2]Nieuw  [ESC]Afsluiten\n");
                            break;
                        case "list":
                            Console.WriteLine(" Lijst: " + arg + "\n");
                            break;
                        case "line":
                            Console.WriteLine(" Lijn: " + arg + "\n");
                            break;
                        case "askcomputername":
                            Console.WriteLine("Kies de naam van de computer :");
                            break;
                        case "computernameincorrect":
                            Console.WriteLine("De computernaam is onjuist, de computernaam moet uit 1-20 tekens bestaan.");
                            break;
                        case "computernamename":
                            Console.Write("Computernaam > ");
                            break;
                        case "computernamesuccess":
                            Console.Write("De nieuwe computernaam is toegepast!");
                            break;
                        case "tips":
                            Console.WriteLine(" * Tips :");
                            break;
                        case "mkdir":
                            Console.WriteLine("Voer de mapnaam in  (mkdir map).");
                            break;
                        case "time":
                            Console.WriteLine("Datum en tijd:             " + Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));
                            break;
                        case "mkdirfilealreadyexist":
                            Console.WriteLine("Die map bestond al, map \"" + arg + "\" is aangemaakt.");
                            break;
                        case "mkdirunsupporteddot":
                            Console.WriteLine("Je kunt geen punt in een mapnaam hebben.");
                            break;
                        case "invalidargument":
                            Console.WriteLine("Dit argument is ongeldig.");
                            break;
                        case "filecopied":
                            Console.WriteLine("Bestand gekopieerd.");
                            break;
                        case "filealreadyexist":
                            Console.WriteLine("Dit bestand bestaat al.");
                            break;
                        case "docpoover":
                            Console.WriteLine("Gebruik 'cp -o bronBestand doelBestand' om het bestand te vervangen.");
                            break;
                        case "sourcefiledoesntexist":
                            Console.WriteLine("Bronbestand bestaat niet.");
                            break;
                        case "usagecp":
                            Console.WriteLine("Gebruik: cp {argumenten} bronBestand doelBestand");
                            break;
                        case "emptyarg":
                            Console.WriteLine("Onbekend Argument.");
                            break;
                        case "about":
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Aura [versie " + Kernel.version + "-" + Kernel.revision + "]");
                            Console.WriteLine("Ontwikkeld door Alexy DA CRUZ en Valentin CHARBONNIER.");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Website: aura-team.com");
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        case "errorthisisafile":
                            Console.WriteLine("Fout: Dit is een bestand.");
                            break;
                        case "volCommand":
                            Console.WriteLine();
                            Console.WriteLine("  Volume ###\tFormaat\tGroote");
                            Console.WriteLine("  ----------\t------\t--------");
                            break;
                        case "NetworkInProgress":
                            Console.WriteLine("We werken aan een netwerk, dus");
                            Console.WriteLine("deze functie is in alfaversie.");
                            Console.WriteLine("Je kunt de roadmap op aura-team.com bezoeken om");
                            Console.WriteLine("de vooruitgang te zien van dit onderdeel.");
                            Console.WriteLine();
                            break;
                        case "MAC":
                            Console.WriteLine("MAC Adres: " + Core.Network.PhysicalAddress());
                            break;
                        case "notavalidscript":
                            Console.WriteLine("Dit bestand is geen geldig script.");
                            break;
                        case "md5":
                            Console.WriteLine("Dit is het resultaat in MD5.");
                            break;
                        case "user:existalready":
                            Console.WriteLine($"{arg} bestaat al !");
                            break;
                        case "user:hasbeencreated":
                            Console.WriteLine($"{arg} is gemaakt !");
                            break;
                        case "user:personaldirectories":
                            Console.WriteLine($"{arg} Persoonlijke mappen zijn gemaakt ! !");
                            break;
                        case "user:doesntexist":
                            Console.WriteLine($"{arg} bestaat niet !");
                            break;
                        case "user:hasbeenremoved":
                            Console.WriteLine($"{arg} is verwijderd !");
                            break;
                        case "user:passwordhasbeenchanged":
                            Console.WriteLine($"Het wachtwoord is veranderd !");
                            break;
                        case "passwd:newpass":
                            Console.Write($"Voer een nieuw wachtwoord in: ");
                            break;
                        case "passwd:retype":
                            Console.Write($"Type het nieuwe wachtwoord opnieuw: ");
                            break;
                        case "passwd:updated":
                            Console.WriteLine($"passwd: wachtwoord is succesvol bijgewerkt.");
                            break;
                        case "adduser":
                            Console.WriteLine($"Probeer: settings adduser gebruiker wachtwoord");
                            break;
                        case "_passuser":
                            Console.WriteLine($"Probeer: settings passuser gebruiker wachtwoord");
                            break;
                        case "remuser":
                            Console.WriteLine($"Probeer: settings remuser gebruiker");
                            break;
                        case "nofilesystem":
                            Console.WriteLine("Er zijn geen volumes gedetecteerd, het bestandssysteem is uitgeschakeld!");
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
                            string text = Drawable.Menu.DispLoginForm("Création d'un compte Aura.");
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
                            string text1 = Drawable.Menu.DispComputernameDialog("║ Choisissez un nom pour votre PC :", "║ Nom du PC : ");
                            return text1;
                    }
                    break;

                case "en_US":

                    switch (ToTranslate)
                    {
                        case "setup":
                            string text = Drawable.Menu.DispLoginForm("Aura account creation.");
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
                            string text2 = Drawable.Menu.DispComputernameDialog("║ Choose your computer name:", "║ Computer name: ");
                            return text2;
                    }
                    break;

                case "nl_NL":

                    switch (ToTranslate)
                    {
                        case "setup":
                            string text = Drawable.Menu.DispLoginForm("Aura-account maken.");
                            return text;
                        case "alreadyuser":
                            Drawable.Menu.DispErrorDialog("Deze gebruiker bestaat al!");
                            break;
                        case "error1":
                            Drawable.Menu.DispErrorDialog("Fout tijdens het maken van de gebruiker!");
                            break;
                        case "error2":
                            Drawable.Menu.DispErrorDialog("Dit wachtwoord is te kort!");
                            break;
                        case "error3":
                            Drawable.Menu.DispErrorDialog("Deze nickname is te kort!");
                            break;
                        case "errorcomputer":
                            Drawable.Menu.DispErrorDialog("De lengte van de computernaam moet 1-20 tekens lang zijn.");
                            break;
                        case "computernamedialog":
                            string text2 = Drawable.Menu.DispComputernameDialog("║ Kies de naam van de computer:", "║ Naam computer: ");
                            return text2;
                    }
                    break;
            }
            return "";
        }

    }
}
