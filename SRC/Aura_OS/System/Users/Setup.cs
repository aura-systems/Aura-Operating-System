/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Setup
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using Aura_OS.System.Filesystem;
using Aura_OS.System.Network;
using Aura_OS.System.Security;
using Aura_OS.System.Users;
using Aura_OS.System.Utils;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Network;

namespace Aura_OS.System
{
    class Setup
    {
        private string FinalUsername;
        private string FinalPassword;
        private string FinalLang;
        private string FinalHostname;
        private string FinalKeyboardLayout;

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
                Volumes.RefreshSystemVolume();

                if (AuraPaths.SystemVolume == null)
                {
                    Console.WriteLine("No FAT volume is mounted, AuraOS cannot be installed.");
                    Console.WriteLine("Use 'vol /lp' to list partitions, 'vol /mp' to create one and 'vol /fp' to format it.");
                    return;
                }

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
                // The username becomes a directory name and a passwd field.
                // GEN3-GAP(fat-names): the FAT driver accepts any character, so reject / \ : * ? " < > | here.
                if (!AuraPath.IsValidName(username))
                {
                    throw new Exception("Username contains invalid characters.");
                }

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
            }
            else if ((language.Equals("fr_FR")) || language.Equals("fr-FR"))
            {
                Kernel.langSelected = "fr_FR";
                FinalLang = "fr_FR";
            }
            else if ((language.Equals("nl_NL")) || language.Equals("nl-NL"))
            {
                Kernel.langSelected = "nl_NL";
                FinalLang = "nl_NL";
            }
            else if ((language.Equals("it_IT")) || language.Equals("it-IT"))
            {
                Kernel.langSelected = "it_IT";
                FinalLang = "it_IT";
            }
            else
            {
                // Unknown language: default to en_US (FinalLang must not stay null, a null deref halts gen3)
                Kernel.langSelected = "en_US";
                FinalLang = "en_US";
            }

            // fr_FR selects AZERTY; otherwise keep the live layout (setkeyboardmap / taskbar, "US" by default),
            // since the setup command always passes en-US. Installation persists it as keyboardLayout.
            FinalKeyboardLayout = FinalLang == "fr_FR" ? "FR" : Input.KeyboardLayouts.CurrentCode;
            if (string.IsNullOrEmpty(FinalKeyboardLayout))
            {
                FinalKeyboardLayout = "US";
            }
            Input.KeyboardLayouts.Set(FinalKeyboardLayout);
        }

        /// <summary>
        /// Create defaults directories of the system
        /// </summary>
        public void InitDirs()
        {
            string[] DefaultSystemDirectories =
                {
                    AuraPaths.SystemDir,
                    AuraPaths.ProgramsDir,
                    AuraPaths.ThemesDir,
                    AuraPaths.WallpapersDir,
                    AuraPaths.UsersDir
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
            if (Directory.Exists(AuraPaths.SystemDir))
            {
                // gen3 has no finalizers: never leave a File.Create stream undisposed (truncate/create instead)
                File.WriteAllText(AuraPaths.SettingsIni, string.Empty);
                File.WriteAllText(AuraPaths.Passwd, string.Empty);
            }
        }

        /// <summary>
        /// Method called to create all users directories
        /// </summary>
        public void CreateUserDirectories(string[] Users)
        {
            foreach (string user in Users)
            {
                if (!Directory.Exists(AuraPaths.UsersDir + user))
                {
                    Directory.CreateDirectory(AuraPaths.UsersDir + user);
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

            // GEN3-GAP(iso-files): theme assets are embedded resources (no ISO volume to copy from)
            Console.WriteLine("Copying SuaveSheet.bmp...");
            File.WriteAllBytes(AuraPaths.ThemesDir + "Suave.bmp", Files.Get("UI/Themes/SuaveSheet.bmp"));
            Console.WriteLine("Copying Suave.skin.xml...");
            File.WriteAllBytes(AuraPaths.ThemesDir + "Suave.xml", Files.Get("UI/Themes/Suave.skin.xml"));
            Console.WriteLine("Saving wallpaper-1.bmp...");
            Filesystem.Entries.SaveFile(AuraPaths.WallpapersDir + "w1.bmp", Files.Wallpaper);
            MemoryInfo.Collect();
            Console.WriteLine("Saving wallpaper-2.bmp...");
            Filesystem.Entries.SaveFile(AuraPaths.WallpapersDir + "w2.bmp", Files.Wallpaper2);
            MemoryInfo.Collect();

            Settings config = new Settings(AuraPaths.SettingsIni);

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

            config.PutValue("keyboardLayout", FinalKeyboardLayout);

            config.PutValue("hostname", FinalHostname);

            config.PutValue("setuptime", Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));

            config.PutValue("autologin", "false");

            config.PutValue("themeBmpPath", AuraPaths.ThemesDir + "Suave.bmp");
            config.PutValue("themeXmlPath", AuraPaths.ThemesDir + "Suave.xml");
            config.PutValue("windowsTransparency", "255");
            config.PutValue("taskbarTransparency", "255");
            // The display resolution, not the UI size (Kernel.ScreenWidth/Height are divided by the scale).
            config.PutValue("screenWidth", (Kernel.Canvas != null ? Kernel.Canvas.Width : (int)Kernel.ScreenWidth).ToString());
            config.PutValue("screenHeight", (Kernel.Canvas != null ? Kernel.Canvas.Height : (int)Kernel.ScreenHeight).ToString());
            config.PutValue("screenScale", Kernel.ScreenScale.ToString());
            config.PutValue("wallpaperPath", AuraPaths.WallpapersDir + "w1.bmp");

            config.PutValue("debugger", "off");

            for (int i = 0; i < NetworkManager.DeviceCount; i++)
            {
                // GEN3-GAP(nic-names): no interface names in gen3, Aura aliases adapters as eth{Index}
                string networkIni = AuraPaths.NetworkIni(NetworkHelper.AliasOf(NetworkManager.GetAdapter(i)));
                File.WriteAllText(networkIni, string.Empty);
                Settings settings = new Settings(networkIni);
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
            if (!string.IsNullOrEmpty(FinalHostname))
            {
                Cosmos.Kernel.System.Network.Config.DnsConfig.HostName = FinalHostname;
            }

            Console.WriteLine("Changing current directory to user directory...");
            Kernel.UserDirectory = AuraPaths.UsersDir + dirUsername + "/";
            Kernel.CurrentVolume = AuraPaths.SystemVolume;
            Kernel.CurrentDirectory = Kernel.UserDirectory;

            Console.WriteLine("AuraOS v" + Kernel.Version + "-" + Kernel.Revision + " is now installed on " + AuraPaths.SystemVolume + " :)");
        }
    }
}