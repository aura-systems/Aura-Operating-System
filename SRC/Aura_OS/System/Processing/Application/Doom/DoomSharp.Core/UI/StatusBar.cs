using DoomSharp.Core.Data;
using DoomSharp.Core.GameLogic;
using DoomSharp.Core.Graphics;
using DoomSharp.Core.Input;
using System;

namespace DoomSharp.Core.UI;

public class StatusBar
{
    public const int Height = 32 * Constants.ScreenMul;
    public const int Width = Constants.ScreenWidth;
    public const int Y = Constants.ScreenHeight - Height;
    public const int X = 0;
    public const int X2 = 104;
    public const int ST_FX = 143;
    public const int ST_FY = 169;

    //
    // Background and foreground screen numbers
    //
    public const int BG = 4;
    public const int FG = 0;

    // Palette indices.
    // For damage/bonus red-/gold-shifts
    public const int STARTREDPALS = 1;
    public const int STARTBONUSPALS = 9;
    public const int NUMREDPALS = 8;
    public const int NUMBONUSPALS = 4;
    // Radiation suit, green shift.
    public const int RADIATIONPAL = 13;

    // N/256*100% probability
    //  that the normal face state will change
    public const int ST_FACEPROBABILITY = 96;

    // Number of status faces.
    public const int ST_NUMPAINFACES = 5;
    public const int ST_NUMSTRAIGHTFACES = 3;
    public const int ST_NUMTURNFACES = 2;
    public const int ST_NUMSPECIALFACES = 3;

    public const int ST_FACESTRIDE = (ST_NUMSTRAIGHTFACES + ST_NUMTURNFACES + ST_NUMSPECIALFACES);

    public const int ST_NUMEXTRAFACES = 2;

    public const int ST_NUMFACES = (ST_FACESTRIDE * ST_NUMPAINFACES + ST_NUMEXTRAFACES);

    public const int ST_TURNOFFSET = (ST_NUMSTRAIGHTFACES);
    public const int ST_OUCHOFFSET = (ST_TURNOFFSET + ST_NUMTURNFACES);
    public const int ST_EVILGRINOFFSET = (ST_OUCHOFFSET + 1);
    public const int ST_RAMPAGEOFFSET = (ST_EVILGRINOFFSET + 1);
    public const int ST_GODFACE = (ST_NUMPAINFACES * ST_FACESTRIDE);
    public const int ST_DEADFACE = (ST_GODFACE + 1);

    public const int ST_FACESX = 143;
    public const int ST_FACESY = 168;

    public const int ST_EVILGRINCOUNT = (2 * Constants.TicRate);
    public const int ST_STRAIGHTFACECOUNT = (Constants.TicRate / 2);
    public const int ST_TURNCOUNT = (1 * Constants.TicRate);
    public const int ST_OUCHCOUNT = (1 * Constants.TicRate);
    public const int ST_RAMPAGEDELAY = (2 * Constants.TicRate);

    public const int ST_MUCHPAIN = 20;

    // AMMO number pos.
    public const int ST_AMMOWIDTH = 3;
    public const int ST_AMMOX = 44;
    public const int ST_AMMOY = 171;

    // HEALTH number pos.
    public const int ST_HEALTHWIDTH = 3;
    public const int ST_HEALTHX = 90;
    public const int ST_HEALTHY = 171;

    // Weapon pos.
    public const int ST_ARMSX = 111;
    public const int ST_ARMSY = 172;
    public const int ST_ARMSBGX = 104;
    public const int ST_ARMSBGY = 168;
    public const int ST_ARMSXSPACE = 12;
    public const int ST_ARMSYSPACE = 10;

    // Frags pos.
    public const int ST_FRAGSX = 138;
    public const int ST_FRAGSY = 171;
    public const int ST_FRAGSWIDTH = 2;

    // ARMOR number pos.
    public const int ST_ARMORWIDTH = 3;
    public const int ST_ARMORX = 221;
    public const int ST_ARMORY = 171;

    // Key icon positions.
    public const int ST_KEY0WIDTH = 8;
    public const int ST_KEY0HEIGHT = 5;
    public const int ST_KEY0X = 239;
    public const int ST_KEY0Y = 171;
    public const int ST_KEY1WIDTH = ST_KEY0WIDTH;
    public const int ST_KEY1X = 239;
    public const int ST_KEY1Y = 181;
    public const int ST_KEY2WIDTH = ST_KEY0WIDTH;
    public const int ST_KEY2X = 239;
    public const int ST_KEY2Y = 191;

