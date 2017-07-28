using System;
using System.Collections.Generic;
using System.Text;
using Alve_OS.System.Users;
using System.IO;

namespace Alve_OS.System
{
    class Setup
    {


        /// <summary>
        /// Vérifie que l'installation d'Alve est complète
        /// </summary>
        public static void SetupVerifyCompleted()
        {
            if (!File.Exists(@"0:\System\setup"))
            {
                MakingUsers();
                SetupUsers();
            }
            
        }


        /// <summary>
        /// Méthode appelé pour créer 'root' et proposer la création d'un utilisateur de base.
        /// </summary>
        public static void MakingUsers()
        {
            try //METHOD CREATIONS UTILISATEURS
            {
                //////////////////////////////////////////////////////////

                try
                {
                    if (!Directory.Exists(@"0:\Users"))
                    {
                        Console.WriteLine("Directory Users");
                        Directory.CreateDirectory("0:\\Users");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("[OK]");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] Directory Users");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                ///////////////////////////////////////////////////////////

                try
                {
                    if (!File.Exists(@"0:\Users\root.usr"))
                    {
                        File.Create(@"0:\Users\root.usr");

                        try
                        {
                            if (File.Exists(@"0:\Users\root.usr"))
                            {
                                string text = "root";
                                File.WriteAllText(@"0:\Users\root.usr", text + "|admin");
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
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] Root file");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                ///////////////////////////////////////////////////////////

                
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] KERNEL USERS MAKING > MakingUsers()");
                Console.ForegroundColor = ConsoleColor.White;
            }


            
        }

        public static void SetupUsers()
        {
            try
            {
                if (Directory.Exists(@"0:\Users"))
                {
                    Console.Clear();

                    Console.WriteLine();
                    Console.WriteLine("Choose a user name for your Alve Account:");
                    Console.WriteLine();
                    Console.Write("> ");
                    var username = Console.ReadLine();

                    if (File.Exists(@"0:\Users\" + username + ".usr"))
                    {
                        Console.WriteLine("This user exist already!");
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("Choose a password for " + username);
                        Console.WriteLine();
                        Console.Write("> ");
                        var password = Console.ReadLine();
                        Console.WriteLine();

                        File.Create(@"0:\Users\" + username + ".usr");

                        if (File.Exists(@"0:\Users\" + username + ".usr"))
                        {
                            File.WriteAllText(@"0:\Users\" + username + ".usr", password + "|standard");

                            Console.WriteLine("Next step ! User created !");

                            if (Directory.Exists(@"0:\System"))
                            {
                                File.Create(@"0:\System\setup");
                            }
                        }
                        else
                        {
                            //error
                        }
                    }                  

                    
                }
            }
            catch
            {

            }
        }

    }
}
