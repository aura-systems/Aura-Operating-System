using System;
using System.Collections.Generic;
using System.Text;
using DoomSharp.Core.Data;
using DoomSharp.Core.Extensions;
using DoomSharp.Core.GameLogic;
using DoomSharp.Core.UI;

namespace DoomSharp.Core.Graphics;

public class RenderEngine
{
    // Lighting constants
    public const int LightLevels = 16;
    public const int LightSegShift = 4;
    public const int MaxLightScale = 48;
    public const int LightScaleShift = 12;
    public const int MaxLightZ = 128;
    public const int LightZShift = 20;

    public const int SlopeRange = 2048;
    public const int SlopeBits = 11;
    public const int DBits = (Constants.FracBits - SlopeBits);

    // Number of diminishing brightness levels.
    // There a 0-31, i.e. 32 LUT in the COLORMAP lump.
    public const int NumColorMaps = 32;

    public const int MaxWidth = 1120;
    public const int MaxHeight = 832;
    public const int FieldOfView = 2048;

    private int _viewAngleOffset;

    // increment every time a check is made
    public int ValidCount { get; set; } = 1;

    private int? _fixedColorMapIdx;

    private int _centerX;
    private int _centerY;

    private Fixed _centerXFrac = Fixed.Zero;
    private Fixed _centerYFrac = Fixed.Zero;
    private Fixed _projection = Fixed.Zero;

    // just for profiling purposes
    private int _frameCount = 0;

    private int _ssCount;
    private int _lineCount;
    private int _loopCount;

    private Fixed _viewX;
    private Fixed _viewY;
    private Fixed _viewZ;

    private Angle _viewAngle;

    private Fixed _viewCos;
    private Fixed _viewSin;

    private Player? _viewPlayer;

    private int[][] _scaleLight = new int[LightLevels][];
    private int[] _scaleLightFixed = new int[MaxLightScale];
    private int[][] _zLight = new int[LightLevels][];

    private int _extraLight;

    private Action _colFunc = () => { };
    private Action _baseColFunc = () => { };
    private Action _fuzzColFunc = () => { };
    private Action _transColFunc = () => { };
    private Action _spanFunc = () => { };

    //
    // All drawing to the view buffer is accomplished in this file.
    // The other refresh files only know about coordinates,
    //  not the architecture of the frame buffer.
    // Conveniently, the frame buffer is a linear one,
    //  and we need only the base address,
    //  and the total size == width*height*depth/8.,
    //

    public int ViewWidth { get; private set; }
    public int ScaledViewWidth { get; private set; }
    public int ViewHeight { get; private set; }
    public int ViewWindowX { get; private set; }
    public int ViewWindowY { get; private set; }
    private int[] _yLookup = new int[MaxHeight];
    private int[] _columnOfs = new int[MaxWidth];
    private byte[][] _translations = new byte[3][];

    // 0 = high, 1 = low
    private int _detailShift;

    //
    // precalculated math tables
    //
    private Angle _clipAngle;

    // The viewangletox[viewangle + FINEANGLES/4] lookup
    // maps the visible view angles to screen X coordinates,
    // flattening the arc to a flat projection plane.
    // There will be many angles mapped to the same X. 
    private int[] _viewAngleToX = new int[DoomMath.FineAngleCount / 2];

    // The xtoviewangleangle[] table maps a screen pixel
    // to the lowest viewangle that maps back to x ranges
    // from clipangle to -clipangle.
    private Angle[] _xToViewAngle = new Angle[Constants.ScreenWidth + 1];

    public bool SetSizeNeeded { get; private set; }
    private int _setBlocks;
    private bool _setHighDetail;

    //
    // Sprite rotation 0 is facing the viewer,
    //  rotation 1 is one angle turn CLOCKWISE around the axis.
    // This is not the same as the angle,
    //  which increases counter clockwise (protractor).
    // There was a lot of stuff grabbed wrong, so I changed it...
    //
    private Fixed _pSpriteScale;
    private Fixed _pSpriteIScale;

    private int[] _spriteLights = Array.Empty<int>();

    // constant arrays
    //  used for psprite clipping and initializing clipping
    private readonly short[] _negoneArray = new short[Constants.ScreenWidth];
    private readonly short[] _screenHeightArray = new short[Constants.ScreenWidth];

    public const int MaxVisSprites = 128;
    private readonly VisSprite?[] _visSprites = new VisSprite?[MaxVisSprites];
    private int _newVisSprite = 0;
    private VisSprite _overflowSprite = new();

    private VisSprite _sortedHead = new();

    private int _firstFlat;
    private int _lastFlat;
    private int _numFlats;

    private int _firstPatch;
    private int _lastPatch;
    private int _numPatches;

    private int _firstSpriteLump;
    private int _lastSpriteLump;
    private int _numSpriteLumps;

    private int _numSprites;
    private Sprite[] _sprites = Array.Empty<Sprite>();
    private readonly SpriteFrame[] _spriteTemp = new SpriteFrame[29];
    private int _maxFrame = -1;
    private string _spriteName = "";

    private int _numTextures;
    private Texture[] _textures = Array.Empty<Texture>();

    private int[] _textureWidthMask = Array.Empty<int>();

    // needed for texture pegging
    public Fixed[] TextureHeight { get; private set; } = Array.Empty<Fixed>();
    private int[] _textureCompositeSize = Array.Empty<int>();
    private short[][] _textureColumnLump = Array.Empty<short[]>();
    private ushort[][] _textureColumnOfs = Array.Empty<ushort[]>();
    private byte[]?[] _textureComposite = Array.Empty<byte[]?>();

    // for global animation
    private int[] _flatTranslation = Array.Empty<int>();
    private int[] _textureTranslation = Array.Empty<int>();

    public int[] FlatTranslation => _flatTranslation;
    public int[] TextureTranslation => _textureTranslation;

    // needed for pre rendering
    private int[] _spriteWidth = Array.Empty<int>();
    private int[] _spriteOffset = Array.Empty<int>();
    private int[] _spriteTopOffset = Array.Empty<int>();

    private byte[] _colorMaps = Array.Empty<byte>();

    // Planes
    private Action<int, int>? _floorFunc;
    private Action<int, int>? _ceilingFunc;

    public const int MaxVisPlanes = 128;
    private VisPlane?[] _visPlanes = new VisPlane?[MaxVisPlanes];
    private int _lastVisPlaneIdx = -1;
    private VisPlane? _floorPlane;
    private VisPlane? _ceilingPlane;

    public const int MaxOpenings = Constants.ScreenWidth * 64;
    private short[] _openings = new short[MaxOpenings];
    private int _lastOpeningIdx = -1;

    //
    // Clip values are the solid pixel bounding the range.
    //  floorclip starts out SCREENHEIGHT
    //  ceilingclip starts out -1
    //
    private short[] _floorClip = new short[Constants.ScreenWidth];
    private short[] _ceilingClip = new short[Constants.ScreenWidth];

    //
    // spanstart holds the start of a plane span
    // initialized to 0 at start
    //
    private int[] _spanStart = new int[Constants.ScreenHeight];
    private int[] _spanStop = new int[Constants.ScreenHeight];

    //
    // texture mapping
    //
    private int[]? _planeZLight;
    private Fixed _planeHeight;

    private Fixed[] _ySlope = new Fixed[Constants.ScreenHeight];
    private Fixed[] _distScale = new Fixed[Constants.ScreenWidth];
    private Fixed _baseXScale;
    private Fixed _baseYScale;

    private Fixed[] _cachedHeight = new Fixed[Constants.ScreenHeight];
    private Fixed[] _cachedDistance = new Fixed[Constants.ScreenHeight];
    private Fixed[] _cachedXStep = new Fixed[Constants.ScreenHeight];
    private Fixed[] _cachedYStep = new Fixed[Constants.ScreenHeight];

    // BSP

    private Segment? _currentLine;
    private SideDef? _sideDef;
    private Line? _lineDef;
    private Sector? _frontSector;
    private Sector? _backSector;

    // Silhouette, needed for clipping Segs (mainly)
    // and sprites representing things.
    public const int SilhouetteNone = 0;
    public const int SilhouetteBottom = 1;
    public const int SilhouetteTop = 2;
    public const int SilhouetteBoth = 3;

    public const int MaxDrawSegs = 256;

    private int _drawSegIdx = -1;
    private readonly DrawSegment?[] _drawSegments = new DrawSegment?[MaxDrawSegs];

    public const int MaxSegs = 32;

    private int _newEnd;
    private ClipWallSegment[] _solidSegments = new ClipWallSegment[MaxSegs];

    // column drawing
    // Source is the top of the column to scale.
    //
    private readonly byte[] _dcColorMap = new byte[256];
    private int _dcX;
    private int _dcYl;
    private int _dcYh;
    private Fixed _dcIScale;
    private Fixed _dcTextureMid;

    // first pixel in a column (possibly virtual) 
    private byte[]? _dcSource;

    // just for profiling 
    private int _dcCount;

    // span drawing
    private int _dsY;
    private int _dsX1;
    private int _dsX2;

    private readonly byte[] _dsColorMap = new byte[256];

    private Fixed _dsXFrac;
    private Fixed _dsYFrac;
    private Fixed _dsXStep;
    private Fixed _dsYStep;

    // start of a 64*64 tile image 
    private byte[]? _dsSource;

    // just for profiling
    private int _dsCount;

    // Used for sprites and masked mid textures.
    // Masked means: partly transparent, i.e. stored
    //  in posts/runs of opaque pixels.
    private short[]? _mFloorClip;
    private int? _mFloorClipIdx;
    private short[]? _mCeilingClip;
    private int? _mCeilingClipIdx;

    private Fixed _sprYScale;
    private Fixed _sprTopScreen;

    public RenderEngine()
    {
        for (var i = 0; i < 3; i++)
        {
            _translations[i] = new byte[256];
        }

        for (var i = 0; i < LightLevels; i++)
        {
            _scaleLight[i] = new int[MaxLightScale];
            _zLight[i] = new int[MaxLightZ];
        }

        for (var i = 0; i < MaxSegs; i++)
        {
            _solidSegments[i] = new ClipWallSegment();
        }
    }

    public Sky Sky { get; } = new();

    public static uint SlopeDiv(Fixed num, Fixed den)
    {
        if ((uint)den.Value < 512)
        {
            return SlopeRange;
        }

        var ans = ((uint)num.Value << 3) / ((uint)den.Value >> 8);
        return ans <= SlopeRange ? ans : SlopeRange;
    }

    public void InitData()
    {
        InitTextures();
        DoomGame.Console.Write($"{Environment.NewLine}InitTextures");

        InitFlats();
        DoomGame.Console.Write($"{Environment.NewLine}InitFlats");

        InitSpriteLumps();
        DoomGame.Console.Write($"{Environment.NewLine}InitSpriteLumps");

        InitColorMaps();
        DoomGame.Console.Write($"{Environment.NewLine}InitColormaps");
    }

    private Column GetColumn(int tex, int col)
    {
        col &= _textureWidthMask[tex];
        var lump = _textureColumnLump[tex][col];
        var ofs = _textureColumnOfs[tex][col];

        if (lump > 0)
        {
            var data = DoomGame.Instance.WadData.GetLumpNum(lump, PurgeTag.Cache)!;
            var patch = Patch.FromBytes(data);
            return patch.GetColumnByOffset(ofs) ?? new Column(0xff, 0, Array.Empty<byte>());
        }

        if (_textureComposite[tex] == null)
        {
            GenerateComposite(tex);
        }

        var colData = new byte[_textureComposite[tex]!.Length - ofs];
        Array.Copy(_textureComposite[tex]!, ofs, colData, 0, _textureComposite[tex]!.Length - ofs);
        return new Column(0, 0, colData);
    }
    
    // Initializes the texture list
    //  with the textures from the world map.
    private void InitTextures()
    {
        // Load the patch names from pnames.lmp.
        var names = DoomGame.Instance.WadData.GetLumpName("PNAMES", PurgeTag.Static)!;
        var numMapPatches = DoomConvert.ToInt32(names[..4]);
        var patchLookup = new int[numMapPatches];

        for (var i = 0; i < numMapPatches; i++)
        {
            var name = Encoding.ASCII.GetString(names, 4 + (i * 8), 8).TrimEnd('\0');
            patchLookup[i] = DoomGame.Instance.WadData.CheckNumForName(name);
        }

        // Load the map texture definitions from textures.lmp.
        // The data is contained in one or two lumps,
        //  TEXTURE1 for shareware, plus TEXTURE2 for commercial.
        var mapTex = DoomGame.Instance.WadData.GetLumpName("TEXTURE1", PurgeTag.Static)!;
        var numTextures1 = DoomConvert.ToInt32(mapTex[..4]);

        var maxOff = DoomGame.Instance.WadData.LumpLength(DoomGame.Instance.WadData.GetNumForName("TEXTURE1"));
        var directory = 4;

        byte[]? mapTex2 = null;
        var numTextures2 = 0;
        var maxOff2 = 0;
        if (DoomGame.Instance.WadData.CheckNumForName("TEXTURE2") != -1)
        {
            mapTex2 = DoomGame.Instance.WadData.GetLumpName("TEXTURE2", PurgeTag.Static)!;
            numTextures2 = DoomConvert.ToInt32(mapTex2[..4]);
            maxOff2 = DoomGame.Instance.WadData.LumpLength(DoomGame.Instance.WadData.GetNumForName("TEXTURE2"));
        }

        _numTextures = numTextures1 + numTextures2;

        _textures = new Texture[_numTextures];

        _textureColumnLump = new short[_numTextures][];
        _textureColumnOfs = new ushort[_numTextures][];
        _textureComposite = new byte[_numTextures][];
        _textureCompositeSize = new int[_numTextures];
        _textureWidthMask = new int[_numTextures];
        TextureHeight = new Fixed[_numTextures];

        var totalWidth = 0;

        //	Really complex printing shit...
        var temp1 = DoomGame.Instance.WadData.GetNumForName("S_START"); // P_???????
        var temp2 = DoomGame.Instance.WadData.GetNumForName("S_END") - 1;
        var temp3 = ((temp2 - temp1 + 63) / 64) + ((_numTextures + 63) / 64);
        DoomGame.Console.Write("[");
        //for (var i = 0; i < temp3; i++)
        //{
        //    DoomGame.Console.Write(" ");
        //}

        //DoomGame.Console.Write("         ]");
        //for (var i = 0; i < temp3; i++)
        //{
        //    DoomGame.Console.Write("\b");
        //}

        //DoomGame.Console.Write("\b\b\b\b\b\b\b\b\b\b");

        for (var i = 0; i < _numTextures; i++, directory += 4)
        {
            if ((i & 63) == 0)
            {
                DoomGame.Console.Write(".");
            }

            if (i == numTextures1 && mapTex2 != null)
            {
                // Start looking in the second texture file.
                mapTex = mapTex2;
                maxOff = maxOff2;
                directory = 4;
            }

            var offset = DoomConvert.ToInt32(mapTex[directory..(directory + 4)]);
            if (offset > maxOff)
            {
                DoomGame.Error("R_InitTextures: bad texture directory");
                return;
            }

            var mapTexture = MapTexture.FromBytes(mapTex[offset..(offset + MapTexture.BaseSize)]);
            mapTexture.ReadMapPatches(mapTex[(offset + MapTexture.BaseSize)..(offset + mapTexture.Size)]);

            _textures[i] = new Texture(mapTexture.Name, mapTexture.Width, mapTexture.Height, mapTexture.PatchCount);

            var texture = _textures[i];
            for (var j = 0; j < texture.PatchCount; j++)
            {
                var mpatch = mapTexture.Patches[j];
                texture.Patches[j] = new TexturePatch(mpatch.OriginX, mpatch.OriginY, patchLookup[mpatch.Patch]);
                if (texture.Patches[j].Patch == -1)
                {
                    DoomGame.Error($"R_InitTextures: Missing patch in texture {texture.Name}");
                    return;
                }
            }

            _textureColumnLump[i] = new short[texture.Width];
            _textureColumnOfs[i] = new ushort[texture.Width];

            var k = 1;
            while (k * 2 <= texture.Width)
            {
                k <<= 1;
            }

            _textureWidthMask[i] = k - 1;
            TextureHeight[i] = new Fixed(texture.Height << Constants.FracBits);
            totalWidth += texture.Width;
        }

        // Precalculate whatever possible.	
        for (var i = 0; i < _numTextures; i++)
        {
            GenerateLookup(i);
        }

        // Create translation table for global animation.
        _textureTranslation = new int[_numTextures + 1];
        for (var i = 0; i < _numTextures; i++)
        {
            _textureTranslation[i] = i;
        }

        DoomGame.Console.Write("]");
    }

