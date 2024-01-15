
using System;
using SFML.System;

namespace SFML.Graphics
{
    public class Sprite
    {
        public Texture Texture { get; internal set; }
        public Vector2f Position { get; internal set; }
        public int Rotation { get; internal set; }
        public Vector2f Scale { get; internal set; }

        public Sprite(Texture sfmlTexture)
        {
            this.Texture = sfmlTexture;
        }

        internal void Dispose()
        {
            // TODO:: implement
        }

        public override string ToString() => $"{Position}, {Rotation}, {Scale}";



    }

}