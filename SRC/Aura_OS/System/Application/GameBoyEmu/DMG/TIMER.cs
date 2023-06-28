using System;

namespace ProjectDMG {
    public class TIMER {

        private const int DMG_DIV_FREQ = 256;              //16384Hz
        private const int CGB_DIV_FREQ = DMG_DIV_FREQ * 2; //32768Hz
        private static readonly int[] TAC_FREQ = { 1024, 16, 64, 256 };
        //00: CPU Clock / 1024 (DMG, CGB:   4096 Hz, SGB:   ~4194 Hz)
        //01: CPU Clock / 16   (DMG, CGB: 262144 Hz, SGB: ~268400 Hz)
        //10: CPU Clock / 64   (DMG, CGB:  65536 Hz, SGB:  ~67110 Hz)
        //11: CPU Clock / 256  (DMG, CGB:  16384 Hz, SGB:  ~16780 Hz)
        private const int TIMER_INTERRUPT = 2; // Bit 2: Timer    Interrupt Request (INT 50h)  (1=Request)

        private int divCounter;
        private int timerCounter;

        public void update(int cycles, MMU mmu) {
            handleDivider(cycles, mmu);
            handleTimer(cycles, mmu);
        }

        private void handleDivider(int cycles, MMU mmu) {
            divCounter += cycles;
            while (divCounter >= DMG_DIV_FREQ) {
                mmu.DIV++;
                divCounter -= DMG_DIV_FREQ;
            }
        }

        private void handleTimer(int cycles, MMU mmu) {
            if (mmu.TAC_ENABLED) {
                timerCounter += cycles;
                while (timerCounter >= TAC_FREQ[mmu.TAC_FREQ]) {
                    mmu.TIMA++;
                    timerCounter -= TAC_FREQ[mmu.TAC_FREQ];
                }
                if (mmu.TIMA == 0xFF) {
                    mmu.requestInterrupt(TIMER_INTERRUPT);
                    mmu.TIMA = mmu.TMA;
                }
            }
        }

    }
}