using System;
using System.Numerics;
using System.Threading.Tasks;
using ManagedDoom;
using SFML.System;

namespace SFML.Window
{

    public class VideoMode
    {
        static public VideoMode CanvasMode { get; internal set; }

        static VideoMode()
        {
            // TODO: no const size
            CanvasMode = new VideoMode(320, 200);
        }

        public VideoMode(uint width, uint height)
        {
            this.Width = width;
            this.Height = height;
        }

        public uint Width { get; }
        public uint Height { get; }

        public void Draw(uint pixel, Vector2f position) { }

        public void Draw(Graphics.Sprite sfmlSprite) { }
    }
}