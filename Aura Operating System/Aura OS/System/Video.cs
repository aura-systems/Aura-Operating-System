﻿/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Video card detection
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

namespace Aura_OS.System
{
    public class Video
    {
        public static string GetVideo()
        {

                Kernel.Consolemode = "VGATextmode";
                return "VGATextmode";
            
        }
    }
}