    // Ammunition counter.
    public const int ST_AMMO0WIDTH = 3;
    public const int ST_AMMO0HEIGHT = 6;
    public const int ST_AMMO0X = 288;
    public const int ST_AMMO0Y = 173;
    public const int ST_AMMO1WIDTH = ST_AMMO0WIDTH;
    public const int ST_AMMO1X = 288;
    public const int ST_AMMO1Y = 179;
    public const int ST_AMMO2WIDTH = ST_AMMO0WIDTH;
    public const int ST_AMMO2X = 288;
    public const int ST_AMMO2Y = 191;
    public const int ST_AMMO3WIDTH = ST_AMMO0WIDTH;
    public const int ST_AMMO3X = 288;
    public const int ST_AMMO3Y = 185;

    // Indicate maximum ammunition.
    // Only needed because backpack exists.
    public const int ST_MAXAMMO0WIDTH = 3;
    public const int ST_MAXAMMO0HEIGHT = 5;
    public const int ST_MAXAMMO0X = 314;
    public const int ST_MAXAMMO0Y = 173;
    public const int ST_MAXAMMO1WIDTH = ST_MAXAMMO0WIDTH;
    public const int ST_MAXAMMO1X = 314;
    public const int ST_MAXAMMO1Y = 179;
    public const int ST_MAXAMMO2WIDTH = ST_MAXAMMO0WIDTH;
    public const int ST_MAXAMMO2X = 314;
    public const int ST_MAXAMMO2Y = 191;
    public const int ST_MAXAMMO3WIDTH = ST_MAXAMMO0WIDTH;
    public const int ST_MAXAMMO3X = 314;
    public const int ST_MAXAMMO3Y = 185;

    // pistol
    public const int ST_WEAPON0X = 110;
    public const int ST_WEAPON0Y = 172;

    // shotgun
    public const int ST_WEAPON1X = 122;
    public const int ST_WEAPON1Y = 172;

    // chain gun
    public const int ST_WEAPON2X = 134;
    public const int ST_WEAPON2Y = 172;

    // missile launcher
    public const int ST_WEAPON3X = 110;
    public const int ST_WEAPON3Y = 181;

    // plasma gun
    public const int ST_WEAPON4X = 122;
    public const int ST_WEAPON4Y = 181;

    // bfg
    public const int ST_WEAPON5X = 134;
    public const int ST_WEAPON5Y = 181;

    // WPNS title
    public const int ST_WPNSX = 109;
    public const int ST_WPNSY = 191;

    // DETH title
    public const int ST_DETHX = 109;
    public const int ST_DETHY = 191;

    private bool _stopped;

    private Player? _player;

    private bool _firstTime;
    private int _veryFirstTime = 1;

    private const string Palette = "PLAYPAL";
    private int _paletteIdx = 0;
    private uint _clock;
    private int _messageCounter;

    private bool _statusBarOn;
    private bool _notDeathMatch;
    private bool _armsOn;
    private bool _fragsOn;

    // main bar left
    private Patch? _statusBar;

    // 0-9, tall numbers
    private readonly Patch?[] _tallNum = new Patch?[10];

    // tall % sign
    private Patch? _tallPercent;

    // 0-9, short, yellow (,different!) numbers
    private readonly Patch?[] _shortNum = new Patch?[10];

    // 3 key-cards, 3 skulls
    private readonly Patch?[] _keys = new Patch?[(int)KeyCardType.NumberOfKeyCards];

    // face status patches
    private readonly Patch?[] _faces = new Patch?[ST_NUMFACES];

    // face background
    private Patch? _faceBackground;

    // main bar right
    private Patch? _armsBackground;

    // weapon ownership patches
    private readonly Patch?[][] _arms =
    {
        new Patch?[] { null, null },
        new Patch?[] { null, null },
        new Patch?[] { null, null },
        new Patch?[] { null, null },
        new Patch?[] { null, null },
        new Patch?[] { null, null }
    };

    // minus patch
    private Patch? _minus;

    // ready-weapon widget
    private NumberWidget? _weaponWidget;

    // in deathmatch only, summary of frags stats
    private NumberWidget? _fragsWidget;

    // health widget
    private PercentWidget? _healthWidget;

    // arms background
    private BinaryIconWidget? _armsBackgroundWidget;

    // weapon ownership widgets
    private readonly MultiIconWidget?[] _armsWidgets =
    {
        null, null, null, null, null, null
    };

    // face status widget
    private MultiIconWidget? _faceStatusWidget;

    // keycard widgets
    private readonly MultiIconWidget?[] _keyBoxWidgets =
    {
        null, null, null
    };

    // armor widget
    private PercentWidget? _armorWidget;

    // ammo widgets
    private readonly NumberWidget?[] _ammoWidgets =
    {
        null, null, null, null
    };