    /// <summary>
    /// Clip and draw a column
    ///  from a patch into a cached post.
    /// </summary>
    private void DrawColumnInCache(Column? patch, byte[] cache, ushort cacheOffset, int originY, int cacheHeight)
    {
        if (patch is null) 
        {
            return; 
        }

        while (patch.TopDelta != 0xff)
        {
            var count = (int)patch.Length;
            var position = originY + patch.TopDelta;

            if (position < 0)
            {
                count += position;
                position = 0;
            }

            if (position + count > cacheHeight)
            {
                count = cacheHeight - position;
            }

            if (count > 0)
            {
                Array.Copy(patch.Pixels, 0, cache, cacheOffset + position, count);
            }

            patch = patch.Next ?? new Column(0xff, 0, Array.Empty<byte>());
        }
    }

    /// <summary>
    /// Using the texture definition,
    ///  the composite texture is created from the patches,
    ///  and each column is cached.
    /// </summary>
    private void GenerateComposite(int texNum)
    {
        var texture = _textures[texNum];

        var block = new byte[_textureCompositeSize[texNum]];
        _textureComposite[texNum] = block;
        
        var colLump = _textureColumnLump[texNum];
        var colOfs = _textureColumnOfs[texNum];

        // Composite the columns together.
        for (var i = 0; i < texture.PatchCount; i++)
        {
            var patch = texture.Patches[i];
            var realPatch = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpNum(patch.Patch, PurgeTag.Cache)!);
            var x1 = patch.OriginX;
            var x2 = x1 + realPatch.Width;

            var x = x1 < 0 ? 0 : x1;

            if (x2 > texture.Width)
            {
                x2 = texture.Width;
            }

            for (; x < x2; x++)
            {
                // Column does not have multiple patches?
                if (colLump[x] >= 0)
                {
                    continue;
                }

                var patchCol = realPatch.Columns[x - x1];
                DrawColumnInCache(patchCol, block, colOfs[x], patch.OriginY, texture.Height);
            }
        }

