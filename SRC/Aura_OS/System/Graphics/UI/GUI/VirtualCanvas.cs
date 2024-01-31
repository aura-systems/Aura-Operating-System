using System.Collections.Generic;
using System.Drawing;
using System;
using Cosmos.Core;

namespace Cosmos.System.Graphics
{
    public class VirtualCanvas : Canvas
    {
        public int X;
        public int Y;

        private Mode _mode;
        private ManagedMemoryBlock _lastbuffer;

        public VirtualCanvas(Mode mode) : base(mode)
        {
            _lastbuffer = new ManagedMemoryBlock(mode.Width * Mode.Height * 4);
            Mode = mode;
        }

        public override void Disable()
        {
            
        }

        public override string Name() => "VirtualCanvas";

        public override Mode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
            }
        }

        #region Display
        public override List<Mode> AvailableModes { get; } = new()
        {
        };

        public override Mode DefaultGraphicsMode => new(50, 50, ColorDepth.ColorDepth32);

        #endregion

        #region Drawing

        public override void Clear(int aColor)
        {
            _lastbuffer.Fill((uint)aColor);
        }

        public override void Clear(Color aColor)
        {
            _lastbuffer.Fill((uint)aColor.ToArgb());
        }

        public override void DrawPoint(Color aColor, int aX, int aY)
        {
            uint offset = (uint)GetPointOffset(aX, aY);

            if (aColor.A < 255)
            {
                if (aColor.A == 0)
                {
                    return;
                }

                aColor = AlphaBlend(aColor, GetPointColor(aX, aY), aColor.A);
            }

            _lastbuffer[offset] = aColor.B;
            _lastbuffer[offset + 1] = aColor.G;
            _lastbuffer[offset + 2] = aColor.R;
            _lastbuffer[offset + 3] = aColor.A;
        }

        public override void DrawArray(Color[] aColors, int aX, int aY, int aWidth, int aHeight)
        {
            ThrowIfCoordNotValid(aX, aY);
            ThrowIfCoordNotValid(aX + aWidth, aY + aHeight);

            for (int i = 0; i < aX; i++)
            {
                for (int ii = 0; ii < aY; ii++)
                {
                    DrawPoint(aColors[i + (ii * aWidth)], i, ii);
                }
            }
        }

        public override void DrawFilledRectangle(Color aColor, int aX, int aY, int aWidth, int aHeight)
        {
            aWidth = (int)(Math.Min(aWidth, Mode.Width - aX) * (int)Mode.ColorDepth / 32);
            var color = aColor.ToArgb();

            for (int i = aY; i < aY + aHeight; i++)
            {
                _lastbuffer.Fill(GetPointOffset(aX, i), aWidth, color);
            }
        }

        public override void DrawImage(Image aImage, int aX, int aY)
        {
            var xBitmap = aImage.RawData;
            var xWidth = (int)aImage.Width;
            var xHeight = (int)aImage.Height;

            int xOffset = GetPointOffset(aX, aY);

            for (int i = 0; i < xHeight; i++)
            {
                _lastbuffer.Copy((i * (int)Mode.Width) + xOffset, xBitmap, i * xWidth, xWidth);
            }
        }

        #endregion

        public override void Display()
        {
            for (int xi = 0; xi < Mode.Width; xi++)
            {
                for (int yi = 0; yi < Mode.Height; yi++)
                {
                    Color color = Color.FromArgb(_lastbuffer.memory[xi + (yi * Mode.Width)]);
                    Aura_OS.Kernel.canvas.DrawPoint(color, X + xi, Y + yi);
                }
            }
        }

        #region Reading

        public override Color GetPointColor(int aX, int aY)
        {
            uint offset = (uint)GetPointOffset(aX, aY);
            int pixel = (_lastbuffer[offset + 3] << 24) | (_lastbuffer[offset + 2] << 16) | (_lastbuffer[offset + 1] << 8) | _lastbuffer[offset];
            return Color.FromArgb(pixel);
        }

        #endregion

    }
}