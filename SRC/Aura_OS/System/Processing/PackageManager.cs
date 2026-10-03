/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Package manager: the programs built in, installed or downloaded (.pkg)
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Filesystem;
using Aura_OS.System.Network;
using Aura_OS.System.Utils;
using JZero;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.System.Processing
{
    public class PackageManager : IManager
    {
        /// <summary>
        /// The online repository Aura comes with.
        /// </summary>
        public const string DefaultRepository = "https://aura.valentin.bzh";

        // The settings.ini key of the repository chosen with SetRepository.
        private const string RepositorySetting = "packageRepository";

        private const string ListFile = "repository.json";

        /// <summary>
        /// The online repository: its address, whose repository.json is the package list, or the
        /// address of the list itself (ending with .json). settings.ini keeps it once changed.
        /// </summary>
        public string RepositoryUrl { get; private set; } = DefaultRepository;

        /// <summary>
        /// The address of the repository's package list.
        /// </summary>
        public string ListUrl
        {
            get
            {
                return RepositoryUrl.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? RepositoryUrl : RepositoryUrl + "/" + ListFile;
            }
        }

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

            if (Kernel.Installed)
            {
                // C6: a missing key reads as "null".
                string repository = new Settings(AuraPaths.SettingsIni).GetValue(RepositorySetting);

                if (IsValidUrl(repository))
                {
                    RepositoryUrl = repository;
                }
            }

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

        /// <summary>
        /// Changes the repository, kept in settings.ini on an installed Aura. The package list goes
        /// with the old repository: Update reads the new one's.
        /// </summary>
        /// <param name="url">An http:// or https:// address (RepositoryUrl); a '/' at its end is dropped.</param>
        /// <returns>False in live mode: the repository is the default one again after a reboot.</returns>
        /// <exception cref="ArgumentException">Not an http:// or https:// address.</exception>
        public bool SetRepository(string url)
        {
            url = (url ?? "").Trim().TrimEnd('/');

            if (!IsValidUrl(url))
            {
                throw new ArgumentException("A repository is an http:// or https:// address, not '" + url + "'.");
            }

            if (url != RepositoryUrl)
            {
                RepositoryUrl = url;
                Repository.Clear();
            }

            if (!Kernel.Installed)
            {
                return false;
            }

            Settings config = new Settings(AuraPaths.SettingsIni);
            config.EditValue(RepositorySetting, url);
            config.Push();
            return true;
        }

        /// <summary>
        /// Downloads the repository's package list (ListUrl). The list stays as it was when it fails.
        /// </summary>
        /// <exception cref="Exception">The download failed, or the list is not valid.</exception>
        public void Update()
        {
            string json = Http.DownloadFile(ListUrl);
            List<RepositoryPackage> packages = new List<RepositoryPackage>();

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

                    // Nothing to download without them.
                    if (string.IsNullOrEmpty(package.Name) || string.IsNullOrEmpty(package.Link))
                    {
                        continue;
                    }

                    Package installed = Find(package.Name);
                    package.Installed = installed != null && !installed.BuiltIn;

                    packages.Add(package);
                }
            }
            rdr.ReadEof();

            // gen2 appended to the list on every update.
            Repository.Clear();
            Repository.AddRange(packages);
        }

        /// <summary>
        /// The repository's entry with that name (any case), null when the package list has none.
        /// </summary>
        public RepositoryPackage FindInRepository(string name)
        {
            foreach (RepositoryPackage entry in Repository)
            {
                if (string.Equals(entry.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return entry;
                }
            }

            return null;
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

        /// <summary>
        /// Downloads a package of the repository's list and installs it, replacing the one of the same
        /// name: a built-in one too, until the download is removed.
        /// </summary>
        /// <param name="saved">False in live mode (no Programs folder): the package is gone after a reboot.</param>
        /// <exception cref="Exception">Not in the package list, or the download failed or is not that package.</exception>
        public Package Add(string name, out bool saved)
        {
            RepositoryPackage entry = FindInRepository(name);

            if (entry == null)
            {
                throw new InvalidOperationException(name + " is not in the package list.");
            }

            Package package = entry.Download();

            // The list and the program would go by different names.
            if (!string.Equals(package.Name, entry.Name, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("The repository's " + entry.Name + " package is named " + package.Name + ".");
            }

            saved = Install(package);
            entry.Installed = true;
            return package;
        }

        /// <summary>
        /// Removes a downloaded package, and its file from Programs/.
        /// </summary>
        /// <returns>True when it had replaced a built-in package, which is back.</returns>
        /// <exception cref="InvalidOperationException">No such package, or a built-in one.</exception>
        public bool Remove(string name)
        {
            Package package = Find(name);

            if (package == null)
            {
                throw new InvalidOperationException(name + " is not installed.");
            }

            if (package.BuiltIn)
            {
                throw new InvalidOperationException(package.Name + " is built into Aura, it cannot be removed.");
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

            RepositoryPackage entry = FindInRepository(package.Name);

            if (entry != null)
            {
                entry.Installed = false;
            }

            return restored;
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
        /// An http:// or https:// address with a host.
        /// </summary>
        private static bool IsValidUrl(string url)
        {
            if (string.IsNullOrEmpty(url) || url.IndexOf(' ') >= 0)
            {
                return false;
            }

            string rest;

            if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                rest = url.Substring(8);
            }
            else if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                rest = url.Substring(7);
            }
            else
            {
                return false;
            }

            return rest.Length > 0 && rest[0] != '/';
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
