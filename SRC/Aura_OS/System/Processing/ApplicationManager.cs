/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Application Manager
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Processing.Applications;
using Aura_OS.System.Processing.Processes;
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

            // Package apps: built in (SRC/Packages) or installed in Programs/.
            foreach (Package package in Kernel.PackageManager.Packages)
            {
                if (package.IsApp && package.InMenu)
                {
                    RegisterApplication(new ApplicationConfig(package, 40, 40));
                }
            }
        }

        /// <summary>
        /// Registers the package apps again and rebuilds the start menu, after a package was added,
        /// upgraded or removed.
        /// </summary>
        public void ReloadApplications()
        {
            ApplicationTemplates.Clear();
            LoadApplications();

            // Null until the desktop starts, which builds it from the apps registered then.
            if (Explorer.StartMenu != null)
            {
                Explorer.RebuildStartMenu();
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
                StartPackage("Picture", new List<string> { Path.Combine(currentPath, fileName) });
            }
            else
            {
                // Any other file opens in the Editor package.
                StartPackage("Editor", new List<string> { Path.Combine(currentPath, fileName) });
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
