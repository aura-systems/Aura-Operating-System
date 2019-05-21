/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Help infos
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Shell = Aura_OS.System.Shell;
using Cosmos.HAL.PCInformation;
using System;
using Aura_OS.System.Network;

namespace Aura_OS.System.Translation
{
    class List_Translation
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
                    
                    if(AConsole.KeyboardShortcuts.Close())
                    {
                        return;
                    }

                    Console.WriteLine();
                    Console.WriteLine("Commandes disponibles (2/2):");
                    Console.WriteLine("- time (récupère la date et l'heure)");
                    Console.WriteLine("- ipconfig (affiche les informations réseau)");
                    Console.WriteLine("- snake (lance le jeu Snake)");
                    Console.WriteLine("- md5 (affiche le hash des arguments)");
                    Console.WriteLine("- sha256 (affiche le hash des arguments)");
                    Console.WriteLine("- crash (crash Aura)");
                    Console.WriteLine("- crashcpu (crash CPU)");
                    Console.WriteLine("- lspci (liste les appareils pci)");
                    Console.WriteLine("- beep (beep)");
                    Console.WriteLine("- debug {args} (informations utiles au debug)");

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

                    if (AConsole.KeyboardShortcuts.Close())
                    {
                        return;
                    }

                    Console.WriteLine();
                    Console.WriteLine("Available commands (2/2):");
                    Console.WriteLine("- time (to get time and date)");
                    Console.WriteLine("- ipconfig (to get network information)");
                    Console.WriteLine("- snake (launch the game Snake)");
                    Console.WriteLine("- md5 (to display hash of arguments)");
                    Console.WriteLine("- sha256 (to display hash of arguments)");
                    Console.WriteLine("- crash (crash Aura)");
                    Console.WriteLine("- crashcpu (crash CPU)");
                    Console.WriteLine("- lspci (list pci devices)");
                    Console.WriteLine("- beep (beep)");
                    Console.WriteLine("- debug {args} (useful information for debugging)");

                    Console.ReadKey();

                    break;

                case "nl_NL":
                    Console.WriteLine("Mogelijke commando's (1/2):");
                    Console.WriteLine("- shutdown (om een ACPI-shutdown te doen)");
                    Console.WriteLine("- reboot (opnieuw opstarten met de CPU)");
                    Console.WriteLine("- clear (om de console leeg te maken)");
                    Console.WriteLine("- cd .. (om naar de bovenliggende map te navigeren)");
                    Console.WriteLine("- cd (om te navigeren naar een folder)");
                    Console.WriteLine("- dir (geeft een weergave van mappen en bestanden)");
                    Console.WriteLine("- ls (geeft een weergave van mappen en bestanden)");
                    Console.WriteLine("- cp (om een bestand naar een andere bestemming te kopiëren)");
                    Console.WriteLine("- mkdir (om een map te maken");
                    Console.WriteLine("- rmdir (om een map te verwijderen)");
                    Console.WriteLine("- mkfil (om een bestand aan te maken)");
                    Console.WriteLine("- rmfil (om een bestand te verwijderen)");
                    Console.WriteLine("- edit (om een bestand te bewerken)");
                    Console.WriteLine("- vol (geeft een lijst met volumes)");
                    Console.WriteLine("- echo (om tekst te echoën)");
                    Console.WriteLine("- systeminfo (om systeeminformatie te weergeven)");
                    Console.WriteLine("- ver (om de systeemversie weer te geven)");
                    Console.WriteLine("- settings {argumenten} (toegang tot instellingen)");
                    Console.WriteLine("- logout (om los te koppelen)");

                    if (AConsole.KeyboardShortcuts.Close())
                    {
                        return;
                    }
                    //page 2 (19 elements per page)

                    Console.WriteLine();
                    Console.WriteLine("Mogelijke commando's (2/2):");
                    Console.WriteLine("- time (om tijd en datum te verkrijgen)");
                    Console.WriteLine("- ipconfig (om netwerk informatie te verkrijgen)");
                    Console.WriteLine("- snake (start het spel Snake)");
                    Console.WriteLine("- md5 (om de hash van argumenten weer te geven)");
                    Console.WriteLine("- sha256 (om de hash van argumenten weer te geven)");
                    Console.WriteLine("- crash (crasht Aura)");
                    Console.WriteLine("- crashcpu (crasht CPU)");
                    Console.WriteLine("- lspci (lijst pci-apparaten)");
                    Console.WriteLine("- beep (beep)");
                    Console.WriteLine("- debug {args} (nuttige informatie voor debuggen)");

                    Console.ReadKey();

                    break;
                    
