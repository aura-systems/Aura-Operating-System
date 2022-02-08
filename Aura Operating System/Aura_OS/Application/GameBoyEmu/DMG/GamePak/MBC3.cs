using System;

namespace ProjectDMG.DMG.GamePak {
    class MBC3 : IGamePak {

        private byte[] ROM;
        private byte[] ERAM = new byte[0x8000]; //MBC1 MAX ERAM on 4 banks
        private bool ERAM_ENABLED;
        private int ROM_BANK = 1; //default as 0 is 0x0000 - 0x3FFF fixed
        private int RAM_BANK;
        private const int ROM_OFFSET = 0x4000;
        private const int ERAM_OFFSET = 0x2000;

        private byte RTC_S;  //08h  RTC S   Seconds   0-59 (0-3Bh)
        private byte RTC_M;  //09h RTC M Minutes   0-59 (0-3Bh)
        private byte RTC_H;  //0Ah RTC H Hours     0-23 (0-17h)
        private byte RTC_DL; //0Bh RTC DL Lower 8 bits of Day Counter(0-FFh)
        private byte RTC_DH; //0Ch RTC DH Upper 1 bit of Day Counter, Carry Bit, Halt Flag
        private byte RTC_0;  //Bit 0  Most significant bit of Day Counter(Bit 8)
        private byte RTC_6;  //Bit 6  Halt(0=Active, 1=Stop Timer)
        private byte RTC_7;  //Bit 7  Day Counter Carry Bit(1=Counter Overflow)

        public void Init(byte[] ROM) {
            this.ROM = ROM;
        }

        public byte ReadERAM(ushort addr) {
            if (ERAM_ENABLED) {
                switch (RAM_BANK) {
                    case int r when RAM_BANK >= 0x00 && RAM_BANK <= 0x03:
                        return ERAM[(ERAM_OFFSET * RAM_BANK) + (addr & 0x1FFF)];
                    case 0x08:
                        return RTC_S;
                    case 0x09:
                        return RTC_M;
                    case 0x0A:
                        return RTC_H;
                    case 0x0B:
                        return RTC_DL;
                    case 0x0C:
                        return RTC_DH;
                    default:
                        return 0xFF;
                }
            } else {
                return 0xFF;
            }
        }

        public byte ReadLoROM(ushort addr) {
            return ROM[addr];
        }

        public byte ReadHiROM(ushort addr) {
            return ROM[(ROM_OFFSET * ROM_BANK) + (addr & 0x3FFF)];
        }

        public void WriteERAM(ushort addr, byte value) {
            if (ERAM_ENABLED) {
                switch (RAM_BANK) {
                    case 0x00: case 0x01: case 0x02: case 0x03:
                        ERAM[(ERAM_OFFSET * RAM_BANK) + (addr & 0x1FFF)] = value;
                        break;
                    case 0x08:
                        RTC_S = value;
                        break;
                    case 0x09:
                        RTC_M = value;
                        break;
                    case 0x0A:
                        RTC_H = value;
                        break;
                    case 0x0B:
                        RTC_DL = value;
                        break;
                    case 0x0C:
                        RTC_DH = value;
                        break;
                }
            }
        }

        public void WriteROM(ushort addr, byte value) {
            switch (addr) {
                case ushort r when addr >= 0x0000 && addr <= 0x1FFF:
                    ERAM_ENABLED = value == 0x0A ? true : false;
                    break;
                case ushort r when addr >= 0x2000 && addr <= 0x3FFF:
                    ROM_BANK = value & 0x7F;
                    if (ROM_BANK == 0x00)
                        ROM_BANK++;
                    break;
                case ushort r when addr >= 0x4000 && addr <= 0x5FFF:
                    switch (value) {
                        case byte v when value >= 0x00 && value <= 0x03:
                        case byte s when value >= 0x08 && value <= 0xC0:
                            RAM_BANK = value;
                            break;
                    }
                    break;
                case ushort r when addr >= 0x6000 && addr <= 0x7FFF:
                    // Latch Clock Data (Write Only)
                    DateTime now = DateTime.Now;
                    RTC_S = (byte)now.Second;
                    RTC_M = (byte)now.Minute;
                    RTC_H = (byte)now.Hour;

                    break;
            }
        }

    }
}
