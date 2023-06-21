using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura_OS
{
    public class Files
    {
        //.bmp
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.AuraLogo.bmp")] 
        public static byte[] AuraImage;

        //.bmp
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.CosmosLogo.bmp")]
        public static byte[] CosmosLogo;

        //.bmp
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.Program.bmp")]
        public static byte[] ProgramImage;

        //.bmp
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.Cursor.bmp")]
        public static byte[] CursorIcon;

        //16x16 .bmp
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.UI.utilities-terminal.bmp")]
        public static byte[] TerminalIcon;

        //16x16 .bmp
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.UI.window.bmp")]
        public static byte[] NoIcon;

        //8x16 .psf (version 1 https://www.zap.org.au/projects/console-fonts-zap/)
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.zap-ext-light16.psf")]
        public static byte[] Font;

        //24x24 .bmp
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.power.bmp")]
        public static byte[] PowerIcon;

        //24x24 .bmp
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.connected.bmp")]
        public static byte[] ConnectedIcon;

        //200x178 .bmp
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.error.bmp")]
        public static byte[] ErrorImage;

        //.gb
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.Tetris.gb")]
        public static byte[] TetrisRom;

        //1920x1024
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.wallpaper1920.bmp")]
        public static byte[] Wallpaper;

        // UI
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.UI.close_normal.bmp")]
        public static byte[] CloseNormal;

        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.UI.start.bmp")]
        public static byte[] Start;
    }
}