    // max ammo widgets
    private readonly NumberWidget?[] _maxAmmoWidgets =
    {
        null, null, null, null
    };

    // number of frags so far in deathmatch
    private int _fragsCount;

    // used to use appopriately pained face
    private int _oldHealth = -1;

    // used for evil grin
    private bool[] _oldWeaponsOwned = new bool[(int)WeaponType.NumberOfWeapons];

    // count until face changes
    private int _faceCount = 0;

    // current face index, used by w_faces
    private int _faceIndex = 0;

    // holds key-type for each key box on bar
    private int[] _keyBoxes = new int[3];

    // a random number per tick
    private int _randomNumber;

    public void Ticker()
    {
        _clock++;
        _randomNumber = DoomRandom.M_Random();
        UpdateWidgets();
        _oldHealth = _player!.Health;
    }

    public void Drawer(bool fullScreen, bool refresh)
    {
        _statusBarOn = (!fullScreen) || DoomGame.Instance.AutoMapActive;
        _firstTime = _firstTime || refresh;

        // Do red-/gold-shifts from damage/items
        DoPaletteStuff();

        // If just after ST_Start(), refresh all
        if (_firstTime)
        {
            DoRefresh();
        }
        // Otherwise, update as little as possible
        else
        {
            DiffDraw();
        }
    }

    public void Start()
    {
        if (!_stopped)
        {
            Stop();
        }

        InitData();
        CreateWidgets();
        _stopped = false;
    }

    public void Stop()
    {
        if (_stopped)
        {
            return;
        }

        DoomGame.Instance.Video.SetPalette(Palette, _paletteIdx);
        _stopped = true;
    }

    public void Init()
    {
        _veryFirstTime = 0;
        LoadData();
        DoomGame.Instance.Video.Screens[4] = new byte[Width * Height];
    }

    private void LoadData()
    {
        _paletteIdx = 0;
        LoadGraphics();
    }

