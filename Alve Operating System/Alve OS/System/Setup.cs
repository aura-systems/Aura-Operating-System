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
            Console.Clear();
            Logo.Print();
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

                File.Create(@"0:\System\lang.set");

                if (File.Exists(@"0:\System\lang.set"))
                {
                    File.WriteAllText(@"0:\System\lang.set", Kernel.langSelected);
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

                File.Create(@"0:\System\lang.set");

                if (File.Exists(@"0:\System\lang.set"))
                {
                    File.WriteAllText(@"0:\System\lang.set", Kernel.langSelected);
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
                Logo.Print();
                L.Text.Display("chooseyourusername");
                AskUser();
            }
            catch
            {
                ErrorDuringSetup("Creating user");
            }
        }


        /// <summary>
        /// Demande du nom pour l'ordinateur
        /// </summary>
        private void Step5()
        {
            try
            {
                Console.Clear();
                Logo.Print();
                AskComputerName();
            }
            catch
            {
                ErrorDuringSetup("Computer Name");
            }
        }

        private void AskComputerName()
        {
            Console.WriteLine();
            L.Text.Display("askcomputername");
            Console.WriteLine();
            L.Text.Display("computernamename");
            var computername = Console.ReadLine();

            if ((computername.Length >= 1) && (computername.Length <= 15)) //15 char max for NETBIOS name resolution (dns)
            {
                Info.setComputerName(computername);
                Step6();
            }
            else
            {
                L.Text.Display("computernameincorrect");
                Console.WriteLine();
                AskComputerName();
            }


        }


        /// <summary>
        /// Méthode permettant de valider l'installation.
        /// </summary>
        private void Step6()
        {
            File.Create(@"0:\System\setup.set");
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
                Console.ReadKey();
                Step4();
            }
            else
            {
                if((username.Length >= 4) && (username.Length <= 20))
                {

                    Console.WriteLine();
                    L.Text.Display("passuser", username);
                    
                    Console.WriteLine();
                    L.Text.Display("passwd");
                    string clearpassword = Console.ReadLine();

                    if ((clearpassword.Length >= 6) && (clearpassword.Length <= 40))
                    {
                        string password = MD5.hash(clearpassword);

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
                        Console.ReadKey();
                        Step4();
                    }
                }
                else
                {
                    L.Text.Display("charmin");
                    Console.ReadKey();
                    Step4();
                }             
            }
        }

    }
}
