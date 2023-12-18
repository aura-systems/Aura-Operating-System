/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Package Manager command
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Cosmos.System.Network.IPv4.UDP.DNS;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.Config;
using CosmosHttp.Client;
using Aura_OS.Interpreter;
using System.IO;
using Aura_OS.System.Network;
using Aura_OS.System.Processing;

namespace Aura_OS.System.Shell.cmdIntr.Network
{
    class CommandPackage : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandPackage(string[] commandvalues) : base(commandvalues, CommandType.Network)
        {
            Description = "to manage Cosmos Executables.";
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
                    Kernel.PackageManager.Update();

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

                    var packageName = arguments[1];

                    Kernel.PackageManager.Add(packageName);

                    return new ReturnInfo(this, ReturnCode.OK);
                }
                else if (command == "/remove")
                {
                    if (arguments.Count != 2)
                    {
                        return new ReturnInfo(this, ReturnCode.ERROR_ARG);
                    }

                    var packageName = arguments[1];

                    Kernel.PackageManager.Remove(packageName);

                    return new ReturnInfo(this, ReturnCode.OK);
                }
                else
                {
                    return new ReturnInfo(this, ReturnCode.ERROR_ARG, "Unknown package command.");
                }
            }
            catch (Exception ex)
            {
                return new ReturnInfo(this, ReturnCode.ERROR_ARG, ex.ToString());
            }
        }

        /// <summary>
        /// Print /help information
        /// </summary>
        public override void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine(" - pkg /update");
            Console.WriteLine(" - pkg /upgrade");
            Console.WriteLine(" - pkg /list");
            Console.WriteLine(" - pkg /add {package_name}");
            Console.WriteLine(" - pkg /remove {package_name}");
        }
    }
}