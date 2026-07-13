/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Embedded resource loading (gen3: everything ships inside the kernel image).
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System;
using Cosmos.Kernel.Core.Runtime;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;

namespace Aura_OS
{
    public class Files
    {
        // GEN3-GAP(iso9660): gen2 read UI assets from the boot ISO (ISO9660 volume); gen3 has no
        // ISO9660 driver and no way to add extra files to the ISO, so every asset is embedded
        // in the kernel image and read back through Cosmos.Kernel.Core.Runtime.ResourceManager.

        //200x178 .bmp
        public static byte[] ErrorImage;

        //.gb
        public static byte[] TetrisRom;
        public static byte[] HelloWorldRom;

        public static byte[] Wallpaper;
        public static byte[] Wallpaper2;
        public static byte[] auralogo_white;

        /// <summary>
        /// Reads an embedded UI asset by its path relative to Resources/UI, e.g.
        /// "Images/Icons/16/up.bmp" (see the LogicalName rule in Aura_OS.csproj).
        /// </summary>
        public static byte[] GetUiResource(string relPath)
        {
            return ResourceManager.GetResourceAsSpan("Aura_OS.UI." + relPath).ToArray();
        }

        public static byte[] GetResource(string name)
        {
            return ResourceManager.GetResourceAsSpan("Aura_OS.Resources." + name).ToArray();
        }

        /// <summary>
        /// Reads an embedded UI asset as UTF-8 text, stripping the byte-order mark if present
        /// (gen2 read these files with File.ReadAllText, which strips BOMs; raw GetString does not).
        /// </summary>
        public static string GetUiResourceText(string relPath)
        {
            byte[] bytes = GetUiResource(relPath);
            int offset = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
            return global::System.Text.Encoding.UTF8.GetString(bytes, offset, bytes.Length - offset);
        }

        public static void LoadFiles()
        {
            // GEN3-TODO: ResourceManager re-parses the resource index on every lookup (cache field
            // commented out upstream), so each asset is read exactly once here and cached.
            ErrorImage = GetResource("error.bmp");
            TetrisRom = GetResource("Tetris.gb");
            HelloWorldRom = GetResource("HelloWorld.gb");
            Wallpaper = GetResource("wallpaper-1.bmp");
            Wallpaper2 = GetResource("wallpaper-2.bmp");
            auralogo_white = GetResource("auralogo_white.bmp");

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
            Kernel.AuraLogo = new Bitmap(GetUiResource("Images/AuraLogo.bmp"));
            CustomConsole.WriteLineOK("AuraLogo.bmp image loaded.");

            Kernel.AuraLogoWhite = new Bitmap(GetUiResource("Images/AuraLogoWhite.bmp"));
            CustomConsole.WriteLineOK("AuraLogoWhite.bmp image loaded.");

            Kernel.AuraLogo2 = new Bitmap(GetUiResource("Images/aura.bmp"));
            CustomConsole.WriteLineOK("aura.bmp image loaded.");

            Kernel.CosmosLogo = new Bitmap(GetUiResource("Images/CosmosLogo.bmp"));
            CustomConsole.WriteLineOK("CosmosLogo.bmp image loaded.");

            // Fonts
            Kernel.font = PCScreenFont.LoadFont(GetUiResource("Fonts/zap-ext-light16.psf"));
            CustomConsole.WriteLineOK("zap-ext-light16.psf font loaded.");
        }

        public static void LoadImages()
        {
            // Icons
            LoadImage("Images/Icons/16/up.bmp", "16");
            LoadImage("Images/Icons/16/close.bmp", "16");
            LoadImage("Images/Icons/16/minimize.bmp", "16");
            LoadImage("Images/Icons/16/explorer.bmp", "16");
            LoadImage("Images/Icons/16/network-idle.bmp", "16");
            LoadImage("Images/Icons/16/network-offline.bmp", "16");
            //LoadImage("Images/Icons/16/network-transmit.bmp", "16");
            LoadImage("Images/Icons/16/program.bmp", "16");
            LoadImage("Images/Icons/16/terminal.bmp", "16");
            LoadImage("Images/Icons/16/drive.bmp", "16");
            LoadImage("Images/Icons/16/drive-readonly.bmp", "16");
            LoadImage("Images/Icons/16/settings.bmp", "16");
            LoadImage("Images/Icons/24/program.bmp", "24");
            LoadImage("Images/Icons/24/reboot.bmp", "24");
            LoadImage("Images/Icons/24/logout.bmp", "24");
            LoadImage("Images/Icons/24/settings.bmp", "24");
            LoadImage("Images/Icons/24/shutdown.bmp", "24");
            LoadImage("Images/Icons/24/terminal.bmp", "24");
            LoadImage("Images/Icons/24/explorer.bmp", "24");
            LoadImage("Images/Icons/32/file.bmp", "32");
            LoadImage("Images/Icons/32/folder.bmp", "32");
            LoadImage("Images/Icons/32/dialog-information.bmp", "32");
            LoadImage("Images/Icons/32/dialog-error.bmp", "32");
            LoadImage("Images/Icons/cursor.bmp", "00");
            LoadImage("Images/Icons/grab.bmp", "00");
            LoadImage("Images/Icons/resize-horizontal.bmp", "00");
            LoadImage("Images/Icons/resize-vertical.bmp", "00");
            LoadImage("Images/Icons/start.bmp", "00");
        }

        public static void LoadImage(string relPath, string type)
        {
            int slash = relPath.LastIndexOf('/');
            string fileName = slash >= 0 ? relPath.Substring(slash + 1) : relPath;
            Bitmap bitmap = new(GetUiResource(relPath));
            Kernel.ResourceManager.AddIcon(type + "-" + fileName, bitmap);
            CustomConsole.WriteLineOK(fileName + " icon loaded.");
        }
    }
}