                    case "it_IT":
                    Console.WriteLine("Comandi disponibili (1/2):");
                    Console.WriteLine("- shutdown (per eseguire un ACPI shutdown)");
                    Console.WriteLine("- reboot (per riavviare la CPU)");
                    Console.WriteLine("- clear (per pulire la console)");
                    Console.WriteLine("- cd .. (per navigare la cartella principale)");
                    Console.WriteLine("- cd (per accedere and una cartella)");
                    Console.WriteLine("- dir (per elencare le cartelle ed i files)");
                    Console.WriteLine("- ls (per elencare le cartelle ed i file)");
                    Console.WriteLine("- cp (per copiare un file in un' altra destinazione)");
                    Console.WriteLine("- mkdir (per creare una cartella)");
                    Console.WriteLine("- rmdir (per eliminare una cartella))");
                    Console.WriteLine("- mkfil (per creare un file)");
                    Console.WriteLine("- rmfil (per eliminare un file)");
                    Console.WriteLine("- edit (per modificare un file)");
                    Console.WriteLine("- vol (per elencare i volumi)");
                    Console.WriteLine("- echo (per eseguire l'echo di  un testo)");
                    Console.WriteLine("- systeminfo (per visualizzare le informazioni di sistema)");
                    Console.WriteLine("- ver (per visualizzare la versione del sistema)");
                    Console.WriteLine("- settings {args} (accedi ai settaggi)");
                    Console.WriteLine("- logout (per disconnetterti)");
                    
                    Console.ReadKey(); //page 2 (19 elements per page)

