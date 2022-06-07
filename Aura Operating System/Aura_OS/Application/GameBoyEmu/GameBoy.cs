using Cosmos.HAL;
using Cosmos.System;
using ProjectDMG;
using ProjectDMG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura_OS.Application.GameBoyEmu
{
    /*
    public class GameBoyEmu : App
    {
        public byte[] Rom;

        private CPU cpu;
        private MMU mmu;
        private PPU ppu;
        private TIMER timer;
        public JOYPAD joypad;

        int cyclesThisUpdate = 0;
        int cpuCycles = 0;

        public GameBoyEmu(uint width, uint height, uint x = 0, uint y = 0) : base("GameBoyEmu", width, height, x, y)
        {
            Rom = Files.TetrisRom;

            mmu = new MMU();
            cpu = new CPU(mmu);
            ppu = new PPU();
            timer = new TIMER();
            joypad = new JOYPAD();

            mmu.loadGamePak(Rom);
        }

        private KeyEvent keyEvent = null;

        public override void UpdateApp()
        {
            if (KeyboardManager.TryReadKey(out keyEvent))
            {
                joypad.handleKeyDown(keyEvent.Key);
            }

            while (cyclesThisUpdate < Constants.CYCLES_PER_UPDATE)
            {
                cpuCycles = cpu.Exe();
                cyclesThisUpdate += cpuCycles;

                timer.update(cpuCycles, mmu);
                ppu.update(cpuCycles, mmu);
                joypad.update(mmu);
                handleInterrupts();
            }
            cyclesThisUpdate -= Constants.CYCLES_PER_UPDATE;

            if (keyEvent != null)
            {
                joypad.handleKeyUp(keyEvent.Key);

                keyEvent = null;
            }
        }

        private void handleInterrupts()
        {
            byte IE = mmu.IE;
            byte IF = mmu.IF;
            for (int i = 0; i < 5; i++)
            {
                if ((((IE & IF) >> i) & 0x1) == 1)
                {
                    cpu.ExecuteInterrupt(i);
                }
            }

            cpu.UpdateIME();
        }
    }*/
}
