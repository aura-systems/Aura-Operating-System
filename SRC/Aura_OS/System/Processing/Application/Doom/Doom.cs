/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Doom
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Processing.Application.Doom;
using System;

namespace Aura_OS.System.Processing.Application
{
    public class DoomApp : Graphics.UI.GUI.Application
    {
        public static string ApplicationName = "Doom";

        static ManagedDoom.DoomApplication app = null;

        public static Debugger debugger = new Debugger();

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
            app.renderer.X = x;
            app.renderer.Y = y;

            if (app == null)
            {
                return;
            }

            uint[] upKeys = { };
            uint[] downKeys = { };

            app.Run(upKeys, downKeys);

            string[] lines = debugger.Text.Split('\n');
            int maxLinesToShow = 17;
            int startIndex = Math.Max(0, lines.Length - maxLinesToShow); // Ensure you don't go below 0
            int dy = 0;

            for (int i = startIndex; i < lines.Length; i++)
            {
                string line = lines[i];
                Kernel.canvas.DrawString(line, Kernel.font, Kernel.BlackColor, x + 2, y + 200 + 4 + dy);
                dy += 12;
            }

        }
    }
}
