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
using Cosmos.Kernel.System.Graphics;
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
            RegisterApplication(typeof(TerminalApp), 40, 40);
            //RegisterApplication(typeof(ExplorerApp), 40, 40, 500, 400);
            RegisterApplication(typeof(MemoryInfoApp), 40, 40);
            RegisterApplication(typeof(SystemInfoApp), 40, 40);
            RegisterApplication(typeof(GameBoyApp), 40, 40, 160 + 6, 144 + 26);
            RegisterApplication(typeof(SettingsApp), 40, 40);
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

        /// <summary>
        /// An app whose window comes from a layout file, which gives its size.
        /// </summary>
        public void RegisterApplication(Type template, int x, int y)
        {
            RegisterApplication(template, x, y, 0, 0);
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
                app = new TerminalApp(config.X, config.Y);
            }
            else if (config.Template == typeof(SystemInfoApp))
            {
                app = new SystemInfoApp(config.X, config.Y);
            }
            else if (config.Template == typeof(MemoryInfoApp))
            {
                app = new MemoryInfoApp(config.X, config.Y);
            }
            else if (config.Template == typeof(GameBoyApp))
            {
                app = new GameBoyApp(config.Width, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(SettingsApp))
            {
                app = new SettingsApp(config.X, config.Y);
            }
            /*else if (config.Template == typeof(ExplorerApp))
            {
                app = new ExplorerApp(Kernel.CurrentVolume, config.Weight, config.Height, config.X, config.Y);
            }*/
            else if (config.Template == typeof(EditorApp))
            {
                app = new EditorApp("", config.X, config.Y);
            }
            else
            {
                throw new InvalidOperationException("Type d'application non reconnu.");
            }

            return app;
        }

        public void StartFileApplication(string fileName, string currentPath)
        {
            // A null dereference halts the kernel on gen3, and Path.Combine throws on a null path.
            if (string.IsNullOrEmpty(fileName) || currentPath == null)
            {
                return;
            }

            if (fileName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
            {
                string path = Path.Combine(currentPath, fileName);
                string name = fileName;
                Bitmap bitmap;

                // GEN3-GAP(bmp): the gen3 BMP loader throws on top-down and < 24 bpp files (BI_BITFIELDS
                // masks are ignored). CheckBmpHeader rejects the headers it cannot survive at all.
                try
                {
                    byte[] bytes = File.ReadAllBytes(path);
                    CheckBmpHeader(bytes);
                    bitmap = new Bitmap(bytes);
                }
                catch (Exception ex)
                {
                    ReportOpenError(path, ex);
                    return;
                }

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
            else if (fileName.EndsWith(".gb", StringComparison.OrdinalIgnoreCase))
            {
                string path = Path.Combine(currentPath, fileName);
                string name = fileName;
                GameBoyApp app;

                try
                {
                    byte[] bytes = File.ReadAllBytes(path);
                    app = new GameBoyApp(bytes, name, 160 + 6, 144 + 26, 40, 40);
                }
                catch (Exception ex)
                {
                    ReportOpenError(path, ex);
                    return;
                }

                app.Initialize();
                app.MarkFocused();
                app.Visible = true;

                Explorer.WindowManager.Applications.Add(app);
                Kernel.ProcessManager.Start(app);

                Explorer.Taskbar.UpdateApplicationButtons();
            }
            else
            {
                string path = Path.Combine(currentPath, fileName);
                EditorApp app;

                try
                {
                    app = new EditorApp(path, 40, 40);
                }
                catch (Exception ex)
                {
                    ReportOpenError(path, ex);
                    return;
                }

                app.Initialize();
                app.MarkFocused();
                app.Visible = true;

                Explorer.WindowManager.Applications.Add(app);
                Kernel.ProcessManager.Start(app);

                Explorer.Taskbar.UpdateApplicationButtons();
            }
        }

        /// <summary>
        /// Reports a file that could not be opened (unreadable file, unsupported image or ROM).
        /// Logged rather than drawn: CustomConsole would paint the full-screen boot console over the desktop.
        /// </summary>
        private static void ReportOpenError(string path, Exception ex)
        {
            Logs.DoOSLog("[Error] Cannot open '" + path + "': " + ex.Message);
        }

        /// <summary>
        /// Throws for a BMP header the gen3 loader cannot survive: it divides by the height (a #DE
        /// halts the kernel) and allocates Width * Height pixels before reading any pixel data.
        /// </summary>
        private static void CheckBmpHeader(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 54)
            {
                throw new InvalidDataException("The file is too small to be a BMP image.");
            }

            int width = BitConverter.ToInt32(bytes, 18);
            int height = BitConverter.ToInt32(bytes, 22);

            // Only 24 and 32 bpp are supported, so every pixel takes at least 3 bytes of the file.
            if (width <= 0 || height <= 0 || (long)width * height * 3 > bytes.Length)
            {
                throw new InvalidDataException("Unsupported BMP size " + width + "x" + height + ".");
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
