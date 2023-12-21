/*
* PROJECT:          Aura Operating System Development
* CONTENT:          HTTP package manager for Cosmos (view https://github.com/aura-systems/CosmosExecutable to make packages)
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Network;
using JZero;
using System;
using System.Collections.Generic;

namespace Aura_OS.System.Processing
{
    public class PackageManager
    {
        public string RepositoryUrl = "http://aura.valentin.bzh/repository.json";
        public List<Package> Repository;
        public List<Package> Packages;

        public void Initialize()
        {
            Repository = new List<Package>();
            Packages = new List<Package>();
        }

        public void Update()
        {
            Console.WriteLine("Updating from '" + RepositoryUrl + "'...");

            string json = Http.DownloadFile(RepositoryUrl);

            var rdr = new JsonReader(json);
            rdr.ReadArrayStart();
            {
                while (rdr.NextElement())
                {
                    var package = new Package();
                    package.Installed = false;

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

                    Kernel.PackageManager.Repository.Add(package);

                }
            }
            rdr.ReadEof();

            Console.WriteLine("Package list updated, you can now add packages.");
        }

        public void Upgrade()
        {
            Console.WriteLine("Upgrading packages...");

            bool upgraded = false;

            foreach (var package in Packages)
            {
                Console.Write("- '" + package.Link + "' ");
                package.Download();
                Console.WriteLine("[OK]");

                upgraded = true;
            }

            if (!upgraded)
            {
                Console.WriteLine("No package found.");
            }
        }

        public void Add(string packageName)
        {
            foreach (var package in Repository)
            {
                if (package.Name == packageName)
                {
                    package.Installed = true;
                    Packages.Add(package);
                    package.Download();

                    Console.WriteLine(packageName + " added.");

                    return;
                }
            }

            Console.WriteLine(packageName + " not found.");
        }

        public void Remove(string packageName)
        {
            foreach (var package in Repository)
            {
                if (package.Name == packageName)
                {
                    package.Installed = false;
                    Packages.Remove(package);

                    Console.WriteLine(packageName + " removed.");

                    return;
                }
            }

            Console.WriteLine(packageName + " not found.");
        }
    }
}
