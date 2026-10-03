/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Application Manager
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Processing.Applications;
using Aura_OS.System.Processing.Applications.Emulators.GameBoyEmu;
using Aura_OS.System.Processing.Processes;
using Cosmos.Kernel.System.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.System.Processing
{
    public class ApplicationConfig
    {
        /// <summary>
        /// The app's class, null for a package app.
        /// </summary>
        public Type Template;

        /// <summary>
        /// A package app's package, else null.
        /// </summary>
        public Package Package;

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

        /// <summary>
        /// A package app, whose layout file gives the size.
        /// </summary>
        public ApplicationConfig(Package package, int x, int y)
        {
            Package = package;
            X = x;
            Y = y;
        }

        /// <summary>
        /// Start menu name: the class name without "App", or the package's display name.
        /// </summary>
        public string Name
        {
            get
            {
                if (Package != null)
                {
                    return Package.DisplayName;
                }

                string name = Template.Name;
                return name.EndsWith("App") && name.Length > 3 ? name.Substring(0, name.Length - 3) : name;
            }
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
            //RegisterApplication(typeof(ExplorerApp), 40, 40, 500, 400);
            RegisterApplication(typeof(GameBoyApp), 40, 40, 160 + 6, 144 + 26);

            // Package apps: built in (SRC/Packages) or installed in Programs/.
            foreach (Package package in Kernel.PackageManager.Packages)
            {
                if (package.IsApp && package.InMenu)
                {
                    RegisterApplication(new ApplicationConfig(package, 40, 40));
                }
            }
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
            Application app;

            // A package app's layout or Lua can fail: report it rather than take the start menu down.
            try
            {
                app = Kernel.ApplicationManager.Instantiate(config);
            }
            catch (Exception ex)
            {
                Logs.DoOSLog("[Error] Cannot start " + config.Name + ": " + ex.Message);
                return;
            }

            Show(app);
        }

        /// <summary>
        /// Opens a package app's window.
        /// </summary>
        /// <param name="args">Its main file's arguments, null for none.</param>
        /// <exception cref="InvalidOperationException">Its layout or main file failed.</exception>
        public void StartPackage(Package package, List<string> args = null)
        {
            Show(new PackageApp(package, 40, 40, args));
        }

        /// <summary>
        /// Opens the window of the package app with that name ("Terminal"). A missing or failing
        /// package is logged.
        /// </summary>
        /// <param name="args">Its main file's arguments, null for none.</param>
        public void StartPackage(string name, List<string> args = null)
        {
            Package package = Kernel.PackageManager.Find(name);

            if (package == null || !package.IsApp)
            {
                Logs.DoOSLog("[Error] Cannot start " + name + ": no such app package.");
                return;
            }

            PackageApp app;

            try
            {
                app = new PackageApp(package, 40, 40, args);
            }
            catch (Exception ex)
            {
                Logs.DoOSLog("[Error] Cannot start " + name + ": " + ex.Message);
                return;
            }

            Show(app);
        }

        /// <summary>
        /// Shows a new app focused, and starts its process.
        /// </summary>
        private void Show(Application app)
        {
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

            if (config.Package != null)
            {
                app = new PackageApp(config.Package, config.X, config.Y);
            }
            else if (config.Template == typeof(GameBoyApp))
            {
                app = new GameBoyApp(config.Width, config.Height, config.X, config.Y);
            }
            /*else if (config.Template == typeof(ExplorerApp))
            {
                app = new ExplorerApp(Kernel.CurrentVolume, config.Weight, config.Height, config.X, config.Y);
            }*/
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
                // Any other file opens in the Editor package.
                StartPackage("Editor", new List<string> { Path.Combine(currentPath, fileName) });
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
