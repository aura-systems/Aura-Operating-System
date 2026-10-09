using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aura_OS.System;
using Cosmos.Kernel.System.Graphics.Fonts;
using Cosmos.Kernel.System.Graphics;

namespace Aura_OS
{
    /// <summary>
    /// Embedded assets. gen3 has no ISO volume: every file under Resources/ is an embedded resource
    /// named "Aura_OS/&lt;path relative to Resources&gt;" (see Aura_OS.csproj), read with Get("UI/...").
    /// The built-in packages are Get("Packages/&lt;Name&gt;.pkg").
    /// </summary>
    public static class Files
    {
        /// <summary>
        /// Sentinel path prefix for an asset that is not on disk ("embedded:UI/Themes/Suave.skin.xml").
        /// Read such paths with ReadAllBytes/ReadAllText.
        /// </summary>
        public const string EmbeddedScheme = "embedded:";

        private const string ResourcePrefix = "Aura_OS/";

        private static byte[] _errorImage;
        private static byte[] _wallpaper;
        private static byte[] _wallpaper2;
        private static byte[] _auralogoWhite;

        private static readonly Dictionary<string, Bitmap> _images = new Dictionary<string, Bitmap>();

        //200x178 .bmp
        public static byte[] ErrorImage
        {
            get
            {
                if (_errorImage == null)
                {
                    _errorImage = Get("error.bmp");
                }

                return _errorImage;
            }
        }

        public static byte[] Wallpaper
        {
            get
            {
                if (_wallpaper == null)
                {
                    _wallpaper = Get("wallpaper-1.bmp");
                }

                return _wallpaper;
            }
        }

        public static byte[] Wallpaper2
        {
            get
            {
                if (_wallpaper2 == null)
                {
                    _wallpaper2 = Get("wallpaper-2.bmp");
                }

                return _wallpaper2;
            }
        }

        public static byte[] auralogo_white
        {
            get
            {
                if (_auralogoWhite == null)
                {
                    _auralogoWhite = Get("auralogo_white.bmp");
                }

                return _auralogoWhite;
            }
        }

        /// <summary>
        /// Embedded asset by its path relative to Resources/ ("UI/Themes/Suave.skin.xml", "error.bmp";
        /// '\' is accepted). Returns a fresh copy on every call: read each asset once and cache it.
        /// </summary>
        /// <exception cref="FileNotFoundException">No such embedded resource.</exception>
        public static byte[] Get(string relPath)
        {
            if (relPath == null)
            {
                throw new ArgumentNullException(nameof(relPath));
            }

            return ReadResource(relPath);
        }

        /// <summary>
        /// Embedded asset, or false (data = null) when it does not exist or cannot be read.
        /// </summary>
        public static bool TryGet(string relPath, out byte[] data)
        {
            data = null;

            if (string.IsNullOrEmpty(relPath))
            {
                return false;
            }

            try
            {
                data = ReadStream(ResourceName(relPath));
            }
            catch (Exception)
            {
                data = null;
            }

            return data != null;
        }

        /// <summary>
        /// The embedded assets under a directory ("Packages/"), by their path relative to Resources/
        /// ("Packages/SystemInfo.pkg"), subdirectories included, in ordinal order.
        /// </summary>
        public static List<string> List(string dirRelPath)
        {
            string prefix = ResourceName(dirRelPath);
            List<string> paths = new List<string>();

            foreach (string name in typeof(Aura_OS.Kernel).Assembly.GetManifestResourceNames())
            {
                if (name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    paths.Add(name.Substring(ResourcePrefix.Length));
                }
            }

            paths.Sort(StringComparer.Ordinal);
            return paths;
        }

        /// <summary>
        /// Embedded text asset, UTF-8 with the BOM stripped (gen2's File.ReadAllText did that;
        /// Suave.skin.xml has one and NanoXML rejects it).
        /// </summary>
        /// <exception cref="FileNotFoundException">No such embedded resource.</exception>
        public static string GetText(string relPath)
        {
            byte[] bytes = Get(relPath);
            int skip = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
            return Encoding.UTF8.GetString(bytes, skip, bytes.Length - skip);
        }

        /// <summary>
        /// Embedded BMP by its path relative to Resources/ ("UI/Images/AuraLogoWhite.bmp"), decoded on first
        /// use and shared afterwards (draw it, do not draw into it).
        /// </summary>
        /// <exception cref="FileNotFoundException">No such embedded resource.</exception>
        public static Bitmap GetImage(string relPath)
        {
            Bitmap bitmap;

            if (!_images.TryGetValue(relPath, out bitmap))
            {
                bitmap = new Bitmap(Get(relPath));
                _images.Add(relPath, bitmap);
            }

            return bitmap;
        }

        /// <summary>
        /// True for an "embedded:..." sentinel path.
        /// </summary>
        public static bool IsEmbeddedPath(string path)
        {
            return path != null && path.StartsWith(EmbeddedScheme, StringComparison.Ordinal);
        }

        /// <summary>
        /// "embedded:X" -> Get(X); any other path -> File.ReadAllBytes(path).
        /// </summary>
        public static byte[] ReadAllBytes(string pathOrEmbedded)
        {
            if (IsEmbeddedPath(pathOrEmbedded))
            {
                return Get(pathOrEmbedded.Substring(EmbeddedScheme.Length));
            }

            return File.ReadAllBytes(pathOrEmbedded);
        }

