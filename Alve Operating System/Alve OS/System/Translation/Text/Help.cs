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
        public static void Display()
        {
            switch (Kernel.langSelected)
            {
                case "fr_FR":
                    Console.WriteLine("Commandes disponible:");
                    Console.WriteLine("- shutdown (Arr" + ASCII.ecircon + "t ACPI)");
                    Console.WriteLine("- reboot (Red" + ASCII.eaigu + "marrage CPU)");
                    Console.WriteLine("- clear (Efface la console)");
                    Console.WriteLine("- cd .. (Pour naviguer dans l'arborescence)");
                    Console.WriteLine("- cd (Pour aller " + ASCII.agrave + " un dossier)");
                    Console.WriteLine("- dir (Liste les fichiers et dossiers)");
                    Console.WriteLine("- mkdir (Pour cr" + ASCII.eaigu + "er un dossier)");
                    Console.WriteLine("- rmdir (Pour supprimer un dossier)");
                    Console.WriteLine("- mkfil (Pour cr" + ASCII.eaigu + "er un fichier)");
                    Console.WriteLine("- rmfil (Pour supprimer un fichier)");
                    Console.WriteLine("- vol (Liste les volumes FAT)");
                    Console.WriteLine("- echo (Affiche un echo)");
                    Console.WriteLine("- systeminfo (Affiche des informations systeme)");
                    Console.WriteLine("- langset (Changer le langage syst" + ASCII.egrave + "me)");
                    Console.WriteLine("- ver (Pour afficher la version syst" + ASCII.egrave + "me)");
                    Console.WriteLine("- color (Permet de changer la couleur de premier plan)");
                    break;

                case "en_US":
                    Console.WriteLine("Available commands:");
                    Console.WriteLine("- shutdown (to do a ACPI Shutdown)");
                    Console.WriteLine("- reboot (to do a CPU Reboot)");
                    Console.WriteLine("- clear (to clear the console)");
                    Console.WriteLine("- cd .. (to navigate to the parent folder)");
                    Console.WriteLine("- cd (to navigate to a folder)");
                    Console.WriteLine("- dir (to list directories and files)");
                    Console.WriteLine("- mkdir (to create a directory");
                    Console.WriteLine("- rmdir (to remove a directory)");
                    Console.WriteLine("- mkfil (to create a file)");
                    Console.WriteLine("- rmfil (to remove a file)");
                    Console.WriteLine("- vol (to list volumes)");
                    Console.WriteLine("- echo (to echo text)");
                    Console.WriteLine("- systeminfo (to display system informations)");
                    Console.WriteLine("- langset (to change system language)");
                    Console.WriteLine("- ver (to display system version)");
                    Console.WriteLine("- color (change foreground colour)");
                    break;
            }
        }
    }
}
