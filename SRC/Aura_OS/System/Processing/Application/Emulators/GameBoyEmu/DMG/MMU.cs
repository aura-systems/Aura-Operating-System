using Aura_OS.System.Processing.Application.Emulators.GameBoyEmu.DMG.GamePak;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using static Aura_OS.System.Processing.Application.Emulators.GameBoyEmu.Utils.BitOps;

namespace Aura_OS.System.Processing.Application.Emulators.GameBoyEmu.DMG
{
    public class MMU
    {

        //GamePak
        private IGamePak gamePak;

        //DMG Memory Map
        private byte[] VRAM = new byte[0x2000];
        private byte[] WRAM0 = new byte[0x1000];
        private byte[] WRAM1 = new byte[0x1000];
        private byte[] OAM = new byte[0xA0];
        private byte[] IO = new byte[0x80];
        private byte[] HRAM = new byte[0x80];

        //Timer IO Regs
        public byte DIV { get { return IO[0x04]; } set { IO[0x04] = value; } } //FF04 - DIV - Divider Register (R/W)
        public byte TIMA { get { return IO[0x05]; } set { IO[0x05] = value; } } //FF05 - TIMA - Timer counter (R/W)
        public byte TMA { get { return IO[0x06]; } set { IO[0x06] = value; } } //FF06 - TMA - Timer Modulo (R/W)
        public byte TAC { get { return IO[0x07]; } set { IO[0x07] = value; } } //FF07 - TAC - Timer Control (R/W)
        public bool TAC_ENABLED { get { return (IO[0x07] & 0x4) != 0; } } // Check if byte 2 is 1
        public byte TAC_FREQ { get { return (byte)(IO[0x07] & 0x3); } } // returns byte 0 and 1

        //Interrupt IO Flags
        //Bit 0: V-Blank Interrupt Enable(INT 40h)  (1=Enable)
        //Bit 1: LCD STAT Interrupt Enable(INT 48h)  (1=Enable)
        //Bit 2: Timer Interrupt Enable(INT 50h)  (1=Enable)
        //Bit 3: Serial Interrupt Enable(INT 58h)  (1=Enable)
        //Bit 4: Joypad Interrupt Enable(INT 60h)  (1=Enable)
        public byte IE { get { return HRAM[0x7F]; } set { HRAM[0x7F] = value; } }//FFFF - IE - Interrupt Enable (R/W)
        public byte IF { get { return IO[0x0F]; } set { IO[0x0F] = value; } }//FF0F - IF - Interrupt Flag (R/W)

        //PPU IO Regs
        public byte LCDC { get { return IO[0x40]; } }//FF40 - LCDC - LCD Control (R/W)
        public byte STAT { get { return IO[0x41]; } set { IO[0x41] = value; } }//FF41 - STAT - LCDC Status (R/W)

        public byte SCY { get { return IO[0x42]; } }//FF42 - SCY - Scroll Y (R/W)
        public byte SCX { get { return IO[0x43]; } }//FF43 - SCX - Scroll X (R/W)
        public byte LY { get { return IO[0x44]; } set { IO[0x44] = value; } }//FF44 - LY - LCDC Y-Coordinate (R) bypasses on write always 0
        public byte LYC { get { return IO[0x45]; } }//FF45 - LYC - LY Compare(R/W)
        public byte WY { get { return IO[0x4A]; } }//FF4A - WY - Window Y Position (R/W)
        public byte WX { get { return IO[0x4B]; } }//FF4B - WX - Window X Position minus 7 (R/W)

        public byte BGP { get { return IO[0x47]; } }//FF47 - BGP - BG Palette Data(R/W) - Non CGB Mode Only
        public byte OBP0 { get { return IO[0x48]; } }//FF48 - OBP0 - Object Palette 0 Data (R/W) - Non CGB Mode Only
        public byte OBP1 { get { return IO[0x49]; } }//FF49 - OBP1 - Object Palette 1 Data (R/W) - Non CGB Mode Only

        //public byte DMA { get { return readByte(0xFF46); } }//FF46 - DMA - DMA Transfer and Start Address (R/W)

        public byte JOYP { get { return IO[0x00]; } set { IO[0x00] = value; } }//FF00 - JOYP

