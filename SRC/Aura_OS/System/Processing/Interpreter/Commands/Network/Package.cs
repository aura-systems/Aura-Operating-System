/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Package Manager command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Aura_OS.System.Network;

namespace Aura_OS.System.Processing.Interpreter.Commands.Network
{
    class CommandPackage : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandPackage(string[] commandvalues) : base(commandvalues, CommandType.Network)
        {
            Description = "to manage packages (.pkg programs).";
        }

        /// <summary>
        /// CommandDns
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute()
        {
            PrintHelp();
            return new ReturnInfo(this, ReturnCode.OK);
        }

        /// <summary>
        /// CommandPackage
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public override ReturnInfo Execute(List<string> arguments)
        {
            if (arguments.Count == 0 || arguments.Count > 2)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG);
            }

            try
            {
                string command = arguments[0];

                if (command == "/update")
                {
                    Console.WriteLine("Updating from '" + Kernel.PackageManager.ListUrl + "'...");
                    Kernel.PackageManager.Update();
                    Console.WriteLine("Package list updated, you can now add packages.");

                    return new ReturnInfo(this, ReturnCode.OK);
                }
                else if (command == "/repository")
                {
                    if (arguments.Count == 2)
                    {
                        if (Kernel.PackageManager.SetRepository(arguments[1]))
                        {
                            Console.WriteLine("Repository changed, 'pkg /update' reads its package list.");
                        }
                        else
                        {
                            Console.WriteLine("Repository changed until the next boot (Aura is not installed).");
                        }
                    }

                    Console.WriteLine("Repository: " + Kernel.PackageManager.RepositoryUrl);

                    return new ReturnInfo(this, ReturnCode.OK);
                }
                else if (command == "/upgrade")
                {
                    Kernel.PackageManager.Upgrade();

                    return new ReturnInfo(this, ReturnCode.OK);
                }
                else if (command == "/list")
                {
                    if (Kernel.PackageManager.Repository.Count == 0)
                    {
                        Console.WriteLine("No package found! Please make 'pkg /update' to update the package list.");
                        return new ReturnInfo(this, ReturnCode.OK);
                    }
                    else
                    {
                        Console.WriteLine("Package list:");

                        foreach (var package in Kernel.PackageManager.Repository)
                        {
                            Console.WriteLine("- " + package.Name + " v" + package.Version + " (by " + package.Author + "), " + (package.Installed ? "installed." : "not installed."));
                            Console.WriteLine("\t" + package.Description);
                        }

                        return new ReturnInfo(this, ReturnCode.OK);
                    }
                }
                else if (command == "/add")
                {
                    if (arguments.Count != 2)
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                    }

                    bool saved;
                    Package package = Kernel.PackageManager.Add(arguments[1], out saved);

                    Console.WriteLine(saved
                        ? package.Name + " installed."
                        : package.Name + " added until the next boot (no Programs folder).");

                    if (package.IsApp)
                    {
                        Console.WriteLine(package.InMenu
                            ? "The start menu or 'run " + package.Name + "' opens it."
                            : "'run " + package.Name + "' opens it.");
                    }

                    return new ReturnInfo(this, ReturnCode.OK);
                }
                else if (command == "/remove")
                {
                    if (arguments.Count != 2)
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                    }

                    string name = arguments[1];

                    Console.WriteLine(Kernel.PackageManager.Remove(name)
                        ? name + " removed, the built-in one is back."
                        : name + " removed.");

                    return new ReturnInfo(this, ReturnCode.OK);
                }
                else
                {
                    return new ReturnInfo(this, ReturnCode.ERROR_ARG, "Unknown package command.");
                }
            }
            catch (Exception ex)
            {
                // Network/HTTP failures (HttpRequestException, whose innermost exception says why), a package
                // that is not in the list, not installed or built in.
                return new ReturnInfo(this, ReturnCode.ERROR, Http.Describe(ex));
            }
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - pkg /repository [url]");
            Console.WriteLine(" - pkg /update");
            Console.WriteLine(" - pkg /upgrade");
            Console.WriteLine(" - pkg /list");
            Console.WriteLine(" - pkg /add {package_name}");
            Console.WriteLine(" - pkg /remove {package_name}");
        }
    }
}