        // Now that the texture has been built in column cache,
        //  it is purgable from zone memory.
        //Z_ChangeTag(block, PU_CACHE);
    }

    private void GenerateLookup(int texNum)
    {
        var texture = _textures[texNum];

        // Composited texture not created yet.
        _textureComposite[texNum] = null;

        _textureCompositeSize[texNum] = 0;
        var colLump = _textureColumnLump[texNum];
        var colOfs = _textureColumnOfs[texNum];

        // Now count the number of columns
        //  that are covered by more than one patch.
        // Fill in the lump / offset, so columns
        //  with only a single patch are all done.
        var patchCount = new byte[texture.Width];
        var x = 0;

        for (var i = 0; i < texture.PatchCount; i++)
        {
            var patch = texture.Patches[i];
            var realPatch = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpNum(patch.Patch, PurgeTag.Cache)!);
            var x1 = patch.OriginX;
            var x2 = x1 + realPatch.Width;

            x = x1 < 0 ? 0 : x1;

            if (x2 > texture.Width)
            {
                x2 = texture.Width;
            }

            for (; x < x2; x++)
            {
                patchCount[x]++;
                colLump[x] = (short)patch.Patch;
                colOfs[x] = (ushort)realPatch.ColumnOffsets[x - x1];
            }
        }

        for (x = 0; x < texture.Width; x++)
        {
            if (patchCount[x] == 0)
            {
                DoomGame.Console.WriteLine($"R_GenerateLookup: column without a patch ({texture.Name})");
                return;
            }

            // I_Error ("R_GenerateLookup: column without a patch");

            if (patchCount[x] > 1)
            {
                // Use the cached block.
                colLump[x] = -1;
                colOfs[x] = (ushort)_textureCompositeSize[texNum];

                if (_textureCompositeSize[texNum] > 0x10000 - texture.Height)
                {
                    DoomGame.Error($"R_GenerateLookup: texture {texNum} is >64k");
                    return;
                }

                _textureCompositeSize[texNum] += texture.Height;
            }
        }
    }

    private void InitFlats()
    {
        _firstFlat = DoomGame.Instance.WadData.GetNumForName("F_START") + 1;
        _lastFlat = DoomGame.Instance.WadData.GetNumForName("F_END") - 1;
        _numFlats = _lastFlat - _firstFlat + 1;

        // Create translation table for global animation.
        _flatTranslation = new int[_numFlats + 1];

        for (var i = 0; i < _numFlats; i++)
        {
            _flatTranslation[i] = i;
        }
    }

    //
    // R_InitSpriteLumps
    // Finds the width and hoffset of all sprites in the wad,
    //  so the sprite does not need to be cached completely
    //  just for having the header info ready during rendering.
    //
    private void InitSpriteLumps()
    {
        _firstSpriteLump = DoomGame.Instance.WadData.GetNumForName("S_START") + 1;
        _lastSpriteLump = DoomGame.Instance.WadData.GetNumForName("S_END") - 1;

        _numSpriteLumps = _lastSpriteLump - _firstSpriteLump + 1;
        _spriteWidth = new int[_numSpriteLumps];
        _spriteOffset = new int[_numSpriteLumps];
        _spriteTopOffset = new int[_numSpriteLumps];

        for (var i = 0; i < _numSpriteLumps; i++)
        {
            if ((i & 63) != 0)
            {
                DoomGame.Console.Write(".");
            }

            var patchData = DoomGame.Instance.WadData.GetLumpNum(_firstSpriteLump + i, PurgeTag.Cache);
            var patch = Patch.FromBytes(patchData!);
            _spriteWidth[i] = patch.Width;
            _spriteOffset[i] = patch.LeftOffset;
            _spriteTopOffset[i] = patch.TopOffset;
        }
    }

    private void InitColorMaps()
    {
        // Load in the light tables, 
        //  256 byte align tables.
        var lump = DoomGame.Instance.WadData.GetNumForName("COLORMAP");
        var length = DoomGame.Instance.WadData.LumpLength(lump) + 255;

        // Something weird is happening here in the original code:
        /*
        
        colormaps = Z_Malloc (length, PU_STATIC, 0); 
        colormaps = (byte *)( ((int)colormaps + 255)&~0xff); 
         */

        _colorMaps = DoomGame.Instance.WadData.GetLumpNum(lump, PurgeTag.Cache)!;
    }

    public int FlatNumForName(string name)
    {
        var i = DoomGame.Instance.WadData.CheckNumForName(name);
        if (i == -1)
        {
            DoomGame.Error($"R_FlatNumForName: {name} not found");
            return -1;
        }

        return i - _firstFlat;
    }

    public int TextureNumForName(string name)
    {
        var i = CheckTextureNumForName(name);
        if (i == -1)
        {
            DoomGame.Error($"R_TextureNumForName: {name} not found");
            return -1;
        }

        return i;
    }

    public int CheckTextureNumForName(string name)
    {
        // "NoTexture" marker.
        if (name[0] == '-')
        {
            return 0;
        }

        for (var i = 0; i < _numTextures; i++)
        {
            if (string.Equals(_textures[i].Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Do not really change anything here,
    ///  because it might be in the middle of a refresh.
    /// The change will take effect next refresh.
    /// </summary>
    public void SetViewSize(int blocks, bool highDetail)
    {
        SetSizeNeeded = true;
        _setBlocks = blocks;
        _setHighDetail = highDetail;
    }

    public void ExecuteSetViewSize()
    {
        if (!SetSizeNeeded)
        {
            return;
        }

        SetSizeNeeded = false;

        if (_setBlocks == 11)
        {
            ScaledViewWidth = Constants.ScreenWidth;
            ViewHeight = Constants.ScreenHeight;
        }
        else
        {
            ScaledViewWidth = _setBlocks * 32;
            ViewHeight = (_setBlocks * 168 / 10) & ~7;
        }

        _detailShift = _setHighDetail ? 0 : 1;
        ViewWidth = ScaledViewWidth >> _detailShift;

        _centerY = ViewHeight / 2;
        _centerX = ViewWidth / 2;
        _centerXFrac = Fixed.FromInt(_centerX);
        _centerYFrac = Fixed.FromInt(_centerY);
        _projection = _centerXFrac;

        if (_detailShift == 0)
        {
            _colFunc = _baseColFunc = DrawColumn;
            _fuzzColFunc = DrawFuzzColumn;
            _transColFunc = DrawTranslatedColumn;
            _spanFunc = DrawSpan;
        }
        else
        {
            _colFunc = _baseColFunc = DrawColumnLow;
            _fuzzColFunc = DrawFuzzColumn;
            _transColFunc = DrawTranslatedColumn;
            _spanFunc = DrawSpanLow;
        }

        InitBuffer(ScaledViewWidth, ViewHeight);

        InitTextureMapping();

        // psprite scales
        _pSpriteScale =  Fixed.FromInt(ViewWidth / Constants.ScreenWidth);
        _pSpriteIScale = Fixed.FromInt(Constants.ScreenWidth / ViewWidth);

        // thing clipping
        for (var i = 0; i < ViewWidth; i++)
        {
            _screenHeightArray[i] = (short)ViewHeight;
        }

        // planes
        for (var i = 0; i < ViewHeight; i++)
        {
            var dy = Fixed.FromInt(i - ViewHeight / 2) + Fixed.Unit / 2;
            dy = Fixed.Abs(dy);
            _ySlope[i] = Fixed.FromInt((ViewWidth << _detailShift) / 2) / dy;
        }

        for (var i = 0; i < ViewWidth; i++)
        {
            var cosAdj = Fixed.Abs(DoomMath.Cos(_xToViewAngle[i]));
            _distScale[i] = Fixed.Unit / cosAdj;
        }

        // Calculate the light levels to use
        //  for each level / scale combination.
        for (var i = 0; i < LightLevels; i++)
        {
            var startMap = ((LightLevels - 1 - i) * 2) * NumColorMaps / LightLevels;
            for (var j = 0; j < MaxLightScale; j++)
            {
                var level = startMap - j * Constants.ScreenWidth / (ViewWidth << _detailShift) / DistMap;

                if (level < 0)
                {
                    level = 0;
                }

                if (level >= NumColorMaps)
                {
                    level = NumColorMaps - 1;
                }

                _scaleLight[i][j] = level * 256;
            }
        }
    }

    // Only inits the zlight table,
    //  because the scalelight table changes with view size.
    public const int DistMap = 2;

    private void InitLightTables()
    {
        // Calculate the light levels to use
        //  for each level / distance combination.
        for (var i = 0; i < LightLevels; i++)
        {
            var startMap = ((LightLevels - 1 - i) * 2) * NumColorMaps / LightLevels;
            for (var j = 0; j < MaxLightZ; j++)
            {
                var scale = Fixed.FromInt(Constants.ScreenWidth / 2) / new Fixed((j + 1) << LightZShift);
                scale >>= LightScaleShift;
                var level = startMap - scale.Value / DistMap;

                if (level < 0)
                {
                    level = 0;
                }

                if (level >= NumColorMaps)
                {
                    level = NumColorMaps - 1;
                }

                _zLight[i][j] = level * 256;
            }
        }
    }

    public void Initialize()
    {
        InitData();
        DoomGame.Console.Write($"{Environment.NewLine}R_InitData");
        // R_InitPointToAngle(); // Unused, there is a fixed table instead
        DoomGame.Console.Write($"{Environment.NewLine}R_InitPointToAngle");
        // R_InitTables(); // Unused, there is a fixed table instead
        // viewwidth / viewheight / detailLevel are set by the defaults
        DoomGame.Console.Write($"{Environment.NewLine}R_InitTables");

        SetViewSize(DoomGame.Instance.Menu.ScreenBlocks, DoomGame.Instance.Menu.DetailLevel);
        // InitPlanes(); // Doesn't do anything
        DoomGame.Console.Write($"{Environment.NewLine}R_InitPlanes");
        InitLightTables();
        DoomGame.Console.Write($"{Environment.NewLine}R_InitLightTables");
        Sky.InitSkyMap();
        DoomGame.Console.Write($"{Environment.NewLine}R_InitSkyMap");
        InitTranslationTables();
        DoomGame.Console.Write($"{Environment.NewLine}R_InitTranslationTables");

        _frameCount = 0;
    }

    //
    // R_PrecacheLevel
    // Preloads all relevant graphics for the level.
    //
    private int _flatMemory;
    private int _textureMemory;
    private int _spriteMemory;

    public void PreCacheLevel()
    {
        var game = DoomGame.Instance.Game;
        if (game.DemoPlayback)
        {
            return;
        }

        // Create an array to track item presence, large enough to hold the largest cache target.
        var arraySize = Math.Max(Math.Max(_numFlats, _numTextures), _numSprites);
        var presence = new bool[arraySize];

        // Precache flats.
        for (var i = 0; i < game.NumSectors; i++)
        {
            presence[game.Sectors[i].FloorPic] = true;
            presence[game.Sectors[i].CeilingPic] = true;
        }

        _flatMemory = 0;
        for (var i = 0; i < _numFlats; i++)
        {
            if (!presence[i])
            {
                continue;
            }

            var lump = _firstFlat + i;
            _flatMemory += DoomGame.Instance.WadData[lump].Lump.Size;
            DoomGame.Instance.WadData.GetLumpNum(lump, PurgeTag.Cache); // Preload, so we're not using the result of this method
        }

        // Precache textures
        Array.Clear(presence); // reuse the array

        for (var i = 0; i < game.NumSides; i++)
        {
            presence[game.Sides[i].TopTexture] = true;
            presence[game.Sides[i].MidTexture] = true;
            presence[game.Sides[i].BottomTexture] = true;
        }

        // Sky texture is always present.
        // Note that F_SKY1 is the name used to
        //  indicate a sky floor/ceiling as a flat,
        //  while the sky texture is stored like
        //  a wall texture, with an episode dependend
        //  name.
        presence[Sky.Texture] = true;

        _textureMemory = 0;
        for (var i = 0; i < _numTextures; i++)
        {
            if (!presence[i])
            {
                continue;
            }

            var texture = _textures[i];

            for (var j = 0; j < texture.PatchCount; j++)
            {
                var lump = texture.Patches[j].Patch;
                _textureMemory += DoomGame.Instance.WadData[lump].Lump.Size;
                DoomGame.Instance.WadData.GetLumpNum(lump, PurgeTag.Cache);
            }
        }

        // Precache sprites.
        Array.Clear(presence);

        foreach (var thinker in game.Thinkers)
        {
            if (thinker is MapObject mo)
            {
                presence[(int)mo.Sprite] = true;
            }
        }

        _spriteMemory = 0;
        for (var i = 0; i < _numSprites; i++)
        {
            if (!presence[i])
            {
                continue;
            }

            for (var j = 0; j < _sprites[i].NumFrames; j++)
            {
                var sf = _sprites[i].Frames[j];
                for (var k = 0; k < 8; k++)
                {
                    var lump = _firstSpriteLump + sf.Lumps[k];
                    _spriteMemory += DoomGame.Instance.WadData[lump].Lump.Size;
                    DoomGame.Instance.WadData.GetLumpNum(lump, PurgeTag.Cache);
                }
            }
        }
    }

    private void InstallSpriteLump(int lump, int frame, int rotation, bool flipped)
    {
        if (frame >= 29 || rotation > 8)
        {
            DoomGame.Error($"R_InstallSpriteLump: Bad frame characters in lump {lump}");
            return;
        }

        if (frame > _maxFrame)
        {
            _maxFrame = frame;
        }

        if (rotation == 0)
        {
            // the lump should be used for all rotations
            if (_spriteTemp[frame].Rotate == false)
            {
                DoomGame.Error($"R_InitSprites: Sprite {_spriteName} frame {'A' + frame} has multip rot=0 lump");
                return;
            }

            if (_spriteTemp[frame].Rotate == true)
            {
                DoomGame.Error($"R_InitSprites: Sprite {_spriteName} frame {'A' + frame} has rotations and a rot=0 lump");
                return;
            }

            _spriteTemp[frame].Rotate = false;
            for (var r = 0; r < 8; r++)
            {
                _spriteTemp[frame].Lumps[r] = (short)(lump - _firstSpriteLump);
                _spriteTemp[frame].Flip[r] = flipped;
            }

            return;
        }

        // the lump is only used for one rotation
        if (_spriteTemp[frame].Rotate == false)
        {
            DoomGame.Error($"R_InitSprites: Sprite {_spriteName} frame {'A' + frame} has rotations and a rot=0 lump");
            return;
        }

        _spriteTemp[frame].Rotate = true;

        // make 0 based
        rotation--;
        if (_spriteTemp[frame].Lumps[rotation] != -1)
        {
            DoomGame.Error($"R_InitSprites: Sprite {_spriteName} : {'A' + frame} : {'1' + rotation} has two lumps mapped to it");
            return;
        }

        _spriteTemp[frame].Lumps[rotation] = (short)(lump - _firstSpriteLump);
        _spriteTemp[frame].Flip[rotation] = flipped;
    }

    /// <summary>
    /// Pass a null terminated list of sprite names
    ///  (4 chars exactly) to be used.
    /// Builds the sprite rotation matrices to account
    ///  for horizontally flipped sprites.
    /// Will report an error if the lumps are inconsistent. 
    /// Only called at startup.
    ///
    /// Sprite lump names are 4 characters for the actor,
    ///  a letter for the frame, and a number for the rotation.
    /// A sprite that is flippable will have an additional
    ///  letter/number appended.
    /// The rotation character can be 0 to signify no rotations.
    /// </summary>
    /// <param name="spriteNames"></param>
    private void InitSpriteDefs(string[] spriteNames)
    {
        _numSprites = spriteNames.Length;
        if (_numSprites == 0)
        {
            _sprites = Array.Empty<Sprite>();
            return;
        }

        _sprites = new Sprite[_numSprites];

        var start = _firstSpriteLump - 1;
        var end = _lastSpriteLump + 1;

        // scan all the lump names for each of the names,
        //  noting the highest frame letter.
        // Just compare 4 characters 
        for (var i = 0; i < _numSprites; i++)
        {
            _spriteName = spriteNames[i];
            Array.Clear(_spriteTemp);
            for (var j = 0; j < _spriteTemp.Length; j++)
            {
                _spriteTemp[j] = new SpriteFrame();
            }

            _maxFrame = -1;

            _sprites[i] = new Sprite();

            // scan the lumps,
            // filling in the frames for whatever is found
            for (var l = start + 1; l < end; l++)
            {
                var lumpName = DoomGame.Instance.WadData[l].Lump.Name;
                if (string.Compare(lumpName, 0, _spriteName, 0, 4, StringComparison.Ordinal) == 0)
                {
                    var frame = lumpName[4] - 'A';
                    var rotation = lumpName[5] - '0';

                    var patched = l;
                    if (DoomGame.Instance.ModifiedGame)
                    {
                        patched = DoomGame.Instance.WadData.GetNumForName(lumpName);
                    }

                    InstallSpriteLump(patched, frame, rotation, false);

                    if (lumpName.Length > 6)
                    {
                        frame = lumpName[6] - 'A';
                        rotation = lumpName[7] - '0';
                        InstallSpriteLump(patched, frame, rotation, true);
                    }
                }
            }

            // check the frames that were found for completeness
            if (_maxFrame == -1)
            {
                _sprites[i].NumFrames = 0;
                continue;
            }

            _maxFrame++;

            for (var frame = 0; frame < _maxFrame; frame++)
            {
                switch (_spriteTemp[frame].Rotate)
                {
                    case null:
                        // no rotations were found for that frame at all
                        DoomGame.Error($"R_InitSprites: No patches found for {spriteNames[i]} frame {'A' + frame}");
                        break;

                    case false:
                        // only the first rotation is needed
                        break;

                    case true:
                        // must have all 8 frames
                        for (var rotation = 0; rotation < 8; rotation++)
                        {
                            if (_spriteTemp[frame].Lumps[rotation] == -1)
                            {
                                DoomGame.Error($"R_InitSprites: Sprite {spriteNames[i]} frame {'A' + frame} is missing rotations");
                            }
                        }

                        break;
                }
            }

            // allocate space for the frames present and copy sprtemp to it
            _sprites[i].NumFrames = _maxFrame;
            _sprites[i].Frames = new SpriteFrame[_maxFrame];
            Array.Copy(_spriteTemp, 0, _sprites[i].Frames, 0, _maxFrame);
        }
    }

    public void InitSprites(string[] spriteNames)
    {
        for (var i = 0; i < Constants.ScreenWidth; i++)
        {
            _negoneArray[i] = -1;
        }

        InitSpriteDefs(spriteNames);
    }

    private void InitTextureMapping()
    {
        // Use tangent table to generate viewangletox:
        //  viewangletox will give the next greatest x
        //  after the view angle.
        //
        // Calc focallength
        //  so FIELDOFVIEW angles covers SCREENWIDTH.
        var focalLength = _centerXFrac / DoomMath.Tan(DoomMath.FineAngleCount / 4 + FieldOfView / 2);
        int i, x, t;

        for (i = 0; i < DoomMath.FineAngleCount / 2; i++)
        {
            if (DoomMath.Tan(i) > Fixed.FromInt(2))
            {
                t = -1;
            }
            else if (DoomMath.Tan(i) < Fixed.FromInt(-2))
            {
                t = ViewWidth + 1;
            }
            else
            {
                t = ((_centerXFrac - DoomMath.Tan(i) * focalLength).Value + Constants.FracUnit - 1) >> Constants.FracBits;

                if (t < -1)
                {
                    t = -1;
                }
                else if (t > ViewWidth + 1)
                {
                    t = ViewWidth + 1;
                }
            }

            _viewAngleToX[i] = t;
        }

        // Scan viewangletox[] to generate xtoviewangle[]:
        //  xtoviewangle will give the smallest view angle
        //  that maps to x.	
        for (x = 0; x <= ViewWidth; x++)
        {
            i = 0;
            while (_viewAngleToX[i] > x)
            {
                i++;
            }

            _xToViewAngle[x] = new Angle((uint)(i << DoomMath.AngleToFineShift)) - Angle.Angle90;
        }

        // Take out the fencepost cases from viewangletox.
        for (i = 0; i < DoomMath.FineAngleCount / 2; i++)
        {
            if (_viewAngleToX[i] == -1)
            {
                _viewAngleToX[i] = 0;
            }
            else if (_viewAngleToX[i] == ViewWidth + 1)
            {
                _viewAngleToX[i] = ViewWidth;
            }
        }

        _clipAngle = _xToViewAngle[0];
    }

    // Drawing methods

    //
    // A column is a vertical slice/span from a wall texture that,
    //  given the DOOM style restrictions on the view orientation,
    //  will always have constant z depth.
    // Thus a special case loop for very fast rendering can
    //  be used. It has also been used with Wolfenstein 3D.
    // 
    private void DrawColumn()
    {
        var count = _dcYh - _dcYl;

        // Zero length, column does not exceed a pixel.
        if (count < 0)
        {
            return;
        }

        //if (_dcX >= Constants.ScreenWidth || _dcYl < 0 || _dcYh >= Constants.ScreenHeight)
        //{
        //    DoomGame.Error($"R_DrawColumn: {_dcYl} to {_dcYh} at {_dcX}");
        //    return;
        //}

        if (_dcSource is null)
        {
            return;
        }

        // Framebuffer destination address.
        // Use ylookup LUT to avoid multiply with ScreenWidth.
        // Use columnofs LUT for subwindows? 
        var dest = _yLookup[_dcYl] + _columnOfs[_dcX];

        // Determine scaling,
        //  which is the only mapping to be done.
        var fracStep = _dcIScale;
        var frac = _dcTextureMid + ((_dcYl - _centerY) * fracStep);

        // Inner loop that does the actual texture mapping,
        //  e.g. a DDA-lile scaling.
        // This is as fast as it gets.
        do
        {
            // Re-map color indices from wall texture column
            //  using a lighting/special effects LUT.
            var sourceIdx = (frac.Value >> Constants.FracBits) & 127;
            if (sourceIdx == _dcSource.Length)
            {
                sourceIdx--; // simulate patches repeating the last pixel byte
            }

            DoomGame.Instance.Video.Screens[0][dest] = _dcColorMap[_dcSource[sourceIdx % _dcSource.Length]];

            dest += Constants.ScreenWidth;
            frac += fracStep;

        } while (count-- != 0);
    }

    private void DrawColumnLow()
    {
        var count = _dcYh - _dcYl;

        // Zero length, column does not exceed a pixel.
        if (count < 0)
        {
            return;
        }

        //if (_dcX >= Constants.ScreenWidth || _dcYl < 0 || _dcYh >= Constants.ScreenHeight)
        //{
        //    DoomGame.Error($"R_DrawColumn: {_dcYl} to {_dcYh} at {_dcX}");
        //    return;
        //}

        if (_dcSource is null)
        {
            return;
        }

        // Blocky mode, need to multiply by 2.
        _dcX <<= 1;

        var dest = _yLookup[_dcYl] + _columnOfs[_dcX];
        var dest2 = _yLookup[_dcYl] + _columnOfs[_dcX + 1];

        var fracStep = _dcIScale;
        var frac = _dcTextureMid + ((_dcYl - _centerY) * fracStep);

        do
        {
            var sourceIdx = (frac.Value >> Constants.FracBits) & 127;
            if (sourceIdx == _dcSource.Length)
            {
                sourceIdx--; // simulate patches repeating the last pixel byte
            }

            DoomGame.Instance.Video.Screens[0][dest2] =
                DoomGame.Instance.Video.Screens[0][dest] = _dcColorMap[_dcSource[sourceIdx % _dcSource.Length]];

            dest += Constants.ScreenWidth;
            dest2 += Constants.ScreenWidth;
            frac += fracStep;
        } while (count-- != 0);
    }

    public const int FuzzTable = 50;
    public const int FuzzOff = Constants.ScreenWidth;

    private int _fuzzPos = 0;
    private static readonly int[] _fuzzOffset =
    {
        FuzzOff,-FuzzOff,FuzzOff,-FuzzOff,FuzzOff,FuzzOff,-FuzzOff,
        FuzzOff,FuzzOff,-FuzzOff,FuzzOff,FuzzOff,FuzzOff,-FuzzOff,
        FuzzOff,FuzzOff,FuzzOff,-FuzzOff,-FuzzOff,-FuzzOff,-FuzzOff,
        FuzzOff,-FuzzOff,-FuzzOff,FuzzOff,FuzzOff,FuzzOff,FuzzOff,-FuzzOff,
        FuzzOff,-FuzzOff,FuzzOff,FuzzOff,-FuzzOff,-FuzzOff,FuzzOff,
        FuzzOff,-FuzzOff,-FuzzOff,-FuzzOff,-FuzzOff,FuzzOff,FuzzOff,
        FuzzOff,FuzzOff,-FuzzOff,FuzzOff,FuzzOff,-FuzzOff,FuzzOff
    };

    //
    // Framebuffer postprocessing.
    // Creates a fuzzy image by copying pixels
    //  from adjacent ones to left and right.
    // Used with an all black colormap, this
    //  could create the SHADOW effect,
    //  i.e. spectres and invisible players.
    //
    private void DrawFuzzColumn()
    {
        // Adjust borders. Low... 
        if (_dcYl == 0)
        {
            _dcYl = 1;
        }

        // .. and high.
        if (_dcYh == ViewHeight - 1)
        {
            _dcYh = ViewHeight - 2;
        }

        var count = _dcYh - _dcYl;

        // Zero length.
        if (count < 0)
        {
            return;
        }

        //if (_dcX >= Constants.ScreenWidth || _dcYl < 0 || _dcYh >= Constants.ScreenHeight)
        //{
        //    DoomGame.Error($"R_DrawFuzzColumn: {_dcYl} to {_dcYh} at {_dcX}");
        //    return;
        //}

        // Keep till detailshift bug in blocky mode fixed,
        //  or blocky mode removed.
        /* WATCOM code 
        if (detailshift)
        {
        if (dc_x & 1)
        {
            outpw (GC_INDEX,GC_READMAP+(2<<8) ); 
            outp (SC_INDEX+1,12); 
        }
        else
        {
            outpw (GC_INDEX,GC_READMAP); 
            outp (SC_INDEX+1,3); 
        }
        dest = destview + dc_yl*80 + (dc_x>>1); 
        }
        else
        {
        outpw (GC_INDEX,GC_READMAP+((dc_x&3)<<8) ); 
        outp (SC_INDEX+1,1<<(dc_x&3)); 
        dest = destview + dc_yl*80 + (dc_x>>2); 
        }*/


        // Does not work with blocky mode.
        var dest = _yLookup[_dcYl] + _columnOfs[_dcX];

        // Looks familiar.
        var fracStep = _dcIScale;
        var frac = _dcTextureMid + ((_dcYl - _centerY) * fracStep);

        // Looks like an attempt at dithering,
        //  using the colormap #6 (of 0-31, a bit
        //  brighter than average).
        do
        {
            // Lookup framebuffer, and retrieve
            //  a pixel that is either one column
            //  left or right of the current one.
            // Add index from colormap to index.

            var pixel = DoomGame.Instance.Video.Screens[0][dest + _fuzzOffset[_fuzzPos]];
            DoomGame.Instance.Video.Screens[0][dest] = _colorMaps[6 * 256 + pixel];

            // Clamp table lookup index.
            if (++_fuzzPos >= FuzzTable)
            {
                _fuzzPos = 0;
            }

            dest += Constants.ScreenWidth;

            frac += fracStep;
        } while (count-- != 0);
    }

    //
    // R_DrawTranslatedColumn
    // Used to draw player sprites
    //  with the green colorramp mapped to others.
    // Could be used with different translation
    //  tables, e.g. the lighter colored version
    //  of the BaronOfHell, the HellKnight, uses
    //  identical sprites, kinda brightened up.
    //
    private readonly byte[] _dcTranslation = new byte[256];
    private byte[] _translationTables = Array.Empty<byte>();

    private void DrawTranslatedColumn()
    {
        var count = _dcYh - _dcYl;
        if (count < 0)
        {
            return;
        }

        //if (_dcX >= Constants.ScreenWidth || _dcYl < 0 || _dcYh > Constants.ScreenHeight)
        //{
        //    DoomGame.Error($"R_DrawColumn: {_dcYl} to {_dcYh} at {_dcX}");
        //    return;
        //}

        if (_dcSource is null)
        {
            return;
        }

        // WATCOM VGA specific.
        /* Keep for fixing.
        if (detailshift)
        {
        if (dc_x & 1)
            outp (SC_INDEX+1,12); 
        else
            outp (SC_INDEX+1,3);

        dest = destview + dc_yl*80 + (dc_x>>1); 
        }
        else
        {
        outp (SC_INDEX+1,1<<(dc_x&3)); 
        dest = destview + dc_yl*80 + (dc_x>>2); 
        }*/


        // FIXME. As above.
        var dest = _yLookup[_dcYl] + _columnOfs[_dcX];

        // Looks familiar.
        var fracStep = _dcIScale;
        var frac = _dcTextureMid + ((_dcYl - _centerY) * fracStep);

        // Here we do an additional index re-mapping.
        do
        {
            // Translation tables are used
            //  to map certain colorramps to other ones,
            //  used with PLAY sprites.
            // Thus the "green" ramp of the player 0 sprite
            //  is mapped to gray, red, black/indigo. 
            var sourceIdx = frac.Value >> Constants.FracBits;
            if (sourceIdx == _dcSource.Length)
            {
                sourceIdx--; // simulate patches repeating the last pixel byte
            }

            DoomGame.Instance.Video.Screens[0][dest] = _dcColorMap[_dcTranslation[_dcSource[sourceIdx]]];
            dest += Constants.ScreenWidth;

            frac += fracStep;
        } while (count-- != 0);
    }

    //
    // R_InitTranslationTables
    // Creates the translation tables to map
    //  the green color ramp to gray, brown, red.
    // Assumes a given structure of the PLAYPAL.
    // Could be read from a lump instead.
    //
    private void InitTranslationTables()
    {
        _translationTables = new byte[256 * 3 + 255];
        // _translationTables = (byte*)(((int)translationtables + 255) & ~255);

        // translate just the 16 green colors
        for (var i = 0; i < 256; i++)
        {
            if (i is >= 0x70 and <= 0x7f)
            {
                // map green ramp to gray, brown, red
                _translationTables[i] = (byte)(0x60 + (i & 0xf));
                _translationTables[i + 256] = (byte)(0x40 + (i & 0xf));
                _translationTables[i + 512] = (byte)(0x20 + (i & 0xf));
            }
            else
            {
                // Keep all other colors as is.
                _translationTables[i] = _translationTables[i + 256] = _translationTables[i + 512] = (byte)i;
            }
        }
    }

    //
    // R_DrawSpan 
    // With DOOM style restrictions on view orientation,
    //  the floors and ceilings consist of horizontal slices
    //  or spans with constant z depth.
    // However, rotation around the world z axis is possible,
    //  thus this mapping, while simpler and faster than
    //  perspective correct texture mapping, has to traverse
    //  the texture at an angle in all but a few cases.
    // In consequence, flats are not stored by column (like walls),
    //  and the inner loop has to step in texture space u and v.
    //
    
    //
    // Draws the actual span.
    private void DrawSpan()
    {
        //if (_dsX2 < _dsX1 || _dsX1 < 0 || _dsX2 >= Constants.ScreenWidth || _dsY > Constants.ScreenHeight)
        //{
        //    DoomGame.Error($"R_DrawSpan: {_dsX1} to {_dsX2} at {_dsY}");
        //    return;
        //}

        var xFrac = _dsXFrac;
        var yFrac = _dsYFrac;

        var dest = _yLookup[_dsY] + _columnOfs[_dsX1];

        // We do not check for zero spans here?
        var count = _dsX2 - _dsX1;

        do
        {
            // Current texture index in u,v.
            var spot = ((yFrac.Value >> (16 - 6)) & (63 * 64)) + ((xFrac.Value >> 16) & 63);
            if (spot == _dsSource!.Length)
            {
                spot--; // simulate patches repeating the last pixel byte
            }
            
            // Lookup pixel from flat texture tile,
            //  re-index using light/colormap.
            DoomGame.Instance.Video.Screens[0][dest++] = _dsColorMap[_dsSource[spot]];

            // Next step in u,v.
            xFrac += _dsXStep;
            yFrac += _dsYStep;
        } while (count-- != 0);
    }

    //
    // Again..
    //
    private void DrawSpanLow()
    {
        //if (_dsX2 < _dsX1 || _dsX1 < 0 || _dsX2 >= Constants.ScreenWidth || _dsY > Constants.ScreenHeight)
        //{
        //    DoomGame.Error($"R_DrawSpan: {_dsX1} to {_dsX2} at {_dsY}");
        //    return;
        //}

        var xFrac = _dsXFrac;
        var yFrac = _dsYFrac;

        // Blocky mode, need to multiply by 2.
        _dsX1 <<= 1;
        _dsX2 <<= 1;

        var dest = _yLookup[_dsY] + _columnOfs[_dsX1];
        var count = _dsX2 - _dsX1;
        do
        {
            var spot = ((yFrac.Value >> (16 - 6)) & (63 * 64)) + ((xFrac.Value >> 16) & 63);
            if (spot == _dsSource!.Length)
            {
                spot--; // simulate patches repeating the last pixel byte
            }

            // Lowres/blocky mode does it twice,
            //  while scale is adjusted appropriately.
            DoomGame.Instance.Video.Screens[0][dest++] = _dsColorMap[_dsSource[spot]];
            DoomGame.Instance.Video.Screens[0][dest++] = _dsColorMap[_dsSource[spot]];

            xFrac += _dsXStep;
            yFrac += _dsYStep;
        } while (count-- != 0);
    }

    //
    // R_InitBuffer 
    // Creats lookup tables that avoid
    //  multiplies and other hazzles
    //  for getting the framebuffer address
    //  of a pixel to draw.
    //
    private void InitBuffer(int width, int height)
    {
        // Handle resize,
        //  e.g. smaller view windows
        //  with border and/or status bar.
        ViewWindowX = (Constants.ScreenWidth - width) >> 1;

        // Column offset. For windows.
        for (var i = 0; i < width; i++)
        {
            _columnOfs[i] = ViewWindowX + i;
        }

        // Same with base row offset.
        if (width == Constants.ScreenWidth)
        {
            ViewWindowY = 0;
        }
        else
        {
            ViewWindowY = (Constants.ScreenHeight - StatusBar.Height - height) >> 1;
        }

        // Pre-calculate all row offsets.
        for (var i = 0; i < height; i++)
        {
            _yLookup[i] = (i + ViewWindowY) * Constants.ScreenWidth;
        }
    }

    //
    // R_FillBackScreen
    // Fills the back screen with a pattern
    //  for variable screen sizes
    // Also draws a beveled edge.
    //
    public void FillBackScreen()
    {
        // DOOM border patch.
        const string name1 = "FLOOR7_2";

        // DOOM II border patch.
        const string name2 = "GRNROCK";

        if (ScaledViewWidth == 320)
        {
            return;
        }

        var name = DoomGame.Instance.GameMode == GameMode.Commercial ? name2 : name1;

        var src = DoomGame.Instance.WadData.GetLumpName(name, PurgeTag.Cache)!;
        var dest = DoomGame.Instance.Video.Screens[1];
        var destIdx = 0;

        for (var y = 0; y < (Constants.ScreenHeight - StatusBar.Height); y++)
        {
            for (var x = 0; x < Constants.ScreenWidth / 64; x++)
            {
                Array.Copy(src, (y & 63) << 6, dest, destIdx, 64);
                destIdx += 64;
            }

            if ((Constants.ScreenWidth & 63) != 0)
            {
                Array.Copy(src, (y & 63) << 6, dest, destIdx, Constants.ScreenWidth & 63);
                destIdx += (Constants.ScreenWidth & 63);
            }
        }

        var patch = DoomGame.Instance.WadData.GetLumpName("brdr_t", PurgeTag.Cache)!;

        for (var x = 0; x < ScaledViewWidth; x += 8)
        {
            DoomGame.Instance.Video.DrawPatch(ViewWindowX + x, ViewWindowY - 8, 1, patch);
        }

        patch = DoomGame.Instance.WadData.GetLumpName("brdr_b", PurgeTag.Cache)!;

        for (var x = 0; x < ScaledViewWidth; x += 8)
        {
            DoomGame.Instance.Video.DrawPatch(ViewWindowX + x, ViewWindowY + ViewHeight, 1, patch);
        }

        patch = DoomGame.Instance.WadData.GetLumpName("brdr_l", PurgeTag.Cache)!;

        for (var y = 0; y < ViewHeight; y += 8)
        {
            DoomGame.Instance.Video.DrawPatch(ViewWindowX - 8, ViewWindowY + y, 1, patch);
        }

        patch = DoomGame.Instance.WadData.GetLumpName("brdr_r", PurgeTag.Cache)!;

        for (var y = 0; y < ViewHeight; y += 8)
        {
            DoomGame.Instance.Video.DrawPatch(ViewWindowX + ScaledViewWidth, ViewWindowY + y, 1, patch);
        }


        // Draw beveled edge. 
        DoomGame.Instance.Video.DrawPatch(ViewWindowX - 8,
            ViewWindowY - 8,
            1,
            DoomGame.Instance.WadData.GetLumpName("brdr_tl", PurgeTag.Cache)!);

        DoomGame.Instance.Video.DrawPatch(ViewWindowX + ScaledViewWidth,
            ViewWindowY - 8,
            1,
            DoomGame.Instance.WadData.GetLumpName("brdr_tr", PurgeTag.Cache)!);

        DoomGame.Instance.Video.DrawPatch(ViewWindowX - 8,
            ViewWindowY + ViewHeight,
            1,
            DoomGame.Instance.WadData.GetLumpName("brdr_bl", PurgeTag.Cache)!);

        DoomGame.Instance.Video.DrawPatch(ViewWindowX + ScaledViewWidth,
            ViewWindowY + ViewHeight,
            1,
            DoomGame.Instance.WadData.GetLumpName("brdr_br", PurgeTag.Cache)!);
    }

    //
    // Copy a screen buffer.
    //
    private void VideoErase(int ofs, int count)
    {
        // LFB copy.
        // This might not be a good idea if memcpy
        //  is not optimal, e.g. byte by byte on
        //  a 32bit CPU, as GNU GCC/Linux libc did
        //  at one point.
        Array.Copy(DoomGame.Instance.Video.Screens[1], ofs, DoomGame.Instance.Video.Screens[0], ofs, count);
    }

    //
    // R_DrawViewBorder
    // Draws the border around the view
    //  for different size windows?
    //
    public void DrawViewBorder()
    {
        if (ScaledViewWidth == Constants.ScreenWidth)
        {
            return;
        }

        var top = ((Constants.ScreenHeight - StatusBar.Height) - ViewHeight) / 2;
        var side = (Constants.ScreenWidth - ScaledViewWidth) / 2;

        // copy top and one line of left side 
        VideoErase(0, top * Constants.ScreenWidth + side);

        // copy one line of right side and bottom 
        var ofs = (ViewHeight + top) * Constants.ScreenWidth - side;
        VideoErase(ofs, top * Constants.ScreenWidth + side);

        // copy sides using wraparound 
        ofs = top * Constants.ScreenWidth + Constants.ScreenWidth - side;
        side <<= 1;

        for (var i = 1; i < ViewHeight; i++)
        {
            VideoErase(ofs, side);
            ofs += Constants.ScreenWidth;
        }

        // ? 
        DoomGame.Instance.Video.MarkRectangle(0, 0, Constants.ScreenWidth, Constants.ScreenHeight - StatusBar.Height);
    }

    public SubSector PointInSubSector(Fixed x, Fixed y)
    {
        // single subsector is a special case
        if (DoomGame.Instance.Game.NumNodes == 0)
        {
            return DoomGame.Instance.Game.SubSectors[0];
        }

        var nodenum = DoomGame.Instance.Game.NumNodes - 1;
        while ((nodenum & Constants.NodeLeafSubSector) == 0)
        {
            var node = DoomGame.Instance.Game.Nodes[nodenum];
            var side = PointOnSide(x, y, node);
            nodenum = node.Children[side];
        }

        return DoomGame.Instance.Game.SubSectors[nodenum & ~Constants.NodeLeafSubSector];
    }

    private void SetupFrame(Player player)
    {
        _viewPlayer = player;
        _viewX = player.MapObject!.X;
        _viewY = player.MapObject.Y;
        _viewAngle = new Angle((uint)(player.MapObject.Angle.Value + _viewAngleOffset));
        _extraLight = player.ExtraLight;

        _viewZ = player.ViewZ;

        _viewSin = DoomMath.Sin(_viewAngle);
        _viewCos = DoomMath.Cos(_viewAngle);

        _ssCount = 0;

        if (player.FixedColorMap != 0)
        {
            _fixedColorMapIdx = player.FixedColorMap * 256;
            _wallLights = _scaleLightFixed;

            for (var i = 0; i < MaxLightScale; i++)
            {
                _scaleLightFixed[i] = _fixedColorMapIdx.Value;
            }
        }
        else
        {
            _fixedColorMapIdx = null;
        }

        _frameCount++;
        ValidCount++;
    }

    public void RenderPlayerView(Player player)
    {
        SetupFrame(player);

        // Clear buffers.
        ClearClipSegs();
        ClearDrawSegs();
        ClearPlanes();
        ClearSprites();

        // check for new console commands.
        DoomGame.Instance.NetUpdate();

        // The head node is the last node output.
        RenderBSPNode(DoomGame.Instance.Game.NumNodes - 1);

        // Check for new console commands.
        DoomGame.Instance.NetUpdate();

        DrawPlanes();

        // Check for new console commands.
        DoomGame.Instance.NetUpdate();

        DrawMasked();

        // Check for new console commands.
        DoomGame.Instance.NetUpdate();
    }

    private void ClearClipSegs()
    {
        _solidSegments[0].First = -0x7fffffff;
        _solidSegments[0].Last = -1;
        _solidSegments[1].First = ViewWidth;
        _solidSegments[1].Last = 0x7fffffff;
        _newEnd = 2;
    }

    private void ClearDrawSegs()
    {
        for (var i = 0; i < _drawSegments.Length; i++)
        {
            _drawSegments[i] = null;
        }

        _drawSegIdx = 0;
    }

    private void ClearPlanes()
    {
        // opening / clipping determination
        for (var i = 0; i < ViewWidth; i++)
        {
            _floorClip[i] = (short)ViewHeight;
            _ceilingClip[i] = -1;
        }

        _lastVisPlaneIdx = 0;
        _lastOpeningIdx = 0;

        // texture calculation
        Array.Clear(_cachedHeight);

        // left to right mapping
        var angle = _viewAngle - Angle.Angle90;

        // scale will be unit scale at SCREENWIDTH/2 distance
        _baseXScale = DoomMath.Cos(angle) / _centerXFrac;
        _baseYScale = -(DoomMath.Sin(angle) / _centerXFrac);
    }

    // BSP Rendering

    // True if any of the segs textures might be visible.
    private bool _segTextured;

    // False if the back side is the same plane.
    private bool _markFloor;
    private bool _markCeiling;

    private bool _maskedTexture;
    private int _topTexture;
    private int _bottomTexture;
    private int _midTexture;

    private Angle _rwNormalAngle;

    // angle to line origin
    private Angle _rwAngle1;

    // regular wall
    private int _rwX;
    private int _rwStopX;
    private Angle _rwCenterAngle;
    private Fixed _rwOffset;
    private Fixed _rwDistance;
    private Fixed _rwScale;
    private Fixed _rwScaleStep;
    private Fixed _rwMidTextureMid;
    private Fixed _rwTopTextureMid;
    private Fixed _rwBottomTextureMid;

    private Fixed _worldTop;
    private Fixed _worldBottom;
    private Fixed _worldHigh;
    private Fixed _worldLow;

    private Fixed _pixHigh;
    private Fixed _pixLow;
    private Fixed _pixHighStep;
    private Fixed _pixLowStep;

    private Fixed _topFrac;
    private Fixed _topStep;

    private Fixed _bottomFrac;
    private Fixed _bottomStep;

    private int[]? _wallLights;
    private short[]? _internalMaskedTextureCol;

    private short[]? MaskedTextureCol
    {
        get => _internalMaskedTextureCol;
        set
        {
            MaskedTextureColIdx = null;
            if (value is { Length: > 0 })
            {
                MaskedTextureColIdx = 0;
            }

            _internalMaskedTextureCol = value;
        }
    }

    private int? MaskedTextureColIdx { get; set; }

    public const int HeightBits = 12;
    public const int HeightUnit = 1 << HeightBits;

    private void RenderMaskedSegmentRange(DrawSegment ds, int x1, int x2)
    {
        // Calculate light table.
        // Use different light tables
        //   for horizontal / vertical / diagonal. Diagonal?
        // OPTIMIZE: get rid of LIGHTSEGSHIFT globally
        _currentLine = ds.CurrentLine!;
        _frontSector = _currentLine.FrontSector;
        _backSector = _currentLine.BackSector;
        
        var texNum = _textureTranslation[_currentLine.SideDef.MidTexture];
        var lightNum = (_frontSector.LightLevel >> LightSegShift) + _extraLight;

        if (_currentLine.V1.Y == _currentLine.V2.Y)
        {
            lightNum--;
        }
        else if (_currentLine.V1.X == _currentLine.V2.X)
        {
            lightNum++;
        }

        _wallLights = lightNum switch
        {
            < 0 => _scaleLight[0],
            >= LightLevels => _scaleLight[LightLevels - 1],
            _ => _scaleLight[lightNum]
        };

        MaskedTextureCol = ds.MaskedTextureCol;
        MaskedTextureColIdx = ds.MaskedTextureColIdx;

        _rwScaleStep = ds.ScaleStep;
        _sprYScale = ds.Scale1 + (x1 - ds.X1) * _rwScaleStep;
        _mFloorClip = ds.SpriteBottomClip;
        _mFloorClipIdx = ds.SpriteBottomClipIdx;
        _mCeilingClip = ds.SpriteTopClip;
        _mCeilingClipIdx = ds.SpriteTopClipIdx;

        // find positioning
        if ((_currentLine.LineDef.Flags & Constants.Line.DontPegBottom) != 0)
        {
            _dcTextureMid = _frontSector.FloorHeight > _backSector!.FloorHeight ? _frontSector.FloorHeight : _backSector.FloorHeight;
            _dcTextureMid = _dcTextureMid + TextureHeight[texNum] - _viewZ;
        }
        else
        {
            _dcTextureMid = _frontSector.CeilingHeight < _backSector!.CeilingHeight ? _frontSector.CeilingHeight : _backSector.CeilingHeight;
            _dcTextureMid -= _viewZ;
        }

        _dcTextureMid += _currentLine.SideDef.RowOffset;

        if (_fixedColorMapIdx != null)
        {
            Array.Copy(_colorMaps, _fixedColorMapIdx.Value, _dcColorMap, 0, 256);
            // dc_colormap = fixedcolormap;
        }

        // draw the columns
        for (_dcX = x1; _dcX <= x2; _dcX++)
        {
            // calculate lighting
            if (MaskedTextureCol![MaskedTextureColIdx.GetValueOrDefault() + _dcX] != short.MaxValue)
            {
                if (_fixedColorMapIdx == null)
                {
                    var index = _sprYScale.Value >> LightScaleShift;

                    if (index >= MaxLightScale)
                    {
                        index = MaxLightScale - 1;
                    }

                    Array.Copy(_colorMaps, _wallLights[index], _dcColorMap, 0, 256);
                    // dc_colormap = _wallLights[index];
                }

                _sprTopScreen = _centerYFrac - (_dcTextureMid * _sprYScale);
                _dcIScale = new Fixed((int)(0xffffffffu / (uint)_sprYScale.Value));

                // draw the texture
                var col = GetColumn(texNum, MaskedTextureCol[MaskedTextureColIdx.GetValueOrDefault() + _dcX]);
                DrawMaskedColumn(col);
                MaskedTextureCol[MaskedTextureColIdx.GetValueOrDefault() + _dcX] = short.MaxValue;
            }
            _sprYScale += _rwScaleStep;
        }
    }

    /// <summary>
    /// Draws zero, one, or two textures (and possibly a masked
    ///  texture) for walls.
    /// Can draw or mark the starting pixel of floor and ceiling
    ///  textures.
    /// CALLED: CORE LOOPING ROUTINE. 
    /// </summary>
    private void RenderSegLoop()
    {
        var textureColumn = 0;

        for (; _rwX < _rwStopX; _rwX++)
        {
            // mark floor / ceiling areas
            var yl = (_topFrac.Value + HeightUnit - 1) >> HeightBits;

            // no space above wall?
            if (yl < _ceilingClip[_rwX] + 1)
            {
                yl = _ceilingClip[_rwX] + 1;
            }

            int top;
            int bottom;
            if (_markCeiling)
            {
                top = _ceilingClip[_rwX] + 1;
                bottom = yl - 1;

                if (bottom >= _floorClip[_rwX])
                {
                    bottom = _floorClip[_rwX] - 1;
                }

                if (top <= bottom)
                {
                    _ceilingPlane.WriteTop(_rwX, (byte)top);
                    _ceilingPlane.WriteBottom(_rwX, (byte)bottom);
                }
            }

            var yh = _bottomFrac.Value >> HeightBits;

            if (yh >= _floorClip[_rwX])
            {
                yh = _floorClip[_rwX] - 1;
            }

            if (_markFloor)
            {
                top = yh + 1;
                bottom = _floorClip[_rwX] - 1;
                if (top <= _ceilingClip[_rwX])
                {
                    top = _ceilingClip[_rwX] + 1;
                }

                if (top <= bottom)
                {
                    _floorPlane.WriteTop(_rwX, (byte)top);
                    _floorPlane.WriteBottom(_rwX, (byte)bottom);
                }
            }

            // texturecolumn and lighting are independent of wall tiers
            if (_segTextured)
            {
                // calculate texture offset
                var angle = _rwCenterAngle + _xToViewAngle[_rwX];
                textureColumn = (_rwOffset - (DoomMath.Tan(angle) * _rwDistance)).Value >> Constants.FracBits;
                
                // calculate lighting
                var index = (uint)(_rwScale.Value >> LightScaleShift);

                if (index >= MaxLightScale)
                {
                    index = MaxLightScale - 1;
                }

                Array.Copy(_colorMaps, _wallLights![index], _dcColorMap, 0, 256);
                _dcX = _rwX;
                _dcIScale = new Fixed((int)(0xffffffffu / (uint)_rwScale.Value));
            }

            // draw the wall tiers
            if (_midTexture != 0)
            {
                // single sided line
                _dcYl = yl;
                _dcYh = yh;
                _dcTextureMid = _rwMidTextureMid;

                var col = GetColumn(_midTexture, textureColumn);
                _dcSource = col.Pixels;
                _colFunc();
                _ceilingClip[_rwX] = (short)ViewHeight;
                _floorClip[_rwX] = -1;
            }
            else
            {
                // two sided line
                int mid;
                if (_topTexture != 0)
                {
                    // top wall
                    mid = _pixHigh.Value >> HeightBits;
                    _pixHigh += _pixHighStep;

                    if (mid >= _floorClip[_rwX])
                    {
                        mid = _floorClip[_rwX] - 1;
                    }

                    if (mid >= yl)
                    {
                        _dcYl = yl;
                        _dcYh = mid;
                        _dcTextureMid = _rwTopTextureMid;

                        var col = GetColumn(_topTexture, textureColumn);
                        _dcSource = col.Pixels;
                        _colFunc();
                        _ceilingClip[_rwX] = (short)mid;
                    }
                    else
                    {
                        _ceilingClip[_rwX] = (short)(yl - 1);
                    }
                }
                else
                {
                    // no top wall
                    if (_markCeiling)
                    {
                        _ceilingClip[_rwX] = (short)(yl - 1);
                    }
                }

                if (_bottomTexture != 0)
                {
                    // bottom wall
                    mid = (_pixLow.Value + HeightUnit - 1) >> HeightBits;
                    _pixLow += _pixLowStep;

                    // no space above wall?
                    if (mid <= _ceilingClip[_rwX])
                    {
                        mid = _ceilingClip[_rwX] + 1;
                    }

                    if (mid <= yh)
                    {
                        _dcYl = mid;
                        _dcYh = yh;
                        _dcTextureMid = _rwBottomTextureMid;

                        var col = GetColumn(_bottomTexture, textureColumn);
                        _dcSource = col.Pixels;
                        _colFunc();
                        _floorClip[_rwX] = (short)mid;
                    }
                    else
                    {
                        _floorClip[_rwX] = (short)(yh + 1);
                    }
                }
                else
                {
                    // no bottom wall
                    if (_markFloor)
                    {
                        _floorClip[_rwX] = (short)(yh + 1);
                    }
                }

                if (_maskedTexture)
                {
                    // save texturecol
                    //  for backdrawing of masked mid texture
                    MaskedTextureCol![MaskedTextureColIdx.GetValueOrDefault() + _rwX] = (short)textureColumn;
                }
            }

            _rwScale += _rwScaleStep;
            _topFrac += _topStep;
            _bottomFrac += _bottomStep;
        }
    }

    /// <summary>
    /// A wall segment will be drawn
    ///  between start and stop pixels (inclusive).
    /// </summary>
    private void StoreWallRange(int start, int stop)
    {
        Fixed vTop;

        // don't overflow and crash
        if (_drawSegIdx == MaxDrawSegs)
        {
            return;
        }
        
        _sideDef = _currentLine!.SideDef;
        _lineDef = _currentLine.LineDef;

        // mark the segment as visible for auto map
        _lineDef.Flags |= Constants.Line.Mapped;

        // calculate rw_distance for scale calculation
        _rwNormalAngle = _currentLine.Angle + Angle.Angle90;
        var offsetAngle = Angle.Abs(_rwNormalAngle - _rwAngle1);

        if (offsetAngle > Angle.Angle90)
        {
            offsetAngle = Angle.Angle90;
        }

        var distAngle = Angle.Angle90 - offsetAngle;
        var hyp = PointToDist(_currentLine.V1.X, _currentLine.V1.Y);
        var sineVal = DoomMath.Sin(distAngle);
        _rwDistance = hyp * sineVal;

        var ds = new DrawSegment();
        _drawSegments[_drawSegIdx] = ds;

        ds.X1 = _rwX = start;
        ds.X2 = stop;
        ds.CurrentLine = _currentLine;
        _rwStopX = stop + 1;

        // calculate scale at both ends and step
        ds.Scale1 = _rwScale = ScaleFromGlobalAngle(_viewAngle + _xToViewAngle[start]);

        if (stop > start)
        {
            ds.Scale2 = ScaleFromGlobalAngle(_viewAngle + _xToViewAngle[stop]);
            ds.ScaleStep = _rwScaleStep = (ds.Scale2 - _rwScale) / (stop - start);
        }
        else
        {
            ds.Scale2 = ds.Scale1;
        }

        // calculate texture boundaries
        //  and decide if floor / ceiling marks are needed
        _worldTop = _frontSector!.CeilingHeight - _viewZ;
        _worldBottom = _frontSector.FloorHeight - _viewZ;

        _midTexture = _topTexture = _bottomTexture = 0;
        _maskedTexture = false;

        ds.MaskedTextureCol = null;

        if (_backSector == null)
        {
            // single sided line
            _midTexture = _textureTranslation[_sideDef.MidTexture];
            // a single sided line is terminal, so it must mark ends
            _markFloor = _markCeiling = true;
            if ((_lineDef.Flags & Constants.Line.DontPegBottom) != 0)
            {
                vTop = _frontSector.FloorHeight + TextureHeight[_sideDef.MidTexture];
                // bottom of texture at bottom
                _rwMidTextureMid = vTop - _viewZ;
            }
            else
            {
                // top of texture at top
                _rwMidTextureMid = _worldTop;
            }

            _rwMidTextureMid += _sideDef.RowOffset;

            ds.Silhouette = SilhouetteBoth;
            ds.SpriteTopClip = _screenHeightArray;
            ds.SpriteBottomClip = _negoneArray;
            ds.BottomSilhouetteHeight = Fixed.MaxValue;
            ds.TopSilhouetteHeight = Fixed.MinValue;
        }
        else
        {
            // two sided line
            ds.SpriteTopClip = ds.SpriteBottomClip = null;
            ds.Silhouette = 0;

            if (_frontSector.FloorHeight > _backSector.FloorHeight)
            {
                ds.Silhouette = SilhouetteBottom;
                ds.BottomSilhouetteHeight = _frontSector.FloorHeight;
            }
            else if (_backSector.FloorHeight > _viewZ)
            {
                ds.Silhouette = SilhouetteBottom;
                ds.BottomSilhouetteHeight = Fixed.MaxValue;
            }

            if (_frontSector.CeilingHeight < _backSector.CeilingHeight)
            {
                ds.Silhouette |= SilhouetteTop;
                ds.TopSilhouetteHeight = _frontSector.CeilingHeight;
            }
            else if (_backSector.CeilingHeight < _viewZ)
            {
                ds.Silhouette |= SilhouetteTop;
                ds.TopSilhouetteHeight = Fixed.MinValue;
            }

            if (_backSector.CeilingHeight <= _frontSector.FloorHeight)
            {
                ds.SpriteBottomClip = _negoneArray;
                ds.BottomSilhouetteHeight = Fixed.MaxValue;
                ds.Silhouette |= SilhouetteBottom;
            }

            if (_backSector.FloorHeight >= _frontSector.CeilingHeight)
            {
                ds.SpriteTopClip = _screenHeightArray;
                ds.TopSilhouetteHeight = Fixed.MinValue;
                ds.Silhouette |= SilhouetteTop;
            }

            _worldHigh = _backSector.CeilingHeight - _viewZ;
            _worldLow = _backSector.FloorHeight - _viewZ;

            // hack to allow height changes in outdoor areas
            if (_frontSector.CeilingPic == Sky.FlatNum && _backSector.CeilingPic == Sky.FlatNum)
            {
                _worldTop = _worldHigh;
            }

            if (_worldLow != _worldBottom ||
                _backSector.FloorPic != _frontSector.FloorPic ||
                _backSector.LightLevel != _frontSector.LightLevel)
            {
                _markFloor = true;
            }
            else
            {
                // same plane on both sides
                _markFloor = false;
            }

            if (_worldHigh != _worldTop ||
                _backSector.CeilingPic != _frontSector.CeilingPic ||
                _backSector.LightLevel != _frontSector.LightLevel)
            {
                _markCeiling = true;
            }
            else
            {
                // same plane on both sides
                _markCeiling = false;
            }

            if (_backSector.CeilingHeight <= _frontSector.FloorHeight ||
                _backSector.FloorHeight >= _frontSector.CeilingHeight)
            {
                // closed door
                _markCeiling = _markFloor = true;
            }

            if (_worldHigh < _worldTop)
            {
                // top texture
                _topTexture = _textureTranslation[_sideDef.TopTexture];
                if ((_lineDef.Flags & Constants.Line.DontPegTop) != 0)
                {
                    // top of texture at top
                    _rwTopTextureMid = _worldTop;
                }
                else
                {
                    vTop = _backSector.CeilingHeight + TextureHeight[_sideDef.TopTexture];

                    // bottom of texture
                    _rwTopTextureMid = vTop - _viewZ;
                }
            }

            if (_worldLow > _worldBottom)
            {
                // bottom texture
                _bottomTexture = _textureTranslation[_sideDef.BottomTexture];

                if ((_lineDef.Flags & Constants.Line.DontPegBottom) != 0)
                {
                    // bottom of texture at bottom
                    // top of texture at top
                    _rwBottomTextureMid = _worldTop;
                }
                else // top of texture at top
                {
                    _rwBottomTextureMid = _worldLow;
                }
            }

            _rwTopTextureMid += _sideDef.RowOffset;
            _rwBottomTextureMid += _sideDef.RowOffset;

            // allocate space for masked texture tables
            if (_sideDef.MidTexture != 0)
            {
                // masked midtexture
                _maskedTexture = true;
                ds.MaskedTextureCol = MaskedTextureCol = _openings;
                ds.MaskedTextureColIdx = MaskedTextureColIdx = (_lastOpeningIdx - _rwX);
                _lastOpeningIdx += _rwStopX - _rwX;
            }
        }

        // calculate rw_offset (only needed for textured lines)
        _segTextured = _midTexture != 0 | _topTexture != 0 | _bottomTexture != 0 | _maskedTexture;

        if (_segTextured)
        {
            offsetAngle = _rwNormalAngle - _rwAngle1;

            if (offsetAngle > Angle.Angle180)
            {
                offsetAngle = -offsetAngle;
            }

            if (offsetAngle > Angle.Angle90)
            {
                offsetAngle = Angle.Angle90;
            }

            sineVal = DoomMath.Sin(offsetAngle);
            _rwOffset = hyp * sineVal;

            if (_rwNormalAngle - _rwAngle1 < Angle.Angle180)
            {
                _rwOffset = -_rwOffset;
            }

            _rwOffset += _sideDef.TextureOffset + _currentLine.Offset;
            _rwCenterAngle = Angle.Angle90 + _viewAngle - _rwNormalAngle;

            // calculate light table
            //  use different light tables
            //  for horizontal / vertical / diagonal
            // OPTIMIZE: get rid of LIGHTSEGSHIFT globally
            if (_fixedColorMapIdx == null)
            {
                var lightNum = (_frontSector.LightLevel >> LightSegShift) + _extraLight;

                if (_currentLine.V1.Y == _currentLine.V2.Y)
                {
                    lightNum--;
                }
                else if (_currentLine.V1.X == _currentLine.V2.X)
                {
                    lightNum++;
                }

                _wallLights = lightNum switch
                {
                    < 0 => _scaleLight[0],
                    >= LightLevels => _scaleLight[LightLevels - 1],
                    _ => _scaleLight[lightNum]
                };
            }
        }

        // if a floor / ceiling plane is on the wrong side
        //  of the view plane, it is definitely invisible
        //  and doesn't need to be marked.


        if (_frontSector.FloorHeight >= _viewZ)
        {
            // above view plane
            _markFloor = false;
        }

        if (_frontSector.CeilingHeight <= _viewZ
            && _frontSector.CeilingPic != Sky.FlatNum)
        {
            // below view plane
            _markCeiling = false;
        }

        // calculate incremental stepping values for texture edges
        _worldTop >>= 4;
        _worldBottom >>= 4;

        _topStep = -(_rwScaleStep * _worldTop);
        _topFrac = (_centerYFrac >> 4) - (_worldTop * _rwScale);

        _bottomStep = -(_rwScaleStep * _worldBottom);
        _bottomFrac = (_centerYFrac >> 4) - (_worldBottom * _rwScale);

        if (_backSector != null)
        {
            _worldHigh >>= 4;
            _worldLow >>= 4;

            if (_worldHigh < _worldTop)
            {
                _pixHigh = (_centerYFrac >> 4) - (_worldHigh * _rwScale);
                _pixHighStep = -(_rwScaleStep * _worldHigh);
            }

            if (_worldLow > _worldBottom)
            {
                _pixLow = (_centerYFrac >> 4) - (_worldLow * _rwScale);
                _pixLowStep = -(_rwScaleStep * _worldLow);
            }
        }

        // render it
        if (_markCeiling)
        {
            _ceilingPlane = CheckPlane(_ceilingPlane!, _rwX, _rwStopX - 1);
        }

        if (_markFloor)
        {
            _floorPlane = CheckPlane(_floorPlane!, _rwX, _rwStopX - 1);
        }

        RenderSegLoop();
        
        // save sprite clipping info
        if (((ds.Silhouette & SilhouetteTop) != 0 || _maskedTexture) && ds.SpriteTopClip == null)
        {
            Array.Copy(_ceilingClip, start, _openings, _lastOpeningIdx, _rwStopX - start);

            ds.SpriteTopClip = _openings;
            ds.SpriteTopClipIdx = _lastOpeningIdx - start;
            _lastOpeningIdx += _rwStopX - start;
        }

        if (((ds.Silhouette & SilhouetteBottom) != 0 || _maskedTexture) && ds.SpriteBottomClip == null)
        {
            Array.Copy(_floorClip, start, _openings, _lastOpeningIdx, _rwStopX - start);

            ds.SpriteBottomClip = _openings;
            ds.SpriteBottomClipIdx = _lastOpeningIdx - start;
            _lastOpeningIdx += _rwStopX - start;
        }

        if (_maskedTexture && (ds.Silhouette & SilhouetteTop) == 0)
        {
            ds.Silhouette |= SilhouetteTop;
            ds.TopSilhouetteHeight = Fixed.MinValue;
        }

        if (_maskedTexture && (ds.Silhouette & SilhouetteBottom) == 0)
        {
            ds.Silhouette |= SilhouetteBottom;
            ds.BottomSilhouetteHeight = Fixed.MaxValue;
        }

        _drawSegIdx++;
    }

    /// <summary>
    /// R_ClipSolidWallSegment
    /// Does handle solid walls,
    ///  e.g. single sided LineDefs (middle texture)
    ///  that entirely block the view.
    /// </summary>
    private void ClipSolidWallSegment(int first, int last)
    {
        // Find the first range that touches the range
        //  (adjacent pixels are touching).
        var start = 0;
        int next;

        while (_solidSegments[start].Last < (first - 1))
        {
            start++;
        }

        if (first < _solidSegments[start].First)
        {
            if (last < (_solidSegments[start].First - 1))
            {
                // Post is entirely visible (above start),
                //  so insert a new clippost.
                StoreWallRange(first, last);
                next = _newEnd;
                _newEnd++;

                while (next != start)
                {
                    _solidSegments[next] = _solidSegments[next - 1];
                    next--;
                }

                _solidSegments[next].First = first;
                _solidSegments[next].Last = last;
                return;
            }

            // There is a fragment above *start.
            StoreWallRange(first, _solidSegments[start].First - 1);
            // Now adjust the clip size.
            _solidSegments[start].First = first;
        }

        // Bottom contained in start?
        if (last <= _solidSegments[start].Last)
        {
            return;
        }

        next = start;
        while (last >= _solidSegments[next + 1].First - 1)
        {
            // There is a fragment between two posts.
            StoreWallRange(_solidSegments[next].Last + 1, _solidSegments[next + 1].First - 1);
            next++;

            if (last <= _solidSegments[next].Last)
            {
                // Bottom is contained in next.
                // Adjust the clip size.
                _solidSegments[start].Last = _solidSegments[next].Last;

                // Remove start+1 to next from the clip list,
                // because start now covers their area.
                if (next == start)
                {
                    // Post just extended past the bottom of one post.
                    return;
                }

                while (next++ != _newEnd)
                {
                    // Remove a post.
                    _solidSegments[++start] = _solidSegments[next];
                }

                _newEnd = start + 1;
                return;
            }
        }

        // There is a fragment after *next.
        StoreWallRange(_solidSegments[next].Last + 1, last);
        // Adjust the clip size.
        _solidSegments[start].Last = last;

        // Remove start+1 to next from the clip list,
        // because start now covers their area.
        if (next == start)
        {
            // Post just extended past the bottom of one post.
            return;
        }
        
        while (next++ != _newEnd)
        {
            // Remove a post.
            _solidSegments[++start] = _solidSegments[next];
        }

        _newEnd = start + 1;
    }

    /// <summary>
    /// R_ClipPassWallSegment
    /// Clips the given range of columns,
    ///  but does not includes it in the clip list.
    /// Does handle windows,
    ///  e.g. LineDefs with upper and lower texture.
    /// </summary>
    private void ClipPassWallSegment(int first, int last)
    {
        // Find the first range that touches the range
        //  (adjacent pixels are touching).
        var start = 0;
        while (_solidSegments[start].Last < first - 1)
        {
            start++;
        }

        if (first < _solidSegments[start].First)
        {
            if (last < _solidSegments[start].First - 1)
            {
                // Post is entirely visible (above start).
                StoreWallRange(first, last);
                return;
            }

            // There is a fragment above *start.
            StoreWallRange(first, _solidSegments[start].First - 1);
        }

        // Bottom contained in start?
        if (last <= _solidSegments[start].Last)
        {
            return;
        }

        while (last >= _solidSegments[start + 1].First - 1)
        {
            // There is a fragment between two posts.
            StoreWallRange(_solidSegments[start].Last + 1, _solidSegments[start + 1].First - 1);
            start++;

            if (last <= _solidSegments[start].Last)
            {
                return;
            }
        }

        // There is a fragment after *next.
        StoreWallRange(_solidSegments[start].Last + 1, last);
    }

    /// <summary>
    /// Clips the given segment
    /// and adds any visible pieces to the line list.
    /// </summary>
    private void AddLine(Segment line)
    {
        _currentLine = line;

        // OPTIMIZE: quickly reject orthogonal back sides.
        var angle1 = PointToAngle(line.V1.X, line.V1.Y);
        var angle2 = PointToAngle(line.V2.X, line.V2.Y);

        // Clip to view edges.
        // OPTIMIZE: make constant out of 2*clipangle (FIELDOFVIEW).
        var span = angle1 - angle2;

        // Back side? I.e. backface culling?
        if (span >= Angle.Angle180)
        {
            return;
        }

        // Global angle needed by segcalc.
        _rwAngle1 = angle1;
        angle1 -= _viewAngle;
        angle2 -= _viewAngle;

        var tspan = angle1 + _clipAngle;
        if (tspan > 2 * _clipAngle)
        {
            tspan -= 2 * _clipAngle;

            // Totally off the left edge?
            if (tspan >= span)
            {
                return;
            }

            angle1 = _clipAngle;
        }

        tspan = _clipAngle - angle2;
        if (tspan > 2 * _clipAngle)
        {
            tspan -= 2 * _clipAngle;

            // Totally off the left edge?
            if (tspan >= span)
            {
                return;
            }

            angle2 = -_clipAngle;
        }

        // The seg is in the view range,
        // but not necessarily visible.

        // TODO possible bug here
        angle1 += Angle.Angle90;
        angle2 += Angle.Angle90;
        var x1 = _viewAngleToX[angle1.Value >> DoomMath.AngleToFineShift];
        var x2 = _viewAngleToX[angle2.Value >> DoomMath.AngleToFineShift];

        // Does not cross a pixel?
        if (x1 == x2)
        {
            return;
        }

        _backSector = line.BackSector;

        // Single sided line?
        if (_backSector == null)
        {
            ClipSolid();
            return;
        }

        // Closed door.
        if (_backSector.CeilingHeight <= _frontSector!.FloorHeight || _backSector.FloorHeight >= _frontSector.CeilingHeight)
        {
            ClipSolid();
            return;
        }

        // Window.
        if (_backSector.CeilingHeight != _frontSector.CeilingHeight || _backSector.FloorHeight != _frontSector.FloorHeight)
        {
            ClipPass();
            return;
        }

        // Reject empty lines used for triggers
        //  and special events.
        // Identical floor and ceiling on both sides,
        // identical light levels on both sides,
        // and no middle texture.
        if (_backSector.CeilingPic == _frontSector.CeilingPic &&
            _backSector.FloorPic == _frontSector.FloorPic &&
            _backSector.LightLevel == _frontSector.LightLevel &&
            _currentLine.SideDef.MidTexture == 0)
        {
            return;
        }

        ClipPass();
        return;

        void ClipPass()
        {
            ClipPassWallSegment(x1, x2 - 1);
        }

        void ClipSolid()
        {
            ClipSolidWallSegment(x1, x2 - 1);
        }
    }

    private static readonly int[][] CheckCoord =
    {
        new[] { 3, 0, 2, 1 },
        new[] { 3, 0, 2, 0 },
        new[] { 3, 1, 2, 0 },
        new[] { 0 },
        new[] { 2, 0, 2, 1 },
        new[] { 0, 0, 0, 0 },
        new[] { 3, 1, 3, 0 },
        new[] { 0 },
        new[] { 2, 0, 3, 1 },
        new[] { 2, 1, 3, 1 },
        new[] { 2, 1, 3, 0 }
    };

    /// <summary>
    /// Checks BSP node/subtree bounding box.
    /// Returns true
    ///  if some part of the bbox might be visible.
    /// </summary>
    private bool CheckBBox(IReadOnlyList<Fixed> bspcoord)
    {
        int boxx;
        int boxy;

        // Find the corners of the box
        // that define the edges from current viewpoint.
        if (_viewX <= bspcoord[BoundingBox.BoxLeft])
        {
            boxx = 0;
        }
        else if (_viewX < bspcoord[BoundingBox.BoxRight])
        {
            boxx = 1;
        }
        else
        {
            boxx = 2;
        }

        if (_viewY >= bspcoord[BoundingBox.BoxTop])
        {
            boxy = 0;
        }
        else if (_viewY > bspcoord[BoundingBox.BoxBottom])
        {
            boxy = 1;
        }
        else
        {
            boxy = 2;
        }

        var boxpos = (boxy << 2) + boxx;
        if (boxpos == 5)
        {
            return true;
        }

        var x1 = bspcoord[CheckCoord[boxpos][0]];
        var y1 = bspcoord[CheckCoord[boxpos][1]];
        var x2 = bspcoord[CheckCoord[boxpos][2]];
        var y2 = bspcoord[CheckCoord[boxpos][3]];

        // check clip list for an open space
        var angle1 = PointToAngle(x1, y1) - _viewAngle;
        var angle2 = PointToAngle(x2, y2) - _viewAngle;

        var span = angle1 - angle2;

        // Sitting on a line?
        if (span >= Angle.Angle180)
        {
            return true;
        }

        var tspan = angle1 + _clipAngle;

        if (tspan > 2 * _clipAngle)
        {
            tspan -= 2 * _clipAngle;

            // Totally off the left edge?
            if (tspan >= span)
            {
                return false;
            }

            angle1 = _clipAngle;
        }

        tspan = _clipAngle - angle2;
        if (tspan > 2 * _clipAngle)
        {
            tspan -= 2 * _clipAngle;

            // Totally off the left edge?
            if (tspan >= span)
            {
                return false;
            }

            angle2 = -_clipAngle;
        }

        // Find the first clippost
        //  that touches the source post
        //  (adjacent pixels are touching).
        angle1 += Angle.Angle90;
        angle2 += Angle.Angle90;
        // TODO possible bug here
        var sx1 = _viewAngleToX[angle1.Value >> DoomMath.AngleToFineShift];
        var sx2 = _viewAngleToX[angle2.Value >> DoomMath.AngleToFineShift];

        // Does not cross a pixel.
        if (sx1 == sx2)
        {
            return false;
        }

        sx2--;

        var start = 0;
        while (_solidSegments[start].Last < sx2)
        {
            start++;
        }

        if (sx1 >= _solidSegments[start].First && sx2 <= _solidSegments[start].Last)
        {
            // The clippost contains the new span.
            return false;
        }

        return true;
    }
        
    /// <summary>
    /// Determine floor/ceiling planes.
    /// Add sprites of things in sector.
    /// Draw one or more line segments.
    /// </summary>
    private void Subsector(int num)
    {
        if (num >= DoomGame.Instance.Game.NumSubSectors)
        {
            DoomGame.Error($"R_Subsector: ss {num} with numss = {DoomGame.Instance.Game.NumSubSectors}");
            return;
        }

        _ssCount++;
        var sub = DoomGame.Instance.Game.SubSectors[num];
        _frontSector = sub.Sector;
        var count = sub.NumLines;
        var line = sub.FirstLine;

        if (_frontSector != null && _frontSector.FloorHeight < _viewZ)
        {
            _floorPlane = FindPlane(_frontSector.FloorHeight, _frontSector.FloorPic, _frontSector.LightLevel);
        }
        else
        {
            _floorPlane = null;
        }

        if (_frontSector != null && (_frontSector.CeilingHeight > _viewZ || _frontSector.CeilingPic == Sky.FlatNum))
        {
            _ceilingPlane = FindPlane(_frontSector.CeilingHeight, _frontSector.CeilingPic, _frontSector.LightLevel);
        }
        else
        {
            _ceilingPlane = null;
        }

        AddSprites(_frontSector);

        while (count-- != 0)
        {
            AddLine(DoomGame.Instance.Game.Segments[line++]);
        }
    }

    /// <summary>
    /// Traverse BSP (sub) tree,
    ///  check point against partition plane.
    /// Returns side 0 (front) or 1 (back).
    /// </summary>
    private int PointOnSide(Fixed x, Fixed y, Node node)
    {
        if (node.Dx == Fixed.Zero)
        {
            if (x <= node.X)
            {
                return node.Dy > Fixed.Zero ? 1 : 0;
            }

            return node.Dy < Fixed.Zero ? 1 : 0;
        }

        if (node.Dy == Fixed.Zero)
        {
            if (y <= node.Y)
            {
                return node.Dx < Fixed.Zero ? 1 : 0;
            }

            return node.Dx > Fixed.Zero ? 1 : 0;
        }

        var dx = (x - node.X);
        var dy = (y - node.Y);

        // Try to quickly decide by looking at sign bits.
        if (((node.Dy.Value ^ node.Dx.Value ^ dx.Value ^ dy.Value) & 0x80000000) != 0)
        {
            if (((node.Dy.Value ^ dx.Value) & 0x80000000) != 0)
            {
                // (left is negative)
                return 1;
            }

            return 0;
        }

        var left = new Fixed(node.Dy.Value >> Constants.FracBits) * dx;
        var right = dy * new Fixed(node.Dx.Value >> Constants.FracBits);

        if (right < left)
        {
            // front side
            return 0;
        }

        // back side
        return 1;
    }

    private int PointOnSegSide(Fixed x, Fixed y, Segment line)
    {
        var lx = line.V1.X;
        var ly = line.V1.Y;

        var ldx = line.V2.X - lx;
        var ldy = line.V2.Y - ly;

        if (ldx == Fixed.Zero)
        {
            if (x <= lx)
            {
                return ldy > Fixed.Zero ? 1 : 0;
            }

            return ldy < Fixed.Zero ? 1 : 0;
        }

        if (ldy == Fixed.Zero)
        {
            if (y <= ly)
            {
                return ldx < Fixed.Zero ? 1 : 0;
            }

            return ldx > Fixed.Zero ? 1 : 0;
        }

        var dx = (x - lx);
        var dy = (y - ly);

        // Try to quickly decide by looking at sign bits.
        if (((ldy.Value ^ ldx.Value ^ dx.Value ^ dy.Value) & 0x80000000) != 0)
        {
            if (((ldy.Value ^ dx.Value) & 0x80000000) != 0)
            {
                // (left is negative)
                return 1;
            }
            return 0;
        }

        var left = new Fixed(ldy.Value >> Constants.FracBits) * dx;
        var right = dy * new Fixed(ldx.Value >> Constants.FracBits);

        if (right < left)
        {
            // front side
            return 0;
        }
        // back side
        return 1;
    }


    /// <summary>
    /// Renders all subsectors below a given node,
    ///  traversing subtree recursively.
    /// Just call with BSP root.
    /// </summary>
    private void RenderBSPNode(int bspNum)
    {
        // Found a subsector?
        if ((bspNum & Constants.NodeLeafSubSector) != 0)
        {
            if (bspNum == -1)
            {
                Subsector(0);
            }
            else
            {
                Subsector(bspNum & (~Constants.NodeLeafSubSector));
            }

            return;
        }

        var bsp = DoomGame.Instance.Game.Nodes[bspNum];

        // Decide which side the view point is on.
        var side = PointOnSide(_viewX, _viewY, bsp);

        // Recursively divide front space.
        RenderBSPNode(bsp.Children[side]);

        // Possibly divide back space.
        if (CheckBBox(bsp.BoundingBox[side ^ 1]))
        {
            RenderBSPNode(bsp.Children[side ^ 1]);
        }
    }

    /// <summary>
    /// Uses global vars:
    ///  planeheight
    ///  ds_source
    ///  basexscale
    ///  baseyscale
    ///  viewx
    ///  viewy
    /// </summary>
    private void MapPlane(int y, int x1, int x2)
    {
        Fixed distance;

        //if (x2 < x1 || x1 < 0 || x2 >= ViewWidth || (uint)y > ViewHeight)
        //{
        //    DoomGame.Error($"R_MapPlane: {x1}, {x2} at {y}");
        //    return;
        //}

        if (_planeHeight != _cachedHeight[y])
        {
            _cachedHeight[y] = _planeHeight;
            distance = _cachedDistance[y] = _planeHeight * _ySlope[y];
            _dsXStep = _cachedXStep[y] = distance * _baseXScale;
            _dsYStep = _cachedYStep[y] = distance * _baseYScale;
        }
        else
        {
            distance = _cachedDistance[y];
            _dsXStep = _cachedXStep[y];
            _dsYStep = _cachedYStep[y];
        }

        var length = distance * _distScale[x1];
        var angle = _viewAngle + _xToViewAngle[x1];
        _dsXFrac = _viewX + (DoomMath.Cos(angle) * length);
        _dsYFrac = -_viewY - (DoomMath.Sin(angle) * length);

        if (_fixedColorMapIdx != null)
        {
            Array.Copy(_colorMaps, _fixedColorMapIdx.Value, _dsColorMap, 0, 256);
        }
        else
        {
            var index = (uint)(distance.Value >> LightZShift);

            if (index >= MaxLightZ)
            {
                index = MaxLightZ - 1;
            }

            Array.Copy(_colorMaps, _planeZLight![index], _dsColorMap, 0, 256);
        }

        _dsY = y;
        _dsX1 = x1;
        _dsX2 = x2;

        // high or low detail
        _spanFunc();
    }

    private VisPlane? FindPlane(Fixed height, int picNum, int lightLevel)
    {
        if (picNum == Sky.FlatNum)
        {
            height = Fixed.Zero; // all sky's map together
            lightLevel = 0;
        }

        int vpIdx;
        VisPlane? check = null;

        for (vpIdx = 0; vpIdx < _lastVisPlaneIdx; vpIdx++)
        {
            check = _visPlanes[vpIdx];
            if (height == check!.Height && picNum == check.PicNum && lightLevel == check.LightLevel)
            {
                break;
            }
        }

        if (vpIdx < _lastVisPlaneIdx)
        {
            return check!;
        }

        if (_lastVisPlaneIdx >= MaxVisPlanes)
        {
            DoomGame.Error("R_FindPlane: no more visplanes");
            return null;
        }

        check = new VisPlane();
        _lastVisPlaneIdx++;

        check.Height = height;
        check.PicNum = picNum;
        check.LightLevel = lightLevel;
        check.MinX = Constants.ScreenWidth;
        check.MaxX = -1;

        _visPlanes[vpIdx] = check;

        return check;
    }

    private VisPlane CheckPlane(VisPlane pl, int start, int stop)
    {
        int intrl;
        int intrh;
        int unionl;
        int unionh;
        int x;

        if (start < pl.MinX)
        {
            intrl = pl.MinX;
            unionl = start;
        }
        else
        {
            unionl = pl.MinX;
            intrl = start;
        }

        if (stop > pl.MaxX)
        {
            intrh = pl.MaxX;
            unionh = stop;
        }
        else
        {
            unionh = pl.MaxX;
            intrh = stop;
        }

        for (x = intrl; x <= intrh; x++)
        {
            if (pl.ReadTop(x) != 0xff)
            {
                break;
            }
        }

        if (x > intrh)
        {
            pl.MinX = unionl;
            pl.MaxX = unionh;

            // use the same one
            return pl;
        }

        // make a new visplane
        var newPlane = new VisPlane();
        
        newPlane.Height = pl.Height;
        newPlane.PicNum = pl.PicNum;
        newPlane.LightLevel = pl.LightLevel;
        newPlane.MinX = start;
        newPlane.MaxX = stop;

        _visPlanes[_lastVisPlaneIdx++] = newPlane;

        return newPlane;
    }

    private void MakeSpans(int x, int t1, int b1, int t2, int b2)
    {
        while (t1 < t2 && t1 <= b1)
        {
            MapPlane(t1, _spanStart[t1], x - 1);
            t1++;
        }
        while (b1 > b2 && b1 >= t1)
        {
            MapPlane(b1, _spanStart[b1], x - 1);
            b1--;
        }

        while (t2 < t1 && t2 <= b2)
        {
            _spanStart[t2] = x;
            t2++;
        }
        while (b2 > b1 && b2 >= t2)
        {
            _spanStart[b2] = x;
            b2--;
        }
    }

    private void DrawPlanes()
    {
        //if (_drawSegIdx > MaxDrawSegs)
        //{
        //    DoomGame.Error($"R_DrawPlanes: drawsegs overflow ({_drawSegIdx})");
        //    return;
        //}

        //if (_lastVisPlaneIdx > MaxVisPlanes)
        //{
        //    DoomGame.Error($"R_DrawPlanes: visplane overflow ({_lastVisPlaneIdx})");
        //    return;
        //}

        //if (_lastOpeningIdx > MaxOpenings)
        //{
        //    DoomGame.Error($"R_DrawPlanes: opening overflow ({_lastOpeningIdx})");
        //    return;
        //}

        for (var i = 0; i < _lastVisPlaneIdx; i++)
        {
            var pl = _visPlanes[i];
            if (pl is null || pl.MinX > pl.MaxX)
            {
                continue;
            }

            // sky flat
            if (pl.PicNum == Sky.FlatNum)
            {
                _dcIScale = _pSpriteIScale >> _detailShift;

                // Sky is always drawn full bright,
                //  i.e. colormaps[0] is used.
                // Because of this hack, sky is not affected
                //  by INVUL inverse mapping.
                Array.Copy(_colorMaps, 0, _dcColorMap, 0, 256);
                
                _dcTextureMid = Sky.TextureMid;
                
                for (var x = pl.MinX; x <= pl.MaxX; x++)
                {
                    _dcYl = pl.ReadTop(x);
                    _dcYh = pl.ReadBottom(x);

                    if (_dcYl <= _dcYh)
                    {
                        var angle = (_viewAngle + _xToViewAngle[x]).Value >> Sky.AngleToSkyShift;
                        _dcX = x;
                        
                        var col = GetColumn(Sky.Texture, (int)angle);
                        _dcSource = col.Pixels;
                        _colFunc();
                    }
                }
                continue;
            }

            // regular flat
            _dsSource = DoomGame.Instance.WadData.GetLumpNum(_firstFlat + _flatTranslation[pl.PicNum], PurgeTag.Static)!;

            _planeHeight = Fixed.Abs(pl.Height - _viewZ);
            var light = (pl.LightLevel >> LightSegShift) + _extraLight;

            if (light >= LightLevels)
            {
                light = LightLevels - 1;
            }

            if (light < 0)
            {
                light = 0;
            }

            _planeZLight = _zLight[light];

            pl.WriteTop(pl.MaxX + 1, 0xff);
            pl.WriteTop(pl.MinX - 1, 0xff);

            var stop = pl.MaxX + 1;

            for (var x = pl.MinX; x <= stop; x++)
            {
                MakeSpans(x, pl.ReadTop(x - 1), pl.ReadBottom(x - 1), pl.ReadTop(x), pl.ReadBottom(x));
            }

            // Z_ChangeTag(ds_source, PU_CACHE);
        }
    }

    /// <summary>
    /// To get a global angle from cartesian coordinates,
    ///  the coordinates are flipped until they are in
    ///  the first octant of the coordinate system, then
    ///  the y (<=x) is scaled and divided by x to get a
    ///  tangent (slope) value which is looked up in the
    ///  tantoangle[] table.
    /// </summary>
    private Angle PointToAngle(Fixed x, Fixed y)
    {
        x -= _viewX;
        y -= _viewY;

        if (x == Fixed.Zero && y == Fixed.Zero)
        {
            return Angle.Angle0;
        }

        if (x >= Fixed.Zero)
        {
            // x >=0
            if (y >= Fixed.Zero)
            {
                // y>= 0

                if (x > y)
                {
                    // octant 0
                    return DoomMath.TanToAngle(SlopeDiv(y, x));
                }
             
                // octant 1
                return new Angle(Angle.Angle90.Value - 1) - DoomMath.TanToAngle(SlopeDiv(x, y));
            }
            

            // y<0
            y = -y;

            if (x > y)
            {
                // octant 8
                return -DoomMath.TanToAngle(SlopeDiv(y, x));
            }
         
            // octant 7
            return Angle.Angle270 + DoomMath.TanToAngle(SlopeDiv(x, y));
        }
         
        // x<0
        x = -x;

        if (y >= Fixed.Zero)
        {
            // y>= 0
            if (x > y)
            {
                // octant 3
                return new Angle(Angle.Angle180.Value - 1) - DoomMath.TanToAngle(SlopeDiv(y, x));
            }

            // octant 2
            return Angle.Angle90 + DoomMath.TanToAngle(SlopeDiv(x, y));
        }
         
        // y<0
        y = -y;

        if (x > y)
        {
            // octant 4
            return Angle.Angle180 + DoomMath.TanToAngle(SlopeDiv(y, x));
        }
     
        // octant 5
        return new Angle(Angle.Angle270.Value - 1) - DoomMath.TanToAngle(SlopeDiv(x, y));
    }

    public Angle PointToAngle2(Fixed x1, Fixed y1, Fixed x2, Fixed y2)
    {
        _viewX = x1;
        _viewY = y1;

        return PointToAngle(x2, y2);
    }

    private Fixed PointToDist(Fixed x, Fixed y)
    {
        var dx = Fixed.Abs(x - _viewX);
        var dy = Fixed.Abs(y - _viewY);

        if (dy > dx)
        {
            (dx, dy) = (dy, dx);
        }

        var angle = DoomMath.TanToAngle((uint)((dy / dx).Value) >> DBits) + Angle.Angle90;
        
        // use as cosine
        return dx / DoomMath.Sin(angle);
    }

    /// <summary>
    /// Returns the texture mapping scale
    ///  for the current line (horizontal span)
    ///  at the given angle.
    /// rw_distance must be calculated first.
    /// </summary>
    private Fixed ScaleFromGlobalAngle(Angle visAngle)
    {
        Fixed scale;

        var angleA = Angle.Angle90 + (visAngle - _viewAngle);
        var angleB = Angle.Angle90 + (visAngle - _rwNormalAngle);

        // both sines are always positive
        var sineA = DoomMath.Sin(angleA);
        var sineB = DoomMath.Sin(angleB);
        var num = (_projection * sineB) << _detailShift;
        var den = _rwDistance * sineA;

        if (den > (num >> 16))
        {
            scale = num / den;

            if (scale > Fixed.FromInt(64))
            {
                scale = Fixed.FromInt(64);
            }
            else if (scale.Value < 256)
            {
                scale = new Fixed(256);
            }
        }
        else
        {
            scale = Fixed.FromInt(64);
        }

        return scale;
    }

    // Sprites

    private void DrawMaskedColumn(Column? column)
    {
        if (_mFloorClip is null || _mCeilingClip is null || column is null)
        {
            return;
        }

        var baseTextureMid = _dcTextureMid;

        for (; column.TopDelta != 0xff; )
        {
            // calculate unclipped screen coordinates
            //  for post
            var topscreen = _sprTopScreen + _sprYScale * column.TopDelta;
            var bottomscreen = topscreen + _sprYScale * column.Length;

            _dcYl = (topscreen.Value + Constants.FracUnit - 1) >> Constants.FracBits;
            _dcYh = (bottomscreen.Value - 1) >> Constants.FracBits;

            if (_dcYh >= _mFloorClip[_mFloorClipIdx.GetValueOrDefault() + _dcX])
            {
                _dcYh = _mFloorClip[_mFloorClipIdx.GetValueOrDefault() + _dcX] - 1;
            }

            if (_dcYl <= _mCeilingClip[_mCeilingClipIdx.GetValueOrDefault() + _dcX])
            {
                _dcYl = _mCeilingClip[_mCeilingClipIdx.GetValueOrDefault() + _dcX] + 1;
            }

            if (_dcYl <= _dcYh)
            {
                _dcSource = column.Pixels;
                _dcTextureMid = baseTextureMid - Fixed.FromInt(column.TopDelta);
                // dc_source = (byte *)column + 3 - column->topdelta;

                // Drawn by either R_DrawColumn
                //  or (SHADOW) R_DrawFuzzColumn.
                _colFunc();
            }
            column = column.Next ?? new Column(0xff, 0, Array.Empty<byte>());
        }

        _dcTextureMid = baseTextureMid; // Reset when done
    }

    //
    // R_DrawVisSprite
    //  mfloorclip and mceilingclip should also be set.
    //
    private void DrawVisSprite(VisSprite vis, int x1, int x2)
    {
        var patch = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpNum(vis.Patch + _firstSpriteLump, PurgeTag.Cache)!);

        if (vis.Colormap != null)
        {
            Array.Copy(vis.Colormap, 0, _dcColorMap, 0, 256);
        }

        if (vis.Colormap == null)
        {
            // NULL colormap = shadow draw
            _colFunc = _fuzzColFunc;
        }
        else if ((vis.MapObjectFlags & MapObjectFlag.MF_TRANSLATION) != 0)
        {
            _colFunc = DrawTranslatedColumn;
            var sourceIdx = (int)(vis.MapObjectFlags & MapObjectFlag.MF_TRANSLATION) >> ((int)MapObjectFlag.MF_TRANSSHIFT - 8);
            Array.Copy(_translationTables, sourceIdx, _dcTranslation, 0, 256);
            //dc_translation = translationtables - 256 +
            //                 ((vis->mobjflags & MF_TRANSLATION) >> (MF_TRANSSHIFT - 8));
        }

        _dcIScale = Fixed.Abs(vis.XiScale) >> _detailShift;
        _dcTextureMid = vis.TextureMid;
        var frac = vis.StartFrac;
        _sprYScale = vis.Scale;
        _sprTopScreen = _centerYFrac - (_dcTextureMid * _sprYScale);

        for (_dcX = vis.X1; _dcX <= vis.X2; _dcX++, frac += vis.XiScale)
        {
            var textureColumn = frac.Value >> Constants.FracBits;
            //# ifdef RANGECHECK
            //            if (texturecolumn < 0 || texturecolumn >= SHORT(patch->width))
            //                I_Error("R_DrawSpriteRange: bad texturecolumn");
            //#endif
            var column = patch.GetColumnByOffset(patch.ColumnOffsets[textureColumn]);
            DrawMaskedColumn(column);
        }

        _colFunc = _baseColFunc;
    }

    private static readonly Fixed MinZ = Fixed.FromInt(4);
    private const int BaseYCenter = 100;

    /// <summary>
    /// Generates a vissprite for a thing if it might be visible
    /// </summary>
    private void ProjectSprite(MapObject thing)
    {
        // transform the origin point
        var tr_x = thing.X - _viewX;
        var tr_y = thing.Y - _viewY;

        var gxt = tr_x * _viewCos;
        var gyt = -(tr_y * _viewSin);

        var tz = gxt - gyt;

        // thing is behind view plane?
        if (tz < MinZ)
        {
            return;
        }

        var xscale = _projection / tz;

        gxt = -(tr_x * _viewSin);
        gyt = tr_y * _viewCos;
        var tx = -(gyt + gxt);

        // too far off the side?
        if (Fixed.Abs(tx) > (tz << 2))
        {
            return;
        }

        // decide which patch to use for sprite relative to player
        var sprdef = _sprites[(int)thing.Sprite];
        var sprframe = sprdef.Frames[thing.Frame & SpriteFrame.FrameMask];
        
        int lump;
        bool flip;
        if (sprframe.Rotate == true)
        {
            // choose a different rotation based on player view
            var ang = PointToAngle(thing.X, thing.Y);
            var rot = (ang.Value - thing.Angle.Value + (Angle.Angle45.Value / 2) * 9) >> 29;
            lump = sprframe.Lumps[rot];
            flip = sprframe.Flip[rot];
        }
        else
        {
            // use single rotation for all views
            lump = sprframe.Lumps[0];
            flip = sprframe.Flip[0];
        }

        // calculate edges of the shape
        tx -= Fixed.FromInt(_spriteOffset[lump]);
        var x1 = (_centerXFrac + (tx * xscale)).Value >> Constants.FracBits;

        // off the right side?
        if (x1 > ViewWidth)
        {
            return;
        }

        tx += Fixed.FromInt(_spriteWidth[lump]);
        var x2 = ((_centerXFrac + (tx * xscale)).Value >> Constants.FracBits) - 1;

        // off the left side
        if (x2 < 0)
        {
            return;
        }

        // store information in a vissprite
        var vis = NewVisSprite();
        vis.MapObjectFlags = thing.Flags;
        vis.Scale = xscale << _detailShift;
        vis.GX = thing.X;
        vis.GY = thing.Y;
        vis.GZ = thing.Z;
        vis.GZTop = thing.Z + Fixed.FromInt(_spriteTopOffset[lump]);
        vis.TextureMid = vis.GZTop - _viewZ;
        vis.X1 = x1 < 0 ? 0 : x1;
        vis.X2 = x2 >= ViewWidth ? ViewWidth - 1 : x2;
        var iscale = Fixed.Unit / xscale;

        if (flip)
        {
            vis.StartFrac = new Fixed(Fixed.FromInt(_spriteWidth[lump]).Value - 1);
            vis.XiScale = -iscale;
        }
        else
        {
            vis.StartFrac = Fixed.Zero;
            vis.XiScale = iscale;
        }

        if (vis.X1 > x1)
        {
            vis.StartFrac += vis.XiScale * (vis.X1 - x1);
        }
        vis.Patch = lump;

        // get light level
        if ((thing.Flags & MapObjectFlag.MF_SHADOW) != 0)
        {
            // shadow draw
            vis.Colormap = null;
        }
        else if (_fixedColorMapIdx != null)
        {
            // fixed map
            var colorMap = new byte[256];
            Array.Copy(_colorMaps, _fixedColorMapIdx.Value, colorMap, 0, 256);
            vis.Colormap = colorMap;
        }
        else if ((thing.Frame & SpriteFrame.FullBright) != 0)
        {
            // full bright
            var colorMap = new byte[256];
            Array.Copy(_colorMaps, 0, colorMap, 0, 256);
            vis.Colormap = colorMap;
        }
        else
        {
            // diminished light
            var index = xscale.Value >> (LightScaleShift - _detailShift);

            if (index >= MaxLightScale)
            {
                index = MaxLightScale - 1;
            }

            var colorMap = new byte[256];
            Array.Copy(_colorMaps, _spriteLights[index], colorMap, 0, 256);
            vis.Colormap = colorMap;
        }
    }

    /// <summary>
    /// During BSP traversal, this adds sprites by sector.
    /// </summary>
    private void AddSprites(Sector? sec)
    {
        if (sec is null)
        {
            return;
        }

        // BSP is traversed by subsector.
        // A sector might have been split into several
        //  subsectors during BSP building.
        // Thus we check whether its already added.
        if (sec.ValidCount == ValidCount)
        {
            return;
        }

        // Well, now it will be done.
        sec.ValidCount = ValidCount;

        var lightNum = (sec.LightLevel >> LightSegShift) + _extraLight;

        if (lightNum < 0)
        {
            _spriteLights = _scaleLight[0];
        }
        else if (lightNum >= LightLevels)
        {
            _spriteLights = _scaleLight[LightLevels - 1];
        }
        else
        {
            _spriteLights = _scaleLight[lightNum];
        }

        // Handle all things in sector.
        var thing = sec.ThingList;
        while (thing != null)
        {
            ProjectSprite(thing);
            thing = thing.SectorNext;
        }
    }

    private void ClearSprites()
    {
        _newVisSprite = 0;
    }

    private VisSprite NewVisSprite()
    {
        if (_newVisSprite == MaxVisSprites)
        {
            return _overflowSprite;
        }

        var vp = new VisSprite();
        _visSprites[_newVisSprite++] = vp;
        
        return vp;
    }

    private void DrawPlayerSprite(PlayerSprite psp)
    {
        if (_viewPlayer is null || psp.State == null)
        {
            return;
        }

        // decide which patch to use
        var sprite = _sprites[(int)psp.State.Sprite];
        var spriteFrame = sprite.Frames[psp.State.Frame & SpriteFrame.FrameMask];
        var lump = spriteFrame.Lumps[0];
        var flip = spriteFrame.Flip[0];

        // calculate edges of the shape
        var tx = psp.SX - Fixed.FromInt(160);
        tx -= Fixed.FromInt(_spriteOffset[lump]);

        var x1 = (_centerXFrac + (tx * _pSpriteScale)).Value >> Constants.FracBits;

        // off the right side
        if (x1 > ViewWidth)
        {
            return;
        }

        tx += Fixed.FromInt(_spriteWidth[lump]);
        var x2 = ((_centerXFrac + (tx * _pSpriteScale)).Value >> Constants.FracBits) - 1;

        // off the left side
        if (x2 < 0)
        {
            return;
        }

        // store information in a vissprite
        var vis = new VisSprite()
        {
            MapObjectFlags = 0,
            TextureMid = Fixed.FromInt(BaseYCenter) + Fixed.Unit / 2 - (psp.SY - Fixed.FromInt(_spriteTopOffset[lump])),
            X1 = x1 < 0 ? 0 : x1,
            X2 = x2 >= ViewWidth ? ViewWidth - 1 : x2,
            Scale = _pSpriteScale << _detailShift
        };

        if (flip)
        {
            vis.XiScale = -_pSpriteIScale;
            vis.StartFrac = new Fixed(Fixed.FromInt(_spriteWidth[lump]).Value - 1);
        }
        else
        {
            vis.XiScale = _pSpriteIScale;
            vis.StartFrac = Fixed.Zero;
        }

        if (vis.X1 > x1)
        {
            vis.StartFrac += vis.XiScale * (vis.X1 - x1);
        }

        vis.Patch = lump;

        if (_viewPlayer.Powers[(int)PowerUpType.Invisibility] > 4 * 32 || 
            (_viewPlayer.Powers[(int)PowerUpType.Invisibility] & 8) != 0)
        {
            // shadow draw
            vis.Colormap = null;
        }
        else if (_fixedColorMapIdx != null)
        {
            // fixed color
            var colorMap = new byte[256];
            Array.Copy(_colorMaps, _fixedColorMapIdx.Value, colorMap, 0, 256);
            vis.Colormap = colorMap;
        }
        else if ((psp.State.Frame & SpriteFrame.FullBright) != 0)
        {
            // full bright
            var colorMap = new byte[256];
            Array.Copy(_colorMaps, 0, colorMap, 0, 256);
            vis.Colormap = colorMap;
        }
        else
        {
            // local light
            var colorMap = new byte[256];
            Array.Copy(_colorMaps, _spriteLights[MaxLightScale - 1], colorMap, 0, 256);
            vis.Colormap = colorMap;
        }

        DrawVisSprite(vis, vis.X1, vis.X2);
    }

    private void DrawPlayerSprites()
    {
        // get light level
        var lightnum = (_viewPlayer!.MapObject!.SubSector!.Sector!.LightLevel >> LightSegShift) + _extraLight;

        if (lightnum < 0)
        {
            _spriteLights = _scaleLight[0];
        }
        else if (lightnum >= LightLevels)
        {
            _spriteLights = _scaleLight[LightLevels - 1];
        }
        else
        {
            _spriteLights = _scaleLight[lightnum];
        }

        // clip to screen bounds
        _mFloorClip = _screenHeightArray;
        _mFloorClipIdx = 0;

        _mCeilingClip = _negoneArray;
        _mCeilingClipIdx = 0;

        if (_viewPlayer != null)
        {
            // add all active psprites
            for (var i = 0; i < (int)PlayerSpriteType.NumPlayerSprites; i++)
            {
                var psp = _viewPlayer.PlayerSprites[i];

                if (psp?.State != null)
                {
                    DrawPlayerSprite(psp);
                }
            }
        }
    }

    private void SortVisSprites()
    {
        var count = _newVisSprite;

        if (count == 0)
        {
            return;
        }

        var unsorted = new VisSprite();
        unsorted.Next = unsorted.Prev = unsorted;
        for (var i = 0; i < count; i++)
        {
            _visSprites[i].Next = i == count - 1 ? _visSprites[0] : _visSprites[i + 1];
            _visSprites[i].Prev = i == 0 ? _visSprites[count - 1] : _visSprites[i - 1];
        }

        _visSprites[0].Prev = unsorted;
        unsorted.Next = _visSprites[0];
        _visSprites[count - 1].Next = unsorted;
        unsorted.Prev = _visSprites[count - 1];

        // pull the vissprites out by scale
        //best = 0;		// shut up the compiler warning

        _sortedHead.Next = _sortedHead.Prev = _sortedHead;

        VisSprite? best = null;
        for (var i = 0; i < count; i++)
        {
            var bestScale = Fixed.MaxValue;
            for (var ds = unsorted.Next; ds != unsorted; ds = ds.Next)
            {
                if (ds.Scale < bestScale)
                {
                    bestScale = ds.Scale;
                    best = ds;
                }
            }
            best.Next.Prev = best.Prev;
            best.Prev.Next = best.Next;
            best.Next = _sortedHead;
            best.Prev = _sortedHead.Prev;
            _sortedHead.Prev.Next = best;
            _sortedHead.Prev = best;
        }
    }

    private void DrawSprite(VisSprite sprite)
    {
        var clipBottom = new short[Constants.ScreenWidth];
        var clipTop = new short[Constants.ScreenWidth];

        for (var x = sprite.X1; x <= sprite.X2; x++)
        {
            clipBottom[x] = clipTop[x] = -2;
        }

        // Scan drawsegs from end to start for obscuring segs.
        // The first drawseg that has a greater scale
        //  is the clip seg.
        for (var i = _drawSegIdx - 1; i >= 0; i--)
        {
            var ds = _drawSegments[i];
            if (ds is null)
            {
                continue;
            }

            // determine if the drawseg obscures the sprite
            if (ds.X1 > sprite.X2
                || ds.X2 < sprite.X1
                || (ds.Silhouette == 0 && ds.MaskedTextureCol == null))
            {
                // does not cover sprite
                continue;
            }

            var r1 = ds.X1 < sprite.X1 ? sprite.X1 : ds.X1;
            var r2 = ds.X2 > sprite.X2 ? sprite.X2 : ds.X2;

            Fixed scale;
            Fixed lowScale;
            if (ds.Scale1 > ds.Scale2)
            {
                lowScale = ds.Scale2;
                scale = ds.Scale1;
            }
            else
            {
                lowScale = ds.Scale1;
                scale = ds.Scale2;
            }

            if (scale < sprite.Scale
                || (lowScale < sprite.Scale
                    && PointOnSegSide(sprite.GX, sprite.GY, ds.CurrentLine!) == 0))
            {
                // masked mid texture?
                if (ds.MaskedTextureCol != null)
                {
                    RenderMaskedSegmentRange(ds, r1, r2);
                }
                // seg is behind sprite
                continue;
            }

            // clip this piece of the sprite
            var silhouette = ds.Silhouette;

            if (sprite.GZ >= ds.BottomSilhouetteHeight)
            {
                silhouette &= ~SilhouetteBottom;
            }

            if (sprite.GZTop <= ds.TopSilhouetteHeight)
            {
                silhouette &= ~SilhouetteTop;
            }

            if (silhouette == 1)
            {
                // bottom sil
                for (var x = r1; x <= r2; x++)
                {
                    if (clipBottom[x] == -2)
                    {
                        clipBottom[x] = ds.SpriteBottomClip[ds.SpriteBottomClipIdx.GetValueOrDefault() + x];
                    }
                }
            }
            else if (silhouette == 2)
            {
                // top sil
                for (var x = r1; x <= r2; x++)
                {
                    if (clipTop[x] == -2)
                    {
                        clipTop[x] = ds.SpriteTopClip[ds.SpriteTopClipIdx.GetValueOrDefault() + x];
                    }
                }
            }
            else if (silhouette == 3)
            {
                // both
                for (var x = r1; x <= r2; x++)
                {
                    if (clipBottom[x] == -2)
                    {
                        clipBottom[x] = ds.SpriteBottomClip[ds.SpriteBottomClipIdx.GetValueOrDefault() + x];
                    }

                    if (clipTop[x] == -2)
                    {
                        clipTop[x] = ds.SpriteTopClip[ds.SpriteTopClipIdx.GetValueOrDefault() + x];
                    }
                }
            }
        }

        // all clipping has been performed, so draw the sprite

        // check for unclipped columns
        for (var x = sprite.X1; x <= sprite.X2; x++)
        {
            if (clipBottom[x] == -2)
            {
                clipBottom[x] = (short)ViewHeight;
            }

            if (clipTop[x] == -2)
            {
                clipTop[x] = -1;
            }
        }

        _mFloorClip = clipBottom;
        _mFloorClipIdx = 0;
        
        _mCeilingClip = clipTop;
        _mCeilingClipIdx = 0;
        DrawVisSprite(sprite, sprite.X1, sprite.X2);
    }

    private void DrawMasked()
    {
        SortVisSprites();

        if (_newVisSprite > 0)
        {
            // draw all vissprites back to front
            for (var spr = _sortedHead.Next; spr != null && spr != _sortedHead; spr = spr?.Next)
            {
                DrawSprite(spr);
            }
        }

        // render any remaining masked mid textures
        for (var i = _drawSegIdx - 1; i >= 0; i--)
        {
            var ds = _drawSegments[i];
            if (ds?.MaskedTextureCol is null)
            {
                continue;
            }
            RenderMaskedSegmentRange(ds, ds.X1, ds.X2);
        }

        // draw the psprites on top of everything
        // but does not draw on side views
        if (_viewAngleOffset == 0)
        { 
            DrawPlayerSprites();
        }
    }
}