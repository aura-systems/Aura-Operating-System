/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Gameboy emulation (ProjectDMG) app
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Processing.Applications.Emulators.GameBoyEmu.DMG;
using Aura_OS.System.Processing.Applications.Emulators.GameBoyEmu.Utils;
using Cosmos.System;

namespace Aura_OS.System.Processing.Applications.Emulators.GameBoyEmu
{
    public class GameBoyApp : Application
    {
        public byte[] Rom;

        private CPU _cpu;
        private MMU _mmu;
        private PPU _ppu;
        private TIMER _timer;
        private JOYPAD _joypad;

        private int _cyclesThisUpdate = 0;
        private int _cpuCycles = 0;

        public GameBoyApp(byte[] rom, string name, int width, int height, int x = 0, int y = 0) : base(name, width, height, x, y)
        {
            Rom = rom;

            _mmu = new MMU();
            _cpu = new CPU(_mmu);
            _ppu = new PPU(x, y);
            _timer = new TIMER();
            _joypad = new JOYPAD();

            _mmu.loadGamePak(Rom);
        }

        public GameBoyApp(int width, int height, int x = 0, int y = 0) : base("GameBoyEmu", width, height, x, y)
        {
            Rom = Files.TetrisRom;

            _mmu = new MMU();
            _cpu = new CPU(_mmu);
            _ppu = new PPU(x, y);
            _timer = new TIMER();
            _joypad = new JOYPAD();

            _mmu.loadGamePak(Rom);
        }

        private KeyEvent keyEvent = null;

        public override void Update()
        {
            base.Update();

            _ppu.X = x;
            _ppu.Y = y;

            if (Focused)
            {
                if (KeyboardManager.TryReadKey(out keyEvent))
                {
                    _joypad.handleKeyDown(keyEvent.Key);
                }
            }

            while (_cyclesThisUpdate < Constants.CYCLES_PER_UPDATE)
            {
                _cpuCycles = _cpu.Exe();
                _cyclesThisUpdate += _cpuCycles;

                _timer.update(_cpuCycles, _mmu);
                _ppu.update(_cpuCycles, _mmu);
                _joypad.update(_mmu);
                handleInterrupts();
            }
            _cyclesThisUpdate -= Constants.CYCLES_PER_UPDATE;

            if (keyEvent != null)
            {
                _joypad.handleKeyUp(keyEvent.Key);

                keyEvent = null;
            }
        }

        private void handleInterrupts()
        {
            byte IE = _mmu.IE;
            byte IF = _mmu.IF;
            for (int i = 0; i < 5; i++)
            {
                if (((IE & IF) >> i & 0x1) == 1)
                {
                    _cpu.ExecuteInterrupt(i);
                }
            }

            _cpu.UpdateIME();
        }
    }
}