        public MMU()
        {
            //FF4D - KEY1 - CGB Mode Only - Prepare Speed Switch
            //HardCoded to FF to identify DMG as 00 is GBC
            IO[0x4D] = 0xFF;

            IO[0x10] = 0x80;
            IO[0x11] = 0xBF;
            IO[0x12] = 0xF3;
            IO[0x14] = 0xBF;
            IO[0x16] = 0x3F;
            IO[0x19] = 0xBF;
            IO[0x1A] = 0x7F;
            IO[0x1B] = 0xFF;
            IO[0x1C] = 0x9F;
            IO[0x1E] = 0xBF;
            IO[0x20] = 0xFF;
            IO[0x23] = 0xBF;
            IO[0x24] = 0x77;
            IO[0x25] = 0xF3;
            IO[0x26] = 0xF1;
            IO[0x40] = 0x91;
            IO[0x47] = 0xFC;
            IO[0x48] = 0xFF;
            IO[0x49] = 0xFF;
        }

        public byte readByte(ushort addr)
        {
            switch (addr)
            {                           // General Memory Map 64KB
                case ushort _ when addr <= 0x3FFF:    //0000-3FFF 16KB ROM Bank 00 (in cartridge, private at bank 00)
                    return gamePak.ReadLoROM(addr);
                case ushort _ when addr <= 0x7FFF:    // 4000-7FFF 16KB ROM Bank 01..NN(in cartridge, switchable bank number)
                    return gamePak.ReadHiROM(addr);
                case ushort _ when addr <= 0x9FFF:    // 8000-9FFF 8KB Video RAM(VRAM)(switchable bank 0-1 in CGB Mode)
                    return VRAM[addr & 0x1FFF];
                case ushort _ when addr <= 0xBFFF:    // A000-BFFF 8KB External RAM(in cartridge, switchable bank, if any)
                    return gamePak.ReadERAM(addr);
                case ushort _ when addr <= 0xCFFF:    // C000-CFFF 4KB Work RAM Bank 0(WRAM) <br/>
                    return WRAM0[addr & 0xFFF];
                case ushort _ when addr <= 0xDFFF:    // D000-DFFF 4KB Work RAM Bank 1(WRAM)(switchable bank 1-7 in CGB Mode) <br/>
                    return WRAM1[addr & 0xFFF];
                case ushort _ when addr <= 0xEFFF:    // E000-FDFF Same as 0xC000-DDFF(ECHO)  
                    return WRAM0[addr & 0xFFF];
                case ushort _ when addr <= 0xFDFF:    // E000-FDFF Same as 0xC000-DDFF(ECHO)
                    return WRAM1[addr & 0xFFF];
                case ushort _ when addr <= 0xFE9F:    // FE00-FE9F Sprite Attribute Table(OAM)
                    return OAM[addr - 0xFE00];
                case ushort _ when addr <= 0xFEFF:    // FEA0-FEFF Not Usable 0
                    return 0x00;
                case ushort _ when addr <= 0xFF7F:    // FF00-FF7F IO Ports
                    return IO[addr & 0x7F];
                case ushort _ when addr <= 0xFFFF:    // FF80-FFFE High RAM(HRAM)
                    return HRAM[addr & 0x7F];
                default:
                    return 0xFF;
            }

            //tests to simplify reads... somehow they are slower :\
            //ushort add = (ushort)(addr >> 12);
            //switch (add) {
            //    case 0x0:
            //    case 0x1:
            //    case 0x2:
            //    case 0x3:return gamePak.ReadLoROM(addr);
            //    case 0x4:
            //    case 0x5:
            //    case 0x6:
            //    case 0x7: return gamePak.ReadHiROM(addr);
            //    case 0x8:
            //    case 0x9: return VRAM[addr & 0x1FFF];
            //    case 0xA:
            //    case 0xB: return gamePak.ReadERAM(addr);
            //    case 0xC: return WRAM0[addr & 0xFFF];
            //    case 0xD: return WRAM1[addr & 0xFFF];
            //    case 0xE: return WRAM0[addr & 0xFFF];
            //    case 0xF:
            //        switch (addr) {
            //            case ushort _ when addr <= 0xFDFF:    // E000-FDFF Same as 0xC000-DDFF(ECHO)
            //                return WRAM1[addr & 0xFFF];
            //            case ushort _ when addr <= 0xFE9F:    // FE00-FE9F Sprite Attribute Table(OAM)
            //                return OAM[addr - 0xFE00];
            //            case ushort _ when addr <= 0xFEFF:    // FEA0-FEFF Not Usable 0
            //                return 0x00;
            //            case ushort _ when addr <= 0xFF7F:    // FF00-FF7F IO Ports
            //                return IO[addr & 0x7F];
            //            case ushort _ when addr <= 0xFFFF:    // FF80-FFFE High RAM(HRAM)
            //                return HRAM[addr & 0x7F];
            //            default:
            //                return 0xFF;
            //        }
            //    default: return 0xFF;
            //}
        }

