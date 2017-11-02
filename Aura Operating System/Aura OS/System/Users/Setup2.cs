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

namespace Aura_OS.System
{
    class Setup2
    {
        private string username;
        private string password;
        private string lang;

        public void RegisterHostname()
        {

        }

        public void RegisterUser()
        {

        }

        public void RegisterLanguage()
        {

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