    private void LoadGraphics()
    {
        var name = "";

        // Load the numbers, tall and short
        for (var i = 0; i < 10; i++)
        {
            name = $"STTNUM{i}";
            _tallNum[i] = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpName(name, PurgeTag.Static)!);

            name = $"STYSNUM{i}";
            _shortNum[i] = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpName(name, PurgeTag.Static)!);
        }

        // Load percent key.
        //Note: why not load STMINUS here, too?
        _tallPercent = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpName("STTPRCNT", PurgeTag.Static)!);

        // key cards
        for (var i = 0; i < (int)KeyCardType.NumberOfKeyCards; i++)
        {
            name = $"STKEYS{i}";
            _keys[i] = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpName(name, PurgeTag.Static)!);
        }

        // arms background
        _armsBackground = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpName("STARMS", PurgeTag.Static)!);

        // arms ownership widgets
        for (var i = 0; i < 6; i++)
        {
            name = $"STGNUM{i + 2}";

            // gray #
            _arms[i][0] = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpName(name, PurgeTag.Static)!);

            // yellow #
            _arms[i][1] = _shortNum[i + 2];
        }

        // face backgrounds for different color players
        name = $"STGNUM{DoomGame.Instance.Game.ConsolePlayer}";
        _faceBackground = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpName(name, PurgeTag.Static)!);

        // status bar background bits
        _statusBar = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpName("STBAR", PurgeTag.Static)!);

        // face states
        var facenum = 0;
        for (var i = 0; i < ST_NUMPAINFACES; i++)
        {
            for (var j = 0; j < ST_NUMSTRAIGHTFACES; j++)
            {
                name = $"STFST{i}{j}";
                _faces[facenum++] = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpName(name, PurgeTag.Static)!);
            }

            name = $"STFTR{i}0"; // turn right
            _faces[facenum++] = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpName(name, PurgeTag.Static)!);
            name = $"STFTL{i}0"; // turn left
            _faces[facenum++] = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpName(name, PurgeTag.Static)!);
            name = $"STFOUCH{i}"; // ouch!
            _faces[facenum++] = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpName(name, PurgeTag.Static)!);
            name = $"STFEVL{i}"; // evil grin ;)
            _faces[facenum++] = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpName(name, PurgeTag.Static)!);
            name = $"STFKILL{i}"; // pissed off
            _faces[facenum++] = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpName(name, PurgeTag.Static)!);
        }
        _faces[facenum++] = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpName("STFGOD0", PurgeTag.Static)!);
        _faces[facenum] = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpName("STFDEAD0", PurgeTag.Static)!);

        _minus = Patch.FromBytes(DoomGame.Instance.WadData.GetLumpName("STTMINUS", PurgeTag.Static)!);
    }

    private void InitData()
    {
        _firstTime = true;
        _player = DoomGame.Instance.Game.Players[DoomGame.Instance.Game.ConsolePlayer];

        _clock = 0;
        //_chatstate = StartChatState;
        //_gamestate = FirstPersonState;

        _statusBarOn = true;
        //_oldChat = _chat = false;
        //_cursorOn = false;

        _faceIndex = 0;
        _paletteIdx = 0;

        _oldHealth = -1;

        for (var i = 0; i < (int)WeaponType.NumberOfWeapons; i++)
        {
            _oldWeaponsOwned[i] = _player.WeaponOwned[i];
        }

        for (var i = 0; i < 3; i++)
        {
            _keyBoxes[i] = -1;
        }
    }

    private void DoPaletteStuff()
    {
        int palette;
        var cnt = _player!.DamageCount;

        if (_player.Powers[(int)PowerUpType.Strength] != 0)
        {
            // slowly fade the berzerk out
            var bzc = 12 - (_player.Powers[(int)PowerUpType.Strength] >> 6);

            if (bzc > cnt)
            {
                cnt = bzc;
            }
        }

        if (cnt != 0)
        {
            palette = (cnt + 7) >> 3;

            if (palette >= NUMREDPALS)
            {
                palette = NUMREDPALS - 1;
            }

            palette += STARTREDPALS;
        }

        else if (_player.BonusCount != 0)
        {
            palette = (_player.BonusCount + 7) >> 3;

            if (palette >= NUMBONUSPALS)
            {
                palette = NUMBONUSPALS - 1;
            }

            palette += STARTBONUSPALS;
        }

        else if (_player.Powers[(int)PowerUpType.IronFeet] > 4 * 32 || (_player.Powers[(int)PowerUpType.IronFeet] & 8) != 0)
        {
            palette = RADIATIONPAL;
        }
        else
        {
            palette = 0;
        }

        if (palette != _paletteIdx)
        {
            _paletteIdx = palette;
            DoomGame.Instance.Video.SetPalette(Palette, _paletteIdx);
        }
    }

    private void DoRefresh()
    {
        _firstTime = false;

        // draw status bar background to off-screen buff
        RefreshBackground();

        // and refresh all widgets
        DrawWidgets(true);
    }

    private void DiffDraw()
    {
        // update all widgets
        DrawWidgets(false);
    }

    private void RefreshBackground()
    {
        if (!_statusBarOn)
        {
            return;
        }

        DoomGame.Instance.Video.DrawPatch(X, 0, BG, _statusBar.Value);

        if (DoomGame.Instance.Game.NetGame)
        {
            DoomGame.Instance.Video.DrawPatch(ST_FX, 0, BG, _faceBackground.Value);
        }

        DoomGame.Instance.Video.CopyRectangle(X, 0, BG, Width, Height, X, Y, FG);
    }

    private void CreateWidgets()
    {
        // ready weapon ammo
        _weaponWidget = new NumberWidget(
            ST_AMMOX, 
            ST_AMMOY, 
            _tallNum!,
            () => _player!.Ammo[(int)WeaponInfo.GetByType(_player.ReadyWeapon).Ammo],
            () => _statusBarOn,
            ST_AMMOWIDTH);

        // the last weapon type
        _weaponWidget.Data = (int)_player!.ReadyWeapon;

        // health percentage
        _healthWidget = new PercentWidget(
            ST_HEALTHX,
            ST_HEALTHY,
            _tallNum!,
            () => _player.Health,
            () => _statusBarOn,
            _tallPercent.Value
        );
        
        // arms background
        _armsBackgroundWidget = new BinaryIconWidget(
            ST_ARMSBGX,
            ST_ARMSBGY,
            _armsBackground.Value,
            () => _notDeathMatch,
            () => _statusBarOn);

        // weapons owned
        for (var i = 0; i < 6; i++)
        {
            var weaponIdx = i;
            _armsWidgets[i] = new MultiIconWidget(
                ST_ARMSX + (i % 3) * ST_ARMSXSPACE,
                ST_ARMSY + (i / 3) * ST_ARMSYSPACE,
                _arms[i],
                () => _player.WeaponOwned[weaponIdx + 1] ? 1 : 0,
                () => _armsOn);
        }

        // frags sum
        _fragsWidget = new NumberWidget(
            ST_FRAGSX,
            ST_FRAGSY,
            _tallNum!,
            () => _fragsCount,
            () => _fragsOn,
            ST_FRAGSWIDTH);

        // faces
        _faceStatusWidget = new MultiIconWidget(
            ST_FACESX,
            ST_FACESY,
            _faces!,
            () => _faceIndex,
            () => _statusBarOn);

        // armor percentage - should be colored later
        _armorWidget = new PercentWidget(
            ST_ARMORX,
            ST_ARMORY,
            _tallNum!,
            () => _player.ArmorPoints,
            () => _statusBarOn, 
            _tallPercent.Value);

        // keyboxes 0-2
        _keyBoxWidgets[0] = new MultiIconWidget(
            ST_KEY0X,
            ST_KEY0Y,
            _keys!,
            () => _keyBoxes[0],
            () => _statusBarOn);

        _keyBoxWidgets[1] = new MultiIconWidget(
            ST_KEY1X,
            ST_KEY1Y,
            _keys!,
            () => _keyBoxes[1],
            () => _statusBarOn);

        _keyBoxWidgets[2] = new MultiIconWidget(
            ST_KEY2X,
            ST_KEY2Y,
            _keys!,
            () => _keyBoxes[2],
            () => _statusBarOn);

        // ammo count (all four kinds)
        _ammoWidgets[0] = new NumberWidget(
            ST_AMMO0X,
            ST_AMMO0Y,
            _shortNum!,
            () => _player.Ammo[0],
            () => _statusBarOn,
            ST_AMMO0WIDTH);

        _ammoWidgets[1] = new NumberWidget(
            ST_AMMO1X,
            ST_AMMO1Y,
            _shortNum!,
            () => _player.Ammo[1],
            () => _statusBarOn,
            ST_AMMO1WIDTH);

        _ammoWidgets[2] = new NumberWidget(
            ST_AMMO2X,
            ST_AMMO2Y,
            _shortNum!,
            () => _player.Ammo[2],
            () => _statusBarOn,
            ST_AMMO2WIDTH);

        _ammoWidgets[3] = new NumberWidget(
            ST_AMMO3X,
            ST_AMMO3Y,
            _shortNum!,
            () => _player.Ammo[3],
            () => _statusBarOn,
            ST_AMMO3WIDTH);

        // max ammo count (all four kinds)
        _maxAmmoWidgets[0] = new NumberWidget(
            ST_MAXAMMO0X,
            ST_MAXAMMO0Y,
            _shortNum!,
            () => _player.MaxAmmo[0],
            () => _statusBarOn,
            ST_MAXAMMO0WIDTH);

        _maxAmmoWidgets[1] = new NumberWidget(
            ST_MAXAMMO1X,
            ST_MAXAMMO1Y,
            _shortNum!,
            () => _player.MaxAmmo[1],
            () => _statusBarOn,
            ST_MAXAMMO1WIDTH);

        _maxAmmoWidgets[2] = new NumberWidget(
            ST_MAXAMMO2X,
            ST_MAXAMMO2Y,
            _shortNum!,
            () => _player.MaxAmmo[2],
            () => _statusBarOn,
            ST_MAXAMMO2WIDTH);

        _maxAmmoWidgets[3] = new NumberWidget(
            ST_MAXAMMO3X,
            ST_MAXAMMO3Y,
            _shortNum!,
            () => _player.MaxAmmo[3],
            () => _statusBarOn,
            ST_MAXAMMO3WIDTH);
    }

    private void DrawWidgets(bool refresh)
    {
        var deathMatch = DoomGame.Instance.Game.DeathMatch;
        // used by w_arms[] widgets
        _armsOn = _statusBarOn && !deathMatch;
        // used by w_frags widget
        _fragsOn = deathMatch && _statusBarOn;

        _weaponWidget!.Update(refresh);

        for (var i = 0; i < 4; i++)
        {
            _ammoWidgets[i]!.Update(refresh);
            _maxAmmoWidgets[i]!.Update(refresh);
        }

        _healthWidget!.Update(refresh);
        _armorWidget!.Update(refresh);

        _armsBackgroundWidget!.Update(refresh);

        for (var i = 0; i < 6; i++)
        {
            _armsWidgets[i]!.Update(refresh);
        }

        _faceStatusWidget!.Update(refresh);

        for (var i = 0; i < 3; i++)
        {
            _keyBoxWidgets[i]!.Update(refresh);
        }
        
        _fragsWidget!.Update(refresh);
    }

    private int _lastCalc;
    private int _lastCalcHealth = -1;
    private int CalcPainOffset()
    {
        var health = _player!.Health > 100 ? 100 : _player.Health;
        if (health != _lastCalcHealth)
        {
            _lastCalc = ST_FACESTRIDE * (((100 - health) * ST_NUMPAINFACES) / 101);
            _lastCalcHealth = health;
        }

        return _lastCalc;
    }

    /// <summary>
    /// This is a not-very-pretty routine which handles
    ///  the face states and their timing.
    /// the precedence of expressions is:
    ///  dead > evil grin > turned head > straight ahead
    /// </summary>
    private int _ufwLastAttackDown = -1;
    private int _ufwPriority = 0;
    private void UpdateFaceWidget()
    {
        if (_ufwPriority < 10)
        {
            // dead
            if (_player!.Health == 0)
            {
                _ufwPriority = 9;
                _faceIndex = ST_DEADFACE;
                _faceCount = 1;
            }
        }

        if (_ufwPriority < 9)
        {
            if (_player!.BonusCount != 0)
            {
                // picking up bonus
                var doEvilGrin = false;

                for (var i = 0; i < (int)WeaponType.NumberOfWeapons; i++)
                {
                    if (_oldWeaponsOwned[i] != _player.WeaponOwned[i])
                    {
                        doEvilGrin = true;
                        _oldWeaponsOwned[i] = _player.WeaponOwned[i];
                    }
                }
                if (doEvilGrin)
                {
                    // evil grin if just picked up weapon
                    _ufwPriority = 8;
                    _faceCount = ST_EVILGRINCOUNT;
                    _faceIndex = CalcPainOffset() + ST_EVILGRINOFFSET;
                }
            }

        }

        if (_ufwPriority < 8)
        {
            if (_player!.DamageCount != 0
                && _player.Attacker != null
                && _player.Attacker != _player.MapObject)
            {
                // being attacked
                _ufwPriority = 7;

                if (_player.Health - _oldHealth > ST_MUCHPAIN)
                {
                    _faceCount = ST_TURNCOUNT;
                    _faceIndex = CalcPainOffset() + ST_OUCHOFFSET;
                }
                else
                {
                    var badGuyAngle = DoomGame.Instance.Renderer.PointToAngle2(_player.MapObject!.X, _player.MapObject.Y, _player.Attacker.X, _player.Attacker.Y);

                    Angle diffAngle;
                    bool faceRight;
                    if (badGuyAngle > _player.MapObject.Angle)
                    {
                        // whether right or left
                        diffAngle = badGuyAngle - _player.MapObject.Angle;
                        faceRight = diffAngle > Angle.Angle180;
                    }
                    else
                    {
                        // whether left or right
                        diffAngle = _player.MapObject.Angle - badGuyAngle;
                        faceRight = diffAngle <= Angle.Angle180;
                    } // confusing, aint it?


                    _faceCount = ST_TURNCOUNT;
                    _faceIndex = CalcPainOffset();

                    if (diffAngle < Angle.Angle45)
                    {
                        // head-on    
                        _faceIndex += ST_RAMPAGEOFFSET;
                    }
                    else if (faceRight)
                    {
                        // turn face right
                        _faceIndex += ST_TURNOFFSET;
                    }
                    else
                    {
                        // turn face left
                        _faceIndex += ST_TURNOFFSET + 1;
                    }
                }
            }
        }

        if (_ufwPriority < 7)
        {
            // getting hurt because of your own damn stupidity
            if (_player!.DamageCount != 0)
            {
                if (_player.Health - _oldHealth > ST_MUCHPAIN)
                {
                    _ufwPriority = 7;
                    _faceCount = ST_TURNCOUNT;
                    _faceIndex = CalcPainOffset() + ST_OUCHOFFSET;
                }
                else
                {
                    _ufwPriority = 6;
                    _faceCount = ST_TURNCOUNT;
                    _faceIndex = CalcPainOffset() + ST_RAMPAGEOFFSET;
                }
            }
        }

        if (_ufwPriority < 6)
        {
            // rapid firing
            if (_player!.AttackDown)
            {
                if (_ufwLastAttackDown == -1)
                {
                    _ufwLastAttackDown = ST_RAMPAGEDELAY;
                }
                else if (--_ufwLastAttackDown == 0)
                {
                    _ufwPriority = 5;
                    _faceIndex = CalcPainOffset() + ST_RAMPAGEOFFSET;
                    _faceCount = 1;
                    _ufwLastAttackDown = 1;
                }
            }
            else
            {
                _ufwLastAttackDown = -1;
            }
        }

        if (_ufwPriority < 5)
        {
            // invulnerability
            if ((_player!.Cheats & Cheat.GodMode) != 0 || _player.Powers[(int)PowerUpType.Invulnerability] != 0)
            {
                _ufwPriority = 4;

                _faceIndex = ST_GODFACE;
                _faceCount = 1;
            }
        }

        // look left or look right if the facecount has timed out
        if (_faceCount == 0)
        {
            _faceIndex = CalcPainOffset() + (_randomNumber % 3);
            _faceCount = ST_STRAIGHTFACECOUNT;
            _ufwPriority = 0;
        }

        _faceCount--;
    }

    private void UpdateWidgets()
    {
        const int largeAmmo = 1994;

        var ammoType = WeaponInfo.GetByType(_player!.ReadyWeapon).Ammo;
        // must redirect the pointer if the ready weapon has changed.
        //  if (w_ready.data != plyr->readyweapon)
        //  {
        if (ammoType == AmmoType.NoAmmo)
        {
            _weaponWidget!.NumFunc = () => largeAmmo;
        }
        else
        {
            _weaponWidget!.NumFunc = () => _player.Ammo[(int)ammoType];
        }
        //{
        // static int tic=0;
        // static int dir=-1;
        // if (!(tic&15))
        //   plyr->ammo[weaponinfo[plyr->readyweapon].ammo]+=dir;
        // if (plyr->ammo[weaponinfo[plyr->readyweapon].ammo] == -100)
        //   dir = 1;
        // tic++;
        // }
        _weaponWidget.Data = (int)_player.ReadyWeapon;

        // if (*w_ready.on)
        //  STlib_updateNum(&w_ready, true);
        // refresh weapon change
        //  }

        // update keycard multiple widgets
        for (var i = 0; i < 3; i++)
        {
            _keyBoxes[i] = _player.Cards[i] ? i : -1;

            if (_player.Cards[i + 3])
            {
                _keyBoxes[i] = i + 3;
            }
        }

        // refresh everything if this is him coming back to life
        UpdateFaceWidget();

        // used by the w_armsbg widget
        var deathMatch = DoomGame.Instance.Game.DeathMatch;
        _notDeathMatch = !deathMatch;

        // used by w_arms[] widgets
        _armsOn = _statusBarOn && !deathMatch;

        // used by w_frags widget
        _fragsOn = deathMatch && _statusBarOn;
        _fragsCount = 0;

        for (var i = 0; i < Constants.MaxPlayers; i++)
        {
            if (i != DoomGame.Instance.Game.ConsolePlayer)
            {
                _fragsCount += _player.Frags[i];
            }
            else
            {
                _fragsCount -= _player.Frags[i];
            }
        }

        //// get rid of chat window if up because of message
        //if (!--st_msgcounter)
        //    st_chat = st_oldchat;
    }

    private abstract class Widget
    {
        protected Widget(int x, int y, Func<bool> onFunc)
        {
            X = x;
            Y = y;
            OnFunc = onFunc;
        }

        public int X { get; }
        public int Y { get; }

        public Func<bool> OnFunc { get; }

        // user data
        public int Data { get; set; }

        public virtual void Update(bool refresh)
        {
            if (!OnFunc())
            {
                return;
            }

            Draw(refresh);
        }

        protected abstract void Draw(bool refresh);
    }

    private class NumberWidget : Widget
    {
        public NumberWidget(int x, int y, Patch?[] digits, Func<int> numFunc, Func<bool> onFunc, int width) 
            : base(x, y, onFunc)
        {
            Digits = digits;
            NumFunc = numFunc;
            Width = width;
        }

        // max # of digits in number
        public int Width { get; }

        // last number value
        public int OldNum { get; set; }

        // pointer to current value
        public Func<int> NumFunc { get; set; }

        // list of patches for 0-9
        public Patch?[] Digits { get; }

        protected override void Draw(bool refresh)
        {
            var numdigits = Width;
            var num = NumFunc();

            var w = Digits[0].Value.Width;
            var h = Digits[0].Value.Height;

            OldNum = NumFunc();

            var neg = num < 0;

            if (neg)
            {
                if (numdigits == 2 && num < -9)
                {
                    num = -9;
                }
                else if (numdigits == 3 && num < -99)
                {
                    num = -99;
                }

                num = -num;
            }

            // clear the area
            var x = X - numdigits * w;

            if (Y - StatusBar.Y < 0)
            {
                DoomGame.Error("drawNum: n->y - ST_Y < 0");
                return;
            }

            DoomGame.Instance.Video.CopyRectangle(x, Y - StatusBar.Y, BG, w * numdigits, h, x, Y, FG);

            // if non-number, do not draw it
            if (num == 1994)
            {
                return;
            }

            x = X;

            // in the special case of 0, you draw 0
            if (num == 0)
            {
                DoomGame.Instance.Video.DrawPatch(x - w, Y, FG, Digits[0].Value);
            }

            // draw the new number
            while (num != 0 && numdigits-- != 0)
            {
                x -= w;
                DoomGame.Instance.Video.DrawPatch(x, Y, FG, Digits[num % 10].Value);
                num /= 10;
            }

            // draw a minus sign if necessary
            if (neg)
            {
                DoomGame.Instance.Video.DrawPatch(x - 8, Y, FG, DoomGame.Instance.StatusBar._minus.Value);
            }
        }
    }

    private class PercentWidget : NumberWidget
    {
        public PercentWidget(int x, int y, Patch?[] digits, Func<int> numFunc, Func<bool> onFunc, Patch percentSign) : base(x, y, digits, numFunc, onFunc, 3)
        {
            PercentSign = percentSign;
        }

        public Patch? PercentSign { get; }

        public override void Update(bool refresh)
        {
            if (refresh && OnFunc())
            {
                DoomGame.Instance.Video.DrawPatch(X, Y, FG, PercentSign.Value);
            }

            base.Update(refresh);
        }
    }

    private class MultiIconWidget : Widget
    {
        public MultiIconWidget(int x, int y, Patch?[] icons, Func<int> iconNumberFunc, Func<bool> onFunc)
            : base(x, y, onFunc)
        {
            Icons = icons;
            IconNumberFunc = iconNumberFunc;
        }

        // last icon number
        public int OldIconNumber { get; set; }

        // pointer to current icon
        public Func<int> IconNumberFunc { get; }

        // list of icons
        public Patch?[] Icons { get; }

        public override void Update(bool refresh)
        {
            if (OnFunc() &&
                (OldIconNumber != IconNumberFunc() || refresh) &&
                (IconNumberFunc() != -1))
            {
                Draw(refresh);
            }
        }

        protected override void Draw(bool refresh)
        {
            if (OldIconNumber != -1)
            {
                var x = X - Icons![OldIconNumber].Value.LeftOffset;
                var y = Y - Icons[OldIconNumber].Value.TopOffset;
                var w = Icons[OldIconNumber].Value.Width;
                var h = Icons[OldIconNumber].Value.Height;

                if (y - StatusBar.Y < 0)
                {
                    DoomGame.Error("updateMultIcon: y - ST_Y < 0");
                    return;
                }

                DoomGame.Instance.Video.CopyRectangle(x, y - StatusBar.Y, BG, w, h, x, y, FG);
            }

            DoomGame.Instance.Video.DrawPatch(X, Y, FG, Icons![IconNumberFunc()].Value);
            OldIconNumber = IconNumberFunc();
        }
    }

    private class BinaryIconWidget : Widget
    {
        public BinaryIconWidget(int x, int y, Patch icon, Func<bool> valueFunc, Func<bool> onFunc)
            : base(x, y, onFunc)
        {
            Icon = icon;
            ValueFunc = valueFunc;
        }

        // last value
        public bool OldValue { get; set; }

        // pointer to current value
        public Func<bool> ValueFunc { get; }

        // list of icons
        public Patch? Icon { get; }

        public override void Update(bool refresh)
        {
            if (OnFunc() &&
                (OldValue != ValueFunc() || refresh))
            {
                Draw(refresh);
            }
        }

        protected override void Draw(bool refresh)
        {
            var x = X - Icon.Value.LeftOffset;
            var y = Y - Icon.Value.TopOffset;
            var w = Icon.Value.Width;
            var h = Icon.Value.Height;

            if (y - StatusBar.Y < 0)
            {
                DoomGame.Error("updateBinIcon: y - ST_Y < 0");
                return;
            }

            if (ValueFunc())
            {
                DoomGame.Instance.Video.DrawPatch(X, Y, FG, Icon.Value);
            }
            else
            {
                DoomGame.Instance.Video.CopyRectangle(x, y - StatusBar.Y, BG, w, h, x, y, FG);
            }

            OldValue = ValueFunc();
        }
    }

    public bool HandleEvent(InputEvent currentEvent)
    {
        //// Filter automap on/off.
        //if (ev->type == ev_keyup
        //    && ((ev->data1 & 0xffff0000) == AM_MSGHEADER))
        //{
        //    switch (ev->data1)
        //    {
        //        case AM_MSGENTERED:
        //            st_gamestate = AutomapState;
        //            st_firsttime = true;
        //            break;

        //        case AM_MSGEXITED:
        //            //	fprintf(stderr, "AM exited\n");
        //            st_gamestate = FirstPersonState;
        //            break;
        //    }
        //}

        //// if a user keypress...
        // else if 
        if (currentEvent.Type == EventType.KeyDown)
        {
            if (!DoomGame.Instance.Game.NetGame)
            {
                // cheats go here
            }
        }

        return false;
    }
}