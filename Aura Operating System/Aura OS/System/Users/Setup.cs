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

namespace Aura_OS.System
{
    class Setup
    {

        /// <summary>
        /// Verify if Aura's Setup is complete
        /// </summary>
        public void SetupVerifyCompleted()
        {
            try
            {
                File.Create(@"0:\filesystem.sys");
                if (File.Exists(@"0:\filesystem.sys"))
                {
                    File.Delete(@"0:\filesystem.sys");
                    if (!File.Exists(@"0:\System\setup.set"))
                    {
                        Kernel.SystemExists = false;
                        StartSetup(true);
                    }
                    else
                    {
                        Kernel.SystemExists = true;
                    }
                }
                else
                {
                    StartSetup(false);
                }
            }
            catch
            {
                StartSetup(false);
            }
        }

        /// <summary>
        /// Start setup
        /// </summary>
        public void StartSetup(bool filesystemexists)
        {
            Step3(filesystemexists);
        }

        /// <summary>
        /// Making directories that are required for Aura.
        /// </summary>
        private void Step1()
        {
            //creating folders

            try
            {
                InitDefaults();

                if (!File.Exists(@"0:\System\color.set"))
                {
                    File.Create(@"0:\System\color.set");
                    File.WriteAllText(@"0:\System\color.set", "7");
                }

                Info.setComputerName("aura-pc");

                if ((Directory.Exists(@"0:\System")) && (Directory.Exists(@"0:\System\Users")) && (Directory.Exists(@"0:\Users")) && (Directory.Exists(@"0:\Users\root")))
                {
                    Step2();
                }
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
                Menu.DispErrorDialog("Error while creating system folders.");
            }
        }

        #region Defaults
        public void InitDefaults()
        {
            string[] DefaultDirctories =
            {
                "System",
                "System\\Users",
                "Users",
                "Users\\root",
            };
            foreach (string dirs in DefaultDirctories)
                if (!Directory.Exists(dirs))
                    Directory.CreateDirectory(dirs);
        }
        #endregion Defaults

        /// <summary>
        /// Method called to make "root"
        /// </summary>
        private void Step2()
        {
            try
            {
                if (!File.Exists(@"0:\System\Users\root.usr"))
                {
                    File.Create(@"0:\System\Users\root.usr");

                    try
                    {
                        if (File.Exists(@"0:\System\Users\root.usr"))
                        {
                            string text = "root";
                            string md5psw = MD5.hash(text);
                            File.WriteAllText(@"0:\System\Users\root.usr", md5psw + "|admin");
                        }
                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Menu.DispErrorDialog("[ERROR] Root writing file");
                    }
                }
            }
            catch
            {
                Menu.DispErrorDialog("Error while creating root.");
            }
        }


        /// <summary>
        /// Asking user for his language
        /// </summary>
        private void Step3(bool filesystemexists)
        {
            string language = Menu.DispLanguageDialog();

            if ((language.Equals("en_US")) || language.Equals("en-US"))
            {
                Kernel.langSelected = "en_US";
                Keyboard.Init();
                if (filesystemexists == true)
                {
                    Step4();
                }
                else if (filesystemexists == false)
                {
                    RootLogin();
                }
            }
            else if ((language.Equals("fr_FR")) || language.Equals("fr-FR"))
            {
                Kernel.langSelected = "fr_FR";
                Keyboard.Init();
                if (filesystemexists == true)
                {
                    Step4();
                }
                else if (filesystemexists == false)
                {
                    RootLogin();
                }
            }
            else
            {
                Step3(filesystemexists);
            }
        }

        string step5_computername;

        /// <summary>
        /// Asking user to choose a name for his computer
        /// </summary>
        private void Step5(string user)
        {
            string computername = Text.Menu("computernamedialog");

            if ((computername.Length >= 1) && (computername.Length <= 15)) //15 char max for NETBIOS name resolution (dns)
            {
                step5_computername = computername;
                Step6(user);
            }
            else
            {
                Text.Menu("errorcomputer");
                Step5(user);
            }
        }


        /// <summary>
        /// Method called to validate the setup.
        /// </summary>
        private void Step6(string user)
        {
            Installation();

            Kernel.userLogged = user;
            Console.Clear();

            WelcomeMessage.Display();
            Text.Display("logged", user);

            Console.WriteLine();
            Kernel.SystemExists = true;
            Kernel.Logged = true;
        }

