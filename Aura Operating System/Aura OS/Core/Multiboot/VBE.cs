/*
* PROJECT:          Aura Operating System Development
* CONTENT:          VBE Controller Informations + VBE Mode Info
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   https://wiki.osdev.org/User:Omarrx024/VESA_Tutorial
*                   http://www.phatcode.net/res/221/files/vbe20.pdf
*/

using System.Runtime.InteropServices;

namespace Aura_OS.Core
{
    public unsafe static class VBE
    {

        [StructLayout(LayoutKind.Explicit, Size = 36)]
        public struct ControllerInfo
        {
            [FieldOffset(0)]
            public uint vbeSignature;
            [FieldOffset(4)]
            public ushort vbeVersion;
            [FieldOffset(6)]
            public uint oemStringPtr;
            [FieldOffset(10)]
            public uint capabilities;
            [FieldOffset(14)]
            public uint videoModePtr;
            [FieldOffset(18)]
            public ushort totalmemory;
            [FieldOffset(20)]

            public ushort oemSoftwareRev;
            [FieldOffset(24)]
            public uint oemVendorNamePtr;
            [FieldOffset(28)]
            public uint oemProductNamePtr;
            [FieldOffset(32)]
            public uint oemProductRevPtr;
        }

        [StructLayout(LayoutKind.Explicit, Size = 256)]
        public struct ModeInfo
        {
            [FieldOffset(0)]
            public ushort attributes; // deprecated, only bit 7 should be of interest to you, and it indicates the mode supports a linear frame buffer.
            [FieldOffset(2)]
            public byte window_a; // deprecated
            [FieldOffset(3)]
            public byte window_b; // deprecated
            [FieldOffset(4)]
            public ushort granularity; // deprecated; used while calculating bank numbers
            [FieldOffset(6)]
            public ushort window_size;
            [FieldOffset(8)]
            public ushort segment_a;
            [FieldOffset(10)]
            public ushort segment_b;
            [FieldOffset(12)]
            public uint win_func_ptr; // deprecated; used to switch banks from protected mode without returning to real mode
            [FieldOffset(16)]
            public ushort pitch; // number of bytes per horizontal line
            [FieldOffset(18)]
            public ushort width; // width in pixels
            [FieldOffset(20)]
            public ushort height; // height in pixels
            [FieldOffset(22)]
            public byte w_char; // unused...
            [FieldOffset(23)]
            public byte y_char; // ...
            [FieldOffset(24)]
            public byte planes;
            [FieldOffset(25)]
            public byte bpp; // bits per pixel in this mode
            [FieldOffset(26)]
            public byte banks; // deprecated; total number of banks in this mode
            [FieldOffset(27)]
            public byte memory_model;
            [FieldOffset(28)]
            public byte bank_size; // deprecated; size of a bank, almost always 64 KB but may be 16 KB...
            [FieldOffset(29)]
            public byte image_pages;
            [FieldOffset(30)]
            public byte reserved0;
            [FieldOffset(31)]
            public byte red_mask;
            [FieldOffset(32)]
            public byte red_position;
            [FieldOffset(33)]
            public byte green_mask;
            [FieldOffset(34)]
            public byte green_position;
            [FieldOffset(35)]
            public byte blue_mask;
            [FieldOffset(36)]
            public byte blue_position;
            [FieldOffset(37)]
            public byte reserved_mask;
            [FieldOffset(38)]
            public byte reserved_position;
            [FieldOffset(39)]
            public byte direct_color_attributes;
            [FieldOffset(40)]
            public uint framebuffer; // physical address of the linear frame buffer; write here to draw to the screen
            [FieldOffset(44)]
            public uint off_screen_mem_off;
            [FieldOffset(48)]
            public ushort off_screen_mem_size; // size of memory in the framebuffer but not being displayed on the screen
            [FieldOffset(50)]
            public fixed byte reserved1[206];
        }

    }

}