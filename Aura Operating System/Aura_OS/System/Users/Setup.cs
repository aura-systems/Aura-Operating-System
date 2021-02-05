/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Setup
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using Aura_OS.System.Security;
using Aura_OS.System.Drawable;
using Aura_OS.System.Translation;
using Aura_OS.System.Utils;
using Cosmos.HAL;

namespace Aura_OS.System
{
    class Setup
    {
        private string username;
        private string password;
        private string hostname;

        private string FinalUsername;
        private string FinalPassword;
        private string FinalLang;
        private string FinalHostname;

        private bool noinstallnofs = false;

        /// <summary>
        /// Verify filesystem
        /// </summary>
        /// <returns>"true", if we don't need to init setup</returns>
        /// <returns>"continue", if we need to init setup with FS</returns>
        /// <returns>"false", if there is not a FS</returns>
        public static string FileSystem()
        {
            if (Kernel.ContainsVolumes())
            {
                if (File.Exists(@"0:\System\settings.conf"))
                {
                    return "true";
                }
                else
                {
                    return "continue";
                }
            }
            else
            {
                return "false";
            }
        }

        /// <summary>
        /// Init setup and verify which mode we use to run Aura_OS (if we start setup or not) 
        /// </summary>
        public void InitSetup()
        {
            RunWithoutFS(false); //TODO enable installation back
            /*if (FileSystem() == "false")
            {
                RunWithoutFS(false);
            }
            else if (FileSystem() == "true")
            {
                Kernel.SystemExists = true;
            }
            else if (FileSystem() == "continue")
            {
                RegisterLanguage();
                RegisterUser();
                if (!noinstallnofs)
                {
                    RegisterHostname();
                    Installation();
                }
            }*/
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

            if (username == "root" || password == "root")
            {
                noinstallnofs = true;
                RunWithoutFS(true);
            }
            else
            {
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
                            password = Sha256.hash(password);
                            FinalUsername = username;
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
        }

        /// <summary>
        /// Method called to create all users directories
        /// </summary>
        public void CreateUserDirectories(string[] Users)
        {
            foreach (string user in Users)
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
            else if ((language.Equals("nl_NL")) || language.Equals("nl-NL"))
            {
                Kernel.langSelected = "nl_NL";
                FinalLang = "nl_NL";
                Keyboard.Init();
            }
            else if ((language.Equals("it_IT")) || language.Equals("it-IT"))
            {
                Kernel.langSelected = "it_IT";
                FinalLang = "it_IT";
                Keyboard.Init();
            }
            else
            {
                RegisterLanguage();
            }
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
                RunWithoutFS(false);
            }
        }

        /// <summary>
        /// Method called to start Aura_OS without using filesystem and loggged to "root"
        /// </summary>
        public void RunWithoutFS(bool nofsroot) //logged with root without using filesystem
        {
            if (!nofsroot)
            {
                RegisterLanguage();
            }

            Kernel.SystemExists = false;
            Kernel.userLogged = "root";
            Kernel.Logged = true;
            Console.Clear();
            //switch (Video.GetVideo())
            //{
            //    case "VGATextmode":
            //        //Kernel.AConsole = new System.Shell.VGA.VGAConsole(null);
            //        break;
            //    case "SVGA":
            //        // TO DO ?
            //        break;
            //    case "VESA":
            //        //Kernel.AConsole = new System.Shell.VESAVBE.VESAVBEConsole();
            //        break;
            //    default:
            //        //Kernel.AConsole = new System.Shell.VGA.VGAConsole(null);
            //        break;
            //}
            WelcomeMessage.Display();
            Text.Display("logged", "root");

            if (Kernel.ContainsVolumes() == false)
            {
                Text.Display("nofilesystem");
            }

            Console.WriteLine();
            Kernel.running = true;
        }

        /// <summary>
        /// Method called to start Aura_OS to run with filesystem and not logged to any user by default
        /// </summary>
        public void Run()
        {
            Console.Clear();

            Kernel.userLogged = username;
            Kernel.JustInstalled = true;
            Kernel.running = true;

            Console.Clear();
            //switch (Video.GetVideo())
            //{
            //    case "VGATextmode":
            //        //Kernel.AConsole = new System.Shell.VGA.VGAConsole(null);
            //        break;
            //    case "SVGA":
            //        // TO DO ?
            //        break;
            //    case "VESA":
            //        //Kernel.AConsole = new System.Shell.VESAVBE.VESAVBEConsole();
            //        break;
            //    default:
            //        //Kernel.AConsole = new System.Shell.VGA.VGAConsole(null);
            //        break;
            //}
            WelcomeMessage.Display();
            Text.Display("logged", username);

            Console.WriteLine();

            Kernel.Logged = true;
        }

        /// <summary>
        /// Installation with progressbar.
        /// </summary>
        public void Installation()
        {
            Menu.DispInstallationDialog(0);

            Menu.DispInstallationDialog(5);

            InitDirs(); //create needed directories if they doesn't exist

            Menu.DispInstallationDialog(10);

            File.Create(@"0:\System\settings.conf");
            File.Create(@"0:\System\passwd");

            Menu.DispInstallationDialog(15);

            System.Users.Users.LoadUsers();

            Menu.DispInstallationDialog(20);

            System.Users.Users.PutUser("user:" + FinalUsername, FinalPassword + ":admin");

            Menu.DispInstallationDialog(30);

            System.Users.Users.PutUser("user:root", Sha256.hash("root") + ":admin");

            Menu.DispInstallationDialog(40);

            string[] Users = { "root", FinalUsername };
            CreateUserDirectories(Users);

            Utils.Settings config = new Utils.Settings(@"0:\System\settings.conf");

            Menu.DispInstallationDialog(50);

            if ((FinalLang.Equals("en_US")) || FinalLang.Equals("en-US"))
            {
                config.PutValue("language", "en_US");
                Menu.DispInstallationDialog(60);
            }
            else if ((FinalLang.Equals("fr_FR")) || FinalLang.Equals("fr-FR"))
            {
                config.PutValue("language", "fr_FR");
                Menu.DispInstallationDialog(60);
            }
            else if ((FinalLang.Equals("nl_NL")) || FinalLang.Equals("nl-NL"))
            {
                config.PutValue("language", "nl_NL");
                Menu.DispInstallationDialog(60);
            }
            else if ((FinalLang.Equals("it_IT")) || FinalLang.Equals("it-IT"))
            {
                config.PutValue("language", "it_IT");
                Menu.DispInstallationDialog(60);
            }

            config.PutValue("hostname", FinalHostname);

            Menu.DispInstallationDialog(70);

            config.PutValue("setuptime", Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true));

            config.PutValue("consolemode", "null");

            Menu.DispInstallationDialog(80);

            Kernel.SystemExists = true;

            config.PutValue("debugger", "off");

            foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
            {
                File.Create(@"0:\System\" + networkDevice.Name + ".conf");
                Settings settings = new Settings(@"0:\System\" + networkDevice.Name + ".conf");
                settings.Add("ipaddress", "0.0.0.0");
                settings.Add("subnet", "0.0.0.0");
                settings.Add("gateway", "0.0.0.0");
                settings.Add("dns01", "0.0.0.0");
                settings.Push();
            }
            
            config.PushValues();

            Menu.DispInstallationDialog(90);

            System.Users.Users.PushUsers();

            Menu.DispInstallationDialog(100);

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

            Run();
        }

    }
}
