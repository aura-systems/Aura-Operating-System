using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Aura_OS.Core
{
    public unsafe static class VBE
    {

        [StructLayout(LayoutKind.Explicit, Size = 51)]
        public struct ModeInfo
        {
            [FieldOffset(0)]
            public ushort attributes; // deprecated, only bit 7 should be of interest to you, and it indicates the mode supports a linear frame buffer.
            public const int VESA_ATTR_HWSUPPORT = 0x01;
            public const int VESA_ATTR_TTY = 0x04;
            public const int VESA_ATTR_COLOR = 0x08;
            public const int VESA_ATTR_GRAPHICS = 0x10;
            public const int VESA_ATTR_NOTVGA = 0x20;
            public const int VESA_ATTR_NOTWINDOW = 0x40;
            public const int VESA_ATTR_LINEARFB = 0x80;
            public const int VESA_ATTR_DOUBLESCAN = 0x100;
            public const int VESA_ATTR_INTERLACE = 0x200;
            public const int VESA_ATTR_TRIPLEBUF = 0x400;
            public const int VESA_ATTR_STEREO = 0x800;
            public const int VESA_ATTR_DUALDISP = 0x1000;

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
