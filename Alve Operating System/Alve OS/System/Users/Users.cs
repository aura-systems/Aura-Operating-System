/*
* PROJECT:          Alve Operating System Development
* CONTENT:          User class
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Alve_OS.System.Translation;

namespace Alve_OS.System.Users
{
    class Users
    {
        public void Login()
        {
            switch (Kernel.langSelected)
            {
                case "fr_FR":
                    Console.Write("Utilisateur > ");
                    var user = Console.ReadLine();
                    Console.WriteLine();

                    if (File.Exists(@"0:\System\Users\" + user + ".usr"))
                    {
                        Console.Write("Mot de passe > ");
                        var psw = Console.ReadLine();
                        Console.WriteLine();

                        string UserFile = File.ReadAllText(@"0:\System\Users\" + user + ".usr");

                        Char delimiter = '|';
                        string[] UserFileContent = UserFile.Split(delimiter);

                        if (psw == UserFileContent[0])
                        {
                            //LOGGED
                            Kernel.userLogged = user;
                            UserLevel.LevelReader(UserFileContent[1]);
                            if(user == "root")
                            {
                                Kernel.userLevelLogged = UserLevel.Administrator();
                            }
                            Console.Clear();
                            WelcomeMessage.Display();
                            Text.Display("logged", user);
                            Console.WriteLine("");
                            Kernel.Logged = true;
                        }
                    }
                    else
                    {
                        Text.Display("unknownuser");
                    }

                    break;

                case "en_US":
                    Console.Write("Login > ");
                    var user2 = Console.ReadLine();
                    Console.WriteLine();

                    if (File.Exists(@"0:\System\Users\" + user2 + ".usr"))
                    {
                        Console.Write("Mot de passe > ");
                        var psw = Console.ReadLine();
                        Console.WriteLine();

                        string UserFile = File.ReadAllText(@"0:\System\Users\" + user2 + ".usr");

                        Char delimiter = '|';
                        string[] UserFileContent = UserFile.Split(delimiter);

                        if (psw == UserFileContent[0])
                        {
                            //LOGGED
                            Kernel.userLogged = user2;
                            UserLevel.LevelReader(UserFileContent[1]);
                            if (user2 == "root")
                            {
                                Kernel.userLevelLogged = UserLevel.Administrator();
                            }
                            Console.Clear();
                            WelcomeMessage.Display();
                            Text.Display("logged", user2);
                            Console.WriteLine("");
                            Kernel.Logged = true;
                        }
                    }
                    else
                    {
                        Text.Display("unknownuser");
                    }

                    break;


            }
        }
    }
}
