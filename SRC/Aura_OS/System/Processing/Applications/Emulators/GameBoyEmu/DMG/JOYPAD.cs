using Cosmos.System;
using static Aura_OS.System.Processing.Applications.Emulators.GameBoyEmu.Utils.BitOps;

namespace Aura_OS.System.Processing.Applications.Emulators.GameBoyEmu.DMG
{
    public class JOYPAD
    {

        private const int JOYPAD_INTERRUPT = 4;
        private const byte PAD_MASK = 0x10;
        private const byte BUTTON_MASK = 0x20;
        private byte pad = 0xF;
        private byte buttons = 0xF;

        public void handleKeyDown(ConsoleKeyEx e)
        {
            byte b = GetKeyBit(e);
            if ((b & PAD_MASK) == PAD_MASK)
            {
                pad = (byte)(pad & ~(b & 0xF));
            }
            else if ((b & BUTTON_MASK) == BUTTON_MASK)
            {
                buttons = (byte)(buttons & ~(b & 0xF));
            }
        }

        public void handleKeyUp(ConsoleKeyEx e)
        {
            byte b = GetKeyBit(e);
            if ((b & PAD_MASK) == PAD_MASK)
            {
                pad = (byte)(pad | b & 0xF);
            }
            else if ((b & BUTTON_MASK) == BUTTON_MASK)
            {
                buttons = (byte)(buttons | b & 0xF);
            }
        }

        public void update(MMU mmu)
        {
            byte JOYP = mmu.JOYP;
            if (!isBit(4, JOYP))
            {
                mmu.JOYP = (byte)(JOYP & 0xF0 | pad);
                if (pad != 0xF) mmu.requestInterrupt(JOYPAD_INTERRUPT);
            }
            if (!isBit(5, JOYP))
            {
                mmu.JOYP = (byte)(JOYP & 0xF0 | buttons);
                if (buttons != 0xF) mmu.requestInterrupt(JOYPAD_INTERRUPT);
            }
            if ((JOYP & 0b00110000) == 0b00110000) mmu.JOYP = 0xFF;
        }

        private byte GetKeyBit(ConsoleKeyEx e)
        {
            switch (e)
            {
                case ConsoleKeyEx.D:
                case ConsoleKeyEx.RightArrow:
                    return 0x11;

                case ConsoleKeyEx.A:
                case ConsoleKeyEx.LeftArrow:
                    return 0x12;

                case ConsoleKeyEx.W:
                case ConsoleKeyEx.UpArrow:
                    return 0x14;

                case ConsoleKeyEx.S:
                case ConsoleKeyEx.DownArrow:
                    return 0x18;

                case ConsoleKeyEx.J:
                case ConsoleKeyEx.Z:
                    return 0x21;

                case ConsoleKeyEx.K:
                case ConsoleKeyEx.X:
                    return 0x22;

                case ConsoleKeyEx.Spacebar:
                case ConsoleKeyEx.C:
                    return 0x24;

                case ConsoleKeyEx.Enter:
                case ConsoleKeyEx.V:
                    return 0x28;
            }
            return 0;
        }
    }
}
