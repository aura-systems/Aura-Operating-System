/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Help infos
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Shell = Aura_OS.System.Shell;
//using Cosmos.HAL.PCInformation;
using System;
using Cosmos.HAL;
using Cosmos.Core;

namespace Aura_OS.System.Translation
{
    class List_Translation
    {
        /// <summary>
        /// Display help on settings commands.
        /// </summary>
        public static void Settings()
        {
            switch (Global.langSelected)
            {
                case "fr_FR":
                    Console.WriteLine("Commandes disponible:");
                    Console.WriteLine("- adduser {user} {pass} (pour créer un compte)");
                    Console.WriteLine("- remuser {user} (pour supprimer un compte)");
                    Console.WriteLine("- passuser {user} {pass} (pour changer le mot de passe)");
                    Console.WriteLine("- setcomputername (pour changer le nom de l'ordinateur)");
                    Console.WriteLine("- setlang {lang} (pour changer la langue du système)");
                    //Console.WriteLine("- consolemode {mode} (pour changer le mode video de la console)");
                    Console.WriteLine("- debugger (pour configurer le debugger)");
                    Console.WriteLine("- ipaddress (pour changer l'adresse IP du PC)");
                    break;

                case "en_US":
                    Console.WriteLine("Available commands:");
                    Console.WriteLine("- adduser {user} {pass} (to create an account)");
                    Console.WriteLine("- remuser {user} (to remove an account)");
                    Console.WriteLine("- passuser {user} {pass} (to change password)");
                    Console.WriteLine("- setcomputername (to change the computer name)");
                    Console.WriteLine("- setlang {lang} (to change the system language)");
                    //Console.WriteLine("- consolemode {mode} (to change the video mode of the console)");
                    Console.WriteLine("- debugger (to configure the debugger)");
                    Console.WriteLine("- ipaddress (to change the IP address of the PC)");
                    break;

                case "nl_NL":
                    Console.WriteLine("Mogelijke commando's:");
                    Console.WriteLine("- adduser {gebruiker} {wachtwoord} (om een account aan te maken)");
                    Console.WriteLine("- remuser {gebruiker} (om een account te verwijderen)");
                    Console.WriteLine("- passuser {gebruiker} {wachtwoord} (om het wachtwoord te veranderen)");
                    Console.WriteLine("- setcomputername (om de naam van de computer te veranderen)");
                    Console.WriteLine("- setlang {lang} (om de systeemtaal aan te passen)");
                    //Console.WriteLine("- consolemode {mode} (om de videomodus van de console te wijzigen)");
                    Console.WriteLine("- debugger (om de debugger te configureren)");
                    Console.WriteLine("- ipaddress (om het IP-adres van de PC te wijzigen)");
                    break;
                    
                case "it_IT":
                    Console.WriteLine("Comandi disponibili:");
                    Console.WriteLine("- adduser {user} {pass} (per creare un account)");
                    Console.WriteLine("- remuser {user} (per rimuovere un account)");
                    Console.WriteLine("- passuser {user} {pass} (per cambiare la password)");
                    Console.WriteLine("- setcomputername (per cambiare il nome del computer)");
                    Console.WriteLine("- setlang {lang} (per cambiare la lingua di sistema)");
                    //Console.WriteLine("- consolemode {mode} (om de videomodus van de console te wijzigen)");
                    Console.WriteLine("- debugger {lang} (per configurare il debugger)");
                    Console.WriteLine("- ipaddress (per modificare l'indirizzo IP del PC)");
                    break;
                    
                case "pl_PL":
                    Console.WriteLine("Dostepne komendy:");
                    Console.WriteLine("- adduser {user} {pass} (do tworzenia konta)");
                    Console.WriteLine("- remuser {user} (do usuwania konta)");
                    Console.WriteLine("- passuser {user} {pass} (do zmiany hasla)");
                    Console.WriteLine("- setcomputername (do zmiany nazwy komputera)");
                    Console.WriteLine("- setlang {lang} (do zmiany jezyka)");
                    //Console.WriteLine("- consolemode {mode} (do zmiany trybu konsoli)");
                    Console.WriteLine("- debugger (do konfiguracji debugera)");
                    Console.WriteLine("- ipaddress (do zmiany adresu IP)");
                    break;
            }
        }

