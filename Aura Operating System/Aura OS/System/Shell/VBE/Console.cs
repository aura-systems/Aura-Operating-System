/*
* PROJECT:          Aura Operating System Development
* CONTENT:          VBE Console
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Shell.VBE
{
    class VBEConsole : Console
    {
        public override int X { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int Y { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override int Width => throw new NotImplementedException();

        public override int Height => throw new NotImplementedException();

        public override int Cols => throw new NotImplementedException();

        public override int Rows => throw new NotImplementedException();

        public override ConsoleColor Foreground { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ConsoleColor Background { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int CursorSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool CursorVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] aText)
        {
            throw new NotImplementedException();
        }
    }
}
