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

        public void isInstalled()
        {
            
        }

        #region Defaults

        public void InitDefaults(string user)
        {
            string[] DefaultDirctories =
            {
                @"0:\Users\" + user +  @"\Desktop",
                @"0:\Users\" + user +  @"\Documents",
                @"0:\Users\" + user +  @"\Downloads",
                @"0:\Users\" + user +  @"\Music"
            };
            foreach (string dirs in DefaultDirctories)
            {
                if (!Directory.Exists(dirs))
                    Directory.CreateDirectory(dirs);
            }
        }

        #endregion Defaults

    }
}
