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
                    string text = Menu.DispLoginForm("Connexion à votre compte Aura.");

                    int middle = text.IndexOf("//////");
                    string user = text.Remove(middle, text.Length - middle);
                    string pass = text.Remove(0, middle + 6);

                    if (File.Exists(@"0:\System\passwd"))
                    {
                        string md5psw = MD5.hash(pass);

                        string[] UserFile = File.ReadAllLines(@"0:\System\passwd");

                        foreach (string users in UserFile)
                        {
                            if (users.Contains(user))
                            {
                                if (users.StartsWith("user:user:" + user + "="))
                                {
                                    string passandlevel = users.Remove(0, 11 + user.Length);
                                    Char delimiter = '|';
                                    string[] passandlevelarray = passandlevel.Split(delimiter);
                                    if (md5psw == passandlevelarray[0])
                                    {
                                        //LOGGED
                                        Kernel.userLogged = user;
                                        UserLevel.LevelReader(passandlevelarray[1]);
                                        InitUserDirs(user);
                                        Console.Clear();
                                        WelcomeMessage.Display();
                                        Text.Display("logged", user);
                                        Console.WriteLine("");
                                        Kernel.Logged = true;
                                        break;
                                    }
                                    else
                                    {
                                        Menu.DispErrorDialog("Mauvais mot de passe.");
                                        Login();
                                        break;
                                    }
                                }
                            }
                            else if (user=="root")
                            {
                                if (users.StartsWith("user:root="))
                                {
                                    string passandlevel = users.Remove(0, 10);
                                    Char delimiter = '|';
                                    string[] passandlevelarray = passandlevel.Split(delimiter);
                                    if (md5psw == passandlevelarray[0])
                                    {
                                        //LOGGED
                                        Kernel.userLogged = user;
                                        UserLevel.LevelReader(passandlevelarray[1]);
                                        Kernel.userLevelLogged = UserLevel.Administrator();
                                        InitUserDirs(user);
                                        Console.Clear();
                                        WelcomeMessage.Display();
                                        Text.Display("logged", user);
                                        Console.WriteLine("");
                                        Kernel.Logged = true;
                                        break;
                                    }
                                    else
                                    {
                                        Menu.DispErrorDialog("Mauvais mot de passe.");
                                        Login();
                                        break;
                                    }
                                }
                            }
                            else
                            {
                               Menu.DispErrorDialog("Utilisateur inconnu.");
                             Login();
                                break;
                            }
                        }
                    }
                    
                    break;

                case "en_US":
                    string text1 = Menu.DispLoginForm("Login to your Aura account.");

                    int middle1 = text1.IndexOf("//////");
                    string user1 = text1.Remove(middle1, text1.Length - middle1);
                    string pass1 = text1.Remove(0, middle1 + 6);

                    if (File.Exists(@"0:\System\passwd"))
                    {
                        string md5psw = MD5.hash(pass1);

                        string[] UserFile = File.ReadAllLines(@"0:\System\passwd");

                        foreach (string users in UserFile)
                        {
                            if (users.Contains(user1))
                            {
                                if (users.StartsWith("user:user:" + user1 + "="))
                                {
                                    string passandlevel = users.Remove(0, 11 + user1.Length);
                                    Char delimiter = '|';
                                    string[] passandlevelarray = passandlevel.Split(delimiter);
                                    if (md5psw == passandlevelarray[0])
                                    {
                                        //LOGGED
                                        Kernel.userLogged = user1;
                                        UserLevel.LevelReader(passandlevelarray[1]);
                                        InitUserDirs(user1);
                                        Console.Clear();
                                        WelcomeMessage.Display();
                                        Text.Display("logged", user1);
                                        Console.WriteLine("");
                                        Kernel.Logged = true;
                                        break;
                                    }
                                    else
                                    {
                                        Menu.DispErrorDialog("Wrong Password.");
                                        Login();
                                        break;
                                    }
                                }
                            }
                            else if (user1 == "root")
                            {
                                if (users.StartsWith("user:root="))
                                {
                                    string passandlevel = users.Remove(0, 10);
                                    Char delimiter = '|';
                                    string[] passandlevelarray = passandlevel.Split(delimiter);
                                    if (md5psw == passandlevelarray[0])
                                    {
                                        //LOGGED
                                        Kernel.userLogged = user1;
                                        UserLevel.LevelReader(passandlevelarray[1]);
                                        Kernel.userLevelLogged = UserLevel.Administrator();
                                        InitUserDirs(user1);
                                        Console.Clear();
                                        WelcomeMessage.Display();
                                        Text.Display("logged", user1);
                                        Console.WriteLine("");
                                        Kernel.Logged = true;
                                        break;
                                    }
                                    else
                                    {
                                        Menu.DispErrorDialog("Wrong Password.");
                                        Login();
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                Menu.DispErrorDialog("Unknown user.");
                                Login();
                                break;
                            }
                        }
                    }

                    break;
            }
        }

        #region UserDirs
        public void InitUserDirs(string user)
        {
            string[] DefaultDirctories =
            {
                @"0:\Users\" + user + @"\Desktop",
                @"0:\Users\" + user + @"\Documents",
                @"0:\Users\" + user + @"\Downloads",
                @"0:\Users\" + user + @"\Music",
            };
            foreach (string dirs in DefaultDirctories)
                if (!Directory.Exists(dirs))
                    Directory.CreateDirectory(dirs);
        }
        #endregion UserDirs

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
