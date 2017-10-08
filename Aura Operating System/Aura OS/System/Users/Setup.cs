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
                if (!File.Exists(@"0:\System\setup.set"))
                {
                    StartSetup();
                }
            }
            catch { }
        }

        /// <summary>
        /// Start setup
        /// </summary>
        public void StartSetup()
        {
            Step1();
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

                            Step3();
                        }
                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ERROR] Root writing file");
                        Console.ForegroundColor = ConsoleColor.White;
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
        private void Step3()
        {
            string language = Menu.DispLanguageDialog();

            if ((language.Equals("en_US")) || language.Equals("en-US"))
            {
                Kernel.langSelected = "en_US";
                Keyboard.Init();
                Step4();
            }
            else if ((language.Equals("fr_FR")) || language.Equals("fr-FR"))
            {
                Kernel.langSelected = "fr_FR";
                Keyboard.Init();
                Step4();
            }
            else
            {
                Step3();
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
            Console.WriteLine("Installation in progress...");
            Installation();
            File.Create(@"0:\System\setup.set");
            Kernel.userLogged = user;
            Console.Clear();
            WelcomeMessage.Display();
            Text.Display("logged", user);
            Console.WriteLine();
            Kernel.Logged = true;
        }

        private void Installation()
        {
            switch (Kernel.langSelected)
            {
                case "en_US":
                    File.Create(@"0:\System\lang.set");
                    if (File.Exists(@"0:\System\lang.set"))
                    {
                        File.WriteAllText(@"0:\System\lang.set", Kernel.langSelected);
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
                    }
                    else
                    {
                        Menu.DispErrorDialog("La configuration des langue existe déjà!");
                    }
                    break;
            }

            File.Create(@"0:\System\Users\" + step4_user + ".usr");
            Directory.CreateDirectory(@"0:\Users\" + step4_user);

            InitDefaults(step4_user);

            if (File.Exists(@"0:\System\Users\" + step4_user + ".usr"))
            {
                File.WriteAllText(@"0:\System\Users\" + step4_user + ".usr", step4_pass + "|standard");
            }
            else
            {
                Text.Menu("error1");
            }

            Info.setComputerName(step5_computername);
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
                if((user.Length >= 4) && (user.Length <= 20))
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
