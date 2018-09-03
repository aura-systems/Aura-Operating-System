using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.GUI.UI.Fonts
{
    public class Font
    {
        private Framework.Graphics.RWStream RawFile = new Framework.Graphics.RWStream();

        // header:
        //magic number: CFF - Custom Font File
        // Font name : string
        // font style : byte
        // font size : short
        // charcounts: int

        //body
        //charindex []
        // charcode asci
        // char Width
        // char Height
        //short data lenth
        // char data.

        public string Name { get; set; }
        public byte Style { get; set; }//TODO add suport for this in FOnconverter and move this to enum
        public short Size { get; set; }
        public int CharsCount { get; set; }

        public List<byte[]> Data { get; set; } = new List<byte[]>();
        public List<char> Char { get; set; } = new List<char>();
        public List<byte> Width { get; set; } = new List<byte>();
        public List<byte> Height { get; set; } = new List<byte>();

        public Font(string File)
        {

        }

        public Font(byte[] File)
        {
            RawFile = new Framework.Graphics.RWStream(File);
            DeserlizeFile();
        }

        private void DeserlizeFile()
        {
            RawFile._index = -1;
            RawFile.ReadString();//magic number            
            Name = RawFile.ReadString();
            Style = RawFile.ReadByte();
            Size = (short)RawFile.ReadInt32();
            CharsCount = RawFile.ReadInt32();

            for (int i = 0; i < CharsCount; i++)
            {
                Char.Add((char)RawFile.ReadByte());
                Width.Add(RawFile.ReadByte());
                Height.Add(RawFile.ReadByte());
                var l = RawFile.ReadInt32();
                var data = new List<byte>();
                for (int dat = 0; dat < l; dat++)
                {
                    data.Add(RawFile.ReadByte());
                }
                Data.Add(data.ToArray());
            }
        }
    }
}
