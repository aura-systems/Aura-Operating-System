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
        public void SetupVerifyCompleted()
        {
            //TO-DO
            throw new NotImplementedException();
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
                                string text = "root|admin";
                                File.WriteAllText(@"0:\Users\root.usr", text);
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

                //TODO Ask user creation
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] KERNEL USERS MAKING > MakingUsers()");
                Console.ForegroundColor = ConsoleColor.White;
                //Crash.StopKernel("Users crash");
            }


            
        }

    }
}
