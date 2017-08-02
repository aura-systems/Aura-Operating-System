/*
* PROJECT:          Alve Operating System Development
* CONTENT:          Setup
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Alve_OS.System.Users;
using System.IO;
using Sys = Cosmos.System;
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
            if (!File.Exists(@"0:\System\setup"))
            {
                StartSetup();
            }

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
        /// Demande de la disposition du clavier
        /// </summary>
        private void Step3()
        {
            Console.Clear();
            Console.WriteLine();
            L.Text.Display("languageask");
            Console.WriteLine();
            L.Text.Display("availablelanguage");
            Console.WriteLine();
            Console.Write("> ");
            var cmd = Console.ReadLine();
            if ((cmd.Equals("en_US")) || cmd.Equals("en-US"))
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
            else if ((cmd.Equals("fr_FR")) || cmd.Equals("fr-FR"))
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

                Console.WriteLine();
                L.Text.Display("chooseyourusername");
                AskUser();

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

        /// <summary>
        /// Méthode permettant de créer un compte utilisateur
        /// </summary>
        private void AskUser()
        {
            Console.WriteLine();
            L.Text.Display("user");
            var username = Console.ReadLine();

            if (File.Exists(@"0:\System\Users\" + username + ".usr"))
            {
                L.Text.Display("alreadyuser");
                AskUser();
                
            }
            else
            {
                if((username.Length >= 4) && (username.Length <= 20))
                {
                    Console.WriteLine();
                    L.Text.Display("passuser", username);

                    psw:

                    Console.WriteLine();
                    L.Text.Display("passwd");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.Black;
                    var clearpassword = Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.White;
                    if ((clearpassword.Length >= 6) && (clearpassword.Length <= 40))
                    {
                        string password = MD5.hash(clearpassword);
                        Console.ForegroundColor = ConsoleColor.White;

                        Console.WriteLine();

                        File.Create(@"0:\System\Users\" + username + ".usr");
                        Directory.CreateDirectory(@"0:\Users\" + username);

                        if (File.Exists(@"0:\System\Users\" + username + ".usr"))
                        {
                            File.WriteAllText(@"0:\System\Users\" + username + ".usr", password + "|standard");

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
                        L.Text.Display("pswcharmin");
                        goto psw;
                    }
                }
                else
                {
                    L.Text.Display("charmin");
                    AskUser();
                }             
            }
        }

    }
}
