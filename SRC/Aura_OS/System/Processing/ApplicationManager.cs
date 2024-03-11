/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Application Manager
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI;
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
        public int Width;
        public int Height;

        public ApplicationConfig(Type template, int x, int y, int width, int height)
        {
            Template = template;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    public class ApplicationManager : IManager
    {
        public List<ApplicationConfig> ApplicationTemplates;

        public void Initialize()
        {
            CustomConsole.WriteLineInfo("Starting application manager...");

            ApplicationTemplates = new List<ApplicationConfig>();

            CustomConsole.WriteLineInfo("Registering applications...");
            LoadApplications();
        }

        public void LoadApplications()
        {
            RegisterApplication(typeof(TerminalApp), 40, 40, 700, 600);
            //RegisterApplication(typeof(ExplorerApp), 40, 40, 500, 400);
            RegisterApplication(typeof(MemoryInfoApp), 40, 40, 400, 300);
            RegisterApplication(typeof(SystemInfoApp), 40, 40, 402, 360);
            RegisterApplication(typeof(CubeApp), 40, 40, 200, 200);
            RegisterApplication(typeof(GameBoyApp), 40, 40, 160 + 6, 144 + 26);
            RegisterApplication(typeof(SampleApp), 40, 40, 500, 500);
            RegisterApplication(typeof(SettingsApp), 40, 40, 340, 350);
        }

        public void RegisterApplication(ApplicationConfig config)
        {
            ApplicationTemplates.Add(config);
        }

        public void RegisterApplication(Type template, int x, int y, int width, int height)
        {
            ApplicationConfig config = new(template, x, y, width, height);
            ApplicationTemplates.Add(config);
        }

        public void StartApplication(ApplicationConfig config)
        {
            Application app = Kernel.ApplicationManager.Instantiate(config);
            app.Initialize();
            app.MarkFocused();
            app.Visible = true;

            Explorer.WindowManager.Applications.Add(app);
            Kernel.ProcessManager.Start(app);

            Explorer.Taskbar.UpdateApplicationButtons();
        }

        public void StartApplication(Type appType)
        {
            Application app = Kernel.ApplicationManager.Instantiate(appType);
            app.Initialize();
            app.MarkFocused();
            app.Visible = true;

            Explorer.WindowManager.Applications.Add(app);
            Kernel.ProcessManager.Start(app);

            Explorer.Taskbar.UpdateApplicationButtons();
        }

        public Application Instantiate(Type appType)
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

        public Application Instantiate(ApplicationConfig config)
        {
            Application app = null;

            if (config.Template == typeof(TerminalApp))
            {
                app = new TerminalApp(config.Width, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(CubeApp))
            {
                app = new CubeApp(config.Width, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(SystemInfoApp))
            {
                app = new SystemInfoApp(config.Width, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(MemoryInfoApp))
            {
                app = new MemoryInfoApp(config.Width, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(GameBoyApp))
            {
                app = new GameBoyApp(config.Width, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(SampleApp))
            {
                app = new SampleApp(config.Width, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(SettingsApp))
            {
                app = new SettingsApp(config.Width, config.Height, config.X, config.Y);
            }
            /*else if (config.Template == typeof(ExplorerApp))
            {
                app = new ExplorerApp(Kernel.CurrentVolume, config.Weight, config.Height, config.X, config.Y);
            }*/
            else if (config.Template == typeof(EditorApp))
            {
                app = new EditorApp("", config.Width, config.Height, config.X, config.Y);
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
                    width = (int)bitmap.Width + 6;
                }

                var app = new PictureApp(name, bitmap, width, (int)bitmap.Height + 26, 40, 40);
                app.Initialize();
                app.MarkFocused();
                app.Visible = true;

                Explorer.WindowManager.Applications.Add(app);
                Kernel.ProcessManager.Start(app);

                Explorer.Taskbar.UpdateApplicationButtons();
            }
            else if (fileName.EndsWith(".gb"))
            {
                string path = currentPath + fileName;
                string name = fileName;
                byte[] bytes = File.ReadAllBytes(path);

                var app = new GameBoyApp(bytes, name, 160 + 6, 144 + 26, 40, 40);
                app.Initialize();
                app.MarkFocused();
                app.Visible = true;

                Explorer.WindowManager.Applications.Add(app);
                Kernel.ProcessManager.Start(app);

                Explorer.Taskbar.UpdateApplicationButtons();
            }
            else
            {
                string path = currentPath + fileName;

                var app = new EditorApp(path, 701, 600, 40, 40);
                app.Initialize();
                app.MarkFocused();
                app.Visible = true;

                Explorer.WindowManager.Applications.Add(app);
                Kernel.ProcessManager.Start(app);

                Explorer.Taskbar.UpdateApplicationButtons();
            }
        }

        public Application GetApplicationByPid(uint pid)
        {
            return Kernel.ProcessManager.GetProcessByPid(pid) as Application;
        }

        /// <summary>
        /// Returns the name of the manager.
        /// </summary>
        /// <returns>The name of the manager.</returns>
        public string GetName()
        {
            return "Application Manager";
        }
    }
}
