/*
* PROJECT:          Aura Operating System Development
* CONTENT:          CPU Informations
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace Aura_OS.Core
{
    /// <summary>
    /// CPU vendor, brand string, TSC frequency, cycle counter and uptime.
    /// GEN3-GAP(cpuinfo): Cosmos has no CPU information API; this reads CPUID through the BCL
    /// (X86Base.CpuId, backed by RhCpuIdEx) and the TSC through the plugged Stopwatch.
    /// </summary>
    public static class CpuInfo
    {
        /// <summary>
        /// CPUID leaf 0 vendor id (e.g. "GenuineIntel"), or "Unknown" off x86.
        /// </summary>
        public static string GetVendor()
        {
            if (!X86Base.IsSupported)
            {
                return "Unknown";
            }

            var leaf = X86Base.CpuId(0, 0);

            Span<byte> bytes = stackalloc byte[12];
            BinaryPrimitives.WriteInt32LittleEndian(bytes, leaf.Ebx);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.Slice(4), leaf.Edx);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.Slice(8), leaf.Ecx);

            return Encoding.ASCII.GetString(bytes);
        }

        /// <summary>
        /// CPUID leaves 0x80000002..0x80000004 brand string, or the vendor id when the CPU has no brand string.
        /// </summary>
        public static string GetBrandString()
        {
            if (!X86Base.IsSupported)
            {
                return "Unknown";
            }

            var extended = X86Base.CpuId(unchecked((int)0x80000000), 0);
            if ((uint)extended.Eax < 0x80000004)
            {
                return GetVendor();
            }

            Span<byte> bytes = stackalloc byte[48];
            for (int i = 0; i < 3; i++)
            {
                var leaf = X86Base.CpuId(unchecked((int)(0x80000002u + (uint)i)), 0);
                Span<byte> slice = bytes.Slice(i * 16);
                BinaryPrimitives.WriteInt32LittleEndian(slice, leaf.Eax);
                BinaryPrimitives.WriteInt32LittleEndian(slice.Slice(4), leaf.Ebx);
                BinaryPrimitives.WriteInt32LittleEndian(slice.Slice(8), leaf.Ecx);
                BinaryPrimitives.WriteInt32LittleEndian(slice.Slice(12), leaf.Edx);
            }

            int length = bytes.IndexOf((byte)0);
            string brand = Encoding.ASCII.GetString(length < 0 ? bytes : bytes.Slice(0, length)).Trim();

            return brand.Length == 0 ? GetVendor() : brand;
        }

        /// <summary>
        /// Calibrated TSC frequency in MHz (x64).
        /// </summary>
        public static long FrequencyMHz
        {
            get
            {
                return Stopwatch.Frequency / 1000000;
            }
        }

        /// <summary>
        /// Raw TSC value (RDTSC on x64).
        /// </summary>
        public static long Cycles
        {
            get
            {
                return Stopwatch.GetTimestamp();
            }
        }

        /// <summary>
        /// Time since boot.
        /// </summary>
        public static TimeSpan Uptime
        {
            get
            {
                return TimeSpan.FromMilliseconds(Environment.TickCount64);
            }
        }
    }
}
