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
        public void Login(string user, string password)
        {
            if(user == "root")
            {
                if (File.Exists(@"0:\Users\root.usr"))
                {
                    string RootPassword = File.ReadAllText(@"0:\Users\root.usr");

                    if (password == RootPassword)
                    {
                        //LOGGED
                        Kernel.userLogged = "root";
                        Kernel.userLevelLogged = UserLevel.Administrator();
                        Console.Clear();
                        WelcomeMessage.Display();
                        Kernel.Logged = true;
                    }
                }
                
            }
            else //Not root, so it's a user created after/during installation
            {
                if (File.Exists(@"0:\Users\" + user + ".usr"))
                {
                    string UserFile = File.ReadAllText(@"0:\Users\" + user + ".usr");

                    Char delimiter = '|';
                    String[] UserFileContent = UserFile.Split(delimiter);

                    if (password == UserFileContent[0])
                    {
                        //LOGGED
                        Kernel.userLogged = user;
                        UserLevel.LevelReader(UserFileContent[1]);
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
            }
        }
    }
}
