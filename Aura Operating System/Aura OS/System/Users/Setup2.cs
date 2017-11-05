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
    class Setup
    {
        private string username;
        private string password;
        private string lang;
        private string hostname;
        private string[] Users;

        private string FinalUsername;
        private string FinalPassword;
        private string FinalLang;
        private string FinalHostname;

        /// <summary>
        /// Verify filesystem
        /// </summary>
        /// <returns>"true", if we don't need to init setup</returns>
        /// <returns>"continue", if we need to init setup with FS</returns>
        /// <returns>"false", if there is not a FS</returns>
        public string FileSystem()
        {
            try
            {
                if (File.Exists(@"0:\System\settings.conf"))
                {
                    return "true";
                } else
                {
                    return "continue";
                }
            }
            catch
            {
                return "false";
            }            
        }

        /// <summary>
        /// Init setup and verify which mode we use to run Aura_OS (if we start setup or not) 
        /// </summary>
        public void InitSetup()
        {           
            if(FileSystem() == "false")
            {
                RunWithoutFS();
            }
            else if(FileSystem() == "true"){
                Run();
            }
            else if(FileSystem() == "continue")
            {
                
                RegisterLanguage();
                RegisterHostname();
                RegisterUser();
                Installation();
            }           
        }

        /// <summary>
        /// Method to register the hostname of the computer (computer name)
        /// </summary>
        public void RegisterHostname()
        {
            hostname = Text.Menu("computernamedialog");

            if ((hostname.Length >= 1) && (hostname.Length <= 15)) //15 char max for NETBIOS name resolution (dns)
            {
                FinalHostname = hostname;
            }
            else
            {
                Text.Menu("errorcomputer");
                RegisterHostname();
            }
        }

        /// <summary>
        /// Method to register a new user
        /// </summary>
        public void RegisterUser()
        {
            Console.Clear();

            string text = Text.Menu("setup");

            int middle = text.IndexOf("//////");
            username = text.Remove(middle, text.Length - middle);
            password = text.Remove(0, middle + 6);

            string tryusername = "";

            if (tryusername.StartsWith("user:" + username))
            {
                Text.Menu("alreadyuser");
                RegisterUser();
            }
            else
            {
                if ((username.Length >= 4) && (username.Length <= 20))
                {
                    if ((password.Length >= 6) && (password.Length <= 40))
                    {
                        //good
                        password = MD5.hash(password);
                        FinalUsername = "user:" + username;
                        FinalPassword = password;
                    }
                    else
                    {
                        Text.Menu("error2");
                        RegisterUser();
                    }
                }
                else
                {
                    Text.Menu("error3");
                    RegisterUser();
                }
            }            
        }

        /// <summary>
        /// Method called to create all users directories
        /// </summary>
        public void CreateUserDirectories(string[] Users)
        {
            foreach(string user in Users)
            {
                if (!Directory.Exists(@"0:\Users\" + user))
                    Directory.CreateDirectory(@"0:\Users\" + user);                
            }
        }

        /// <summary>
        /// Method to register the language that will be used on the computer
        /// </summary>
        public void RegisterLanguage()
        {
            string language = Menu.DispLanguageDialog();

            if ((language.Equals("en_US")) || language.Equals("en-US"))
            {
                Kernel.langSelected = "en_US";
                FinalLang = "en_US";
                Keyboard.Init();                
            }
            else if ((language.Equals("fr_FR")) || language.Equals("fr-FR"))
            {
                Kernel.langSelected = "fr_FR";
                FinalLang = "fr_FR";
                Keyboard.Init();
            }
            else
            {
                RegisterLanguage();
            }
        }

        /// <summary>
        /// Called to define default colors
        /// </summary>
        public void RegisterDefaults()
        {
            Settings.PutValue("foregroundcolor","7");
            Settings.PutValue("backgroundcolor", "0");

            Settings.PushValues();
        }

        /// <summary>
        /// Create defaults directories of the system
        /// </summary>
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
                RunWithoutFS();
            }            
        }

        /// <summary>
        /// Method called to start Aura_OS without using filesystem and loggged to "root"
        /// </summary>
        public void RunWithoutFS() //logged with root without using filesystem
        {
            Kernel.SystemExists = false;
            Kernel.userLogged = "root";
            Kernel.Logged = true;
        }

        /// <summary>
        /// Method called to start Aura_OS to run with filesystem and not logged to any user by default
        /// </summary>
        public void Run()
        {
            Console.Clear();
            Kernel.SystemExists = true;
            Kernel.userLogged = FinalUsername;
            Kernel.JustInstalled = true;
            Kernel.running = true;

            Console.Clear();

            WelcomeMessage.Display();
            Text.Display("logged", FinalUsername);

            Kernel.Logged = true;
        }

        public void Installation()
        {
            Menu.DispInstallationDialog(0);

            Menu.DispInstallationDialog(5);            

            InitDirs(); //create needed directories if they doesn't exist

            Menu.DispInstallationDialog(15);

            File.Create(@"0:\System\settings.conf");
            Settings.LoadValues();

            Menu.DispInstallationDialog(10);

            Settings.PutValue("hostname", FinalHostname);

            Menu.DispInstallationDialog(20);

            Settings.PutValue("user:" + FinalUsername, FinalPassword + "|admin");

            Menu.DispInstallationDialog(30);

            Settings.PutValue("user:root", MD5.hash("root") + "|admin");

            Menu.DispInstallationDialog(40);

            string[] Users = { "root", FinalUsername };
            CreateUserDirectories(Users);

            Menu.DispInstallationDialog(50);

            if ((FinalLang.Equals("en_US")) || FinalLang.Equals("en-US"))
            {
                Settings.PutValue("language", "en_US");
                Menu.DispInstallationDialog(60);
            }
            else if ((FinalLang.Equals("fr_FR")) || FinalLang.Equals("fr-FR"))
            {
                Settings.PutValue("language", "fr_FR");
                Menu.DispInstallationDialog(60);
            }

            Menu.DispInstallationDialog(80);

            RegisterDefaults();

            Menu.DispInstallationDialog(100);

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

            Run();
        }
    }
}
