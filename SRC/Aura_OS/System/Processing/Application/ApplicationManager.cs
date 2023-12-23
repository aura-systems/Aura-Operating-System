/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Application Manager
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Filesystem;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Processing.Application.Emulators.GameBoyEmu;
using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.System.Processing.Application
{
    public class ApplicationConfig
    {
        public Type Template;
        public int X;
        public int Y;
        public int Weight;
        public int Height;

        public ApplicationConfig(Type template, int x, int y, int weight, int height)
        {
            Template = template;
            X = x;
            Y = y;
            Weight = weight;
            Height = height;
        }
    }

    public class ApplicationManager
    {
        public List<ApplicationConfig> ApplicationTemplates;

        public ApplicationManager()
        {
            ApplicationTemplates = new List<ApplicationConfig>();
        }

        public void LoadApplications()
        {
            RegisterApplication(typeof(Terminal), 40, 40, 700, 600);
            RegisterApplication(typeof(MemoryInfo), 40, 40, 400, 300);
            RegisterApplication(typeof(Explorer), 40, 40, 500, 400);
            RegisterApplication(typeof(SystemInfo), 40, 40, 402, 360);
            RegisterApplication(typeof(Cube), 40, 40, 200, 200);
            RegisterApplication(typeof(GameBoyEmu), 40, 40, 160 + 4, 144 + 22);
        }

        public void RegisterApplication(ApplicationConfig config)
        {
            ApplicationTemplates.Add(config);
        }

        public void RegisterApplication(Type template, int x, int y, int weight, int height)
        {
            ApplicationConfig config = new(template, x, y, weight, height);
            ApplicationTemplates.Add(config);
        }

        public Graphics.UI.GUI.Application Instantiate(ApplicationConfig config)
        {
            Graphics.UI.GUI.Application app = null;

            if (config.Template == typeof(Terminal))
            {
                var terminal = new Terminal(config.Weight, config.Height, config.X, config.Y);
                Kernel.console = terminal;
                app = terminal;
            }
            else if (config.Template == typeof(Cube))
            {
                app = new Cube(config.Weight, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(SystemInfo))
            {
                app = new SystemInfo(config.Weight, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(MemoryInfo))
            {
                app = new MemoryInfo(config.Weight, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(GameBoyEmu))
            {
                app = new GameBoyEmu(config.Weight, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(Explorer))
            {
                app = new Explorer(config.Weight, config.Height, config.X, config.Y);
            }
            else
            {
                throw new InvalidOperationException("Type d'application non reconnu.");
            }

            return app;
        }

        public void StartFileApplication(string fileName, string currentPath)
        {
            if (fileName.EndsWith(".bmp"))
            {
                string path = currentPath + fileName;
                string name = fileName;
                byte[] bytes = File.ReadAllBytes(path);
                Bitmap bitmap = new Bitmap(bytes);
                int width = name.Length * 8 + 50;

                if (width < bitmap.Width)
                {
                    width = (int)bitmap.Width + 1;
                }

                var app = new Picture(name, bitmap, width, (int)bitmap.Height + 20);

                app.Initialize();

                Kernel.WindowManager.apps.Add(app);
                app.zIndex = Kernel.WindowManager.GetTopZIndex() + 1;
                Kernel.WindowManager.MarkStackDirty();

                app.Visible = true;
                app.Focused = true;

                Kernel.ProcessManager.Start(app);

                Kernel.Taskbar.UpdateApplicationButtons();
                Kernel.WindowManager.UpdateFocusStatus();
            }
            else
            {

            }
        }
    }
}
