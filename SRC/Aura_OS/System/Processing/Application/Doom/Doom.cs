/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Doom
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Processing.Application.Doom;
using Cosmos.System;
using Cosmos.System.FileSystem;
using System;
using System.IO;

namespace Aura_OS.System.Processing.Application
{
    public class DoomApp : Graphics.UI.GUI.Application
    {
        public static string ApplicationName = "Doom";

        public Panel TopPanel;
        public Button Screenshot;

        static ManagedDoom.DoomApplication app = null;

        public static Debugger debugger = new Debugger();

        bool calledAfterRender = false;

        static float framesPerSecond;

        private string[] args = { };
        private string[] configLines = { };

        public DoomApp(int width, int height, int x = 0, int y = 0) : base(ApplicationName, width, height, x, y)
        {
            TopPanel = new Panel(Kernel.Gray, x + 1, y + 1, width - 6, 22);
            TopPanel.Borders = true;

            string text = "Screenshot";
            int textWidth = text.Length * (Kernel.font.Width + 1);
            Screenshot = new Button(text, x + 3, y + 3, textWidth, 18);
            Screenshot.Action = new Action(() =>
            {
                File.Create(Kernel.CurrentDirectory + "screenshot.bmp");
                app.renderer.bitmap.Bitmap.Save(Kernel.CurrentDirectory + "screenshot.bmp");
            });

            app = null;
            var commandLineArgs = new ManagedDoom.CommandLineArgs(args);
            app = new ManagedDoom.DoomApplication(commandLineArgs, configLines);
        }

        public override void UpdateApp()
        {
            app.renderer.X = x;
            app.renderer.Y = y + 23;

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
                Kernel.canvas.DrawString(line, Kernel.font, Kernel.BlackColor, x + 2, y + 23 + 200 + 4 + dy);
                dy += 12;
            }

            TopPanel.X = x + 1;
            TopPanel.Y = y + 1;
            TopPanel.Update();
            Screenshot.X = x + 3;
            Screenshot.Y = y + 3;
            Screenshot.Update();
        }

        public override void HandleLeftClick()
        {
            base.HandleLeftClick();

            if (Screenshot.IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                Screenshot.Action();
                return;
            }
        }
    }
}
