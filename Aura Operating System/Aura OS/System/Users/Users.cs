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
using Aura_OS.System.Utils;

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

                    string md5psw = MD5.hash(pass);

                    string uservaluetosplit = Settings.GetUser("user:" + user);

                    Char twodots = ':';

                    string[] userarray = uservaluetosplit.Split(twodots);

                    if (Settings.GetUser("user:" + user) != "null")
                    {

                        /* 1 = md5 password
                         * 2 = level
                         */

                        if (md5psw == userarray[0])
                        {
                            //LOGGED
                            Kernel.userLogged = user;
                            UserLevel.LevelReader(userarray[1]);
                            InitUserDirs(user);
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
                    else if (Settings.GetUser("user:root") != "null")
                    {

                        /* 1 = md5 password
                         * 2 = level
                         */

                        if (md5psw == userarray[0])
                        {
                            //LOGGED
                            Kernel.userLogged = user;
                            UserLevel.LevelReader(userarray[1]);
                            Kernel.userLevelLogged = UserLevel.Administrator();
                            InitUserDirs(user);
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
                        break;
                    }



                    break;

                case "en_US":

                    string text1 = Menu.DispLoginForm("Login to your Aura account.");

                    int middle1 = text1.IndexOf("//////");
                    string user1 = text1.Remove(middle1, text1.Length - middle1);
                    string pass1 = text1.Remove(0, middle1 + 6);

                    string md5psw1 = MD5.hash(pass1);

                    string uservaluetosplit1 = Settings.GetUser("user:" + user1);

                    Char twodots1 = ':';

                    string[] userarray1 = uservaluetosplit1.Split(twodots1);

                    if (Settings.GetUser("user:" + user1) != "null")
                    {

                        /* 1 = md5 password
                         * 2 = level
                         */

                        if (md5psw1 == userarray1[0])
                        {
                            //LOGGED
                            Kernel.userLogged = user1;
                            UserLevel.LevelReader(userarray1[1]);
                            InitUserDirs(user1);
                            Console.Clear();
                            WelcomeMessage.Display();
                            Text.Display("logged", user1);
                            Console.WriteLine("");
                            Kernel.Logged = true;
                        }
                        else
                        {
                            Menu.DispErrorDialog("Wrong password.");
                            Login();
                        }

                    }
                    else if (Settings.GetUser("user:root") != "null")
                    {

                        /* 1 = md5 password
                         * 2 = level
                         */

                        if (md5psw1 == userarray1[0])
                        {
                            //LOGGED
                            Kernel.userLogged = user1;
                            UserLevel.LevelReader(userarray1[1]);
                            Kernel.userLevelLogged = UserLevel.Administrator();
                            InitUserDirs(user1);
                            Console.Clear();
                            WelcomeMessage.Display();
                            Text.Display("logged", user1);
                            Console.WriteLine("");
                            Kernel.Logged = true;
                        }
                        else
                        {
                            Menu.DispErrorDialog("Wrong password.");
                            Login();
                        }

                    }
                    else
                    {
                        Menu.DispErrorDialog("Unknow user.");
                        Login();
                        break;
                    }



                    break;
            }
        }

        #region UserDirs
        public void InitUserDirs(string user)
        {
            if (user == "root")
            {
                string[] RootDirectories =
                {
                @"0:\Users\" + user + @"\Root"
                };
                foreach (string dirs in RootDirectories)
                    if (!Directory.Exists(dirs))
                        Directory.CreateDirectory(dirs);
                return;
            }

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
