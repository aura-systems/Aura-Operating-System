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
                if (!File.Exists(@"0:\System\setup"))
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
        /// Demande la langue
        /// </summary>
        private void Step3()
        {
            Console.Clear();
            //Logo.Print();
            //L.Text.Display("languageask");
            //Console.WriteLine();
            //L.Text.Display("availablelanguage");
            //Console.WriteLine();
            //Console.Write("> ");
            //var cmd = Console.ReadLine();

            string language = Menu.DispLanguageDialog();

            if ((language.Equals("en_US")) || language.Equals("en-US"))
            {
                Kernel.langSelected = "en_US";
                L.Keyboard.Init();

                File.Create(@"0:\System\lang");

                if (File.Exists(@"0:\System\lang"))
                {
                    File.WriteAllText(@"0:\System\lang", Kernel.langSelected);
                }
                else
                {
                    ErrorDuringSetup("Lang Register");
                }

                Step4();
            }
            else if ((language.Equals("fr_FR")) || language.Equals("fr-FR"))
            {
                Kernel.langSelected = "fr_FR";
                L.Keyboard.Init();

                File.Create(@"0:\System\lang");

                if (File.Exists(@"0:\System\lang"))
                {
                    File.WriteAllText(@"0:\System\lang", Kernel.langSelected);
                }
                else
                {
                    ErrorDuringSetup("Lang Register");
                }

                Step4();
            }
            else
            {
                Step3();
            }
        }


        /// <summary>
        /// Méthode permettant la création d'un compte utilisateur.
        /// </summary>
        private void Step4()
        {
            try
            {
                Console.Clear();
                //Logo.Print();
               // L.Text.Display("chooseyourusername");
                string text = Menu.DispLoginForm("Création d'un compte Alve.");
                //Console.WriteLine();
                //L.Text.Display("user");
                //var username = Console.ReadLine();

                int middle = text.IndexOf("//////");
                string user = text.Remove(middle, text.Length - middle);
                string pass = text.Remove(0, middle + 6);

                if (File.Exists(@"0:\System\Users\" + user + ".usr"))
                {
                    Menu.DispErrorDialog("Cet utilisateur existe déjà !");
                    //L.Text.Display("alreadyuser");
                    //Console.ReadKey();
                    Step4();
                }
                else
                {
                    if ((user.Length >= 4) && (user.Length <= 20))
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
                                    Step5();
                                }
                            }
                            else
                            {
                                ErrorDuringSetup("Creating user");
                            }
                        }
                        else
                        {
                            Menu.DispErrorDialog("Ce mot de passe est trop court !");
                            Step4();
                        }
                    }
                    else
                    {
                        Menu.DispErrorDialog("Ce pseudo est trop court !");
                        Step4();
                    }
                }
            }
            catch
            {
                ErrorDuringSetup("Creating user");
            }
        }


        /// <summary>
        /// Méthode permettant de valider l'installation.
        /// </summary>
        private void Step5()
        {
            File.Create(@"0:\System\setup");
            Console.Clear();
        }
    }
}
