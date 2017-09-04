/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Help infos
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;

namespace Alve_OS.System.Translation
{
    class Help
    {

        /// <summary>
        /// Display help on general commands.
        /// </summary>
        public static void HelpD()
        {
            switch (Kernel.langSelected)
            {
                case "fr_FR":
                    Console.WriteLine("Commandes disponibles (1/2):");
                    Console.WriteLine();
                    Console.WriteLine("- shutdown (Arrêt ACPI)");
                    Console.WriteLine("- reboot (Redémarrage CPU)");
                    Console.WriteLine("- clear (Efface la console)");
                    Console.WriteLine("- cd .. (Pour naviguer dans l'arborescence)");
                    Console.WriteLine("- cd (Pour aller à un dossier)");
                    Console.WriteLine("- dir (Liste les fichiers et dossiers)");
                    Console.WriteLine("- ls (Liste les fichiers et dossiers)");
                    Console.WriteLine("- cp (Pour copier un fichier à une autre destination)");
                    Console.WriteLine("- mkdir (Pour créer un dossier)");
                    Console.WriteLine("- rmdir (Pour supprimer un dossier)");
                    Console.WriteLine("- mkfil (Pour créer un fichier)");
                    Console.WriteLine("- rmfil (Pour supprimer un fichier)");
                    Console.WriteLine("- edit (Pour éditer un fichier)");
                    Console.WriteLine("- vol (Liste les volumes FAT)");
                    Console.WriteLine("- echo (Affiche un echo)");
                    Console.WriteLine("- systeminfo (Affiche des informations systeme)");
                    Console.WriteLine("- ver (Pour afficher la version système)");
                    Console.WriteLine("- textcolor (Permet de changer la couleur de premier plan)");
                    Console.WriteLine("- backgroundcolor (Permet de changer la couleur de dernier plan)");
                    Console.WriteLine("- settings {args} (Permet d'accéder aux paramètres)");
                    Console.WriteLine("- logout (Permet de se déconnecter)");

                    Console.ReadKey(); //page 2

                    Console.WriteLine("Commandes disponibles (2/2):");
                    Console.WriteLine();
                    Console.WriteLine("- time (Récupère le temps et la date du jour)");
                    Console.WriteLine("- ipconfig (Affiche les informations réseau)");

                    Console.ReadKey();

                    break;

                case "en_US":
                    Console.WriteLine("Available commands (1/2):");
                    Console.WriteLine();
                    Console.WriteLine("- shutdown (to do a ACPI Shutdown)");
                    Console.WriteLine("- reboot (to do a CPU Reboot)");
                    Console.WriteLine("- clear (to clear the console)");
                    Console.WriteLine("- cd .. (to navigate to the parent folder)");
                    Console.WriteLine("- cd (to navigate to a folder)");
                    Console.WriteLine("- dir (to list directories and files)");
                    Console.WriteLine("- ls (to list directories and files)");
                    Console.WriteLine("- cp (to copy a file to an another destination)");
                    Console.WriteLine("- mkdir (to create a directory");
                    Console.WriteLine("- rmdir (to remove a directory)");
                    Console.WriteLine("- mkfil (to create a file)");
                    Console.WriteLine("- rmfil (to remove a file)");
                    Console.WriteLine("- edit (to edit a file)");
                    Console.WriteLine("- vol (to list volumes)");
                    Console.WriteLine("- echo (to echo text)");
                    Console.WriteLine("- systeminfo (to display system informations)");
                    Console.WriteLine("- ver (to display system version)");
                    Console.WriteLine("- textcolor (change foreground colour)");
                    Console.WriteLine("- backgroundcolor (change background colour)");
                    Console.WriteLine("- settings {args} (Access to settings)");
                    Console.WriteLine("- logout (To disconnect)");
                    
                    Console.ReadKey(); //page 2

                    Console.WriteLine("Available commands (2/2):");
                    Console.WriteLine();
                    Console.WriteLine("- time (to get time and date)");
                    Console.WriteLine("- ipconfig (to get network informations)");

                    Console.ReadKey();

                    break;
            }
        }

        /// <summary>
        /// Display help on settings commands.
        /// </summary>
        public static void Settings()
        {
            switch (Kernel.langSelected)
            {
                case "fr_FR":
                    Console.WriteLine("Commandes disponible:");
                    Console.WriteLine("- adduser (Pour créer un compte)");
                    Console.WriteLine("- setcomputername (Nom de l'ordinateur)");
                    Console.WriteLine("- setlang {lang} (Change la langue du système)");
                    break;

                case "en_US":
                    Console.WriteLine("Available commands:");
                    Console.WriteLine("- adduser (To create an account)");
                    Console.WriteLine("- setcomputername (Computer name)");
                    Console.WriteLine("- setlang {lang} (Change system language)");
                    break;
            }
        }

    }
}