        /// <summary>
        /// Display help on settings commands.
        /// </summary>
        public static void Systeminfo()
        {
            Utils.Settings config = new Utils.Settings(@"0:\System\settings.conf");
            switch (Global.langSelected)
            {
                case "fr_FR":
                    Console.WriteLine("Nom du PC:                     " + Global.ComputerName);
                    Console.WriteLine("Nom du système d'exploitation: Aura");
                    Console.WriteLine("Nom du noyau:                  Cosmos");
                    Console.WriteLine("Version du système:            " + Global.version);
                    Console.WriteLine("Révision du système:           " + Global.revision);
                    Console.WriteLine("Date et heure:                 " + Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));
                    if (Global.SystemExists)
                    {
                        Console.WriteLine("Date d'installation originale: " + config.GetValue("setuptime"));
                    }
                    Console.WriteLine("Heure de démarrage du système: " + Global.boottime);
                    Console.WriteLine("Mémoire totale:                " + Core.MemoryManager.TotalMemory + "MB");
                    Console.WriteLine("Mémoire utilisée:              " + Core.MemoryManager.GetUsedMemory() + "MB");
                    Console.WriteLine("Mémoire restante:              " + Core.MemoryManager.GetFreeMemory() + "MB");
                    Console.WriteLine("Processeur(s):                 " + CPU.GetCPUBrandString());
                    Console.WriteLine("Mode de la console:            " + AConsole.ConsoleMode.GetConsoleInfo());
                    Console.WriteLine("Taille de la console:          " + Global.AConsole.Cols + "x" + Global.AConsole.Rows);
                    break;

                case "en_US":
                    Console.WriteLine("Computer name:             " + Global.ComputerName);
                    Console.WriteLine("Operating system name:     Aura");
                    Console.WriteLine("Kernel name:               Cosmos");
                    Console.WriteLine("Operating system version:  " + Global.version);
                    Console.WriteLine("Operating system revision: " + Global.revision);
                    Console.WriteLine("Date and time:             " + Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));
                    if (Global.SystemExists)
                    {
                        Console.WriteLine("Original Install Date:     " + config.GetValue("setuptime"));
                    }
                    Console.WriteLine("System Boot Time:          " + Global.boottime);
                    Console.WriteLine("Total memory:              " + Core.MemoryManager.TotalMemory + "MB");
                    Console.WriteLine("Used memory:               " + Core.MemoryManager.GetUsedMemory() + "MB");
                    Console.WriteLine("Free memory:               " + Core.MemoryManager.GetFreeMemory() + "MB");
                    Console.WriteLine("Processor(s):              " + CPU.GetCPUBrandString());
                    Console.WriteLine("Console mode:              " + AConsole.ConsoleMode.GetConsoleInfo());
                    Console.WriteLine("Console size:              " + Global.AConsole.Cols + "x" + Global.AConsole.Rows);
                    break;

                case "nl_NL":
                    Console.WriteLine("Naam computer:             " + Global.ComputerName);
                    Console.WriteLine("Naam besturingssysteem:    Aura");
                    Console.WriteLine("Kernel name:               Cosmos"); //TODO TRAD
                    Console.WriteLine("Versie besturingssysteem:  " + Global.version);
                    Console.WriteLine("Revisie besturingssysteem: " + Global.revision);
                    Console.WriteLine("Datum en tijd:             " + Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));
                    if (Global.SystemExists)
                    {
                        Console.WriteLine("Installeer datum:          " + config.GetValue("setuptime"));
                    }
                    Console.WriteLine("Starttijd van het systeem: " + Global.boottime);
                    Console.WriteLine("Totaal geheugen:           " + Core.MemoryManager.TotalMemory + "MB");
                    Console.WriteLine("Gebruikt geheugen:         " + Core.MemoryManager.GetUsedMemory() + "MB");
                    Console.WriteLine("Gratis geheugen:           " + Core.MemoryManager.GetFreeMemory() + "MB");
                    Console.WriteLine("Processor(s):              " + CPU.GetCPUBrandString());
                    Console.WriteLine("Consolewijze:              " + AConsole.ConsoleMode.GetConsoleInfo());
                    Console.WriteLine("Console size:              " + Global.AConsole.Cols + "x" + Global.AConsole.Rows); //TODO TRAD
                    break;

