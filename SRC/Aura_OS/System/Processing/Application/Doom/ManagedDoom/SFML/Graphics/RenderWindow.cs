using System;
using System.Threading.Tasks;
using SFML.System;
using SFML.Window;

namespace SFML.Graphics
{

    public class RenderWindow : SFML.Window.Window
    {
        public bool IsOpen => true;

        public event EventHandler<KeyEventArgs> KeyPressed = null;
        public event EventHandler<KeyEventArgs> KeyReleased = null;

        public RenderWindow(VideoMode videoMode, string title, Styles style)
        {
            VideoMode = videoMode;
        }

        public virtual Vector2u Size
        {
            get { return new Vector2u(VideoMode.Width, VideoMode.Height); }
        }

        public VideoMode VideoMode { get; }

        public void Clear(Color color)
        {

        }

        public void Display()
        {

        }

        internal void SetFramerateLimit(int v)
        {
            // TODO: implement
        }

        internal void DispatchEvents(uint[] keys)
        {

        }

        internal void Dispose()
        {
            // TODO: implement
        }

        internal void Draw(Sprite sfmlSprite, RenderStates sfmlStates)
        {
            //await VideoMode.Draw(sfmlSprite);
            /*for (uint i = 0; i < sfmlSprite.Texture.Width; i++)
            {
                for (uint j = 0; j < sfmlSprite.Texture.Height; j++)
                {
                    float x = sfmlSprite.Position.X + i;
                    float y = sfmlSprite.Position.Y + j;
                    await VideoMode.Draw(sfmlSprite.Texture[i, j], new Vector2f(x, y));
                }
            }*/
        }

        internal void SetMouseCursorGrabbed(bool v)
        {
            // TODO: implement
        }

        internal void SetMouseCursorVisible(bool v)
        {
            // TODO: implement
        }
    }
}