using DoomSharp.Core.Data;
using System;

namespace DoomSharp.Core.Graphics;

public class Video
{
    private IGraphics _graphics;
    
    private readonly byte[][] _screens = new byte[5][];
    private readonly Fixed[] _dirtyBox = new Fixed[4];

    private int _gamma;
    private static readonly byte[][] GammaTable = new byte[5][];

    private static bool _wipeGo;
    private static byte[] _wipeScreenStart = Array.Empty<byte>();
    private static byte[] _wipeScreenEnd = Array.Empty<byte>();
    private static byte[] _wipeScreen = Array.Empty<byte>();

    private int[]? _wipeMeltPos;

    static Video()
    {
        GammaTable[0] = new byte[] {1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
            17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,
            33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,
            49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,
            65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,
            81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,
            97,98,99,100,101,102,103,104,105,106,107,108,109,110,111,112,
            113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,128,
            128,129,130,131,132,133,134,135,136,137,138,139,140,141,142,143,
            144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,159,
            160,161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,
            176,177,178,179,180,181,182,183,184,185,186,187,188,189,190,191,
            192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,207,
            208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,223,
            224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,
            240,241,242,243,244,245,246,247,248,249,250,251,252,253,254,255};

        GammaTable[1] = new byte[] {2,4,5,7,8,10,11,12,14,15,16,18,19,20,21,23,24,25,26,27,29,30,31,
            32,33,34,36,37,38,39,40,41,42,44,45,46,47,48,49,50,51,52,54,55,
            56,57,58,59,60,61,62,63,64,65,66,67,69,70,71,72,73,74,75,76,77,
            78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,
            99,100,101,102,103,104,105,106,107,108,109,110,111,112,113,114,
            115,116,117,118,119,120,121,122,123,124,125,126,127,128,129,129,
            130,131,132,133,134,135,136,137,138,139,140,141,142,143,144,145,
            146,147,148,148,149,150,151,152,153,154,155,156,157,158,159,160,
            161,162,163,163,164,165,166,167,168,169,170,171,172,173,174,175,
            175,176,177,178,179,180,181,182,183,184,185,186,186,187,188,189,
            190,191,192,193,194,195,196,196,197,198,199,200,201,202,203,204,
            205,205,206,207,208,209,210,211,212,213,214,214,215,216,217,218,
            219,220,221,222,222,223,224,225,226,227,228,229,230,230,231,232,
            233,234,235,236,237,237,238,239,240,241,242,243,244,245,245,246,
            247,248,249,250,251,252,252,253,254,255};

        GammaTable[2] = new byte[] {4,7,9,11,13,15,17,19,21,22,24,26,27,29,30,32,33,35,36,38,39,40,42,
            43,45,46,47,48,50,51,52,54,55,56,57,59,60,61,62,63,65,66,67,68,69,
            70,72,73,74,75,76,77,78,79,80,82,83,84,85,86,87,88,89,90,91,92,93,
            94,95,96,97,98,100,101,102,103,104,105,106,107,108,109,110,111,112,
            113,114,114,115,116,117,118,119,120,121,122,123,124,125,126,127,128,
            129,130,131,132,133,133,134,135,136,137,138,139,140,141,142,143,144,
            144,145,146,147,148,149,150,151,152,153,153,154,155,156,157,158,159,
            160,160,161,162,163,164,165,166,166,167,168,169,170,171,172,172,173,
            174,175,176,177,178,178,179,180,181,182,183,183,184,185,186,187,188,
            188,189,190,191,192,193,193,194,195,196,197,197,198,199,200,201,201,
            202,203,204,205,206,206,207,208,209,210,210,211,212,213,213,214,215,
            216,217,217,218,219,220,221,221,222,223,224,224,225,226,227,228,228,
            229,230,231,231,232,233,234,235,235,236,237,238,238,239,240,241,241,
            242,243,244,244,245,246,247,247,248,249,250,251,251,252,253,254,254,
            255};

        GammaTable[3] = new byte[] {8,12,16,19,22,24,27,29,31,34,36,38,40,41,43,45,47,49,50,52,53,55,
            57,58,60,61,63,64,65,67,68,70,71,72,74,75,76,77,79,80,81,82,84,85,
            86,87,88,90,91,92,93,94,95,96,98,99,100,101,102,103,104,105,106,107,
            108,109,110,111,112,113,114,115,116,117,118,119,120,121,122,123,124,
            125,126,127,128,129,130,131,132,133,134,135,135,136,137,138,139,140,
            141,142,143,143,144,145,146,147,148,149,150,150,151,152,153,154,155,
            155,156,157,158,159,160,160,161,162,163,164,165,165,166,167,168,169,
            169,170,171,172,173,173,174,175,176,176,177,178,179,180,180,181,182,
            183,183,184,185,186,186,187,188,189,189,190,191,192,192,193,194,195,
            195,196,197,197,198,199,200,200,201,202,202,203,204,205,205,206,207,
            207,208,209,210,210,211,212,212,213,214,214,215,216,216,217,218,219,
            219,220,221,221,222,223,223,224,225,225,226,227,227,228,229,229,230,
            231,231,232,233,233,234,235,235,236,237,237,238,238,239,240,240,241,
            242,242,243,244,244,245,246,246,247,247,248,249,249,250,251,251,252,
            253,253,254,254,255};

        GammaTable[4] = new byte[] {16,23,28,32,36,39,42,45,48,50,53,55,57,60,62,64,66,68,69,71,73,75,76,
            78,80,81,83,84,86,87,89,90,92,93,94,96,97,98,100,101,102,103,105,106,
            107,108,109,110,112,113,114,115,116,117,118,119,120,121,122,123,124,
            125,126,128,128,129,130,131,132,133,134,135,136,137,138,139,140,141,
            142,143,143,144,145,146,147,148,149,150,150,151,152,153,154,155,155,
            156,157,158,159,159,160,161,162,163,163,164,165,166,166,167,168,169,
            169,170,171,172,172,173,174,175,175,176,177,177,178,179,180,180,181,
            182,182,183,184,184,185,186,187,187,188,189,189,190,191,191,192,193,
            193,194,195,195,196,196,197,198,198,199,200,200,201,202,202,203,203,
            204,205,205,206,207,207,208,208,209,210,210,211,211,212,213,213,214,
            214,215,216,216,217,217,218,219,219,220,220,221,221,222,223,223,224,
            224,225,225,226,227,227,228,228,229,229,230,230,231,232,232,233,233,
            234,234,235,235,236,236,237,237,238,239,239,240,240,241,241,242,242,
            243,243,244,244,245,245,246,246,247,247,248,248,249,249,250,250,251,
            251,252,252,253,254,254,255,255};
    }