        public void writeByte(ushort addr, byte b)
        {
            switch (addr)
            {                            // General Memory Map 64KB
                case ushort _ when addr <= 0x7FFF:     //0000-3FFF 16KB ROM Bank 00 (in cartridge, private at bank 00) 4000-7FFF 16KB ROM Bank 01..NN(in cartridge, switchable bank number)
                    gamePak.WriteROM(addr, b);
                    break;
                case ushort _ when addr <= 0x9FFF:    // 8000-9FFF 8KB Video RAM(VRAM)(switchable bank 0-1 in CGB Mode)
                    VRAM[addr & 0x1FFF] = b;
                    break;
                case ushort _ when addr <= 0xBFFF:    // A000-BFFF 8KB External RAM(in cartridge, switchable bank, if any)
                    gamePak.WriteERAM(addr, b);
                    break;
                case ushort _ when addr <= 0xCFFF:    // C000-CFFF 4KB Work RAM Bank 0(WRAM) <br/>
                    WRAM0[addr & 0xFFF] = b;
                    break;
                case ushort _ when addr <= 0xDFFF:    // D000-DFFF 4KB Work RAM Bank 1(WRAM)(switchable bank 1-7 in CGB Mode)
                    WRAM1[addr & 0xFFF] = b;
                    break;
                case ushort _ when addr <= 0xEFFF:    // E000-FDFF Same as 0xC000-DDFF(ECHO)  
                    WRAM0[addr & 0xFFF] = b;
                    break;
                case ushort _ when addr <= 0xFDFF:    // E000-FDFF Same as 0xC000-DDFF(ECHO)
                    WRAM1[addr & 0xFFF] = b;
                    break;
                case ushort _ when addr <= 0xFE9F:    // FE00-FE9F Sprite Attribute Table(OAM)
                    OAM[addr & 0x9F] = b;
                    break;
                case ushort _ when addr <= 0xFEFF:    // FEA0-FEFF Not Usable
                    //Console.WriteLine("Warning: Tried to write to NOT USABLE space");
                    break;
                case ushort _ when addr <= 0xFF7F:    // FF00-FF7F IO Ports
                    switch (addr)
                    {
                        case 0xFF0F: b |= 0xE0; break; // IF returns 1 on first 3 unused bits
                        case 0xFF04:                //DIV on write = 0
                        case 0xFF44: b = 0; break;  //LY on write = 0
                        case 0xFF46: DMA(b); break;
                            //case 0xFF00: b |= 0xC0; break;
                    }
                    //if (addr == 0xFF02 && b == 0x81) { //Temp Serial Link output for debug
                    //Console.Write(Convert.ToChar(readByte(0xFF01)));
                    //Console.ReadLine();
                    //}
                    IO[addr & 0x7F] = b;
                    break;
                case ushort _ when addr <= 0xFFFF:    // FF80-FFFE High RAM(HRAM)
                    HRAM[addr & 0x7F] = b;
                    break;
            }
        }

        public ushort readWord(ushort addr)
        {
            return (ushort)(readByte((ushort)(addr + 1)) << 8 | readByte(addr));
        }

        public void writeWord(ushort addr, ushort w)
        {
            writeByte((ushort)(addr + 1), (byte)(w >> 8));
            writeByte(addr, (byte)w);
        }

        public byte readOAM(int addr)
        {
            return OAM[addr];
        }

        public byte readVRAM(int addr)
        {
            return VRAM[addr & 0x1FFF];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void requestInterrupt(byte b)
        {
            IF = bitSet(b, IF);
        }

        private void DMA(byte b)
        {
            ushort addr = (ushort)(b << 8);
            for (byte i = 0; i < OAM.Length; i++)
            {
                OAM[i] = readByte((ushort)(addr + i));
            }
        }

        public void loadGamePak(byte[] rom)
        {
            switch (rom[0x147])
            {
                case 0x00:
                    gamePak = new MBC0();
                    break;
                case 0x01:
                case 0x02:
                case 0x03:
                    gamePak = new MBC1();
                    break;
                case 0x05:
                case 0x06:
                    gamePak = new MBC2();
                    break;
                case 0x0F:
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                    gamePak = new MBC3();
                    break;
                case 0x19:
                case 0x1A:
                case 0x1B:
                    gamePak = new MBC5();
                    break;
                default:
                    Kernel.console.WriteLine("Unsupported MBC: " + rom[0x147].ToString("x2"));
                    break;
            }
            gamePak.Init(rom);
        }

    }
}


