using System;
using System.Collections.Generic;
using System.Text;

namespace WMCommandFramework.COSMOS
{
    public class CommandVersion
    {
        private int[] arrx = new int[0];
        private string tag = "";

        /// <summary>
        /// Creates version information for a command.
        /// </summary>
        /// <param name="major">The major version of the command.</param>
        /// <param name="minor">The minor version of the command.</param>
        /// <param name="build">The build version of the command.</param>
        /// <param name="revision">The revision version of the command.</param>
        /// <param name="tag">The optional display tag. I.E. BETA</param>
        public CommandVersion(int major, int minor, int build, int revision, string tag = "")
        {
            arrx = new int[] { major, minor, build, revision };
            Tag = tag;
        }

        /// <summary>
        /// Creates version information for a command.
        /// </summary>
        /// <param name="minor">The minor version of the command.</param>
        /// <param name="build">The build version of the command.</param>
        /// <param name="revision">The revision version of the command.</param>
        /// <param name="tag">The optional display tag. I.E. BETA</param>
        public CommandVersion(int minor, int build, int revision, string tag = "")
        {
            arrx = new int[] { minor, build, revision };
            Tag = tag;
        }

        /// <summary>
        /// Creates version information for a command.
        /// </summary>
        /// <param name="build"></param>
        /// <param name="revision"></param>
        /// <param name="tag"></param>
        public CommandVersion(int build, int revision, string tag = "")
        {
            arrx = new int[] { build, revision };
            Tag = tag;
        }

        /// <summary>
        /// The MAJOR version.
        /// </summary>
        public int Major
        {
            get
            {
                if (arrx.Length == 4)
                    return arrx[0];
                else
                    return 0;
            }
        }

        /// <summary>
        /// The MINOR version.
        /// </summary>
        public int Minor
        {
            get
            {
                if (arrx.Length == 4)
                    return arrx[1];
                else if (arrx.Length == 3)
                    return arrx[0];
                else return 0;
            }
        }

        /// <summary>
        /// The BUILD version.
        /// </summary>
        public int Build
        {
            get
            {
                if (arrx.Length == 4)
                    return arrx[2];
                else if (arrx.Length == 3)
                    return arrx[1];
                else if (arrx.Length == 2)
                    return arrx[0];
                else return 0;
            }
        }

        /// <summary>
        /// The REBUILD version.
        /// </summary>
        public int Revision
        {
            get
            {
                if (arrx.Length == 4)
                    return arrx[3];
                else if (arrx.Length == 3)
                    return arrx[2];
                else if (arrx.Length == 2)
                    return arrx[1];
                else return 0;
            }
        }

        /// <summary>
        /// Optional display tag.
        /// </summary>
        public string Tag
        {
            get
            {
                if (!(tag == "" | tag == null))
                    return tag;
                else
                    return "";
            }
            private set => tag = value;
        }

        /// <summary>
        /// Checks if this version is equal to that of another.
        /// </summary>
        /// <param name="version">The version to compare.</param>
        /// <returns>Both versions are equal to each other.</returns>
        public bool IsExact(CommandVersion version)
        {
            if (Major == version.Major && Minor == version.Minor && Build == version.Build && Revision == version.Revision)
                return true;
            return false;
        }

        /// <summary>
        /// Checks if the current MAJOR value is greater then that of another.
        /// </summary>
        /// <param name="version">The version to compare.</param>
        /// <returns>This version MAJOR value is greater.</returns>
        public bool IsMajorGreater(CommandVersion version)
        {
            if (Major > version.Major) return true;
            return false;
        }

        /// <summary>
        /// Checks if the current MINOR value is greater then that of another.
        /// </summary>
        /// <param name="version">The version to compare.</param>
        /// <returns>This version MINOR value is greater.</returns>
        public bool IsMinorGreater(CommandVersion version)
        {
            if (Minor > version.Minor) return true;
            return false;
        }

        /// <summary>
        /// Checks if the current BUILD value is greater then that of another.
        /// </summary>
        /// <param name="version">The version to compare.</param>
        /// <returns>This version BUILD value is greater.</returns>
        public bool IsBuildGreater(CommandVersion version)
        {
            if (Build > version.Build) return true;
            return false;
        }

