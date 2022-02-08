using static ProjectDMG.Utils.BitOps;

namespace ProjectDMG {
    public class JOYPAD {

        private const int JOYPAD_INTERRUPT = 4;
        private const byte PAD_MASK = 0x10;
        private const byte BUTTON_MASK = 0x20;
        private byte pad = 0xF;
        private byte buttons = 0xF;

        public void update(MMU mmu) {
            byte JOYP = mmu.JOYP;
            if(!isBit(4, JOYP)) {
                mmu.JOYP = (byte)((JOYP & 0xF0) | pad);
                if(pad != 0xF) mmu.requestInterrupt(JOYPAD_INTERRUPT);
            }
            if (!isBit(5, JOYP)) {
                mmu.JOYP = (byte)((JOYP & 0xF0) | buttons);
                if (buttons != 0xF) mmu.requestInterrupt(JOYPAD_INTERRUPT);
            }
            if ((JOYP & 0b00110000) == 0b00110000) mmu.JOYP = 0xFF;
        }
    }
}
