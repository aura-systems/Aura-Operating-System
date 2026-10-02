/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Gameboy emulation (ProjectDMG) app
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Collections.Generic;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Processing.Applications.Emulators.GameBoyEmu.DMG;
using Aura_OS.System.Processing.Applications.Emulators.GameBoyEmu.Utils;
using Cosmos.Kernel.System.Keyboard;
using CPU = Aura_OS.System.Processing.Applications.Emulators.GameBoyEmu.DMG.CPU;

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
            _ppu = new PPU(this);
            _timer = new TIMER();
            _joypad = new JOYPAD();

            _mmu.loadGamePak(Rom);
        }

        public GameBoyApp(int width, int height, int x = 0, int y = 0) : base("GameBoyEmu", width, height, x, y)
        {
            Rom = Files.TetrisRom;

            _mmu = new MMU();
            _cpu = new CPU(_mmu);
            _ppu = new PPU(this);
            _timer = new TIMER();
            _joypad = new JOYPAD();

            _mmu.loadGamePak(Rom);
        }

        // Keys pressed this frame. Reused to avoid a per-frame allocation.
        private List<ConsoleKeyEx> _pressedKeys = new List<ConsoleKeyEx>();

        public override void Update()
        {
            base.Update();

            if (Focused)
            {
                KeyEvent keyEvent;

                while (Input.KeyboardManager.TryGetKey(out keyEvent))
                {
                    _joypad.handleKeyDown(keyEvent.Key);
                    _pressedKeys.Add(keyEvent.Key);
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

            // GEN3-GAP(key-release): only key presses are queued, so a key is held for one emulation
            // slice and released here. (gen2 meant to do this but released nothing: the last failing
            // TryGetKey had already set its out field to null.)
            for (int i = 0; i < _pressedKeys.Count; i++)
            {
                _joypad.handleKeyUp(_pressedKeys[i]);
            }
            _pressedKeys.Clear();
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
