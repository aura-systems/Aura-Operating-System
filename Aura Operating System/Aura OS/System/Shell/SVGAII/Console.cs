using Cosmos.HAL.Drivers.PCI.Video;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Shell.SVGAII
{
    class VMWareSVGAConsole : Console
    {

        Graphics graphics;

        public VMWareSVGAConsole()
        {
            Name = "VGA Textmode";
            graphics = new Graphics();
            graphics.Init();
        }

        public override int X { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int Y { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override int Cols => throw new NotImplementedException();

        public override int Rows => throw new NotImplementedException();

        public override ConsoleColor Foreground { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ConsoleColor Background { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int CursorSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool CursorVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Clear()
        {
            graphics.Clear();
        }

        public override void Write(byte[] aText)
        {
            throw new NotImplementedException();
        }
    }
}
