using Aura_OS.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Shell.VBE
{
    public unsafe class Surface
    {
        public List<Layer> Layers { get; set; } = new List<Layer>();
        public int SelectedLayer { get; set; } = -1;

        private Layer _tmpLayer;

        public Layer ActiveLayer
        {
            get => Layers[SelectedLayer];
            set => Layers[SelectedLayer] = value;
        }

        public int Width { get; set; }
        public int Height { get; set; }

        public Surface(int width, int height)
        {
            Width = width;
            Height = height;

            PushLayer();
            _tmpLayer = new Layer(this);
        }

        public void PushLayer()
        {
            Layers.Add(new Layer(this));
            SelectedLayer++;
        }

        public void Clear(uint color)
        {
            Memory.Memset(ActiveLayer.Pixels, color, (uint)(Width * Height));
        }

        public void DrawLine(int x, int y, int x2, int y2, uint c)
        {
            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1;
            else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1;
            else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1;
            else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1;
                else if (h > 0) dy2 = 1;
                dx2 = 0;
            }

            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                FillRectangle(x, y, 1, 1, c);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }

        public void FillRectangle(int x, int y, int w, int h, uint color)
        {
            for (int i = 0; i < h; i++)
            {
                Memory.Memset((uint*)((uint)ActiveLayer.Pixels + (x * 4) + ((i + y) * Width) * 4), color, (uint)w);
            }
        }

        public void FlushToDisplay()
        {
            Memory.Memset(_tmpLayer.Pixels, 0, (uint)(Width * Height));

            for (var index = 0; index < Layers.Count; index++)
            {
                Blend.BlendSurfaceInplace(_tmpLayer.Pixels, Layers[index].Pixels, Width, Height);
            }

            Memory.Memcpy((uint*)0xE0000000, _tmpLayer.Pixels, Width * Height);
        }
    }
}
