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
                    Text.Display("user:existalready", username);
                    return;
                }
                Settings.PutUser("user:" + username, password + "|" + type);
                Settings.PushUsers();
                Text.Display("user:hasbeencreated", username);

                InitUserDirs(username);
                Text.Display("user:personaldirectories", username);
            }
            catch
            {
                Text.Display("errorwhileusercreating");
            }
        }

        public void Remove(string username)
        {
            if (Settings.GetUser("user").StartsWith(username))
            {
                Settings.LoadUsers();
                Settings.DeleteUser(username);
                //Directory.Delete(@"0:\Users\" + username, true);
                Text.Display("user:hasbeenremoved", username);
            }
            else
            {
                Text.Display("user:doesntexist", username);
            }
        }

        public void ChangePassword(string username, string password)
        {
            if (Settings.GetUser(username) == "null")
            {
                Text.Display("user:doesntexist", username);
            }
            else
            {
                Settings.LoadUsers();
                Settings.EditUser(username, password);
                Settings.PushUsers();
                //Directory.Delete(@"0:\Users\" + username, true);
                Text.Display("user:passwordhasbeenchanged", username);
            }
        }

    }
}
