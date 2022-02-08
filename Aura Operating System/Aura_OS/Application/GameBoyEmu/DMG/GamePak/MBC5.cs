using System;

namespace ProjectDMG.DMG.GamePak {
    class MBC5 : IGamePak {

        private byte[] ROM;
        private byte[] ERAM = new byte [0x20000]; //MBC5 MAX 128 KBytes (16 banks of 8KBytes each)
        private bool ERAM_ENABLED;
        private int ROM_BANK_LOW_BITS = 1; //default as 0 is 0x000 - 0x3FFF fixed
        private int ROM_BANK_HI_BIT;
        private const int ROM_OFFSET = 0x4000;
        private const int ERAM_OFFSET = 0x2000;
        private int RAM_BANK;

        public void Init(byte[] ROM) {
            this.ROM = ROM;
        }

        public byte ReadERAM(ushort addr) {
            if (ERAM_ENABLED){
                return ERAM[(ERAM_OFFSET * RAM_BANK) + (addr & 0x1FFF)];
            } else {
                return 0xFF;
            }
        }

        public byte ReadLoROM(ushort addr) {
            return ROM[addr];
        }

        public byte ReadHiROM(ushort addr) {
            return ROM[(ROM_OFFSET * (ROM_BANK_HI_BIT + ROM_BANK_LOW_BITS)) + (addr & 0x3FFF)];
        }

        public void WriteERAM(ushort addr, byte value) {
            if (ERAM_ENABLED) {
               ERAM[(ERAM_OFFSET * RAM_BANK) + (addr & 0x1FFF)] = value;
            }
        }

        public void WriteROM(ushort addr, byte value) {
            switch (addr) {
                case ushort r when addr >= 0x0000 && addr <= 0x1FFF:
                    ERAM_ENABLED = value == 0x0A ? true : false;
                    break;
                case ushort r when addr >= 0x2000 && addr <= 0x2FFF:
                    ROM_BANK_LOW_BITS = value;
                    break;
                case ushort r when addr >= 0x3000 && addr <= 0x3FFF:
                    ROM_BANK_HI_BIT = value;
                    break;
                case ushort r when addr >= 0x4000 && addr <= 0x5FFF:
                    RAM_BANK = value & 0xF;
                    break;
            }
        }

    }
}
