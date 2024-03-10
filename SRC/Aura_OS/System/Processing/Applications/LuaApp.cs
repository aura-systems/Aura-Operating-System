/*
* PROJECT:          Aura Operating System Development
* CONTENT:          CosmosExecutable application.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI;

namespace Aura_OS.System.Processing.Applications
{
    public class LuaApp : Application
    {
        public LuaApp(string name, int width, int height, int x = 0, int y = 0) : base(name, width, height, x, y)
        {

        }

        public override void Update()
        {
            base.Update();

            foreach (var child in Window.Children)
            {
                child.Update();
            }
        }

        public override void Draw()
        {
            base.Draw();

            foreach (var child in Window.Children)
            {
                child.Draw();
                child.DrawInParent();
            }
        }
    }
}