    public Video(IGraphics graphics)
    {
        _graphics = graphics;
    }

    public byte[][] Screens => _screens;

    public void Initialize()
    {
        for (var i = 0; i < 4; i++)
        {
            _screens[i] = new byte[Constants.ScreenWidth * Constants.ScreenHeight];
        }

        BoundingBox.ClearBox(_dirtyBox);
    }

    public void ToggleGamma()
    {
        _gamma++;
        if (_gamma > 4)
        {
            _gamma = 0;
        }

        SetPalette("PLAYPAL");
    }

    public void SetPalette(string paletteName, int paletteIndex = 0)
    {
        const int bytesPerPalette = 256 * 3;
        var paletteLump = DoomGame.Instance.WadData.GetLumpName(paletteName, PurgeTag.Cache)!;
        var palette = new byte[bytesPerPalette];
        var j = 0;

        for (var i = paletteIndex * bytesPerPalette; i < (paletteIndex + 1) * bytesPerPalette; i++)
        {
            var c = GammaTable[_gamma][paletteLump[i]];
            palette[j++] = (byte)((c << 8) + c);
        }
        
        _graphics.UpdatePalette(palette);
    }

    public void SetOutputRenderer(IGraphics renderer)
    {
        _graphics = renderer;
    }

    public void SignalOutputReady(int screen = 0)
    {
        _graphics.ScreenReady(_screens[screen]);
    }

    //
    // V_DrawBlock
    // Draw a linear block of pixels into the view buffer.
    //
    public void DrawBlock(int x, int y, int screen, int width, int height, byte[] src)
    {
        // Range check
        if (x < 0 ||
            (x + width) > Constants.ScreenWidth ||
            y < 0 ||
            (y + height) > Constants.ScreenHeight ||
            screen > 4)
        {
            DoomGame.Console.WriteLine("Bad V_DrawBlock");
            return;
        }
        // End range check

        MarkRectangle(0, 0, width, height);

        var destIdx = y * Constants.ScreenWidth + x;
        var sourceIdx = 0;
        while (height-- != 0)
        {
            Array.Copy(src, sourceIdx, _screens[screen], destIdx, width);
            sourceIdx += width;
            destIdx += Constants.ScreenWidth;
        }
    }

