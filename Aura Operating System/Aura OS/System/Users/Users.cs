/*
* PROJECT:          Aura Operating System Development
* CONTENT:          User class
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System;
using System.IO;
using Aura_OS.System.Translation;
using Aura_OS.System.Security;
using Aura_OS.System.Drawable;

namespace Aura_OS.System.Users
{
    class Users
    {

        /// <summary>
        /// Display login form
        /// </summary>
        public void Login()
        {
            switch (Kernel.langSelected)
            {
                case "fr_FR":
                    string text = Menu.DispLoginForm("Connexion à un compte Aura.");

                    int middle = text.IndexOf("//////");
                    string user = text.Remove(middle, text.Length - middle);
                    string pass = text.Remove(0, middle + 6);

                    if (File.Exists(@"0:\System\Users\" + user + ".usr"))
                    {
                        string md5psw = MD5.hash(pass);
                        Console.WriteLine();

                        string UserFile = File.ReadAllText(@"0:\System\Users\" + user + ".usr");

                        Char delimiter = '|';
                        string[] UserFileContent = UserFile.Split(delimiter);

                        if (md5psw == UserFileContent[0])
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
                        else
                        {
                            Menu.DispErrorDialog("Mauvais mot de passe.");
                            Login();
                        }
                    }
                    else
                    {
                        Menu.DispErrorDialog("Utilisateur inconnu.");
                        Login();
                    }

                    break;

                case "en_US":
                    string text1 = Menu.DispLoginForm("Login to an Aura account.");

                    int middle1 = text1.IndexOf("//////");
                    string user1 = text1.Remove(middle1, text1.Length - middle1);
                    string pass1 = text1.Remove(0, middle1 + 6);

                    if (File.Exists(@"0:\System\Users\" + user1 + ".usr"))
                    {
                        string md5psw = MD5.hash(pass1);
                        Console.WriteLine();

                        string UserFile = File.ReadAllText(@"0:\System\Users\" + user1 + ".usr");

                        Char delimiter = '|';
                        string[] UserFileContent = UserFile.Split(delimiter);

                        if (md5psw == UserFileContent[0])
                        {
                            //LOGGED
                            Kernel.userLogged = user1;
                            UserLevel.LevelReader(UserFileContent[1]);
                            if (user1 == "root")
                            {
                                Kernel.userLevelLogged = UserLevel.Administrator();
                            }
                            Console.Clear();
                            WelcomeMessage.Display();
                            Text.Display("logged", user1);
                            Console.WriteLine("");
                            Kernel.Logged = true;
                        }
                        else
                        {
                            Menu.DispErrorDialog("Wrong Password.");
                            Login();
                        }
                    }
                    else
                    {
                        Menu.DispErrorDialog("Unknown user.");
                        Login();
                    }
                    break;
            }
        }

        /// <summary>
        /// Method called to create an user
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="userlevel">User type</param>
        public void Create(string username)
        {
            try
            {
                Console.WriteLine();
                Text.Display("chooseyourusername");
                Console.WriteLine();
                Text.Display("user");
                username = Console.ReadLine();

                if (File.Exists(@"0:\System\Users\" + username + ".usr"))
                {
                    Text.Display("alreadyuser");
                    Create(username);
                }
                else
                {
                    if ((username.Length >= 4) && (username.Length <= 20))
                    {
                        Console.WriteLine();
                        Text.Display("passuser", username);

                        psw:

                        Console.WriteLine();
                        Text.Display("passwd");
                        Console.WriteLine();

                        Console.ForegroundColor = ConsoleColor.Black;
                        string clearpassword = Console.ReadLine();
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

                                Console.WriteLine();
                                Text.Display("whattypeuser");
                                groups:
                                Text.Display("groupsavailable");
                                Console.WriteLine();

                                string userlevel = Console.ReadLine();

                                if (userlevel == UserLevel.Administrator())
                                {
                                    File.WriteAllText(@"0:\System\Users\" + username + ".usr", password + "|admin");
                                }
                                else if (userlevel == UserLevel.StandardUser())
                                {
                                    File.WriteAllText(@"0:\System\Users\" + username + ".usr", password + "|standard");
                                }
                                else
                                {
                                    Console.WriteLine();
                                    goto groups;
                                }
                            }
                            else
                            {
                                Text.Display("passwd");
                            }
                        }
                        else
                        {
                            Text.Display("pswcharmin");
                            goto psw;
                        }
                    }
                    else
                    {
                        Text.Display("charmin");
                    }
                }
            }
            catch
            {
                Text.Display("errorwhileusercreating");
            }          
        }

    }
}