/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Setup
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using Aura_OS.System.Security;
using Aura_OS.System.Computer;
using Aura_OS.System.Drawable;
using Aura_OS.System.Translation;
using Aura_OS.System.Utils;

namespace Aura_OS.System
{
    class Setup2
    {
        private string username;
        private string password;
        private string lang;
        private string hostname;

        public void RegisterHostname()
        {
            Console.WriteLine("Hostname:");
            hostname = Console.ReadLine();
            Settings.PutValue("hostname", hostname);
        }

        public void RegisterUser()
        {
            Console.WriteLine("Username:");
            username = Console.ReadLine();
            Console.WriteLine("Password");
            password = MD5.hash(Console.ReadLine());
            Settings.PutValue("user:" + username, password);
            Settings.PutValue("user:root", MD5.hash("root"));
        }

        public void RegisterLanguage()
        {
            Console.WriteLine("Language (fr_FR, en_US):");
            lang = Console.ReadLine();
            Settings.PutValue("language", lang);
        }

        public void InitDefaults()
        {
            InitDirs();
        }

        public void InitFiles()
        {
            try
            {
                string[] DefaultSystemFiles =
                {
                    @"0:\System\settings.conf"
                };

                foreach (string file in DefaultSystemFiles)
                {
                    if (!File.Exists(file))
                        File.Create(file);
                }
            }
            catch
            {
                NoFileSystem();
            }            
        }

        #region Defaults

        public void InitDirs()
        {
            try
            {
                string[] DefaultSystemDirectories =
                {
                    @"0:\System\",
                    @"0:\Users\"
                };

                foreach (string dirs in DefaultSystemDirectories)
                {
                    if (!Directory.Exists(dirs))
                        Directory.CreateDirectory(dirs);
                }

                InitFiles();
            }
            catch
            {
                NoFileSystem();
            }            
        }

        public void NoFileSystem()
        {
            Kernel.SystemExists = false;
            Kernel.userLogged = "root";
            Kernel.Logged = true;
        }

        #endregion Defaults

    }
}
