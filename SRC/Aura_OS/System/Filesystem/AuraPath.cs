/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Path helpers: gen3 Unix paths (/N/...), gen2 DOS input (N:\...), system paths
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;

namespace Aura_OS.System.Filesystem
{
    /// <summary>
    /// Path helpers. The N-th FAT volume is mounted at /N, so gen2 "N:\a\b" is gen3 "/N/a/b";
    /// "/" is the virtual root listing the volumes.
    /// </summary>
    public static class AuraPath
    {
        public const char Separator = '/';

        /// <summary>
        /// gen2 "0:\a\b" -> AuraPaths.SystemVolume (or "/0/") + "a/b"; "N:\x" -> "/N/x"; any '\' -> '/'.
        /// For settings.ini values written by gen2 installs and for DOS-style user input. null/"" unchanged.
        /// </summary>
        public static string FromLegacy(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            int colon = path.IndexOf(':');
            if (colon > 0 && IsDigits(path, colon))
            {
                string drive = path.Substring(0, colon);
                string rest = path.Substring(colon + 1).TrimStart('\\', '/');

                // gen2 0:\ was by construction the volume holding System\settings.ini.
                string root = drive == "0" ? (AuraPaths.SystemVolume ?? "/0/") : "/" + drive + "/";

                path = root + rest;
            }

            return path.Replace('\\', Separator);
        }

        /// <summary>
        /// Absolute, normalised path for any user/argument input: legacy conversion, then anchored at
        /// Kernel.CurrentDirectory when relative, then Path.GetFullPath ("." and ".." collapsed).
        /// "" or null -> Kernel.CurrentDirectory.
        /// </summary>
        public static string Resolve(string input)
        {
            string current = AsDirectory(Kernel.CurrentDirectory);
            string path = FromLegacy(input ?? string.Empty);

            if (path.Length == 0)
            {
                return current;
            }

            if (path[0] != Separator)
            {
                path = current + path;
            }

            try
            {
                return Path.GetFullPath(path); // rooted input: pure string operation
            }
            catch (Exception)
            {
                return path;
            }
        }

        /// <summary>
        /// Ensures a trailing '/' ("/0/Users" -> "/0/Users/"). null or "" -> "/".
        /// </summary>
        public static string AsDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "/";
            }

            return path[path.Length - 1] == Separator ? path : path + Separator;
        }

        /// <summary>
        /// False for an empty name or one containing / \ : * ? " &lt; &gt; |
        /// GEN3-GAP(fat-names): the FAT driver accepts any character in long names.
        /// </summary>
        public static bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            for (int i = 0; i < name.Length; i++)
            {
                switch (name[i])
                {
                    case '/':
                    case '\\':
                    case ':':
                    case '*':
                    case '?':
                    case '"':
                    case '<':
                    case '>':
                    case '|':
                        return false;
                }
            }

            return true;
        }

        private static bool IsDigits(string s, int end)
        {
            for (int i = 0; i < end; i++)
            {
                if (!char.IsDigit(s[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// System paths. Always end with '/' for directories.
    /// </summary>
    public static class AuraPaths
    {
        /// <summary>
        /// "/N/" holding System/settings.ini, else "/0/" if mounted, else null (live mode). Set by Volumes.
        /// </summary>
        public static string SystemVolume { get; internal set; }

        public static string SystemDir => (SystemVolume ?? "/0/") + "System/";
        public static string SettingsIni => SystemDir + "settings.ini";
        public static string Passwd => SystemDir + "passwd";
        public static string ProgramsDir => SystemDir + "Programs/";
        public static string ThemesDir => SystemDir + "Themes/";
        public static string WallpapersDir => SystemDir + "Wallpapers/";
        public static string UsersDir => (SystemVolume ?? "/0/") + "Users/";

        public static string NetworkIni(string alias) => SystemDir + alias + ".ini";
    }
}
