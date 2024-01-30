/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Application Manager
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Processing.Applications;
using Aura_OS.System.Processing.Applications.Emulators.GameBoyEmu;
using Aura_OS.System.Processing.Applications.Terminal;
using Aura_OS.System.Processing.Processes;
using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.System.Processing
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
            RegisterApplication(typeof(TerminalApp), 40, 40, 700, 600);
            RegisterApplication(typeof(ExplorerApp), 40, 40, 500, 400);
            RegisterApplication(typeof(MemoryInfoApp), 40, 40, 400, 300);
            RegisterApplication(typeof(SystemInfoApp), 40, 40, 402, 360);
            RegisterApplication(typeof(CubeApp), 40, 40, 200, 200);
            RegisterApplication(typeof(GameBoyApp), 40, 40, 160 + 4, 144 + 22);
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

        public void StartApplication(ApplicationConfig config)
        {
            Graphics.UI.GUI.Application app = Kernel.ApplicationManager.Instantiate(config);
            app.Initialize();

            Explorer.WindowManager.Applications.Add(app);
            app.zIndex = Explorer.WindowManager.GetTopZIndex() + 1;
            Explorer.WindowManager.MarkStackDirty();

            app.Visible = true;
            app.Focused = true;

            Kernel.ProcessManager.Start(app);

            Explorer.Taskbar.UpdateApplicationButtons();
            Explorer.WindowManager.UpdateFocusStatus();
        }

        public void StartApplication(Type appType)
        {
            Graphics.UI.GUI.Application app = Kernel.ApplicationManager.Instantiate(appType);
            app.Initialize();

            Explorer.WindowManager.Applications.Add(app);
            app.zIndex = Explorer.WindowManager.GetTopZIndex() + 1;
            Explorer.WindowManager.MarkStackDirty();

            app.Visible = true;
            app.Focused = true;

            Kernel.ProcessManager.Start(app);

            Explorer.Taskbar.UpdateApplicationButtons();
            Explorer.WindowManager.UpdateFocusStatus();
        }

        public Graphics.UI.GUI.Application Instantiate(Type appType)
        {
            foreach (var config in ApplicationTemplates)
            {
                if (config.Template == appType)
                {
                    return Instantiate(config);
                }
            }

            throw new InvalidOperationException("Unknown app type.");
        }

        public Graphics.UI.GUI.Application Instantiate(ApplicationConfig config)
        {
            Graphics.UI.GUI.Application app = null;

            if (config.Template == typeof(TerminalApp))
            {
                app = new TerminalApp(config.Weight, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(CubeApp))
            {
                app = new CubeApp(config.Weight, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(SystemInfoApp))
            {
                app = new SystemInfoApp(config.Weight, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(MemoryInfoApp))
            {
                app = new MemoryInfoApp(config.Weight, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(GameBoyApp))
            {
                app = new GameBoyApp(config.Weight, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(ExplorerApp))
            {
                app = new ExplorerApp(Kernel.CurrentVolume, config.Weight, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(EditorApp))
            {
                app = new EditorApp("", config.Weight, config.Height, config.X, config.Y);
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

                var app = new PictureApp(name, bitmap, width, (int)bitmap.Height + 20);

                app.Initialize();

                Explorer.WindowManager.Applications.Add(app);
                app.zIndex = Explorer.WindowManager.GetTopZIndex() + 1;
                Explorer.WindowManager.MarkStackDirty();

                app.Visible = true;
                app.Focused = true;

                Kernel.ProcessManager.Start(app);

                Explorer.Taskbar.UpdateApplicationButtons();
                Explorer.WindowManager.UpdateFocusStatus();
            }
            if (fileName.EndsWith(".txt"))
            {
                string path = currentPath + fileName;
                var app = new EditorApp(path, 700, 600, 40, 40);

                app.Initialize();

                Explorer.WindowManager.Applications.Add(app);
                app.zIndex = Explorer.WindowManager.GetTopZIndex() + 1;
                Explorer.WindowManager.MarkStackDirty();

                app.Visible = true;
                app.Focused = true;

                Kernel.ProcessManager.Start(app);

                Explorer.Taskbar.UpdateApplicationButtons();
                Explorer.WindowManager.UpdateFocusStatus();
            }
            else if (fileName.EndsWith(".gb"))
            {
                string path = currentPath + fileName;
                string name = fileName;
                byte[] bytes = File.ReadAllBytes(path);

                var app = new GameBoyApp(bytes, name, 160 + 4, 144 + 22, 40, 40);

                app.Initialize();

                Explorer.WindowManager.Applications.Add(app);
                app.zIndex = Explorer.WindowManager.GetTopZIndex() + 1;
                Explorer.WindowManager.MarkStackDirty();

                app.Visible = true;
                app.Focused = true;

                Kernel.ProcessManager.Start(app);

                Explorer.Taskbar.UpdateApplicationButtons();
                Explorer.WindowManager.UpdateFocusStatus();
            }
        }
    }
}
