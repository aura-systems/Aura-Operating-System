/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Setup
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Aura_OS.System.Security;
using Aura_OS.System.Users;
using Aura_OS.System.Utils;
using Cosmos.Kernel.System.Network.Config;

namespace Aura_OS.System
{
    class Setup
    {
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
        public static string InstallExists()
        {
            if (Kernel.Installed)
            {
                return "true";
            }
            else
            {
                return "continue";
            }
        }

        /// <summary>
        /// Init setup and verify which mode we use to run Aura_OS (if we start setup or not) 
        /// </summary>
        public void InitSetup(string username, string password, string hostname, string language)
        {
            string state = InstallExists();

            if (state == "true")
            {
                Console.WriteLine("Install already exists.");
            }
            if (InstallExists() == "continue")
            {
                RegisterLanguage(language);
                RegisterUser(username, password);
                RegisterHostname(hostname);
                Installation();
            }
        }

        /// <summary>
        /// Method to register the hostname of the computer (computer name)
        /// </summary>
        public void RegisterHostname(string hostname)
        {
            if ((hostname.Length >= 1) && (hostname.Length <= 15)) //15 char max for NETBIOS name resolution (dns)
            {
                FinalHostname = hostname;
            }
        }

        /// <summary>
        /// Method to register a new user
        /// </summary>
        public void RegisterUser(string username, string password)
        {
            if ((username.Length >= 4) && (username.Length <= 20))
            {
                if ((password.Length >= 6) && (password.Length <= 40))
                {
                    FinalUsername = username;
                    FinalPassword = Sha256.hash(password);
                }
                else
                {
                    throw new Exception("Password too weak.");
                }
            }
            else
            {
                throw new Exception("Username too short or too big.");
            }
        }

        /// <summary>
        /// Method to register the language that will be used on the computer
        /// </summary>
        public void RegisterLanguage(string language)
        {
            if ((language.Equals("en_US")) || language.Equals("en-US"))
            {
                Kernel.langSelected = "en_US";
                FinalLang = "en_US";
                //Keyboard.Init();
            }
            else if ((language.Equals("fr_FR")) || language.Equals("fr-FR"))
            {
                Kernel.langSelected = "fr_FR";
                FinalLang = "fr_FR";
                //Keyboard.Init();
            }
            else if ((language.Equals("nl_NL")) || language.Equals("nl-NL"))
            {
                Kernel.langSelected = "nl_NL";
                FinalLang = "nl_NL";
                //Keyboard.Init();
            }
            else if ((language.Equals("it_IT")) || language.Equals("it-IT"))
            {
                Kernel.langSelected = "it_IT";
                FinalLang = "it_IT";
                //Keyboard.Init();
            }
        }

        /// <summary>
        /// Create defaults directories of the system
        /// </summary>
        public void InitDirs()
        {
            string[] DefaultSystemDirectories =
                {
                    Kernel.RootVolume + "/System",
                    Kernel.RootVolume + "/System/Programs",
                    Kernel.RootVolume + "/System/Themes",
                    Kernel.RootVolume + "/System/Wallpapers",
                    Kernel.RootVolume + "/Users"
                };

            foreach (string dirs in DefaultSystemDirectories)
            {
                if (!Directory.Exists(dirs))
                    Directory.CreateDirectory(dirs);
            }
        }

        /// <summary>
        /// Create defaults directories of the system
        /// </summary>
        public void InitFiles()
        {
            if (Directory.Exists(Kernel.RootVolume + "/System"))
            {
                // Dispose immediately: gen3 has no finalizers, an undisposed FileStream
                // permanently leaks one of the 64 file descriptors.
                File.Create(Kernel.RootVolume + "/System/settings.ini").Dispose();
                File.Create(Kernel.RootVolume + "/System/passwd").Dispose();
            }
        }

        /// <summary>
        /// Method called to create all users directories
        /// </summary>
        public void CreateUserDirectories(string[] Users)
        {
            foreach (string user in Users)
            {
                if (!Directory.Exists(Kernel.RootVolume + "/Users/" + user))
                {
                    Directory.CreateDirectory(Kernel.RootVolume + "/Users/" + user);
                    System.Users.Users.InitUserDirs(user);
                }

            }
        }

