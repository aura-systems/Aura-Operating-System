/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Doom
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;

namespace Aura_OS.System.Processing.Application
{
    public class DoomApp : Graphics.UI.GUI.Application
    {
        public static string ApplicationName = "Doom";

        static ManagedDoom.DoomApplication app = null;

        bool calledAfterRender = false;

        static float framesPerSecond;

        private string[] args = { };
        private string[] configLines = { };

        public DoomApp(int width, int height, int x = 0, int y = 0) : base(ApplicationName, width, height, x, y)
        {
            app = null;
            var commandLineArgs = new ManagedDoom.CommandLineArgs(args);
            app = new ManagedDoom.DoomApplication(commandLineArgs, configLines);
        }

        public override void UpdateApp()
        {
            if (app == null)
            {
                return;
            }

            uint[] upKeys = { };
            uint[] downKeys = { };

            app.Run(upKeys, downKeys);
        }
    }
}
