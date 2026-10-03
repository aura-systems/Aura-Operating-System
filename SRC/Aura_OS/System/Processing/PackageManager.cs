/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Package manager: the programs built in, installed or downloaded (.pkg)
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Filesystem;
using Aura_OS.System.Network;
using JZero;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.System.Processing
{
    public class PackageManager : IManager
    {
        public string RepositoryUrl = "https://aura.valentin.bzh/repository.json";

        /// <summary>
        /// The repository's package list, filled by Update.
        /// </summary>
        public List<RepositoryPackage> Repository;

        /// <summary>
        /// The programs Aura can run: the packages built into the kernel (SRC/Packages), then those in
        /// Programs/, which replace a built-in one of the same name.
        /// </summary>
        public List<Package> Packages;

        public void Initialize()
        {
            CustomConsole.WriteLineInfo("Starting package manager...");

            Repository = new List<RepositoryPackage>();
            Packages = new List<Package>();

            foreach (string file in Files.List("Packages/"))
            {
                if (file.EndsWith(Package.Extension, StringComparison.OrdinalIgnoreCase))
                {
                    Load(file, () => Files.Get(file), true);
                }
            }

            if (Directory.Exists(AuraPaths.ProgramsDir))
            {
                foreach (string file in Directory.GetFiles(AuraPaths.ProgramsDir))
                {
                    if (file.EndsWith(Package.Extension, StringComparison.OrdinalIgnoreCase))
                    {
                        string path = Path.Combine(AuraPaths.ProgramsDir, Path.GetFileName(file));
                        Load(path, () => File.ReadAllBytes(path), false);
                    }
                }
            }
        }

        /// <summary>
        /// The package with that name (any case), null when there is none.
        /// </summary>
        public Package Find(string name)
        {
            foreach (Package package in Packages)
            {
                if (string.Equals(package.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return package;
                }
            }

            return null;
        }

        public void Update()
        {
            Console.WriteLine("Updating from '" + RepositoryUrl + "'...");

            string json;
            try
            {
                json = Http.DownloadFile(RepositoryUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to download the package list: " + ex.Message);
                return;
            }

            // gen2 appended to the list on every update.
            Repository.Clear();

            var rdr = new JsonReader(json);
            rdr.ReadArrayStart();
            {
                while (rdr.NextElement())
                {
                    var package = new RepositoryPackage();

                    rdr.ReadObjectStart();
                    {
                        while (rdr.NextProperty())
                        {
                            var charSegment = rdr.ReadPropertyName();
                            var charSegment2 = rdr.ReadString();

                            string propertyName = new string(charSegment.Array, charSegment.Offset, charSegment.Count);
                            string propertyValue = new string(charSegment2.Array, charSegment2.Offset, charSegment2.Count);
                            switch (propertyName)
                            {
                                case "name":
                                    package.Name = propertyValue;
                                    break;
                                case "display-name":
                                    package.DisplayName = propertyValue;
                                    break;
                                case "description":
                                    package.Description = propertyValue;
                                    break;
                                case "author":
                                    package.Author = propertyValue;
                                    break;
                                case "link":
                                    package.Link = propertyValue;
                                    break;
                                case "version":
                                    package.Version = propertyValue;
                                    break;
                            }
                        }
                    }

                    Package installed = Find(package.Name);
                    package.Installed = installed != null && !installed.BuiltIn;

                    Repository.Add(package);
                }
            }
            rdr.ReadEof();

            Console.WriteLine("Package list updated, you can now add packages.");
        }

        public void Upgrade()
        {
            Console.WriteLine("Upgrading packages...");

            bool upgraded = false;

            foreach (RepositoryPackage entry in Repository)
            {
                if (!entry.Installed)
                {
                    continue;
                }

                Console.Write("- '" + entry.Link + "' ");

                try
                {
                    Install(entry.Download());
                    Console.WriteLine("[OK]");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[FAILED] " + ex.Message);
                }

                upgraded = true;
            }

            if (!upgraded)
            {
                Console.WriteLine("No package found.");
            }
        }

        public void Add(string packageName)
        {
            foreach (RepositoryPackage entry in Repository)
            {
                if (entry.Name == packageName)
                {
                    Package package;

                    try
                    {
                        package = entry.Download();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to download " + packageName + ": " + ex.Message);
                        return;
                    }

                    entry.Installed = true;

                    if (Install(package))
                    {
                        Console.WriteLine(package.Name + " installed.");
                    }
                    else
                    {
                        Console.WriteLine(package.Name + " added until the next boot (no Programs folder).");
                    }

                    if (package.IsApp)
                    {
                        Console.WriteLine(package.InMenu
                            ? "The start menu or 'run " + package.Name + "' opens it."
                            : "'run " + package.Name + "' opens it.");
                    }

                    return;
                }
            }

            Console.WriteLine(packageName + " not found.");
        }

        public void Remove(string packageName)
        {
            Package package = Find(packageName);

            if (package == null)
            {
                Console.WriteLine(packageName + " not found.");
                return;
            }

            if (package.BuiltIn)
            {
                Console.WriteLine(package.Name + " is built into Aura, it cannot be removed.");
                return;
            }

            Packages.Remove(package);

            // A download that replaced a built-in package (pkg /add Settings) gives the built-in one back.
            bool restored = RestoreBuiltIn(package.Name);
            ReloadApplications();

            string path = AuraPaths.ProgramsDir + package.Name + Package.Extension;

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            foreach (RepositoryPackage entry in Repository)
            {
                if (string.Equals(entry.Name, package.Name, StringComparison.OrdinalIgnoreCase))
                {
                    entry.Installed = false;
                }
            }

            Console.WriteLine(restored ? package.Name + " removed, the built-in one is back." : package.Name + " removed.");
        }

        /// <summary>
        /// Makes the package available (replacing one of the same name), in the start menu too, and
        /// writes it to Programs/.
        /// </summary>
        /// <returns>False when there is no Programs folder (live mode): the package is gone after a reboot.</returns>
        private bool Install(Package package)
        {
            Use(package);
            ReloadApplications();

            if (!Directory.Exists(AuraPaths.ProgramsDir))
            {
                return false;
            }

            File.WriteAllBytes(AuraPaths.ProgramsDir + package.Name + Package.Extension, package.RawData);
            return true;
        }

        private void Use(Package package)
        {
            Package existing = Find(package.Name);

            if (existing != null)
            {
                Packages.Remove(existing);
            }

            Packages.Add(package);
        }

        /// <summary>
        /// Makes the package built into the kernel with that name available again.
        /// </summary>
        /// <returns>False when the kernel has none, or it cannot be read.</returns>
        private bool RestoreBuiltIn(string name)
        {
            byte[] data;

            if (!Files.TryGet("Packages/" + name + Package.Extension, out data))
            {
                return false;
            }

            // Not Load, whose CustomConsole messages paint the boot console over the desktop.
            try
            {
                Package package = new Package(data);
                package.BuiltIn = true;
                Use(package);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Shows the apps of the packages as they are now in the start menu.
        /// </summary>
        private static void ReloadApplications()
        {
            // Null while boot.bat runs: the application manager reads the packages when it starts.
            if (Kernel.ApplicationManager != null)
            {
                Kernel.ApplicationManager.ReloadApplications();
            }
        }

        /// <summary>
        /// Reads a package at boot. A broken one is reported and skipped.
        /// </summary>
        private void Load(string path, Func<byte[]> read, bool builtIn)
        {
            try
            {
                Package package = new Package(read());
                package.BuiltIn = builtIn;
                Use(package);

                CustomConsole.WriteLineOK(package.Name + " package loaded.");
            }
            catch (Exception ex)
            {
                CustomConsole.WriteLineError("Cannot load " + path + ": " + ex.Message);
            }
        }

        /// <summary>
        /// Returns the name of the manager.
        /// </summary>
        /// <returns>The name of the manager.</returns>
        public string GetName()
        {
            return "Package Manager";
        }
    }
}