                case "it_IT":
                    Console.WriteLine("Nome del computer:               " + Global.ComputerName);
                    Console.WriteLine("Nome del sistema operativo:      Aura");
                    Console.WriteLine("Kernel name:                     Cosmos"); //TODO TRAD
                    Console.WriteLine("Versione del sistema operativo:  " + Global.version);
                    Console.WriteLine("Revisione del sistema operativo: " + Global.revision);
                    Console.WriteLine("Data e ora:                      " + Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));
                    if (Global.SystemExists)
                    {
                        Console.WriteLine("Data Installazione Sistema:      " + config.GetValue("setuptime"));
                    }
                    Console.WriteLine("System Boot Time:                " + Global.boottime);
                    Console.WriteLine("Memoria totale:                  " + Core.MemoryManager.TotalMemory + "MB");
                    Console.WriteLine("Memoria utilizzata:              " + Core.MemoryManager.GetUsedMemory() + "MB");
                    Console.WriteLine("Memoria libera:                  " + Core.MemoryManager.GetFreeMemory() + "MB");
                    Console.WriteLine("Processor(s):                    " + CPU.GetCPUBrandString());
                    Console.WriteLine("Modalità console:                " + AConsole.ConsoleMode.GetConsoleInfo());
                    Console.WriteLine("Console size:                    " + Global.AConsole.Cols + "x" + Global.AConsole.Rows); //TODO TRAD
                    break;
                
                case "pl_PL":
                    Console.WriteLine("Nazwa Komputera:           " + Global.ComputerName);
                    Console.WriteLine("Nazwa systemu opr.:         Aura");
                    Console.WriteLine("Kernel name:                Cosmos"); //TODO TRAD
                    Console.WriteLine("Wersja systemu opr.:       " + Global.version);
                    Console.WriteLine("Podwersja systemu opr.:    " + Global.revision);
                    Console.WriteLine("Data i godzina:            " + Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));
                    if (Global.SystemExists)
                    {
                        Console.WriteLine("Data instalacji:       " + config.GetValue("setuptime"));
                    }
                    Console.WriteLine("Czas od startu:            " + Global.boottime);
                    Console.WriteLine("Pamiec RAM:                " + Core.MemoryManager.TotalMemory + "MB");
                    Console.WriteLine("RAM w uzyciu:              " + Core.MemoryManager.GetUsedMemory() + "MB");
                    Console.WriteLine("Wolny RAM:                 " + Core.MemoryManager.GetFreeMemory() + "MB");
                    Console.WriteLine("Procesor(y):               " + CPU.GetCPUBrandString());
                    Console.WriteLine("Tryb konsoli:              " + AConsole.ConsoleMode.GetConsoleInfo());
                    Console.WriteLine("Console size:              " + Global.AConsole.Cols + "x" + Global.AConsole.Rows); //TODO TRAD
                    break;
            }
        }

        /// <summary>
        /// Display informations about Aura OS
        /// </summary>
        public static void About()
        {
            switch (Global.langSelected)
            {
                case "fr_FR":
                    Console.WriteLine("À Propos d'Aura OS:");
                    Console.WriteLine("Aura Team créé par Valentin CHARBONNIER et Alexy DA CRUZ.");
                    Console.WriteLine();
                    Console.WriteLine("Contributeurs:");
                    Console.WriteLine("- djlw78");
                    Console.WriteLine("- Og-Rok");
                    Console.WriteLine();
                    Console.WriteLine("Merci aussi aux contributeurs de Cosmos.");
                    Console.WriteLine("https://github.com/aura-systems/Aura-Operating-System");
                    break;

                case "en_US":
                    Console.WriteLine("About Aura OS:");
                    Console.WriteLine("Aura Team created by Valentin CHARBONNIER and Alexy DA CRUZ.");
                    Console.WriteLine();
                    Console.WriteLine("Contributors:");
                    Console.WriteLine("- djlw78");
                    Console.WriteLine("- Og-Rok");
                    Console.WriteLine();
                    Console.WriteLine("Thanks also to the Cosmos contributors.");
                    Console.WriteLine("https://github.com/aura-systems/Aura-Operating-System");
                    break;

                case "nl_NL":
                    Console.WriteLine("Over Aura OS:");
                    Console.WriteLine("Aura Team opgericht door Valentin CHARBONNIER en Alexy DA CRUZ.");
                    Console.WriteLine();
                    Console.WriteLine("Bijdragers:");
                    Console.WriteLine("- djlw78");
                    Console.WriteLine("- Og-Rok");
                    Console.WriteLine();
                    Console.WriteLine("Dank ook aan de Cosmos donateurs.");
                    Console.WriteLine("https://github.com/aura-systems/Aura-Operating-System");
                    break;

                case "it_IT":
                    Console.WriteLine("Informazioni su Aura OS:");
                    Console.WriteLine("Aura Team creato da Valentin CHARBONNIER e Alexy DA CRUZ.");
                    Console.WriteLine();
                    Console.WriteLine("Collaboratori:");
                    Console.WriteLine("- djlw78");
                    Console.WriteLine("- Og-Rok");
                    Console.WriteLine();
                    Console.WriteLine("Grazie anche ai collaboratori di Cosmos.");
                    Console.WriteLine("https://github.com/aura-systems/Aura-Operating-System");
                    break;
                    
                case "pl_PL":
                    Console.WriteLine("O Aura OS:");
                    Console.WriteLine("Aura Team stworzony przez Valentin CHARBONNIER i Alexy DA CRUZ.");
                    Console.WriteLine();
                    Console.WriteLine("Wspoltworcy:");
                    Console.WriteLine("- djlw78");
                    Console.WriteLine("- Og-Rok");
                    Console.WriteLine("Tlumaczenie: KaliMasterDev");
                    Console.WriteLine();
                    Console.WriteLine("Dziekujemy rowniez wspierajacym AuraOS.");
                    Console.WriteLine("https://github.com/aura-systems/Aura-Operating-System");
                    break;

            }
        }

    }
}
