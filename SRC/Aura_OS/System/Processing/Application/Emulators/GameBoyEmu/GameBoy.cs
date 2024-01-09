using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Processing.Application.Emulators.GameBoyEmu.DMG;
using Aura_OS.System.Processing.Application.Emulators.GameBoyEmu.Utils;
using Cosmos.HAL;
using Cosmos.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura_OS.System.Processing.Application.Emulators.GameBoyEmu
{
    public class GameBoyEmu : Graphics.UI.GUI.Application
    {
        public byte[] Rom;

        private CPU cpu;
        private MMU mmu;
        private PPU ppu;
        private TIMER timer;
        public JOYPAD joypad;

        int cyclesThisUpdate = 0;
        int cpuCycles = 0;

        public GameBoyEmu(byte[] rom, string name, int width, int height, int x = 0, int y = 0) : base(name, width, height, x, y)
        {
            Rom = rom;

            mmu = new MMU();
            cpu = new CPU(mmu);
            ppu = new PPU(x, y);
            timer = new TIMER();
            joypad = new JOYPAD();

            mmu.loadGamePak(Rom);
        }

        public GameBoyEmu(int width, int height, int x = 0, int y = 0) : base("GameBoyEmu", width, height, x, y)
        {
            Rom = Files.TetrisRom;

            mmu = new MMU();
            cpu = new CPU(mmu);
            ppu = new PPU(x, y);
            timer = new TIMER();
            joypad = new JOYPAD();

            mmu.loadGamePak(Rom);
        }

        private KeyEvent keyEvent = null;

        public override void UpdateApp()
        {
            if (Focused)
            {
                if (KeyboardManager.TryReadKey(out keyEvent))
                {
                    joypad.handleKeyDown(keyEvent.Key);
                }
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
            ppu.X = x;
            ppu.Y = y;
        }

        private void handleInterrupts()
        {
            byte IE = mmu.IE;
            byte IF = mmu.IF;
            for (int i = 0; i < 5; i++)
            {
                if (((IE & IF) >> i & 0x1) == 1)
                {
                    cpu.ExecuteInterrupt(i);
                }
            }

            cpu.UpdateIME();
        }
    }
}
