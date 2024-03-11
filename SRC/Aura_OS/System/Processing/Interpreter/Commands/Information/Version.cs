/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Version
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.System.Processing.Interpreter;
using Aura_OS.System.Network;
using Cosmos.System.Network;
using JZero;
using System;
using System.Linq;

namespace Aura_OS.System.Processing.Interpreter.Commands.SystemInfomation
{
    class CommandVersion : ICommand
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CommandVersion(string[] commandvalues) : base(commandvalues)
        {
            Description = "to display system version";
        }

        /// <summary>
        /// RebootCommand
        /// </summary>
        public override ReturnInfo Execute()
        {
            bool isOutdated = false;

            Console.ForegroundColor = ConsoleColor.White;

            if (NetworkStack.ConfigEmpty())
            {
                Console.WriteLine("Aura [version " + Kernel.Version + "-" + Kernel.Revision + "]");
            }
            else
            {
                (string latestVersion, string latestRevision, string latestReleaseUrl) = System.Network.Version.GetLastVersionInfo();

                int versionComparisonResult = System.Network.Version.CompareVersions(Kernel.Version, latestVersion);

                if (string.IsNullOrEmpty(Kernel.Version) || string.IsNullOrEmpty(latestVersion) || string.IsNullOrEmpty(Kernel.Revision) || string.IsNullOrEmpty(latestRevision))
                {
                    Console.WriteLine("Aura [version " + Kernel.Version + "-" + Kernel.Revision + "]");
                }
                else
                {
                    if (versionComparisonResult > 0)
                    {
                        Console.WriteLine("Aura [version " + Kernel.Version + "-" + Kernel.Revision + "], you are on a dev version (last release is " + latestVersion + "-" + latestRevision + ").");
                    }
                    else if (versionComparisonResult < 0)
                    {
                        Console.WriteLine("Aura [version " + Kernel.Version + "-" + Kernel.Revision + "], your version is outdated (last release is " + latestVersion + "-" + latestRevision + ").");
                        isOutdated = true;
                    }
                    else
                    {
                        int revisionComparisonResult = System.Network.Version.CompareRevisions(Kernel.Revision, latestRevision);
                        if (revisionComparisonResult > 0)
                        {
                            Console.WriteLine("Aura [version " + Kernel.Version + "-" + Kernel.Revision + "], you are on a dev version (last release is " + latestVersion + "-" + latestRevision + ").");
                        }
                        else if (revisionComparisonResult < 0)
                        {
                            Console.WriteLine("Aura [version " + Kernel.Version + "-" + Kernel.Revision + "], your revision is outdated (last release is " + latestVersion + "-" + latestRevision + ").");
                            isOutdated = true;
                        }
                        else
                        {
                            Console.WriteLine("Aura [version " + Kernel.Version + "-" + Kernel.Revision + "], you are up to date.");
                        }
                    }
                }

                if (isOutdated)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Download last .iso at: " + latestReleaseUrl);
                    Console.ForegroundColor = ConsoleColor.White;

                    Console.WriteLine();
                }
            }

            Console.WriteLine("Created by Alexy DA CRUZ and Valentin CHARBONNIER.");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Website: github.com/aura-systems");
            Console.ForegroundColor = ConsoleColor.White;

            return new ReturnInfo(this, ReturnCode.OK);
        }
    }
}