        /// <summary>
        /// "embedded:X" -> GetText(X); any other path -> File.ReadAllText(path).
        /// </summary>
        public static string ReadAllText(string pathOrEmbedded)
        {
            if (IsEmbeddedPath(pathOrEmbedded))
            {
                return GetText(pathOrEmbedded.Substring(EmbeddedScheme.Length));
            }

            return File.ReadAllText(pathOrEmbedded);
        }

        private static string ResourceName(string relPath)
        {
            return ResourcePrefix + relPath.Replace('\\', '/');
        }

        private static byte[] ReadResource(string relPath)
        {
            string name = ResourceName(relPath);
            byte[] data = ReadStream(name);

            if (data == null)
            {
                throw new FileNotFoundException("Embedded resource not found: " + name, name);
            }

            return data;
        }

        /// <summary>
        /// Whole manifest resource, or null when it does not exist.
        /// </summary>
        private static byte[] ReadStream(string name)
        {
            // GEN3-GAP(resources): GetManifestResourceStream works but is untested and undocumented upstream;
            // the internal ResourceManager is what PCScreenFont uses. Only Files.cs reads resources.
            Stream stream = typeof(Aura_OS.Kernel).Assembly.GetManifestResourceStream(name);

            if (stream == null)
            {
                return null;
            }

            byte[] data = null;
            Exception error = null;

            try
            {
                data = new byte[stream.Length];

                // Read may return short counts.
                int offset = 0;
                while (offset < data.Length)
                {
                    int read = stream.Read(data, offset, data.Length - offset);
                    if (read <= 0)
                    {
                        break;
                    }

                    offset += read;
                }

                if (offset != data.Length)
                {
                    error = new EndOfStreamException("Embedded resource truncated: " + name);
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }

            // gen3: finally/using do not run on the exception path, dispose explicitly.
            stream.Dispose();

            if (error != null)
            {
                throw new IOException("Cannot read embedded resource " + name + ": " + error.Message, error);
            }

            return data;
        }

        public static void LoadFiles()
        {
            // Files
            Kernel.errorLogo = new Bitmap(ErrorImage);
            CustomConsole.WriteLineOK("error.bmp image loaded.");

            // Wallpapers
            Kernel.wallpaper1 = new Bitmap(Wallpaper);
            CustomConsole.WriteLineOK("wallpaper-1.bmp wallpaper loaded.");

            Kernel.wallpaper2 = new Bitmap(Wallpaper2);
            CustomConsole.WriteLineOK("wallpaper-2.bmp wallpaper loaded.");

            // Logo
            Kernel.auralogo_white = new Bitmap(auralogo_white);
            CustomConsole.WriteLineOK("auralogo_white.bmp wallpaper loaded.");

            // Images
            Kernel.AuraLogoWhite = GetImage("UI/Images/AuraLogoWhite.bmp");
            CustomConsole.WriteLineOK("AuraLogoWhite.bmp image loaded.");

            Kernel.AuraLogo2 = GetImage("UI/Images/aura.bmp");
            CustomConsole.WriteLineOK("aura.bmp image loaded.");

            // Fonts
            Kernel.font = PCScreenFont.LoadFont(Get("UI/Fonts/zap-ext-light16.psf"));
            CustomConsole.WriteLineOK("zap-ext-light16.psf font loaded.");
        }

        public static void LoadImages()
        {
            // The kernel's own icons: the desktop's, the taskbar's, the start menu's, the dialogs' and the
            // cursors. An app's are images of its package (package.xml).
            LoadImage("UI/Images/Icons/16/close.bmp", "16");
            LoadImage("UI/Images/Icons/16/minimize.bmp", "16");
            LoadImage("UI/Images/Icons/16/network-idle.bmp", "16");
            LoadImage("UI/Images/Icons/16/network-offline.bmp", "16");
            //LoadImage("UI/Images/Icons/16/network-transmit.bmp", "16");
            LoadImage("UI/Images/Icons/16/program.bmp", "16");
            LoadImage("UI/Images/Icons/16/drive.bmp", "16");
            LoadImage("UI/Images/Icons/16/drive-readonly.bmp", "16");
            LoadImage("UI/Images/Icons/16/paste.bmp", "16");
            LoadImage("UI/Images/Icons/24/program.bmp", "24");
            LoadImage("UI/Images/Icons/24/reboot.bmp", "24");
            LoadImage("UI/Images/Icons/24/logout.bmp", "24");
            LoadImage("UI/Images/Icons/24/shutdown.bmp", "24");
            LoadImage("UI/Images/Icons/32/file.bmp", "32");
            LoadImage("UI/Images/Icons/32/folder.bmp", "32");
            LoadImage("UI/Images/Icons/32/dialog-information.bmp", "32");
            LoadImage("UI/Images/Icons/32/dialog-error.bmp", "32");
            LoadImage("UI/Images/Icons/cursor.bmp", "00");
            LoadImage("UI/Images/Icons/grab.bmp", "00");
            LoadImage("UI/Images/Icons/resize-horizontal.bmp", "00");
            LoadImage("UI/Images/Icons/resize-vertical.bmp", "00");
            LoadImage("UI/Images/Icons/start.bmp", "00");
        }

        /// <summary>
        /// Loads an embedded icon ("UI/Images/Icons/16/up.bmp") into the ResourceManager as type + "-" + file name.
        /// </summary>
        public static void LoadImage(string relPath, string type)
        {
            string fileName = Path.GetFileName(relPath.Replace('\\', '/'));
            Bitmap bitmap = new(Get(relPath));
            Kernel.ResourceManager.AddIcon(type + "-" + fileName, bitmap);
            CustomConsole.WriteLineOK(fileName + " icon loaded.");
        }
    }
}
