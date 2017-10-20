/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Help infos
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;

namespace Aura_OS.System.Translation
{
    class Help
    {

        /// <summary>
        /// Display help on general commands.
        /// </summary>
        public static void _Help()
        {
            switch (Kernel.langSelected)
            {
                case "fr_FR":
                    Console.WriteLine("Commandes disponibles (1/2):");
                    Console.WriteLine("- shutdown (arrêt ACPI)");
                    Console.WriteLine("- reboot (redémarrage CPU)");
                    Console.WriteLine("- clear (efface la console)");
                    Console.WriteLine("- cd .. (pour naviguer dans le dossier parent)");
                    Console.WriteLine("- cd (pour aller à un dossier)");
                    Console.WriteLine("- dir (liste les fichiers et dossiers)");
                    Console.WriteLine("- ls (liste les fichiers et dossiers)");
                    Console.WriteLine("- cp (pour copier un fichier à une autre destination)");
                    Console.WriteLine("- mkdir (pour créer un dossier)");
                    Console.WriteLine("- rmdir (pour supprimer un dossier)");
                    Console.WriteLine("- mkfil (pour créer un fichier)");
                    Console.WriteLine("- rmfil (pour supprimer un fichier)");
                    Console.WriteLine("- edit (pour éditer un fichier)");
                    Console.WriteLine("- vol (list les volumes FAT)");
                    Console.WriteLine("- echo (affiche un echo)");
                    Console.WriteLine("- systeminfo (affiche des informations systeme)");
                    Console.WriteLine("- ver (pour afficher la version système)");
                    Console.WriteLine("- settings {args} (permet d'accéder aux paramètres)");
                    Console.WriteLine("- logout (permet de se déconnecter)");

                    Console.ReadKey(); //page 2
                    Console.WriteLine();
                    Console.WriteLine("Commandes disponibles (2/2):");
                    Console.WriteLine("- time (récupère la date et l'heure)");
                    Console.WriteLine("- ipconfig (affiche les informations réseau)");

                    Console.ReadKey();

                    break;

                case "en_US":
                    Console.WriteLine("Available commands (1/2):");
                    Console.WriteLine("- shutdown (to do an ACPI Shutdown)");
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
                    Console.WriteLine("- systeminfo (to display system information)");
                    Console.WriteLine("- ver (to display system version)");
                    Console.WriteLine("- settings {args} (access to settings)");
                    Console.WriteLine("- logout (to disconnect)");
                    
                    Console.ReadKey(); //page 2

                    Console.WriteLine();
                    Console.WriteLine("Available commands (2/2):");
                    Console.WriteLine("- time (to get time and date)");
                    Console.WriteLine("- ipconfig (to get network information)");

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
                    Console.WriteLine("- adduser (pour créer un compte)");
                    Console.WriteLine("- setcomputername (pour changer le nom de l'ordinateur)");
                    Console.WriteLine("- setlang {lang} (pour changer la langue du système)");
                    Console.WriteLine("- backgroundcolor {colorID} (permet de changer la couleur de dernier plan)");
                    Console.WriteLine("- textcolor {colorID} (permet de changer la couleur de premier plan)");
                    break;

                case "en_US":
                    Console.WriteLine("Available commands:");
                    Console.WriteLine("- adduser (to create an account)");
                    Console.WriteLine("- setcomputername (to change the computer name)");
                    Console.WriteLine("- setlang {lang} (to change the system language)");
                    Console.WriteLine("- backgroundcolor {colorID} (to change background color)");
                    Console.WriteLine("- textcolor {colorID} (to change foreground colour)");
                    break;
            }
        }

    }
}