                    Console.WriteLine();
                    Console.WriteLine("Comandi disponibili (2/2):");
                    Console.WriteLine("- time (per visualizzare data e ora)");
                    Console.WriteLine("- ipconfig (per visualizzare le informazioni di rete)");
                    Console.WriteLine("- snake (avvia il gioco Snake)");
                    Console.WriteLine("- md5 (per visualizzare l'hash degli argomenti)");
                    Console.WriteLine("- sha256 (per visualizzare l'hash degli argomenti)");
                    Console.WriteLine("- crash (crash Aura)");
                    Console.WriteLine("- crashcpu (crash CPU)");
                    Console.WriteLine("- lspci (elenco dispositivi PCI)");
                    Console.WriteLine("- beep (beep)");
                    Console.WriteLine("- debug {args} (informazioni utili per il debug)");

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
            }
        }

        /// <summary>
        /// Display help on settings commands.
        /// </summary>
        public static void Systeminfo()
        {
            Utils.Settings config = new Utils.Settings(@"0:\System\settings.conf");
            switch (Kernel.langSelected)
            {
                case "fr_FR":
                    Console.WriteLine("Nom du PC:                     " + Kernel.ComputerName);
                    Console.WriteLine("Nom du système d'exploitation: Aura");
                    Console.WriteLine("Version du système:            " + Kernel.version);
                    Console.WriteLine("Révision du système:           " + Kernel.revision);
                    Console.WriteLine("Date et heure:                 " + Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));
                    if (Kernel.SystemExists)
                    {
                        Console.WriteLine("Date d'installation originale: " + config.GetValue("setuptime"));
                    }
                    Console.WriteLine("Heure de démarrage du système: " + Kernel.boottime);
                    Console.WriteLine("Mémoire totale:                " + Core.MemoryManager.TotalMemory + "MB");
                    Console.WriteLine("Mémoire utilisée:              " + Core.MemoryManager.GetUsedMemory() + "MB");
                    Console.WriteLine("Mémoire restante:              " + Core.MemoryManager.GetFreeMemory() + "MB");
                    Console.WriteLine("Processeur(s):                 " + Computer.Info.GetNumberOfCPU() + " processeur(s) installé(s).");
                    int i = 1;
                    foreach (Processor processor in Computer.CPUInfo.Processors)
                    {
                        Console.WriteLine("[" + i + "] : " + processor.GetBrandName() + (int)processor.Frequency + " Mhz");
                        i++;
                    }
                    Computer.CPUInfo.Processors.Clear();
                    Console.WriteLine("Mode de la console:            " + Kernel.AConsole.Name);
                    break;

                case "en_US":
                    Console.WriteLine("Computer name:             " + Kernel.ComputerName);
                    Console.WriteLine("Operating system name:     Aura");
                    Console.WriteLine("Operating system version:  " + Kernel.version);
                    Console.WriteLine("Operating system revision: " + Kernel.revision);
                    Console.WriteLine("Date and time:             " + Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));
                    if (Kernel.SystemExists)
                    {
                        Console.WriteLine("Original Install Date:     " + config.GetValue("setuptime"));
                    }
                    Console.WriteLine("System Boot Time:          " + Kernel.boottime);
                    Console.WriteLine("Total memory:              " + Core.MemoryManager.TotalMemory + "MB");
                    Console.WriteLine("Used memory:               " + Core.MemoryManager.GetUsedMemory() + "MB");
                    Console.WriteLine("Free memory:               " + Core.MemoryManager.GetFreeMemory() + "MB");
                    Console.WriteLine("Processor(s):              " + Computer.Info.GetNumberOfCPU() + " installed processor(s).");
                    int j = 1;
                    foreach (Processor processor in Computer.CPUInfo.Processors)
                    {
                        Console.WriteLine("[" + j + "] : " + processor.GetBrandName() + (int)processor.Frequency + " Mhz");
                        j++;
                    }
                    Computer.CPUInfo.Processors.Clear();
                    Console.WriteLine("Console mode:              " + Kernel.AConsole.Name);
                    break;

                case "nl_NL":
                    Console.WriteLine("Naam computer:             " + Kernel.ComputerName);
                    Console.WriteLine("Naam besturingssysteem:    Aura");
                    Console.WriteLine("Versie besturingssysteem:  " + Kernel.version);
                    Console.WriteLine("Revisie besturingssysteem: " + Kernel.revision);
                    Console.WriteLine("Datum en tijd:             " + Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));
                    if (Kernel.SystemExists)
                    {
                        Console.WriteLine("Installeer datum:          " + config.GetValue("setuptime"));
                    }
                    Console.WriteLine("Starttijd van het systeem: " + Kernel.boottime);
                    Console.WriteLine("Totaal geheugen:           " + Core.MemoryManager.TotalMemory + "MB");
                    Console.WriteLine("Gebruikt geheugen:         " + Core.MemoryManager.GetUsedMemory() + "MB");
                    Console.WriteLine("Gratis geheugen:           " + Core.MemoryManager.GetFreeMemory() + "MB");
                    Console.WriteLine("Processor(s):              " + Computer.Info.GetNumberOfCPU() + " processor(s) geïnstalleerd.");
                    int k = 1;
                    foreach (Processor processor in Computer.CPUInfo.Processors)
                    {
                        Console.WriteLine("[" + k + "] : " + processor.GetBrandName() + (int)processor.Frequency + " Mhz");
                        k++;
                    }
                    Computer.CPUInfo.Processors.Clear();
                    Console.WriteLine("Consolewijze:              " + Kernel.AConsole.Name);
                    break;

                case "it_IT":
                    Console.WriteLine("Nome del computer:               " + Kernel.ComputerName);
                    Console.WriteLine("Nome del sistema operativo:      Aura");
                    Console.WriteLine("Versione del sistema operativo:  " + Kernel.version);
                    Console.WriteLine("Revisione del sistema operativo: " + Kernel.revision);
                    Console.WriteLine("Data e ora:                      " + Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));
                    if (Kernel.SystemExists)
                    {
                        Console.WriteLine("Data Installazione Sistema:      " + config.GetValue("setuptime"));
                    }
                    Console.WriteLine("System Boot Time:                " + Kernel.boottime);
                    Console.WriteLine("Memoria totale:                  " + Core.MemoryManager.TotalMemory + "MB");
                    Console.WriteLine("Memoria utilizzata:              " + Core.MemoryManager.GetUsedMemory() + "MB");
                    Console.WriteLine("Memoria libera:                  " + Core.MemoryManager.GetFreeMemory() + "MB");
                    Console.WriteLine("Processor(s):                    " + Computer.Info.GetNumberOfCPU() + " installed processor(s).");
                    int h = 1;
                    foreach (Processor processor in Computer.CPUInfo.Processors)
                    {
                        Console.WriteLine("[" + h + "] : " + processor.GetBrandName() + (int)processor.Frequency + " Mhz");
                        h++;
                    }
                    Computer.CPUInfo.Processors.Clear();
                    break;
            }
        }

        /// <summary>
        /// Display informations about Aura OS
        /// </summary>
        public static void About()
        {
            switch (Kernel.langSelected)
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

            }
        }

        /// <summary>
        /// Display IP Configuration and MAC Address
        /// </summary>
        public static void Ipconfig()
        {
            switch (Kernel.langSelected)
            {
                case "fr_FR":
                    int counter = 0;
                    foreach (HAL.Drivers.Network.NetworkDevice device in NetworkConfig.Keys)
                    {
                        switch (device.CardType)
                        {
                            case HAL.Drivers.Network.CardType.Ethernet:
                                Console.WriteLine("Carte Ethernet " + counter + " - " + device.Name);
                                break;
                            case HAL.Drivers.Network.CardType.Wireless:
                                Console.WriteLine("Carte réseau sans fil " + counter + " :");
                                break;
                        }
                        Utils.Settings settings = new Utils.Settings(@"0:\System\" + device.Name + ".conf");
                        Console.WriteLine("Adresse MAC           : " + device.MACAddress.ToString());
                        Console.WriteLine("Adresse IP            : " + Kernel.LocalNetworkConfig.IPAddress.ToString());
                        Console.WriteLine("Masque de sous-réseau : " + Kernel.LocalNetworkConfig.SubnetMask.ToString());
                        Console.WriteLine("Passerelle par défaut : " + Kernel.LocalNetworkConfig.DefaultGateway.ToString());
                        Console.WriteLine("Serveur DNS préféré   : " + Kernel.LocalNetworkConfig.PreferredDNS.ToString());
                        counter++;
                    }
                    counter = 0;
                    break;

                case "en_US":
                    int counter1 = 0;
                    foreach (HAL.Drivers.Network.NetworkDevice device in NetworkConfig.Keys)
                    {
                        switch (device.CardType)
                        {
                            case HAL.Drivers.Network.CardType.Ethernet:
                                Console.WriteLine("Ethernet Card  " + counter1 + " - " + device.Name);
                                break;
                            case HAL.Drivers.Network.CardType.Wireless:
                                Console.WriteLine("Wireless Card " + counter1 + " - " + device.Name);
                                break;
                        }
                        Utils.Settings settings = new Utils.Settings(@"0:\System\" + device.Name + ".conf");
                        Console.WriteLine("MAC Address          : " + device.MACAddress.ToString());
                        Console.WriteLine("IP Address           : " + Kernel.LocalNetworkConfig.IPAddress.ToString());
                        Console.WriteLine("Subnet mask          : " + Kernel.LocalNetworkConfig.SubnetMask.ToString());
                        Console.WriteLine("Default Gateway      : " + Kernel.LocalNetworkConfig.DefaultGateway.ToString());
                        Console.WriteLine("Preferred DNS server : " + Kernel.LocalNetworkConfig.PreferredDNS.ToString());
                        counter1++;
                    }
                    counter1 = 0;
                    break;

                case "nl_NL":
                    int counter2 = 0;
                    foreach (HAL.Drivers.Network.NetworkDevice device in NetworkConfig.Keys)
                    {
                        switch (device.CardType)
                        {
                            case HAL.Drivers.Network.CardType.Ethernet:
                                Console.WriteLine("Ethernetkaart " + counter2 + " - " + device.Name);
                                break;
                            case HAL.Drivers.Network.CardType.Wireless:
                                Console.WriteLine("Draadloze Netwerkkaart " + counter2 + " :");
                                break;
                        }
                        Utils.Settings settings = new Utils.Settings(@"0:\System\" + device.Name + ".conf");
                        Console.WriteLine("MAC-adres           : " + device.MACAddress.ToString());
                        Console.WriteLine("IP-adres            : " + Kernel.LocalNetworkConfig.IPAddress.ToString());
                        Console.WriteLine("Subnetmasker        : " + Kernel.LocalNetworkConfig.SubnetMask.ToString());
                        Console.WriteLine("Standaardgateway    : " + Kernel.LocalNetworkConfig.DefaultGateway.ToString());
                        Console.WriteLine("Voorkeur DNS-server : " + Kernel.LocalNetworkConfig.PreferredDNS.ToString());
                        counter2++;
                    }
                    counter2 = 0;
                    break;

                case "it_IT":
                    int counter3 = 0;
                    foreach (HAL.Drivers.Network.NetworkDevice device in NetworkConfig.Keys)
                    {
                        switch (device.CardType)
                        {
                            case HAL.Drivers.Network.CardType.Ethernet:
                                Console.WriteLine("Scheda Ethernet " + counter3 + " - " + device.Name);
                                break;
                            case HAL.Drivers.Network.CardType.Wireless:
                                Console.WriteLine("Scheda di rete senza fili " + counter3 + " :");
                                break;
                        }
                        Utils.Settings settings = new Utils.Settings(@"0:\System\" + device.Name + ".conf");
                        Console.WriteLine("Indirizzo MAC         : " + device.MACAddress.ToString());
                        Console.WriteLine("Indirizzo IP          : " + Kernel.LocalNetworkConfig.IPAddress.ToString());
                        Console.WriteLine("Maschera di sottorete : " + Kernel.LocalNetworkConfig.SubnetMask.ToString());
                        Console.WriteLine("Gateway predefinito   : " + Kernel.LocalNetworkConfig.DefaultGateway.ToString());
                        Console.WriteLine("Server DNS preferito  : " + Kernel.LocalNetworkConfig.PreferredDNS.ToString());
                        counter3++;
                    }
                    counter3 = 0;
                    break;

            }
        }

    }
}