        private void Installation()
        {

            Menu.DispInstallationDialog(0);

            Step1();

            Menu.DispInstallationDialog(10);

            switch (Kernel.langSelected)
            {
                case "en_US":
                    File.Create(@"0:\System\lang.set");
                    if (File.Exists(@"0:\System\lang.set"))
                    {
                        File.WriteAllText(@"0:\System\lang.set", Kernel.langSelected);
                        Menu.DispInstallationDialog(20);
                    }
                    else
                    {
                        Menu.DispErrorDialog("The language configuration already exists!");
                    }
                    break;
                case "fr_FR":
                    File.Create(@"0:\System\lang.set");
                    if (File.Exists(@"0:\System\lang.set"))
                    {
                        File.WriteAllText(@"0:\System\lang.set", Kernel.langSelected);
                        Menu.DispInstallationDialog(20);
                    }
                    else
                    {
                        Menu.DispErrorDialog("La configuration des langue existe déjà!");
                    }
                    break;
            }

            File.Create(@"0:\System\Users\" + step4_user + ".usr");

            Menu.DispInstallationDialog(30);

            Directory.CreateDirectory(@"0:\Users\" + step4_user);

            Menu.DispInstallationDialog(40);

            InitDefaults(step4_user);

            Menu.DispInstallationDialog(50);

            if (File.Exists(@"0:\System\Users\" + step4_user + ".usr"))
            {
                File.WriteAllText(@"0:\System\Users\" + step4_user + ".usr", step4_pass + "|standard");

                Menu.DispInstallationDialog(60);

            }
            else
            {
                Text.Menu("error1");
            }

            Info.setComputerName(step5_computername);

            Menu.DispInstallationDialog(70);

            File.Create(@"0:\System\setup.set");

            Menu.DispInstallationDialog(80);

            Kernel.langSelected = File.ReadAllText(@"0:\System\lang.set");

            #region Language

            Keyboard.Init();

            #endregion

            Menu.DispInstallationDialog(85);

            Kernel.RootContent = File.ReadAllText(@"0:\System\Users\root.usr");

            Info.getComputerName();

            Menu.DispInstallationDialog(90);

            Computer.Color.GetBackgroundColor();

            Menu.DispInstallationDialog(95);

            Kernel.color = Computer.Color.GetTextColor();

            Menu.DispInstallationDialog(100);

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

            Kernel.JustInstalled = true;
            Kernel.running = true;
        }

        public static void RootLogin()
        {
            Kernel.SystemExists = false;
            Kernel.userLogged = "root";
            Kernel.Logged = true;
        }

        string step4_user;
        string step4_pass;

        /// <summary>
        /// Method to create users.
        /// </summary>
        private void Step4()
        {
            Console.Clear();

            string text = Text.Menu("setup");

            int middle = text.IndexOf("//////");
            string user = text.Remove(middle, text.Length - middle);
            string pass = text.Remove(0, middle + 6);

            if (File.Exists(@"0:\System\Users\" + user + ".usr"))
            {
                Text.Menu("alreadyuser");
                Step4();
            }
            else
            {
                if ((user.Length >= 4) && (user.Length <= 20))
                {

                    if ((pass.Length >= 6) && (pass.Length <= 40))
                    {
                        string password = MD5.hash(pass);
                        step4_pass = password;
                        step4_user = user;
                        Step5(step4_user);
                    }
                    else
                    {
                        Text.Menu("error2");
                        Step4();
                    }
                }
                else
                {
                        Text.Menu("error3");
                        Step4();
                }
            }  
        }

        #region Defaults

        public void InitDefaults(string user)
        {
            string[] DefaultDirctories =
            {
                @"0:\Users\" + user +  @"\Desktop",
                @"0:\Users\" + user +  @"\Documents",
                @"0:\Users\" + user +  @"\Downloads",
                @"0:\Users\" + user +  @"\Music"
            };
            foreach (string dirs in DefaultDirctories)
            {
                if (!Directory.Exists(dirs))
                    Directory.CreateDirectory(dirs);
            }
        }

        #endregion Defaults

    }
}
