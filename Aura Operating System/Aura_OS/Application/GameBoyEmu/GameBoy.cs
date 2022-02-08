using ProjectDMG;
using ProjectDMG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura_OS.Application.GameBoyEmu
{
    public class GameBoyEmu : App
    {
        public byte[] Rom;

        public GameBoyLogs Logs;

        private CPU cpu;
        private MMU mmu;
        private PPU ppu;
        private TIMER timer;
        //public JOYPAD joypad;

        int cyclesThisUpdate = 0;
        int cpuCycles = 0;

        public GameBoyEmu(uint width, uint height, uint x = 0, uint y = 0) : base("GameBoyEmu", width, height, x, y)
        {
            Rom = Convert.FromBase64String(Files.b64TetrisRom);

            mmu = new MMU();
            cpu = new CPU(mmu);
            ppu = new PPU();
            timer = new TIMER();
            //joypad = new JOYPAD();

            Logs = new GameBoyLogs();

            Logs.WriteLine("Emu started.");

            mmu.loadGamePak(Rom);

            Logs.WriteLine("Rom loaded.");
        }

        public override void UpdateApp()
        {
            while (cyclesThisUpdate < Constants.CYCLES_PER_UPDATE)
            {
                cpuCycles = cpu.Exe();
                cyclesThisUpdate += cpuCycles;

                timer.update(cpuCycles, mmu);
                ppu.update(cpuCycles, mmu);
                //joypad.update(mmu);
                handleInterrupts();
            }
            cyclesThisUpdate -= Constants.CYCLES_PER_UPDATE;

            PrintLogs();
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

        void PrintLogs()
        {
            uint _y = y;
            uint _x = x;

            for (int i = 0; i < Logs.Logs.Length; i++)
            {
                if (Logs.Logs[i] == '\n')
                {
                    if (_y > y + height)
                    {
                        Logs.Logs = "";
                    }

                    _y += Kernel.font.Height;
                    _x = x;
                }

                Kernel.canvas.DrawChar(Logs.Logs[i], Kernel.font, Kernel.BlackPen, (int)(_x + ppu.bmp.Bitmap.Width + 2), (int)_y);

                _x += Kernel.font.Width;
            }
        }
    }

    public class GameBoyLogs
    {
        public string Logs = "";

        public void WriteLine(string text)
        {
            Logs += text + Environment.NewLine;
        }
    }
}
