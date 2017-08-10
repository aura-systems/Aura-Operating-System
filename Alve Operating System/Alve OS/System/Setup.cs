/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Setup
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using L = Alve_OS.System.Translation;
using Alve_OS.System.Security;
using Alve_OS.System.Computer;
using Alve_OS.System.Drawable;
using Alve_OS.System.Translation;

namespace Alve_OS.System
{
    class Setup
    {

        /// <summary>
        /// Vérifie que l'installation d'Alve est complète
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
        /// Void appelé lors d'une erreur lors de l'installation
        /// </summary>
        public void ErrorDuringSetup(string error = "Setup Error")
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("  Error during installation");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("  Error: " + error);
            Console.WriteLine();
        }


        /// <summary>
        /// Démarre l'installation
        /// </summary>
        public void StartSetup()
        {
            Step1();
        }

        /// <summary>
        /// Créations des dossiers requis.
        /// </summary>
        private void Step1()
        {
            //creating folders

            try
            {
                if (!Directory.Exists(@"0:\System"))
                {
                    Directory.CreateDirectory(@"0:\System");
                }

                if (!Directory.Exists(@"0:\System\Users"))
                {
                    Directory.CreateDirectory(@"0:\System\Users");
                }

                if (!Directory.Exists(@"0:\Users"))
                {
                    Directory.CreateDirectory(@"0:\Users");
                }

                if (!Directory.Exists(@"0:\Users\root"))
                {
                    Directory.CreateDirectory(@"0:\Users\root");
                }

                if (!File.Exists(@"0:\System\color.set"))
                {
                    File.Create(@"0:\System\color.set");
                    File.WriteAllText(@"0:\System\color.set", "7");
                }

                Info.setComputerName("Alve-PC");

                if ((Directory.Exists(@"0:\System")) && (Directory.Exists(@"0:\System\Users")) && (Directory.Exists(@"0:\Users")) && (Directory.Exists(@"0:\Users\root")))
                {
                    Step2();
                }
            }
            catch
            {
                ErrorDuringSetup("Creating system folders");
            }
        }


        /// <summary>
        /// Méthode appelé pour créer 'root'.
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
                ErrorDuringSetup("Creating root");
            }
        }


        /// <summary>
        /// Demande de la disposition du clavier
        /// </summary>
        private void Step3()
        {
            string language = Menu.DispLanguageDialog();

            if ((language.Equals("en_US")) || language.Equals("en-US"))
            {
                Kernel.langSelected = "en_US";
                L.Keyboard.Init();

                File.Create(@"0:\System\lang.set");

                if (File.Exists(@"0:\System\lang.set"))
                {
                    File.WriteAllText(@"0:\System\lang.set", Kernel.langSelected);
                }
                else
                {
                    Menu.DispErrorDialog("The language configuration already exists!");
                }

                Step4();
            }
            else if ((language.Equals("fr_FR")) || language.Equals("fr-FR"))
            {
                Kernel.langSelected = "fr_FR";
                L.Keyboard.Init();

                File.Create(@"0:\System\lang.set");

                if (File.Exists(@"0:\System\lang.set"))
                {
                    File.WriteAllText(@"0:\System\lang.set", Kernel.langSelected);
                }
                else
                {
                    Menu.DispErrorDialog("La configuration des langue existe déjà!");
                }

                Step4();
            }
            else
            {
                Step3();
            }
        }

        /// <summary>
        /// Demande du nom pour l'ordinateur
        /// </summary>
        private void Step5(string user)
        {
            string computername = Menu.DispCompuernameDialog();

            if ((computername.Length >= 1) && (computername.Length <= 15)) //15 char max for NETBIOS name resolution (dns)
            {
                Info.setComputerName(computername);
                Step6(user);
            }
            else
            {
                Menu.DispErrorDialog("Computer name length must be 1-20 characters.");
                Step5(user);
            }
        }


        /// <summary>
        /// Méthode permettant de valider l'installation.
        /// </summary>
        private void Step6(string user)
        {
            File.Create(@"0:\System\setup.set");
            Kernel.userLogged = user;
            Console.Clear();
            WelcomeMessage.Display();
            Text.Display("logged", user);
            Console.WriteLine();
            Kernel.Logged = true;
        }

        /// <summary>
        /// Méthode permettant de créer un compte utilisateur
        /// </summary>
        private void Step4()
        {
            Console.Clear();

            string text = Menu.DispLoginForm("Création d'un compte Alve.");

            int middle = text.IndexOf("//////");
            string user = text.Remove(middle, text.Length - middle);
            string pass = text.Remove(0, middle + 6);

            if (File.Exists(@"0:\System\Users\" + user + ".usr"))
            {
                Menu.DispErrorDialog("Cet utilisateur existe déjà !");

                Step4();
            }
            else
            {
                if((user.Length >= 4) && (user.Length <= 20))
                {

                    if ((pass.Length >= 6) && (pass.Length <= 40))
                    {
                        string password = MD5.hash(pass);

                        Console.WriteLine();

                        File.Create(@"0:\System\Users\" + user + ".usr");
                        Directory.CreateDirectory(@"0:\Users\" + user);

                        if (File.Exists(@"0:\System\Users\" + user + ".usr"))
                        {
                            File.WriteAllText(@"0:\System\Users\" + user + ".usr", password + "|standard");

                            if (Directory.Exists(@"0:\System"))
                            {
                                Step5(user);
                            }
                        }
                        else
                        {
                            Menu.DispErrorDialog("Erreur pendant la création de l'utilisateur!");
                        }
                    }
                    else
                    {
                        Menu.DispErrorDialog("Ce mot de passe est trop court!");
                        Step4();
                    }
                }
                else
                {
                    Menu.DispErrorDialog("Ce pseudo est trop court!");
                    Step4();
                }             
            }
        }

    }
}
