/*
* PROJECT:          Aura Operating System Development
* CONTENT:          CIF Decoder
* PROGRAMMERS:      Myvar
*                   https://github.com/Myvar/CosmosGuiFramework/blob/master/CosmosGuiFramework/CGF.System/Imaging/Decoders/CIFDecoder.cs
*/

using Aura_OS.System.Utils;

namespace Aura_OS.System.Graphics.Decoders
{
    public class CIFDecoder : IImageDecoder
    {
        public int[] Map { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public string MagicNumber { get; set; }

        public CIFDecoder()
        {
            MagicNumber = "CIF"; //magicnumber
        }

        public void Load(byte[] raw)
        {
            BinaryReader str = new BinaryReader(raw);

            Width = str.GetInt32();
            Height = str.GetInt32();
            Map = new int[Width * Height];

            for (int i = 0; i < Width * Height; i++)
            {
                Map[i] = str.GetInt32();//read hex value of pixle
            }
        }

        public string ReadMagicNumber(byte[] raw)
        {
            // RWStream str = new RWStream(raw);
            return "CIF";//str.ReadString();//magicnumber
        }
    }
}
