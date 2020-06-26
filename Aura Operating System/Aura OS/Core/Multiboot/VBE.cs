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
    public static class VBE
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

        public struct ModeInfo
        {
            public ushort attributes; // deprecated, only bit 7 should be of interest to you, and it indicates the mode supports a linear frame buffer.
            public byte window_a; // deprecated
            public byte window_b; // deprecated
            public ushort granularity; // deprecated; used while calculating bank numbers
            public ushort window_size;
            public ushort segment_a;
            public ushort segment_b;
            public uint win_func_ptr; // deprecated; used to switch banks from protected mode without returning to real mode
            public ushort pitch; // number of bytes per horizontal line
            public ushort width; // width in pixels
            public ushort height; // height in pixels
            public byte w_char; // unused...
            public byte y_char; // ...
            public byte planes;
            public byte bpp; // bits per pixel in this mode
            public byte banks; // deprecated; total number of banks in this mode
            public byte memory_model;
            public byte bank_size; // deprecated; size of a bank, almost always 64 KB but may be 16 KB...
            public byte image_pages;
            public byte reserved0;
            public byte red_mask;
            public byte red_position;
            public byte green_mask;
            public byte green_position;
            public byte blue_mask;
            public byte blue_position;
            public byte reserved_mask;
            public byte reserved_position;
            public byte direct_color_attributes;
            public uint framebuffer; // physical address of the linear frame buffer; write here to draw to the screen
            public uint off_screen_mem_off;
            public ushort off_screen_mem_size; // size of memory in the framebuffer but not being displayed on the screen
            //[FieldOffset(50)]
            //public fixed byte reserved1[206];
        }

    }

}