    //
    // V_DrawPatch
    // Masks a column based masked pic to the screen. 
    //
    public void DrawPatch(int x, int y, int screen, Patch patch)
    {
        y -= patch.TopOffset;
        x -= patch.LeftOffset;

        // Range check
        if (x < 0 ||
            (x + patch.Width) > Constants.ScreenWidth ||
            y < 0 ||
            (y + patch.Height) > Constants.ScreenHeight ||
            screen > 4)
        {
            DoomGame.Console.WriteLine($"Patch at {x}, {y} exceeds LFB");
            DoomGame.Console.WriteLine("V_DrawPatch: bad patch (ignored)");
            return;
        }
        // End range check

        if (screen == 0)
        {
            MarkRectangle(x, y, patch.Width, patch.Height);
        }

        var col = 0;
        var destTop = y * Constants.ScreenWidth + x;
        var width = patch.Width;

        for (; col < width; x++, col++, destTop++)
        {
            var column = patch.Columns[col];

            if (column is null || column.TopDelta == 255)
            {
                continue;
            }

            // step through the posts in a column 
            while (column != null && column.TopDelta != 255)
            {
                var source = 0;
                var dest = destTop + column.TopDelta * Constants.ScreenWidth;
                var count = column.Length;

                while (count-- > 0)
                {
                    _screens[screen][dest] = column.Pixels[source++];
                    dest += Constants.ScreenWidth;
                }

                column = column.Next;
            }
        }
    }

    //
    // V_DrawPatch
    // Masks a column based masked pic to the screen. 
    //
    public void DrawPatch(int x, int y, int screen, byte[]? patch)
    {
        if (patch is null)
        {
            return;
        }

        DrawPatch(x, y, screen, Patch.FromBytes(patch));
    }

    //
    // V_DrawPatchDirect
    // Draws directly to the screen on the pc. 
    //
    public void DrawPatchDirect(int x, int y, int screen, Patch patch)
    {
        DrawPatch(x, y, screen, patch);
    }

    //
    // V_DrawPatchDirect
    // Draws directly to the screen on the pc. 
    //
    public void DrawPatchDirect(int x, int y, int screen, byte[]? patch)
    {
        if (patch is null)
        {
            return;
        }

        DrawPatch(x, y, screen, Patch.FromBytes(patch));
    }

    // Wiping / melting functions
    public int WipeStartScreen(int x, int y, int width, int height)
    {
        _wipeScreenStart = _screens[2];
        ReadScreen(_wipeScreenStart);
        return 0;
    }

    public int WipeEndScreen(int x, int y, int width, int height)
    {
        _wipeScreenEnd = _screens[3];
        ReadScreen(_wipeScreenEnd);
        DrawBlock(x, y, 0, width, height, _wipeScreenStart); // restore start screen.
        return 0;
    }

    public bool WipeScreenEffect(WipeMethod wipeMethod, int x, int y, int width, int height, int tics)
    {
        var wipeNo = (int)wipeMethod;

        var wipeFunctions = new Func<int, int, int, bool>[]
        {
            InitializeColorTransform, DoColorTransform, ExitColorTransform,
            InitializeMelt, DoMelt, ExitMelt
        };

        // initial stuff
        if (!_wipeGo)
        {
            _wipeGo = true;
            // wipe_scr = (byte *) Z_Malloc(width*height, PU_STATIC, 0); // DEBUG
            _wipeScreen = _screens[0];
            wipeFunctions[wipeNo * 3](width, height, tics);
        }

        // do a piece of wipe-in
        MarkRectangle(0, 0, width, height);
        var rc = wipeFunctions[wipeNo * 3 + 1](width, height, tics);
        //  V_DrawBlock(x, y, 0, width, height, wipe_scr); // DEBUG

        // final stuff
        if (rc)
        {
            _wipeGo = false;
            wipeFunctions[wipeNo * 3 + 2](width, height, tics);
        }

        return !_wipeGo;
    }

    private bool InitializeColorTransform(int width, int height, int tics)
    {
        Array.Copy(_wipeScreenStart, 0, _wipeScreen, 0, Constants.ScreenWidth * Constants.ScreenHeight);
        return false;
    }

    private bool DoColorTransform(int width, int height, int tics)
    {
        var changed = false;

        var targetIdx = 0;
        var sourceIdx = 0;

        while (targetIdx != width * height)
        {
            if (_wipeScreen[targetIdx] != _wipeScreenEnd[sourceIdx])
            {
                byte newValue;
                if (_wipeScreen[targetIdx] > _wipeScreenEnd[sourceIdx])
                {
                    newValue = (byte)(_wipeScreen[targetIdx] - tics);
                    if (newValue < _wipeScreenEnd[sourceIdx])
                    {
                        _wipeScreen[targetIdx] = _wipeScreenEnd[sourceIdx];
                    }
                    else
                    {
                        _wipeScreen[targetIdx] = newValue;
                    }
                    
                    changed = true;
                }
                else if (_wipeScreen[targetIdx] < _wipeScreenEnd[sourceIdx])
                {
                    newValue = (byte)(_wipeScreen[targetIdx] + tics);
                    if (newValue > _wipeScreenEnd[sourceIdx])
                    {
                        _wipeScreen[targetIdx] = _wipeScreenEnd[sourceIdx];
                    }
                    else
                    {
                        _wipeScreen[targetIdx] = newValue;
                    }

                    changed = true;
                }
            }

            targetIdx++;
            sourceIdx++;
        }

        return !changed;
    }

