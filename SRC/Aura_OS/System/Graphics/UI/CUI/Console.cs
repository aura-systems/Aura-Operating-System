/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Textmode console (gen3: cell-grid console on the graphical canvas)
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   https://github.com/CosmosOS/Cosmos/blob/master/source/Cosmos.System2/Console.cs
*/

using System;
using System.Runtime.CompilerServices;
using Aura_OS.System.Graphics.UI;
using Cosmos.Kernel.System.Graphics;

namespace Aura_OS.System.Graphics.UI.CUI
{
    // GEN3-GAP(textmode): gen3 is UEFI/GOP only, there is no VGA text mode (TextScreenBase is
    // gone). This console is re-targeted at Cosmos.Kernel.System.Graphics.KernelConsole.Default,
    // the gen3 cell-grid console drawn on the framebuffer canvas. If the kernel console is not
    // available every operation degrades to a no-op.
    public class Console : UI.Console
    {
        protected int mX = 0;
        public override int X
        {
            get { return mX; }
            set
            {
                mX = value;
                UpdateCursor();
            }
        }

        protected int mY = 0;
        public override int Y
        {
            get { return mY; }
            set
            {
                mY = value;
                UpdateCursor();
            }
        }

        public override int Cols
        {
            get { return mText == null ? 80 : mText.Cols; }
        }

        public override int Rows
        {
            get { return mText == null ? 25 : mText.Rows; }
        }

        protected KernelConsole mText;

        private ConsoleColor mForeground = ConsoleColor.White;
        private ConsoleColor mBackground = ConsoleColor.Black;
        private int mCursorSize = 25;

        public Console()
        {
            Name = "Textmode";
            Type = ConsoleType.Text;

            mText = KernelConsole.Default;
        }

        public override void Clear()
        {
            if (mText != null)
            {
                mText.Clear();
            }
            mX = 0;
            mY = 0;
            UpdateCursor();
        }

        public override void Clear(uint color)
        {
            Clear();
        }

        //TODO: This is slow, batch it and only do it at end of updates
        public override void UpdateCursor()
        {
            if (mText != null)
            {
                mText.SetCursorPosition(mX, mY);
            }
        }

        private void DoLineFeed()
        {
            mY++;
            mX = 0;
            if (mY == Rows)
            {
                ScrollUp();
                mY--;
            }
            UpdateCursor();
        }

        /// <summary>
        /// Scrolls the cell grid up by one row (KernelConsole has no public scroll API,
        /// so rows are moved cell by cell).
        /// </summary>
        private void ScrollUp()
        {
            if (mText == null)
            {
                return;
            }

            for (int row = 1; row < mText.Rows; row++)
            {
                for (int col = 0; col < mText.Cols; col++)
                {
                    mText.SetCellAt(col, row - 1, mText.GetCellAt(col, row));
                }
            }

            Cell empty = Cell.Empty(KernelConsole.ConsoleColorToUint(mForeground), KernelConsole.ConsoleColorToUint(mBackground));
            for (int col = 0; col < mText.Cols; col++)
            {
                mText.SetCellAt(col, mText.Rows - 1, empty);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DoCarriageReturn()
        {
            mX = 0;
            UpdateCursor();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DoTab()
        {
            Write((byte)Space);
            Write((byte)Space);
            Write((byte)Space);
            Write((byte)Space);
        }

        public void Write(byte aChar)
        {
            if (mText != null)
            {
                mText.SetCellAt(mX, mY, new Cell((char)aChar,
                    KernelConsole.ConsoleColorToUint(mForeground),
                    KernelConsole.ConsoleColorToUint(mBackground)));
            }

            mX++;
            if (mX == Cols)
            {
                DoLineFeed();
            }
            UpdateCursor();
        }

        public override void Write(char[] aText)
        {
            //throw new NotImplementedException();
        }

        //TODO: Optimize this
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(byte[] aText)
        {
            if (aText == null)
            {
                return;
            }

            for (int i = 0; i < aText.Length; i++)
            {
                switch (aText[i])
                {
                    case (byte)LineFeed:
                        DoLineFeed();
                        break;

                    case (byte)CarriageReturn:
                        DoCarriageReturn();
                        break;

                    case (byte)Tab:
                        DoTab();
                        break;

                    /* Normal characters, simply write them */
                    default:
                        Write(aText[i]);
                        break;
                }
            }
        }

        public override void DrawImage(ushort X, ushort Y, Bitmap image)
        {
            // Do nothing
        }

        public override ConsoleColor Foreground
        {
            get { return mForeground; }
            set
            {
                mForeground = value;
                if (mText != null)
                {
                    mText.SetForegroundColor(value);
                }
            }
        }
        public override ConsoleColor Background
        {
            get { return mBackground; }
            set
            {
                mBackground = value;
                if (mText != null)
                {
                    mText.SetBackgroundColor(value);
                }
            }
        }

        public override int CursorSize
        {
            // GEN3-GAP(textmode): KernelConsole has no cursor-size concept (block cursor only);
            // the value is kept so callers still round-trip it.
            get { return mCursorSize; }
            set
            {
                // Value should be a percentage from [1, 100].
                if (value < 1 || value > 100)
                    throw new ArgumentOutOfRangeException("value", value, "CursorSize value " + value + " out of range (1 - 100)");

                mCursorSize = value;
            }
        }

        public override bool CursorVisible
        {
            get { return mText != null && mText.CursorVisible; }
            set
            {
                if (mText != null)
                {
                    mText.CursorVisible = value;
                }
            }
        }

        public override int Width
        {
            get { return Cols; }
        }

        public override int Height
        {
            get { return Rows; }
        }

    }
}
