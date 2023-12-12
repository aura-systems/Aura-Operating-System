using IL2CPU.API.Attribs;

namespace Aura_OS
{
    public class Files
    {
        //200x178 .bmp
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.error.bmp")]
        public static byte[] ErrorImage;

        //.gb
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.Tetris.gb")]
        public static byte[] TetrisRom;

        //1920x1024
        [ManifestResourceStream(ResourceName = "Aura_OS.Resources.wallpaper1920.bmp")]
        public static byte[] Wallpaper;
    }
}
