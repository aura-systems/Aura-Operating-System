/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Image decoder interface
* PROGRAMMERS:      Myvar
*                   https://github.com/Myvar/CosmosGuiFramework/blob/master/CosmosGuiFramework/CGF.System/Imaging/IImageDecode.cs
*/

namespace Aura_OS.System.Graphics
{
    public interface IImageDecoder
    {
        int[] Map { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        string MagicNumber { get; set; }

        void Load(byte[] raw);

        string ReadMagicNumber(byte[] raw);
    }
}
