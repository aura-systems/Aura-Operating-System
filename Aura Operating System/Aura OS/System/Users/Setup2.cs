/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Setup
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using Aura_OS.System.Security;
using Aura_OS.System.Computer;
using Aura_OS.System.Drawable;
using Aura_OS.System.Translation;
using Aura_OS.System.Utils;
using System.Collections.Generic;

namespace Aura_OS.System
{
    class Setup2
    {
        private string username;
        private string password;
        private string lang;
        private string hostname;

        private List<string> Users = new List<string>();

        public void RegisterHostname()
        {
            hostname = Text.Menu("computernamedialog");

            if ((hostname.Length >= 1) && (hostname.Length <= 15)) //15 char max for NETBIOS name resolution (dns)
            {
                Settings.PutValue("hostname", hostname);
            }
            else
            {
                Text.Menu("errorcomputer");
                RegisterHostname();
            }
        }

        public void RegisterUser()
        {
            Console.WriteLine("Username:");
            username = Console.ReadLine();
            Console.WriteLine("Password");
            password = MD5.hash(Console.ReadLine());
            Settings.PutValue("user:" + username, password + "|admin");
            Settings.PutValue("user:root", MD5.hash("root") + "|admin");
            Users.Add(username);
            Users.Add("root");
            CreateUserDirectories();
        }

        public void CreateUserDirectories()
        {
            foreach(string user in Users)
            {
                if (!Directory.Exists(@"0:\Users\" + user))
                    Directory.CreateDirectory(@"0:\Users\" + user);                
            }
        }

        public void RegisterLanguage()
        {
            string language = Menu.DispLanguageDialog();

            if ((language.Equals("en_US")) || language.Equals("en-US"))
            {
                Kernel.langSelected = "en_US";
                Keyboard.Init();
                Settings.PutValue("language", "en_US");
            }
            else if ((language.Equals("fr_FR")) || language.Equals("fr-FR"))
            {
                Kernel.langSelected = "fr_FR";
                Keyboard.Init();
                Settings.PutValue("language", "fr_FR");
            }
            else
            {
                RegisterLanguage();
            }
        }

        public void RegisterDefaults()
        {
            Settings.PutValue("foregroundcolor","7");
            Settings.PutValue("backgroundcolor", "0");
        }

        public void InitSetup()
        {
            InitDirs();

            if (!File.Exists(@"0:\System\settings.conf"))
            {
                File.Create(@"0:\System\settings.conf");
                RegisterLanguage();
                RegisterHostname();
                RegisterUser();
                RegisterDefaults();
                YesFileSystem();
            }
            else
            {
                YesFileSystem();
            }                
        }

        public void InitDirs()
        {
            try
            {
                string[] DefaultSystemDirectories =
                {
                    @"0:\System\",
                    @"0:\Users\"
                };

                foreach (string dirs in DefaultSystemDirectories)
                {
                    if (!Directory.Exists(dirs))
                        Directory.CreateDirectory(dirs);
                }
            }
            catch
            {
                NoFileSystem();
            }            
        }

        public void NoFileSystem() //logged with root without using filesystem
        {
            Kernel.SystemExists = false;
            Kernel.userLogged = "root";
            Kernel.Logged = true;
        }

        public void YesFileSystem() //not logged using filesystem
        {
            Kernel.SystemExists = false;
            Kernel.running = true;
        }

    }
}