        /// <summary>
        /// Installation with progressbar.
        /// </summary>
        public void Installation()
        {
            Console.WriteLine("Creating files and directories...");
            InitDirs(); //create needed directories if they doesn't exist
            InitFiles();

            Console.WriteLine("Creating user config...");
            System.Users.Users.LoadUsers();

            System.Users.Users.PutUser("user:" + FinalUsername, FinalPassword + ":admin");
            System.Users.Users.PutUser("user:root", Sha256.hash("root") + ":admin");

            Console.WriteLine("Creating user directories...");

            string dirUsername = FinalUsername;
            if (dirUsername.Length > 11)
            {
                dirUsername = dirUsername.Substring(0, 11);
            }
            string[] Users = { "root", dirUsername };
            CreateUserDirectories(Users);

            // gen2 copied the theme assets from the boot ISO (Files.IsoVolume); gen3 has no
            // ISO9660 driver, so they are embedded resources written out at install time.
            Console.WriteLine("Copying SuaveSheet.bmp...");
            Filesystem.Entries.SaveFile(Kernel.RootVolume + "/System/Themes/Suave.bmp", Files.GetUiResource("Themes/SuaveSheet.bmp"));
            Console.WriteLine("Copying Suave.skin.xml...");
            Filesystem.Entries.SaveFile(Kernel.RootVolume + "/System/Themes/Suave.xml", Files.GetUiResource("Themes/Suave.skin.xml"));
            Console.WriteLine("Saving wallpaper-1.bmp...");
            Filesystem.Entries.SaveFile(Kernel.RootVolume + "/System/Wallpapers/w1.bmp", Files.Wallpaper);
            GC.Collect();
            Console.WriteLine("Saving wallpaper-2.bmp...");
            Filesystem.Entries.SaveFile(Kernel.RootVolume + "/System/Wallpapers/w2.bmp", Files.Wallpaper2);
            GC.Collect();

            Settings config = new Settings(Kernel.RootVolume + "/System/settings.ini");

            if ((FinalLang.Equals("en_US")) || FinalLang.Equals("en-US"))
            {
                config.PutValue("language", "en_US");

            }
            else if ((FinalLang.Equals("fr_FR")) || FinalLang.Equals("fr-FR"))
            {
                config.PutValue("language", "fr_FR");

            }
            else if ((FinalLang.Equals("nl_NL")) || FinalLang.Equals("nl-NL"))
            {
                config.PutValue("language", "nl_NL");

            }
            else if ((FinalLang.Equals("it_IT")) || FinalLang.Equals("it-IT"))
            {
                config.PutValue("language", "it_IT");
            }

            config.PutValue("hostname", FinalHostname);

            config.PutValue("setuptime", Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));

            config.PutValue("autologin", "false");

            config.PutValue("themeBmpPath", Kernel.RootVolume + "/System/Themes/Suave.bmp");
            config.PutValue("themeXmlPath", Kernel.RootVolume + "/System/Themes/Suave.xml");
            config.PutValue("windowsTransparency", "255");
            config.PutValue("taskbarTransparency", "255");
            config.PutValue("screenWidth", Kernel.ScreenWidth.ToString());
            config.PutValue("screenHeight", Kernel.ScreenHeight.ToString());
            config.PutValue("wallpaperPath", Kernel.RootVolume + "/System/Wallpapers/w1.bmp");

            config.PutValue("debugger", "off");

            // gen2: NetworkConfiguration.NetworkConfigs + Device.NameID;
            // gen3: NetworkConfigManager.NetworkConfigs + INetworkDevice.Name.
            foreach (NetworkConfigEntry networkConfig in NetworkConfigManager.NetworkConfigs)
            {
                string nicConfigPath = Kernel.RootVolume + "/System/" + networkConfig.Device.Name + ".ini";
                File.Create(nicConfigPath).Dispose();
                Settings settings = new Settings(nicConfigPath);
                settings.Add("ipaddress", "0.0.0.0");
                settings.Add("subnet", "0.0.0.0");
                settings.Add("gateway", "0.0.0.0");
                settings.Add("dns01", "0.0.0.0");
                settings.Push();
            }

            Console.WriteLine("Saving user configuration...");

            config.PushValues();

            System.Users.Users.PushUsers();

            Kernel.userLogged = FinalUsername;
            Kernel.ComputerName = FinalHostname;

            Console.WriteLine("Changing current directory to user directory...");
            Kernel.UserDirectory = Kernel.RootVolume + "/Users/" + dirUsername + "/";
            Kernel.CurrentDirectory = Kernel.UserDirectory;

            Console.WriteLine("AuraOS v" + Kernel.Version + "-" + Kernel.Revision + " is now installed on " + Kernel.RootVolume + " :)");
        }
    }
}