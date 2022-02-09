using System;

namespace ProjectDMG.DMG.GamePak {
    class MBC2 : IGamePak {

        private byte[] ROM;
        private byte[] ERAM = new byte [0x200]; //MBC2 512x4b internal ram
        private bool ERAM_ENABLED;
        private int ROM_BANK;
        private const int ROM_OFFSET = 0x4000;

        public void Init(byte[] ROM) {
            this.ROM = ROM;
        }

        public byte ReadERAM(ushort addr) {
            if (ERAM_ENABLED){
                return ERAM[addr & 0x1FFF];
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
               ERAM[addr & 0x1FFF] = value;
            }
        }

        public void WriteROM(ushort addr, byte value) {
            switch (addr) {
                case ushort r when addr >= 0x0000 && addr <= 0x1FFF:
                    ERAM_ENABLED = ((value & 0x1) == 0x0) ? true : false;
                    break;
                case ushort r when addr >= 0x2000 && addr <= 0x3FFF:
                    ROM_BANK = value & 0xF; //only last 4bits are used
                    break;
            }
        }

    }
}
