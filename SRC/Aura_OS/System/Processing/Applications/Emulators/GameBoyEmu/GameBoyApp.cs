/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Gameboy emulation (ProjectDMG) app
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Aura_OS.Processing;
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

        // Emulation runs on the app's Work() thread; the UI thread only handles
        // input (Update) and blits the finished frame (Draw). Frames are
        // double-buffered and handed over by an atomic reference swap (see Work)
        // rather than a lock: lock/Monitor's contended path routes through the
        // kernel LowLevelMonitor that faults under concurrency. The UI always
        // reads a complete _front, never a half-rendered one.
        private volatile DirectBitmap _front = new DirectBitmap();

        public GameBoyApp(byte[] rom, string name, int width, int height, int x = 0, int y = 0) : base(name, width, height, x, y)
        {
            // The emulation worker produces a fresh frame continuously, so the
            // window must be recomposited every frame (Draw() blits it).
            ForceDirty = true;
            RunsWorker = true;
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
            ForceDirty = true;
            RunsWorker = true;
            Rom = Files.TetrisRom;

            _mmu = new MMU();
            _cpu = new CPU(_mmu);
            _ppu = new PPU(this);
            _timer = new TIMER();
            _joypad = new JOYPAD();

            _mmu.loadGamePak(Rom);
        }

        private KeyEvent keyEvent = null;

        /// <summary>
        /// UI thread: only forward keyboard input to the joypad. Joypad state is
        /// written here and read by the emulation worker; a torn button bit is
        /// harmless (a dropped/late keypress), so this stays lock-free rather
        /// than contending with the emulation loop.
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (Focused)
            {
                while (Input.KeyboardManager.TryGetKey(out keyEvent))
                {
                    _joypad.handleKeyDown(keyEvent.Key);
                }

                if (keyEvent != null)
                {
                    _joypad.handleKeyUp(keyEvent.Key);

                    keyEvent = null;
                }
            }
        }

        /// <summary>
        /// Work() thread: run one frame of emulation, then hand the finished
        /// frame to the UI thread by swapping buffers. The swap happens between
        /// emulation slices (the PPU is not mid-write), and publishing _front is
        /// a single atomic reference write, so Draw never sees a torn frame and
        /// the PPU never renders into the buffer the UI is currently blitting.
        /// </summary>
        protected override void Work()
        {
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

            if (_ppu.FrameReady)
            {
                DirectBitmap finished = _ppu.bmp;
                DirectBitmap recycled = _front;
                _front = finished;    // atomic publish: UI now blits the finished frame
                _ppu.bmp = recycled;  // PPU renders the next frame into the old display buffer
                _ppu.FrameReady = false;
            }
        }

        /// <summary>UI thread: blit the last complete frame into the window.</summary>
        public override void Draw()
        {
            base.Draw();

            // Single volatile read of the current front buffer.
            DirectBitmap front = _front;
            DrawImage(front.Bitmap, 0, 0);
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
