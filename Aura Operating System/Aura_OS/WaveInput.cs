using Cosmos.System;
using WaveOS.GUI;
using Mouse = Cosmos.System.MouseManager;

namespace WaveOS.Managers
{
    public static class WaveInput
    {
        public static MouseState mState;
        public static MouseState oldMState;

        public static bool MouseHit = false;
        public static void BeforeUpdate()
        {
            mState = Mouse.MouseState;
        }

        public static void AfterUpdate()
        {
            oldMState = mState;
            MouseHit = false;
        }

        public static bool WasLMBPressed()
        {
            if(mState == MouseState.Left && oldMState != MouseState.Left)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool WasLMBUnPressed()
        {
            if (mState != MouseState.Left && oldMState == MouseState.Left)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsMouseWithin(int X, int Y, int Width, int Height)
        {
            return Mouse.X >= X && Mouse.Y >= Y && Mouse.X <= X + Width && Mouse.Y <= Y + Height;
        }

        public static bool IsMouseWithin(WaveElement element)
        {
            return Mouse.X >= element.relativeX && Mouse.Y >= element.relativeY && Mouse.X <= element.relativeX + element.Width && Mouse.Y <= element.relativeY + element.Height;
        }
    }
}