    private bool ExitColorTransform(int width, int height, int tics)
    {
        return false;
    }

    private bool InitializeMelt(int width, int height, int tics)
    {
        // copy start screen to main screen
        Array.Copy(_wipeScreenStart, 0, _wipeScreen, 0, width * height);

        // makes this wipe faster (in theory)
        // to have stuff in column-major format
        ShittyColorMajorTransform(_wipeScreenStart, width, height);
        ShittyColorMajorTransform(_wipeScreenEnd, width, height);

        // setup initial column positions
        // (y<0 => not ready to scroll yet)
        _wipeMeltPos = new int[width];
        _wipeMeltPos[0] = -(DoomRandom.M_Random() % 16);
        for (var i = 1; i < width; i++)
        {
            var r = (DoomRandom.M_Random() % 3) - 1;
            _wipeMeltPos[i] = _wipeMeltPos[i - 1] + r;
            if (_wipeMeltPos[i] > 0)
            {
                _wipeMeltPos[i] = 0;
            }
            else if (_wipeMeltPos[i] == -16)
            {
                _wipeMeltPos[i] = -15;
            }
        }

        return false;
    }

    private void ShittyColorMajorTransform(byte[] array, int width, int height)
    {
        var dest = new byte[width * height];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                dest[x * height + y] = array[y * width + x];
            }
        }

        Array.Copy(dest, 0, array, 0, dest.Length);
    }

    private bool DoMelt(int width, int height, int tics)
    {
        var done = true;

        if (_wipeMeltPos is null)
        {
            DoomGame.Error("wipe_doMelt: y is not initialized!");
            return false;
        }

        while (tics-- > 0)
        {
            for (var i = 0; i < width; i++)
            {
                if (_wipeMeltPos[i] < 0)
                {
                    _wipeMeltPos[i]++; 
                    done = false;
                }
                else if (_wipeMeltPos[i] < height)
                {
                    var dy = (_wipeMeltPos[i] < 16) ? _wipeMeltPos[i] + 1 : 8;
                    if (_wipeMeltPos[i] + dy >= height)
                    {
                        dy = height - _wipeMeltPos[i];
                    }

                    var sourceIdx = i * height + _wipeMeltPos[i];
                    var destIdx = _wipeMeltPos[i] * width + i;
                    
                    for (var j = dy; j != 0; j--)
                    {
                        _wipeScreen[destIdx] = _wipeScreenEnd[sourceIdx++];
                        destIdx += width;
                    }

                    _wipeMeltPos[i] += dy;
                    sourceIdx = i * height;
                    destIdx = _wipeMeltPos[i] * width + i;
                    
                    for (var j = height - _wipeMeltPos[i]; j != 0; j--)
                    {
                        _wipeScreen[destIdx] = _wipeScreenStart[sourceIdx++];
                        destIdx += width;
                    }

                    done = false;
                }
            }
        }

        return done;
    }

    private bool ExitMelt(int width, int height, int tics)
    {
        _wipeMeltPos = null;
        return false;
    }

    private void ReadScreen(byte[] target)
    {
        Array.Copy(_screens[0], 0, target, 0, Constants.ScreenWidth * Constants.ScreenHeight);
    }

    public void MarkRectangle(int x, int y, int width, int height)
    {
        BoundingBox.AddToBox(_dirtyBox, Fixed.FromInt(x), Fixed.FromInt(y));
        BoundingBox.AddToBox(_dirtyBox, Fixed.FromInt(x + width - 1), Fixed.FromInt(y + height - 1));
    }

    public void CopyRectangle(int srcX, int srcY, int srcScreen, int width, int height, int destX, int destY, int destScreen)
    {
        MarkRectangle(destX, destY, width, height);

        var src = Constants.ScreenWidth * srcY + srcX;
        var dest = Constants.ScreenWidth * destY + destX;

        for (; height > 0; height--)
        {
            Array.Copy(_screens[srcScreen], src, _screens[destScreen], dest, width);
            src += Constants.ScreenWidth;
            dest += Constants.ScreenWidth;
        }
    }
}