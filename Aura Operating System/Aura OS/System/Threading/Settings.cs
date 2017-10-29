using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Threading
{
    public delegate void ChangeVideoMode();
    public enum DisplayModes
    {
        Text,
        Graphics
    }

    public static unsafe class Settings
    {
        public static bool HeapUseBasic = false;
        public static bool UseDebugVideoMode = false;

        public static uint* DisplayBuffer
        {
            get
            {
                if (OffScreenBuffer == null)
                    OffScreenBuffer = (uint*)Heap.alloc((uint)(DisplayWidth * DisplayHeight) * 4);
                return OffScreenBuffer;
            }
            set
            {
                OffScreenBuffer = value;
            }
        }
        public static uint* DisplayVRam;
        private static uint* OffScreenBuffer = null;

        public static int DisplayWidth = 1024;
        public static int DisplayHeight = 768;

        public static int CursorX = 100;
        public static int CursorY = 100;

        public static ChangeVideoMode EnterTextMode;
        public static ChangeVideoMode EnterVideoMode;
    }
}
