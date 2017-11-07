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
        #region UserDirs
        public void InitUserDirs(string user)
        {
            if (user == "root")
            {
                string[] RootDirectories =
                {
                    @"0:\Users\" + user + @"\root"
                };
                foreach (string dirs in RootDirectories)
                    if (!Directory.Exists(dirs))
                        Directory.CreateDirectory(dirs);
                return;
            }
            else
            {
                string[] DefaultDirectories =
                {
                    @"0:\Users\" + user + @"\Desktop",
                    @"0:\Users\" + user + @"\Documents",
                    @"0:\Users\" + user + @"\Downloads",
                    @"0:\Users\" + user + @"\Music",
                };
                foreach (string dirs in DefaultDirectories)
                    if (!Directory.Exists(dirs))
                        Directory.CreateDirectory(dirs);
            }            
        }
        #endregion UserDirs


        public void Create(string username, string password, string type = "standard")
        {
            try
            {
                password = MD5.hash(password);
                Settings.LoadUsers();
                if (Settings.GetUser("user").StartsWith(username))
                {
                    Console.WriteLine($"[NotTranslated] {username} exist already !");
                    return;
                }
                Settings.PutUser("user:" + username, password + "|" + type);
                Settings.PushUsers();
                Console.WriteLine($"[NotTranslated] {username} has been created !");

                InitUserDirs(username);
                Console.WriteLine("[NotTranslated] Personal directories has been created !");
            }
            catch
            {
                Text.Display("errorwhileusercreating");
            }
        }

    }
}
