/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - Version
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.Interpreter;
using Aura_OS.System.Network;
using Cosmos.System.Network;
using JZero;
using System;
using System.Linq;

namespace Aura_OS.System.Shell.cmdIntr.SystemInfomation
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
            Console.ForegroundColor = ConsoleColor.White;

            if (NetworkStack.ConfigEmpty())
            {
                Console.WriteLine("Aura [version " + Kernel.Version + "-" + Kernel.Revision + "]");
            }
            else
            {
                string json = Http.DownloadFile("http://aura.valentin.bzh");

                JsonReader rdr = new(json);
                rdr.ReadObjectStart();

                string latestVersion = null;
                string latestRevision = null;

                while (rdr.NextProperty())
                {
                    var charSegment = rdr.ReadPropertyName();
                    string propertyName = new string(charSegment.Array, charSegment.Offset, charSegment.Count);

                    if (propertyName.Equals("last-version"))
                    {
                        var latestVersionCharSegment = rdr.ReadString();
                        latestVersion = new string(latestVersionCharSegment.Array, latestVersionCharSegment.Offset, latestVersionCharSegment.Count);
                    }
                    else if (propertyName.Equals("last-revision"))
                    {

                        var latestRevisionCharSegment = rdr.ReadString();
                        latestRevision = new string(latestRevisionCharSegment.Array, latestRevisionCharSegment.Offset, latestRevisionCharSegment.Count);
                    }
                }

                rdr.ReadEof();

                int versionComparisonResult = CompareVersions(Kernel.Version, latestVersion);

                if (versionComparisonResult > 0)
                {
                    Console.WriteLine("Aura [version " + Kernel.Version + "-" + Kernel.Revision + "], you are on a dev version.");
                }
                else if (versionComparisonResult < 0)
                {
                    Console.WriteLine("Aura [version " + Kernel.Version + "-" + Kernel.Revision + "], your version is outdated.");
                }
                else
                {
                    int revisionComparisonResult = string.Compare(Kernel.Revision, latestRevision);
                    if (revisionComparisonResult < 0)
                    {
                        Console.WriteLine("Aura [version " + Kernel.Version + "-" + Kernel.Revision + "], your version is outdated.");
                    }
                    else
                    {
                        Console.WriteLine("Aura [version " + Kernel.Version + "-" + Kernel.Revision + "], you are up to date.");
                    }
                }
            }

            Console.WriteLine("Created by Alexy DA CRUZ and Valentin CHARBONNIER.");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Website: github.com/aura-systems");
            Console.ForegroundColor = ConsoleColor.White;

            return new ReturnInfo(this, ReturnCode.OK);
        }

        int CompareVersions(string version1, string version2)
        {
            var version1Parts = version1.Split('.').Select(int.Parse).ToArray();
            var version2Parts = version2.Split('.').Select(int.Parse).ToArray();

            for (int i = 0; i < Math.Min(version1Parts.Length, version2Parts.Length); i++)
            {
                if (version1Parts[i] > version2Parts[i])
                    return 1;
                if (version1Parts[i] < version2Parts[i])
                    return -1;
            }

            return version1Parts.Length.CompareTo(version2Parts.Length);
        }
    }
}