using System.Runtime.CompilerServices;

namespace ProjectDMG.Utils {
    public static class BitOps {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte bitSet(byte n, byte v) {
            return v |= (byte)(1 << n);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte bitClear(int n, byte v) {
            return v &= (byte)~(1 << n);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool isBit(int n, int v) {
            return ((v >> n) & 1) == 1;
        }
    }
}
