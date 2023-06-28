namespace ProjectDMG.DMG.GamePak {
    class MBC0 : IGamePak {

        private byte[] ROM;

        public void Init(byte[] ROM) {
            this.ROM = ROM;
        }

        public byte ReadERAM(ushort addr) {
            return 0xFF; //MBC0 dosn't have ERAM
        }

        public byte ReadLoROM(ushort addr) {
            return ROM[addr];
        }
        
        public byte ReadHiROM(ushort addr){
            return ROM[addr];
        }

        public void WriteERAM(ushort addr, byte value) {
            //MBC0 should ignore writes
        }

        public void WriteROM(ushort addr, byte value) {
            //MBC0 should ignore writes
        }
    }
}