        /// <summary>
        /// Checks if the current REVISION value is greater then that of another.
        /// </summary>
        /// <param name="version">The version to compare.</param>
        /// <returns>This version REVISION value is greater.</returns>
        public bool IsRevisionGreater(CommandVersion version)
        {
            if (Revision > version.Revision) return true;
            return false;
        }

        /// <summary>
        /// Takes all values from the constructer and places them all into a decimal-based version string.
        /// </summary>
        /// <returns>A decimal-based version string. [major].[minor].[build].[rebuild]-[tag]</returns>
        public override string ToString()
        {
            string x = "";
            if (arrx.Length == 4)
            {
                x = $"{Major}.{Minor}.{Build}.{Revision}";
                if (Tag != null || Tag != "")
                    x += $"-{Tag}";
            }
            else if (arrx.Length == 3)
            {
                x = $"{Minor}.{Build}.{Revision}";
                if (Tag != null || Tag != "")
                    x += $"-{Tag}";
            }
            else if (arrx.Length == 2)
            {
                x = $"{Build}.{Revision}";
                if (Tag != null || Tag != "")
                    x += $"-{Tag}";
            }
            else
                x = "";
            return x;
        }
        
        /// <summary>
        /// A blank versions with all minimum values set to 0.
        /// </summary>
        public static CommandVersion Blank
        {
            get => new CommandVersion(0,0);
        }

        /// <summary>
        /// Parses a decimal-based string into a classifying version class.
        /// </summary>
        /// <param name="version">The version string to parse.</param>
        /// <returns>A CommandVersion class if successfully parsed.</returns>
        public static CommandVersion Parse(string version)
        {
            if (version.Contains("-"))
            {
                //Has Tag!
                var dat = version.Split('-');
                var tag = dat[1];
                var spl = dat[0].Split('.');
                int major = 0;
                int minor = 0;
                int build = 0;
                int revision = 0;
                if (spl.Length == 4)
                {
                    major = int.Parse(spl[0]);
                    minor = int.Parse(spl[1]);
                    build = int.Parse(spl[2]);
                    revision = int.Parse(spl[4]);
                    return new CommandVersion(major, minor, build, revision, tag);
                }
                else if (spl.Length == 3)
                {
                    minor = int.Parse(spl[0]);
                    build = int.Parse(spl[1]);
                    revision = int.Parse(spl[2]);
                    return new CommandVersion(minor, build, revision, tag);
                }
                else if (spl.Length == 2)
                {
                    build = int.Parse(spl[0]);
                    revision = int.Parse(spl[1]);
                    return new CommandVersion(build, revision, tag);
                }
                else
                {
                    return Blank;
                }
            }
            else
            {
                //Doesn't have Tag.
                var spl = version.Split('.');
                int major = 0;
                int minor = 0;
                int build = 0;
                int revision = 0;
                if (spl.Length == 4)
                {
                    major = int.Parse(spl[0]);
                    minor = int.Parse(spl[1]);
                    build = int.Parse(spl[2]);
                    revision = int.Parse(spl[4]);
                    return new CommandVersion(major, minor, build, revision);
                }
                else if (spl.Length == 3)
                {
                    minor = int.Parse(spl[0]);
                    build = int.Parse(spl[1]);
                    revision = int.Parse(spl[2]);
                    return new CommandVersion(minor, build, revision);
                }
                else if (spl.Length == 2)
                {
                    build = int.Parse(spl[0]);
                    revision = int.Parse(spl[1]);
                    return new CommandVersion(build, revision);
                }
                else
                {
                    return Blank;
                }
            }
        }

        /// <summary>
        /// Trys to parce a decimal-based string and returns if it was able to or not.
        /// </summary>
        /// <param name="version">The decimal-based string.</param>
        /// <param name="commandVersion">The output value that was parsed if the function could parse the data.</param>
        /// <returns>Whether the data was able to be parsed.</returns>
        public static bool TryParse(string version, out CommandVersion commandVersion)
        {
            var cmdver = Parse(version);
            if (cmdver == null)
            {
                commandVersion = new CommandVersion(0,0,0);
                return false;
            }
            else
            {
                commandVersion = cmdver;
                return true;
            }
        }
    }
}
