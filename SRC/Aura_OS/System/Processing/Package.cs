﻿/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Package class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Network;

namespace Aura_OS.System.Processing
{
    public class Package
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Link { get; set; }
        public string Version { get; set; }
        public bool Installed { get; set; }
        public Executable Executable { get; set; }

        public void Download()
        {
            byte[] executable = Http.DownloadRawFile(Link);
            Executable = new Executable(executable);
        }
    }
}
