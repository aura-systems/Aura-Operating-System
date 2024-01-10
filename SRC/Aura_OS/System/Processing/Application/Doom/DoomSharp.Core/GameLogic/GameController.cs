using DoomSharp.Core.Data;
using DoomSharp.Core.Input;
using DoomSharp.Core.Networking;
using DoomSharp.Core.Graphics;
using DoomSharp.Core.Sound;
using DoomSharp.Core.UI;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

namespace DoomSharp.Core.GameLogic;

public class GameController
{
    public static readonly int[] MaxAmmo = { 200, 50, 300, 50 };
    public static readonly int[] ClipAmmo = { 10, 4, 20, 1 };

    public const int BonusAdd = 6;

    private bool _secretExit;

    private int _saveGameSlot = 0;
    private string _saveDescription = "";

    private byte[] _demoData = Array.Empty<byte>();
    private int _demoDataIdx = -1;
    private int _demoEnd = -1;
    private SkillLevel _d_skill;
    private int _d_episode;
    private int _d_map;

    private WorldMapInfo _wmInfo = new();

    private readonly short[][] _consistency = new short[Constants.MaxPlayers][];

    // 
    // controls (have defaults) 
    // 
    private int _keyRight = (int)Keys.RightArrow;
    private int _keyLeft = (int)Keys.LeftArrow;

    private int _keyUp = (int)Keys.UpArrow;
    private int _keyDown = (int)Keys.DownArrow;
    private int _keyStrafeLeft = 'A';
    private int _keyStrafeRight = 'D';
    private int _keyFire = ' ';
    private int _keyUse = (int)Keys.Enter;
    private int _keyStrafe = (int)Keys.LAlt;
    private int _keySpeed = (int)Keys.RShift;

    private int _mouseButtonFire = 0;
    private int _mouseButtonStrafe = 2;
    private int _mouseButtonForward = 1;

    private int _joyButtonFire = 0;
    private int _joyButtonStrafe = 1;
    private int _joyButtonUse = 2;
    private int _joyButtonSpeed = 3;

    public const int TurboThreshold = 0x32;

    private static readonly int[] ForwardMove = { 0x19, 0x32 };
    private static readonly int[] SideMove = { 0x18, 0x28 };
    private static readonly int[] AngleTurn = { 640, 1280, 320 };

    public const int SlowTurnTics = 6;
    public const int NumKeys = 256;

    private readonly bool[] _gameKeyDown = new bool[NumKeys];
    private int _turnHeld;

    private readonly bool[] _mouseButtons = new bool[4];

    // mouse values are used once 
    private int _mouseX;
    private int _mouseY;

    private int _doubleClickTime;
    private bool _doubleClickState;
    private int _doubleClicks;
    private int _doubleClickTime2;
    private bool _doubleClickState2;
    private int _doubleClicks2;

    // joystick values are repeated 
    private int _joyXMove;
    private int _joyYMove;
    private bool[] _joyButtons = new bool[5];

    public int LevelTime { get; private set; }
    private LinkedList<Thinker> _thinkers = new();

    public const int BodyQueueSize = 32;
    private MapObject[] _bodyQueue = new MapObject[BodyQueueSize];
    private int _bodyQueueSlot = 0;

    //
    // MAP related Lookup tables.
    // Store VERTEXES, LINEDEFS, SIDEDEFS, etc.
    //
    private int _numVertices;
    private Vertex[] _vertices = Array.Empty<Vertex>();

    private int _numSegments;
    private Segment[] _segments = Array.Empty<Segment>();

    private int _numSectors;
    private List<Sector> _sectors = new();

    private int _numSubSectors;
    private SubSector[] _subSectors = Array.Empty<SubSector>();

    private int _numNodes;
    private Node[] _nodes = Array.Empty<Node>();

    private int _numLines;
    private Line[] _lines = Array.Empty<Line>();

    private int _numSides;
    private SideDef[] _sides = Array.Empty<SideDef>();

    // BLOCKMAP
    // Created from axis aligned bounding box
    // of the map, a rectangular array of
    // blocks of size ...
    // Used to speed up collision detection
    // by spatial subdivision in 2D.
    //
    // Blockmap size.
    private int _blockMapWidth;
    private int _blockMapHeight; // size in map blocks

    private ushort[] _blockMapOffsets = Array.Empty<ushort>();

    private short[] _blockMap = Array.Empty<short>(); // int for larger maps TODO ????
    // offsets in blockmap are from here
    private short[] _blockMapLump = Array.Empty<short>();
    // origin of block map
    private Fixed _blockMapOriginX = Fixed.Zero;
    private Fixed _blockMapOriginY = Fixed.Zero;
    // For thing chains
    private MapObject?[] _blockLinks = Array.Empty<MapObject?>();

    // REJECT
    // For fast sight rejection.
    // Speeds up enemy AI by skipping detailed
    //  LineOf Sight calculation.
    // Without special effect, this could be
    //  used as a PVS lookup as well.
    //
    private byte[] _rejectMatrix = Array.Empty<byte>();

    // Maintain single and multi player starting spots.
    public const int MaxDeathMatchStarts = 10;

    private MapThing[] _deathMatchStarts = new MapThing[MaxDeathMatchStarts];
    private int _deathMatchStartIdx = 0;
    private MapThing[] _playerStarts = new MapThing[Constants.MaxPlayers];

    private readonly int[] _switchList = new int[Constants.MaxSwitches * 2];
    private int _numSwitches = -1;
    private readonly Button[] _buttonList = new Button[Constants.MaxButtons];

    private readonly AnimatingItem[] _animations = new AnimatingItem[Constants.MaxAnimations];
    private AnimatingItem? _lastAnimation = null;

    private int _numLineSpecials;
    private Line[] _lineSpecialList = new Line[Constants.MaxLineAnimations];

    private readonly Ceiling?[] _activeCeilings = new Ceiling?[Ceiling.MaxCeilings];
    private readonly Platform?[] _activePlats = new Platform?[Platform.MaxPlats];

    // Map tracking

    private Fixed[] _tmBoundingBox = { Fixed.Zero, Fixed.Zero, Fixed.Zero, Fixed.Zero };
    private MapObject? _tmThing;
    private MapObjectFlag _tmFlags;
    private Fixed _tmX;
    private Fixed _tmY;

    // If "floatok" is true, move would be ok if within "tmfloorz - tmceilingz"
    private bool _floatOk;

    private Fixed _tmFloorZ;
    private Fixed _tmCeilingZ;
    private Fixed _tmDropOffZ;

    // keep track of the line that lowers the ceiling,
    // so missiles don't explode against sky hack walls
    private Line? _ceilingLine;

    // keep track of special lines as they are hit,
    // but don't process them until the move is proven valid
    private const int MaxSpecialCross = 8;

    private readonly Line[] _specHit = new Line[MaxSpecialCross];
    private int _numSpecHit;

    private MapObject? _lineTarget;
    private MapObject? _shootThing;

    // Height if not aiming up or down
    private Fixed _shootZ;

    private int _laDamage;
    private Fixed _attackRange;
    
    private Fixed _aimSlope;

    public GameController()
    {
        for (var i = 0; i < Constants.MaxButtons; i++)
        {
            _buttonList[i] = new();
        }

        for (var i = 0; i < Constants.MaxPlayers; i++)
        {
            InitPlayer(i);
            _consistency[i] = new short[Constants.BackupTics];
        }
    }

    public GameAction GameAction { get; set; } = GameAction.Nothing;
    public GameState GameState { get; set; } = GameState.Wipe;
    public GameState WipeGameState { get; set; } = GameState.DemoScreen;

    public SkillLevel GameSkill { get; set; }
    public bool RespawnMonsters { get; set; }
    public int GameEpisode { get; set; }
    public int GameMap { get; set; }

    public bool Paused { get; set; }
    public bool SendPause { get; set; }
    public bool SendSave { get; set; }
    public bool UserGame { get; set; }

    public bool TimingDemo { get; set; }
    public bool NoDrawers { get; set; }
    public bool NoBlit { get; set; }
    public int StartTime { get; set; }

    public bool ViewActive { get; set; }

    public bool DeathMatch { get; set; }
    public bool NetGame { get; set; } = false;
    public bool[] PlayerInGame { get; } = new bool[Constants.MaxPlayers];
    public Player[] Players { get; } = new Player[Constants.MaxPlayers];

    public int ConsolePlayer { get; set; } = 0;
    public int DisplayPlayer { get; set; } = 0;
    public int GameTic { get; set; } = 0;
    public int LevelStartTic { get; set; } = 0;
    public int TotalKills { get; set; } = 0;
    public int TotalItems { get; set; } = 0;
    public int TotalSecrets { get; set; } = 0;

    public string DemoName { get; set; } = "";
    public bool DemoRecording { get; set; }
    public bool DemoPlayback { get; set; }
    public bool NetDemo { get; set; }
    public bool SingleDemo { get; set; }

    /// <summary>
    /// If true, load all graphics at start
    /// </summary>
    public bool PreCache { get; set; } = true;

    public LinkedList<Thinker> Thinkers => _thinkers;

    public int NumSectors => _numSectors;
    public List<Sector> Sectors => _sectors;

    public int NumSides => _numSides;
    public SideDef[] Sides => _sides;

    public int NumNodes => _numNodes;
    public Node[] Nodes => _nodes;

    public int NumSubSectors => _numSubSectors;
    public SubSector[] SubSectors => _subSectors;

    public int NumSegments => _numSegments;
    public Segment[] Segments => _segments;
    public bool AutomapActive { get; set; }

    public MapObject? LineTarget => _lineTarget;
    public Fixed BulletSlope => _bulletSlope;

    public void Ticker()
    {
        var buf = 0;

        // do player reborns if needed
        for (var i = 0; i < Constants.MaxPlayers; i++)
        {
            if (PlayerInGame[i] && Players[i].PlayerState == PlayerState.Reborn)
            {
                DoReborn(i);
            }
        }

        // do things to change the game state
        while (GameAction != GameAction.Nothing)
        {
            switch (GameAction)
            {
                case GameAction.LoadLevel:
                    DoLoadLevel();
                    break;
                case GameAction.NewGame:
                    DoNewGame();
                    break;
                case GameAction.LoadGame:
                    // G_DoLoadGame();
                    break;
                case GameAction.SaveGame:
                    // G_DoSaveGame();
                    break;
                case GameAction.PlayDemo:
                    DoPlayDemo();
                    break;
                case GameAction.Completed:
                    DoCompleted();
                    break;
                case GameAction.Victory:
                    // F_StartFinale();
                    break;
                case GameAction.WorldDone:
                    DoWorldDone();
                    break;
                case GameAction.Screenshot:
                    // M_ScreenShot();
                    GameAction = GameAction.Nothing;
                    break;
                case GameAction.Nothing:
                    break;
            }
        }

        // get commands, check consistancy,
        // and build new consistancy check
        buf = GameTic / DoomGame.Instance.TicDup % Constants.BackupTics;

        for (var i = 0; i < Constants.MaxPlayers; i++)
        {
            if (PlayerInGame[i])
            {
                var cmd = DoomGame.Instance.NetCommands[i][buf] ?? new();

                if (DemoPlayback)
                {
                    ReadDemoTicCommand(cmd);
                }

                if (DemoRecording)
                {
                    WriteDemoTicCommand(cmd);
                }

                Players[i].Command = cmd;

                // check for turbo cheats
                if (cmd.ForwardMove > TurboThreshold && (GameTic & 31) == 0 && ((GameTic >> 5) & 3) == i)
                {
                    //static char turbomessage[80];
                    //extern char* player_names[4];
                    //sprintf(turbomessage, "%s is turbo!", player_names[i]);
                    Players[ConsolePlayer].Message = "Player is turbo!";
                }

                if (NetGame && !NetDemo && (GameTic % DoomGame.Instance.TicDup) == 0)
                {
                    if (GameTic > Constants.BackupTics && _consistency[i][buf] != cmd.Consistency)
                    {
                        DoomGame.Error($"consistency failure ({cmd.Consistency} should be {_consistency[i][buf]})");
                        return;
                    }

                    if (Players[i].MapObject != null)
                    {
                        _consistency[i][buf] = (short)Players[i].MapObject!.X.Value;
                    }
                    else
                    {
                        _consistency[i][buf] = (short)DoomRandom.RandomIndex;
                    }
                }
            }
        }

        // check for special buttons
        for (var i = 0; i < Constants.MaxPlayers; i++)
        {
            if (PlayerInGame[i])
            {
                if ((Players[i].Command.Buttons & ButtonCode.Special) != 0)
                {
                    switch (Players[i].Command.Buttons & ButtonCode.SpecialMask)
                    {
                        case ButtonCode.Pause:
                            Paused = !Paused;
                            if (Paused)
                            {
                                DoomGame.Instance.Sound.PauseSound();
                            }
                            else
                            {
                                DoomGame.Instance.Sound.ResumeSound();
                            }

                            break;

                        case ButtonCode.SaveGame:
                            if (string.IsNullOrWhiteSpace(_saveDescription))
                            {
                                _saveDescription = "NET GAME";
                            }

                            _saveGameSlot = (int)(Players[i].Command.Buttons & ButtonCode.SaveMask) >> (int)ButtonCode.SaveShift;
                            GameAction = GameAction.SaveGame;
                            break;
                    }
                }
            }
        }

        // do main actions
        switch (GameState)
        {
            case GameState.Level:
                P_Ticker();
                DoomGame.Instance.StatusBar.Ticker();
                //AM_Ticker();
                DoomGame.Instance.Hud.Ticker();
                break;

            case GameState.Intermission:
                DoomGame.Instance.Intermission.Ticker();
                break;

            case GameState.Finale:
                // F_Ticker();
                break;

            case GameState.DemoScreen:
                DoomGame.Instance.PageTicker();
                break;
        }
    }

    //
    // G_InitPlayer 
    // Called at the start.
    // Called by the game initialization functions.
    //
    public void InitPlayer(int player)
    {
        // set up the saved info         
        Players[player] = new Player();

        // clear everything else to defaults 
        PlayerReborn(player);
    }

    /// <summary>
    /// Can when a player completes a level.
    /// </summary>
    public void PlayerFinishLevel(int player)
    {
        var p = Players[player];

        Array.Clear(p.Powers);
        Array.Clear(p.Cards);
        p.MapObject!.Flags &= ~MapObjectFlag.MF_SHADOW; // cancel invisibility
        p.ExtraLight = 0; // cancel gun flashes
        p.FixedColorMap = 0; // cancel IR goggles
        p.DamageCount = 0; // no palette changes
        p.BonusCount = 0;
    }

    /// <summary>
    /// Called after a player dies 
    /// almost everything is cleared and initialized 
    /// </summary>
    private Player PlayerReborn(int player)
    {
        var frags = Players[player].Frags;
        var killcount = Players[player].KillCount;
        var itemcount = Players[player].ItemCount;
        var secretcount = Players[player].SecretCount;

        var p = new Player();
        Players[player] = p;
        p.Frags = frags;
        p.KillCount = killcount;
        p.ItemCount = itemcount;
        p.SecretCount = secretcount;

        p.UseDown = p.AttackDown = true;  // don't do anything immediately 
        p.PlayerState = PlayerState.Alive;
        p.Health = Constants.MaxHealth;
        p.ReadyWeapon = p.PendingWeapon = WeaponType.Pistol;
        p.WeaponOwned[(int)WeaponType.Fist] = true;
        p.WeaponOwned[(int)WeaponType.Pistol] = true;
        p.Ammo[(int)AmmoType.Clip] = 50;

        for (var i = 0; i < (int)AmmoType.NumAmmo; i++)
        {
            p.MaxAmmo[i] = MaxAmmo[i];
        }

        return p;
    }

    /// <summary>
    /// Returns false if the player cannot be respawned
    /// at the given mapthing_t spot  
    /// because something is occupying it 
    /// </summary>
    private bool CheckSpot(int playernum, MapThing mthing)
    {
        if (Players[playernum].MapObject == null)
        {
            // first spawn of level, before corpses
            for (var i = 0; i < playernum; i++)
            {
                if (Players[i].MapObject!.X == Fixed.FromInt(mthing.X) && Players[i].MapObject!.Y == Fixed.FromInt(mthing.Y))
                {
                    return false;
                } 
            }
            return true;
        }

        var x = Fixed.FromInt(mthing.X);
        var y = Fixed.FromInt(mthing.Y);

        if (!P_CheckPosition(Players[playernum].MapObject!, x, y))
        {
            return false;
        }

        // flush an old corpse if needed 
        if (_bodyQueueSlot >= BodyQueueSize)
        {
            P_RemoveMapObject(_bodyQueue[_bodyQueueSlot % BodyQueueSize]);
        }
        _bodyQueue[_bodyQueueSlot % BodyQueueSize] = Players[playernum].MapObject!;
        _bodyQueueSlot++;

        // spawn a teleport fog 
        var ss = DoomGame.Instance.Renderer.PointInSubSector(x, y);
        var mapThingAngle = new Angle(Angle.Angle45.Value * (uint)(mthing.Angle / 45));
        var an = (int)((Angle.Angle45.Value >> DoomMath.AngleToFineShift) *
                    ((int)Math.Round(mapThingAngle.ToDegrees()) / 45));

        var mo = P_SpawnMapObject(x + 20 * DoomMath.Cos(an), y + 20 * DoomMath.Sin(an), ss.Sector!.FloorHeight, MapObjectType.MT_TFOG);

        if (Players[ConsolePlayer].ViewZ != new Fixed(1))
        {
            DoomGame.Instance.Sound.StartSound(mo, SoundType.sfx_telept); // don't start sound on first frame 
        }

        return true;
    }

    /// <summary>
    /// Spawns a player at one of the random death match spots 
    /// called at level load and each death 
    /// </summary>
    private void DeathMatchSpawnPlayer(int playerNum)
    {
        var selections = _deathMatchStartIdx;
        if (selections < 4)
        {
            DoomGame.Error($"Only {selections} deathmatch spots, 4 required");
            return;
        }

        for (var j = 0; j < 20; j++)
        {
            var i = DoomRandom.P_Random() % selections;
            if (CheckSpot(playerNum, _deathMatchStarts[i]))
            {
                _deathMatchStarts[i].Type = (short)(playerNum + 1);
                P_SpawnPlayer(_deathMatchStarts[i]);
                return;
            }
        }

        // no good spot, so the player will probably get stuck 
        P_SpawnPlayer(_playerStarts[playerNum]);
    }

    private void DoReborn(int player)
    {
        if (!NetGame)
        {
            GameAction = GameAction.LoadLevel;
        }
        else
        {
            // respawn at the start

            // first dissasociate the corpse 
            Players[player].MapObject!.Player = null;

            // spawn at random spot if in death match 
            if (DeathMatch)
            {
                DeathMatchSpawnPlayer(player);
                return;
            }

            if (CheckSpot(player, _playerStarts[player]))
            {
                P_SpawnPlayer(_playerStarts[player]);
                return;
            }

            // try to spawn at one of the other players spots 
            for (var i = 0; i < Constants.MaxPlayers; i++)
            {
                if (CheckSpot(player, _playerStarts[i]))
                {
                    _playerStarts[i].Type = (short)(player + 1);   // fake as other player 
                    P_SpawnPlayer(_playerStarts[i]);
                    _playerStarts[i].Type = (short)(i + 1);       // restore 
                    return;
                }
                // he's going to be inside something.  Too bad.
            }
            P_SpawnPlayer(_playerStarts[player]);
        }
    }

    public void DeferedPlayDemo(string demo)
    {
        DemoName = demo;
        GameAction = GameAction.PlayDemo;
    }

    private void DoPlayDemo()
    {
        GameAction = GameAction.Nothing;
        _demoDataIdx = 0;
        _demoData = DoomGame.Instance.WadData.GetLumpName(DemoName, PurgeTag.Static) ?? Array.Empty<byte>();
        if (_demoData.Length == 0 || _demoData[_demoDataIdx++] != DoomGame.Version)
        {
            DoomGame.Console.WriteLine("Demo is from a different game version!");
            GameAction = GameAction.Nothing;
            return;
        }

        var skill = (SkillLevel)_demoData[_demoDataIdx++];
        var episode = _demoData[_demoDataIdx++];
        var map = _demoData[_demoDataIdx++];
        DeathMatch = _demoData[_demoDataIdx++] != 0;
        var respawnparm = _demoData[_demoDataIdx++];
        var fastparm = _demoData[_demoDataIdx++];
        var nomonsters = _demoData[_demoDataIdx++];
        ConsolePlayer = _demoData[_demoDataIdx++];

        for (var i = 0; i < Constants.MaxPlayers; i++)
        {
            PlayerInGame[i] = _demoData[_demoDataIdx++] != 0;
        }
        if (PlayerInGame[1])
        {
            NetGame = true;
            NetDemo = true;
        }

        // don't spend a lot of time in loadlevel 
        PreCache = false;
        InitNew(skill, episode, map);
        PreCache = true;

        UserGame = false;
        DemoPlayback = true;
    }

    public void DeferedInitNew(SkillLevel skill, int episode, int map)
    {
        _d_skill = skill;
        _d_episode = episode;
        _d_map = map;
        GameAction = GameAction.NewGame;
    }

    public void DoNewGame()
    {
        DemoPlayback = false;
        NetDemo = false;
        NetGame = false;
        DeathMatch = false;
        PlayerInGame[1] = PlayerInGame[2] = PlayerInGame[3] = false;
        //respawnparm = false;
        //fastparm = false;
        //nomonsters = false;
        ConsolePlayer = 0;
        InitNew(_d_skill, _d_episode, _d_map);
        GameAction = GameAction.Nothing;
    }

    public void InitNew(SkillLevel skill, int episode, int map)
    {
        if (Paused)
        {
            Paused = false;
            DoomGame.Instance.Sound.ResumeSound();
        }

        // This was quite messy with SPECIAL and commented parts.
        // Supposedly hacks to make the latest edition work.
        // It might not work properly.
        if (episode < 1)
        {
            episode = 1;
        }

        if (DoomGame.Instance.GameMode == GameMode.Retail)
        {
            if (episode > 4)
            {
                episode = 4;
            }
        }
        else if (DoomGame.Instance.GameMode == GameMode.Shareware)
        {
            if (episode > 1)
            {
                episode = 1;    // only start episode 1 on shareware
            }
        }
        else
        {
            if (episode > 3)
            {
                episode = 3;
            }
        }

        if (map < 1)
        {
            map = 1;
        }

        if (map > 9 && DoomGame.Instance.GameMode != GameMode.Commercial)
        {
            map = 9;
        }

        DoomRandom.ClearRandom();

        //if (skill == sk_nightmare || respawnparm)
        //    respawnmonsters = true;
        //else
        //    respawnmonsters = false;

        //if (fastparm || (skill == sk_nightmare && gameskill != sk_nightmare))
        //{
        //    for (i = S_SARG_RUN1; i <= S_SARG_PAIN2; i++)
        //        states[i].tics >>= 1;
        //    mobjinfo[MT_BRUISERSHOT].speed = 20 * FRACUNIT;
        //    mobjinfo[MT_HEADSHOT].speed = 20 * FRACUNIT;
        //    mobjinfo[MT_TROOPSHOT].speed = 20 * FRACUNIT;
        //}
        //else if (skill != sk_nightmare && gameskill == sk_nightmare)
        //{
        //    for (i = S_SARG_RUN1; i <= S_SARG_PAIN2; i++)
        //        states[i].tics <<= 1;
        //    mobjinfo[MT_BRUISERSHOT].speed = 15 * FRACUNIT;
        //    mobjinfo[MT_HEADSHOT].speed = 10 * FRACUNIT;
        //    mobjinfo[MT_TROOPSHOT].speed = 10 * FRACUNIT;
        //}


        // force players to be initialized upon first level load         
        for (var i = 0; i < Constants.MaxPlayers; i++)
        {
            Players[i].PlayerState = PlayerState.Reborn;
        }

        UserGame = true;                // will be set false if a demo 
        Paused = false;
        DemoPlayback = false;
        DoomGame.Instance.AutoMapActive = false;
        ViewActive = true;
        GameEpisode = episode;
        GameMap = map;
        GameSkill = skill;

        ViewActive = true;

        var sky = DoomGame.Instance.Renderer.Sky;
        // set the sky map for the episode
        if (DoomGame.Instance.GameMode == GameMode.Commercial)
        {
            sky.Texture = DoomGame.Instance.Renderer.TextureNumForName("SKY3");
            if (GameMap < 12)
            {
                sky.Texture = DoomGame.Instance.Renderer.TextureNumForName("SKY1");
            }
            else if (GameMap < 21)
            {
                sky.Texture = DoomGame.Instance.Renderer.TextureNumForName("SKY2");
            }
        }
        else
        {
            switch (episode)
            {
                case 1:
                    sky.Texture = DoomGame.Instance.Renderer.TextureNumForName("SKY1");
                    break;
                case 2:
                    sky.Texture = DoomGame.Instance.Renderer.TextureNumForName("SKY2");
                    break;
                case 3:
                    sky.Texture = DoomGame.Instance.Renderer.TextureNumForName("SKY3");
                    break;
                case 4: // Special Edition sky
                    sky.Texture = DoomGame.Instance.Renderer.TextureNumForName("SKY4");
                    break;
            }
        }

        DoLoadLevel();
    }

    private void ReadDemoTicCommand(TicCommand cmd)
    {
        if (_demoData[_demoDataIdx] == Constants.DemoMarker)
        {
            // end of demo data stream 
            CheckDemoStatus();
            return;
        }

        cmd.ForwardMove = (sbyte)_demoData[_demoDataIdx++];
        cmd.SideMove = (sbyte)_demoData[_demoDataIdx++];
        cmd.AngleTurn = (short)(_demoData[_demoDataIdx++] << 8);
        cmd.Buttons = (ButtonCode)_demoData[_demoDataIdx++];
    }

    private void WriteDemoTicCommand(TicCommand cmd)
    {
        if (_gameKeyDown['q']) // press q to end demo recording 
        {
            CheckDemoStatus();
        }

        _demoData[_demoDataIdx++] = (byte)cmd.ForwardMove;
        _demoData[_demoDataIdx++] = (byte)cmd.SideMove;
        _demoData[_demoDataIdx++] = (byte)(cmd.AngleTurn + 128 >> 8);
        _demoData[_demoDataIdx++] = (byte)cmd.Buttons;

        _demoDataIdx -= 4;
        if (_demoDataIdx > _demoEnd - 16)
        {
            // no more space 
            CheckDemoStatus();
            return;
        }

        ReadDemoTicCommand(cmd);         // make SURE it is exactly the same 
    }

    private void RecordDemo(string name)
    {
        UserGame = false;
        DemoName = name + ".lmp";
        var maxSize = 0x20000;
        //i = M_CheckParm("-maxdemo");
        //if (i && i < myargc - 1)
        //    maxsize = atoi(myargv[i + 1]) * 1024;
        _demoData = new byte[maxSize];
        _demoEnd = maxSize;

        DemoRecording = true;
    }

    private void BeginRecording()
    {
        _demoDataIdx = 0;

        _demoData[_demoDataIdx++] = DoomGame.Version;
        _demoData[_demoDataIdx++] = (byte)GameSkill;
        _demoData[_demoDataIdx++] = (byte)GameEpisode;
        _demoData[_demoDataIdx++] = (byte)GameMap;
        _demoData[_demoDataIdx++] = (byte)(DeathMatch ? 1 : 0);
        _demoData[_demoDataIdx++] = 0; // respawnparam
        _demoData[_demoDataIdx++] = 0; // fastparm;
        _demoData[_demoDataIdx++] = 0; // nomonsters;
        _demoData[_demoDataIdx++] = (byte)ConsolePlayer;

        for (var i = 0; i < Constants.MaxPlayers; i++)
        {
            _demoData[_demoDataIdx++] = (byte)(PlayerInGame[i] ? 1 : 0);
        }
    }

    private void DoLoadLevel()
    {
        // Set the sky map.
        // First thing, we have a dummy sky texture name,
        //  a flat. The data is in the WAD only because
        //  we look for an actual index, instead of simply
        //  setting one.
        var sky = DoomGame.Instance.Renderer.Sky;
        sky.FlatNum = DoomGame.Instance.Renderer.FlatNumForName(Sky.FlatName);
        
        // DOOM determines the sky texture to be used
        // depending on the current episode, and the game version.
        if (DoomGame.Instance.GameMode == GameMode.Commercial) // or Pack TNT/Plutonium
        {
            sky.Texture = DoomGame.Instance.Renderer.TextureNumForName("SKY3");
            if (GameMap < 12)
            {
                sky.Texture = DoomGame.Instance.Renderer.TextureNumForName("SKY1");
            }
            else if (GameMap < 21)
            {
                sky.Texture = DoomGame.Instance.Renderer.TextureNumForName("SKY2");
            }
        }

        LevelStartTic = GameTic; // for time calculation

        if (WipeGameState == GameState.Level)
        {
            WipeGameState = GameState.Wipe; // force a wipe 
        }

        GameState = GameState.Level;

        for (var i = 0; i < Constants.MaxPlayers; i++)
        {
            if (PlayerInGame[i] && Players[i].PlayerState == PlayerState.Dead)
            {
                Players[i].PlayerState = PlayerState.Reborn;
            }

            for (var j = 0; j < Players[i].Frags.Length; j++)
            {
                Players[i].Frags[j] = 0;
            }
        }

        SetupLevel(GameEpisode, GameMap, 0, GameSkill);
        DisplayPlayer = ConsolePlayer;		// view the guy you are playing    
        StartTime = DoomGame.Instance.GetTime();
        GameAction = GameAction.Nothing;
        //Z_CheckHeap();

        // clear cmd building stuff
        for (var i = 0; i < _gameKeyDown.Length; i++)
        {
            _gameKeyDown[i] = false;
        }

        _joyXMove = _joyYMove = 0;
        _mouseX = _mouseY = 0;
        SendPause = SendSave = Paused = false;

        for (var i = 0; i < _mouseButtons.Length; i++)
        {
            _mouseButtons[i] = false;
        }

        for (var i = 0; i < _joyButtons.Length; i++)
        {
            _joyButtons[i] = false;
        }
    }

    public bool CheckDemoStatus()
    {
        if (TimingDemo)
        {
            var endTime = DoomGame.Instance.GetTime();
            DoomGame.Error($"timed {GameTic} gametics in {endTime - StartTime} realtics");
            return true;
        }

        if (DemoPlayback)
        {
            if (SingleDemo)
            {
                DoomGame.Instance.Quit();
                return true;
            }

            // Z_ChangeTag(demobuffer, PU_CACHE);
            DemoPlayback = false;
            NetDemo = false;
            NetGame = false;
            DeathMatch = false;
            PlayerInGame[1] = PlayerInGame[2] = PlayerInGame[3] = false;
            //respawnparm = false;
            //fastparm = false;
            //nomonsters = false;
            ConsolePlayer = 0;
            DoomGame.Instance.AdvanceDemo();
            return true;
        }

        if (DemoRecording)
        {
            _demoData[_demoDataIdx++] = Constants.DemoMarker;
            //M_WriteFile(demoname, demobuffer, demo_p - demobuffer);
            //Z_Free(demobuffer);
            DemoRecording = false;
            DoomGame.Error($"Demo {DemoName} recorded");
        }

        return false;
    }

    public void InitThinkers()
    {
        _thinkers = new LinkedList<Thinker>();
    }

    /// <summary>
    /// Adds a new thinker at the end of the list
    /// </summary>
    /// <param name="thinker"></param>
    public void AddThinker(Thinker thinker)
    {
        _thinkers.AddLast(thinker);
    }

    /// <summary>
    /// Deallocation is lazy -- it will not actually be freed until its thinking turn comes up.
    /// </summary>
    /// <param name="thinker"></param>
    public void RemoveThinker(Thinker thinker)
    {
        thinker.Action = null;
    }

    private void RunThinkers()
    {
        
        var thinkerNode = _thinkers.First;

        while (thinkerNode != null)
        {
            var thinker = thinkerNode.Value;
            if (thinker.Action == null)
            {
                // Time to remove this thinker
                var next = thinkerNode.Next;
                _thinkers.Remove(thinkerNode);
                thinkerNode = next;
            }
            else
            {
                thinker.Action(new ActionParams(thinker as MapObject, Thinker: thinker));
                thinkerNode = thinkerNode.Next;
            }
        }
    }

    public void P_Ticker()
    {
        // run the tic
        if (Paused)
        {
            return;
        }

        // pause if in menu and at least one tic has been run
        if (!NetGame
            && DoomGame.Instance.MenuActive
            && !DemoPlayback
            && Players[ConsolePlayer].ViewZ != new Fixed(1))
        {
            return;
        }


        for (var i = 0; i < Constants.MaxPlayers; i++)
        {
            if (PlayerInGame[i])
            {
                Players[i].Think();
            }
        }

        RunThinkers();
        UpdateSpecials();
        //RespawnSpecials();

        // for par times
        LevelTime++;
    }

    private bool _levelTimer;
    private int _levelTimeCount;

    private void UpdateSpecials()
    {
        //	LEVEL TIMER
        if (_levelTimer)
        {
            _levelTimeCount--;
            if (_levelTimeCount == 0)
            {
                ExitLevel();
            }
        }

        //	ANIMATE FLATS AND TEXTURES GLOBALLY
        foreach (var anim in _animations)
        {
            for (var i = anim.BasePic; i < anim.BasePic + anim.NumPics; i++)
            {
                var pic = anim.BasePic + ((LevelTime / anim.Speed + i) % anim.NumPics);
                if (anim.IsTexture)
                {
                    DoomGame.Instance.Renderer.TextureTranslation[i] = pic;
                }
                else
                {
                    DoomGame.Instance.Renderer.FlatTranslation[i] = pic;
                }
            }
        }

        //	ANIMATE LINE SPECIALS
        for (var i = 0; i < _numLineSpecials; i++)
        {
            var line = _lineSpecialList[i];
            switch (line.Special)
            {
                case 48:
                    // EFFECT FIRSTCOL SCROLL +
                    Sides[line.SideNum[0]].TextureOffset += Fixed.Unit;
                    break;
            }
        }

        //	DO BUTTONS
        for (var i = 0; i < Constants.MaxButtons; i++)
        {
            if (_buttonList[i].Timer != 0)
            {
                _buttonList[i].Timer--;
                if (_buttonList[i].Timer == 0)
                {
                    switch (_buttonList[i].Where)
                    {
                        case ButtonWhere.Top:
                            Sides[_buttonList[i].Line!.SideNum[0]].TopTexture = _buttonList[i].Texture;
                            break;

                        case ButtonWhere.Middle:
                            Sides[_buttonList[i].Line!.SideNum[0]].MidTexture = _buttonList[i].Texture;
                            break;

                        case ButtonWhere.Bottom:
                            Sides[_buttonList[i].Line!.SideNum[0]].BottomTexture = _buttonList[i].Texture;
                            break;
                    }

                    DoomGame.Instance.Sound.StartSound(_buttonList[i].SoundOrigin, SoundType.sfx_swtchn);
                    _buttonList[i] = new Button();
                }
            }
        }
    }

    private void P_LoadVertexes(int lump)
    {
        // Determine number of lumps:
        //  total lump length / vertex record length.
        _numVertices = DoomGame.Instance.WadData.LumpLength(lump) / 4; // two shorts

        // Allocate zone memory for buffer.
        _vertices = new Vertex[_numVertices];

        // Load data into cache.
        var data = DoomGame.Instance.WadData.GetLumpNum(lump, PurgeTag.Static)!;
        using var stream = new MemoryStream(data, false);
        using var reader = new BinaryReader(stream);

        // Copy and convert vertex coordinates,
        // internal representation as fixed.
        for (var i = 0; i < _numVertices; i++)
        {
            _vertices[i] = new Vertex(
                new Fixed(reader.ReadInt16() << Constants.FracBits),
                new Fixed(reader.ReadInt16() << Constants.FracBits)
            );
        }
    }

    private void P_LoadSegments(int lump)
    {
        const int sizeOfMapSeg = 2 + 2 + 2 + 2 + 2 + 2;
        _numSegments = DoomGame.Instance.WadData.LumpLength(lump) / sizeOfMapSeg;
        _segments = new Segment[_numSegments];
        
        var data = DoomGame.Instance.WadData.GetLumpNum(lump, PurgeTag.Static)!;
        using var stream = new MemoryStream(data, false);
        using var reader = new BinaryReader(stream);

        for (var i = 0; i < _numSegments; i++)
        {
            _segments[i] = Segment.ReadFromWadData(reader, _vertices, _sides, _lines);
        }
    }

    private void P_LoadSubSectors(int lump)
    {
        _numSubSectors = DoomGame.Instance.WadData.LumpLength(lump) / 4; // two shorts
        _subSectors = new SubSector[_numSubSectors];
        
        var data = DoomGame.Instance.WadData.GetLumpNum(lump, PurgeTag.Static)!;
        using var stream = new MemoryStream(data, false);
        using var reader = new BinaryReader(stream);

        for (var i = 0; i < _numSubSectors; i++)
        {
            _subSectors[i] = SubSector.ReadFromWadData(reader);
        }
    }

    private void P_LoadSectors(int lump)
    {
        const int sizeOfMapSector = 2 + 2 + 8 + 8 + 2 + 2 + 2;
        _numSectors = DoomGame.Instance.WadData.LumpLength(lump) / sizeOfMapSector;
        _sectors = new List<Sector>(_numSectors);
        
        var data = DoomGame.Instance.WadData.GetLumpNum(lump, PurgeTag.Static)!;
        using var stream = new MemoryStream(data, false);
        using var reader = new BinaryReader(stream);

        for (var i = 0; i < _numSectors; i++)
        {
            _sectors.Add(Sector.ReadFromWadData(reader));
        }
    }

    private void P_LoadNodes(int lump)
    {
        const int sizeOfMapNode = 2 + 2 + 2 + 2 + (2 * 2 * 4) + 2 + 2;
        _numNodes = DoomGame.Instance.WadData.LumpLength(lump) / sizeOfMapNode;
        _nodes = new Node[_numNodes];
        
        var data = DoomGame.Instance.WadData.GetLumpNum(lump, PurgeTag.Static)!;
        using var stream = new MemoryStream(data, false);
        using var reader = new BinaryReader(stream);

        for (var i = 0; i < _numNodes; i++)
        {
            _nodes[i] = Node.ReadFromWadData(reader);
        }
    }

    private void P_LoadThings(int lump)
    {
        var data = DoomGame.Instance.WadData.GetLumpNum(lump, PurgeTag.Static)!;
        var numThings = DoomGame.Instance.WadData.LumpLength(lump) / MapThing.SizeOfStruct;

        using var stream = new MemoryStream(data, false);
        using var reader = new BinaryReader(stream);

        for (var i = 0; i < numThings; i++)
        {
            var spawn = true;
            var mt = MapThing.FromWadData(reader);

            // Do not spawn cool, new monsters if !commercial
            if (DoomGame.Instance.GameMode != GameMode.Commercial)
            {
                switch (mt.Type)
                {
                    case 68:    // Arachnotron
                    case 64:    // Archvile
                    case 88:    // Boss Brain
                    case 89:    // Boss Shooter
                    case 69:    // Hell Knight
                    case 67:    // Mancubus
                    case 71:    // Pain Elemental
                    case 65:    // Former Human Commando
                    case 66:    // Revenant
                    case 84:    // Wolf SS
                        spawn = false;
                        break;
                }
            }
            if (spawn == false)
                break;

            // Do spawn all other stuff.
            P_SpawnMapThing(mt);
        }
    }

    /// <summary>
    /// Called when a player is spawned on the level.
    /// Most of the player structure stays unchanged
    ///  between levels.
    /// </summary>
    private void P_SpawnPlayer(MapThing mthing)
    {
        Fixed x;
        Fixed y;
        Fixed z;

        int i;

        // not playing?
        if (!PlayerInGame[mthing.Type - 1])
        {
            return;
        }

        var p = Players[mthing.Type - 1];

        if (p.PlayerState == PlayerState.Reborn)
        {
            p = PlayerReborn(mthing.Type - 1);
        }

        x = Fixed.FromInt(mthing.X);
        y = Fixed.FromInt(mthing.Y);
        z = Constants.OnFloorZ;
        var mobj = P_SpawnMapObject(x, y, z, MapObjectType.MT_PLAYER);

        // set color translations for player sprites
        if (mthing.Type > 1)
        {
            mobj.Flags |= (MapObjectFlag)((mthing.Type - 1) << (int)MapObjectFlag.MF_TRANSSHIFT);
        }

        mobj.Angle = new Angle(Angle.Angle45.Value * (uint)(mthing.Angle / 45));
        mobj.Player = p;
        mobj.Health = p.Health;

        p.MapObject = mobj;
        p.PlayerState = PlayerState.Alive;
        p.Refire = 0;
        p.Message = null;
        p.DamageCount = 0;
        p.BonusCount = 0;
        p.ExtraLight = 0;
        p.FixedColorMap = 0;
        p.ViewHeight = Constants.ViewHeight;

        // setup gun psprite
        P_SetupPlayerSprites(p);

        // give all cards in death match mode
        if (DeathMatch)
        {
            for (i = 0; i < (int)KeyCardType.NumberOfKeyCards; i++)
            {
                p.Cards[i] = true;
            }
        }

        if (mthing.Type - 1 == ConsolePlayer)
        {
            // wake up the status bar
            DoomGame.Instance.StatusBar.Start();
            // wake up the heads up text
            DoomGame.Instance.Hud.Start();
        }
    }

    private void P_SetupPlayerSprites(Player player)
    {
        // remove all psprites
        for (var i = 0; i < (int)PlayerSpriteType.NumPlayerSprites; i++)
        {
            player.PlayerSprites[i].State = null;
        }

        // spawn the gun
        player.PendingWeapon = player.ReadyWeapon;
        P_BringUpWeapon(player);
    }

    //
    // P_MovePsprites
    // Called every tic by player thinking routine.
    //
    public void P_MovePlayerSprites(Player player)
    {
        for (var i = 0; i < (int)PlayerSpriteType.NumPlayerSprites; i++)
        {
            var psp = player.PlayerSprites[i];

            // a null state means not active
            var state = psp.State;
            if (state != null)
            {
                // drop tic count and possibly change state

                // a -1 tic count never changes
                if (psp.Tics != -1)
                {
                    psp.Tics--;
                    if (psp.Tics == 0)
                    {
                        P_SetPlayerSprite(player, (PlayerSpriteType)i, state.NextState);
                    }
                }
            }
        }

        player.PlayerSprites[(int)PlayerSpriteType.Flash].SX = player.PlayerSprites[(int)PlayerSpriteType.Weapon].SX;
        player.PlayerSprites[(int)PlayerSpriteType.Flash].SY = player.PlayerSprites[(int)PlayerSpriteType.Weapon].SY;
    }

    public void P_SetPlayerSprite(Player player, PlayerSpriteType position, StateNum stnum)
    {
        var psp = player.PlayerSprites[(int)position];

        do
        {
            if (stnum == StateNum.S_NULL)
            {
                // object removed itself
                psp.State = null;
                break;
            }

            var state = State.GetSpawnState(stnum);
            psp.State = state;
            psp.Tics = state.Tics;    // could be 0

            if (state.Misc1 != 0)
            {
                // coordinate set
                psp.SX = Fixed.FromInt(state.Misc1);
                psp.SY = Fixed.FromInt(state.Misc2);
            }

            // Call action routine.
            // Modified handling.
            if (state.Action != null)
            {
                state.Action(new ActionParams(Player: player, PlayerSprite: psp));
                if (psp.State == null)
                {
                    break;
                }
            }

            stnum = psp.State.NextState;
        } while (psp.Tics == 0);
        // an initial state of 0 could cycle through
    }

    /// <summary>
    /// Starts bringing the pending weapon up
    /// from the bottom of the screen.
    /// Uses player
    /// </summary>
    public void P_BringUpWeapon(Player player)
    {
        if (player.PendingWeapon == WeaponType.NoChange)
        {
            player.PendingWeapon = player.ReadyWeapon;
        }

        if (player.PendingWeapon == WeaponType.Chainsaw)
        {
            DoomGame.Instance.Sound.StartSound(player.MapObject, SoundType.sfx_sawup);
        }

        var newstate = WeaponInfo.GetByType(player.PendingWeapon).UpState;

        player.PendingWeapon = WeaponType.NoChange;
        player.PlayerSprites[(int)PlayerSpriteType.Weapon].SY = WeaponInfo.WeaponBottom;

        P_SetPlayerSprite(player, PlayerSpriteType.Weapon, newstate);
    }

    public bool P_CheckAmmo(Player player)
    {
        var ammo = WeaponInfo.GetByType(player.ReadyWeapon).Ammo;

        // Minimal amount for one shot varies.
        var count = 1;
        if (player.ReadyWeapon == WeaponType.Bfg)
        {
            count = WeaponInfo.BFGCells;
        }
        else if (player.ReadyWeapon == WeaponType.SuperShotgun)
        {
            count = 2;
        }

        // Some do not need ammunition anyway.
        // Return if current ammunition sufficient.
        if (ammo == AmmoType.NoAmmo || player.Ammo[(int)ammo] >= count)
        {
            return true;
        }

        // Out of ammo, pick a weapon to change to.
        // Preferences are set here.
        do
        {
            if (player.WeaponOwned[(int)WeaponType.Plasma]
                && player.Ammo[(int)AmmoType.Cell] != 0
                && (DoomGame.Instance.GameMode != GameMode.Shareware))
            {
                player.PendingWeapon = WeaponType.Plasma;
            }
            else if (player.WeaponOwned[(int)WeaponType.SuperShotgun]
                 && player.Ammo[(int)AmmoType.Shell] > 2
                 && (DoomGame.Instance.GameMode == GameMode.Commercial))
            {
                player.PendingWeapon = WeaponType.SuperShotgun;
            }
            else if (player.WeaponOwned[(int)WeaponType.Chaingun]
                 && player.Ammo[(int)AmmoType.Clip] != 0)
            {
                player.PendingWeapon = WeaponType.Chaingun;
            }
            else if (player.WeaponOwned[(int)WeaponType.Shotgun]
                 && player.Ammo[(int)AmmoType.Shell] != 0)
            {
                player.PendingWeapon = WeaponType.Shotgun;
            }
            else if (player.Ammo[(int)AmmoType.Clip] != 0)
            {
                player.PendingWeapon = WeaponType.Pistol;
            }
            else if (player.WeaponOwned[(int)WeaponType.Chainsaw])
            {
                player.PendingWeapon = WeaponType.Chainsaw;
            }
            else if (player.WeaponOwned[(int)WeaponType.Missile]
                 && player.Ammo[(int)AmmoType.Missile] != 0)
            {
                player.PendingWeapon = WeaponType.Missile;
            }
            else if (player.WeaponOwned[(int)WeaponType.Bfg]
                 && player.Ammo[(int)AmmoType.Cell] > 40
                 && (DoomGame.Instance.GameMode != GameMode.Shareware))
            {
                player.PendingWeapon = WeaponType.Bfg;
            }
            else
            {
                // If everything fails.
                player.PendingWeapon = WeaponType.Fist;
            }

        } while (player.PendingWeapon == WeaponType.NoChange);

        // Now set appropriate weapon overlay.
        P_SetPlayerSprite(player, PlayerSpriteType.Weapon, WeaponInfo.GetByType(player.ReadyWeapon).DownState);

        return false;
    }

    public void P_FireWeapon(Player player)
    {
        if (!P_CheckAmmo(player))
        {
            return;
        }

        player.MapObject!.SetState(StateNum.S_PLAY_ATK1);
        var newState = WeaponInfo.GetByType(player.ReadyWeapon).AttackState;
        P_SetPlayerSprite(player, PlayerSpriteType.Weapon, newState);
        P_NoiseAlert(player.MapObject, player.MapObject);
    }

    private void P_SpawnMapThing(MapThing mapThing)
    {
        // count deathmatch start positions
        if (mapThing.Type == 11)
        {
            if (_deathMatchStartIdx < 10)
            {
                _deathMatchStarts[_deathMatchStartIdx++] = mapThing;
            }
            return;
        }

        // check for players specially
        if (mapThing.Type <= 4)
        {
            // save spots for respawning in network games
            _playerStarts[mapThing.Type - 1] = mapThing;
            if (!DeathMatch)
            {
                P_SpawnPlayer(mapThing);
            }

            return;
        }

        // check for appropriate skill level
        if (!NetGame && (mapThing.Options & 16) != 0)
        {
            return;
        }

        var bit = GameSkill switch
        {
            SkillLevel.Baby => 1,
            SkillLevel.Nightmare => 4,
            _ => 1 << ((int)GameSkill - 1)
        };

        if ((mapThing.Options & bit) == 0)
        {
            return;
        }

        // find which type to spawn
        var moInfo = MapObjectInfo.FindByDoomedNum(mapThing.Type);
        if (moInfo is null)
        {
            DoomGame.Error($"P_SpawnMapThing: Unknown type {mapThing.Type} at ({mapThing.X}, {mapThing.Y})");
            return;
        }

        // don't spawn keycards and players in deathmatch
        if (DeathMatch && (moInfo.Value.Flags & MapObjectFlag.MF_NOTDMATCH) != 0)
        {
            return;
        }

        // don't spawn any monsters if -nomonsters
        //if (NoMonsters
        //    && (i == MT_SKULL
        //        || (mobjinfo[i].flags & MF_COUNTKILL)))
        //{
        //    return;
        //}

        // spawn it
        var x = Fixed.FromInt(mapThing.X);
        var y = Fixed.FromInt(mapThing.Y);
        var z = Constants.OnFloorZ;

        if ((moInfo.Value.Flags & MapObjectFlag.MF_SPAWNCEILING) != 0)
        {
            z = Constants.OnCeilingZ;
        }

        var mapObject = P_SpawnMapObject(x, y, z, moInfo.Value.Type);
        mapObject.Spawnpoint = mapThing;

        if (mapObject.Tics > 0)
        {
            mapObject.Tics = 1 + (DoomRandom.P_Random() % mapObject.Tics);
        }
        
        if ((mapObject.Flags & MapObjectFlag.MF_COUNTKILL) != 0)
        {
            TotalKills++;
        }

        if ((mapObject.Flags & MapObjectFlag.MF_COUNTITEM) != 0)
        {
            TotalItems++;
        }

        mapObject.Angle = new Angle(Angle.Angle45.Value * (uint)(mapThing.Angle / 45));
        if ((mapThing.Options & (int)MapThingFlag.MTF_AMBUSH) != 0)
        {
            mapObject.Flags |= MapObjectFlag.MF_AMBUSH;
        }
    }

    private void P_UnsetThingPosition(MapObject thing)
    {
        if ((thing.Flags & MapObjectFlag.MF_NOSECTOR) == 0)
        {
            // inert things don't need to be in blockmap?
            // unlink from subsector
            if (thing.SectorNext != null)
            {
                thing.SectorNext.SectorPrev = thing.SectorPrev;
            }

            if (thing.SectorPrev != null)
            {
                thing.SectorPrev.SectorNext = thing.SectorNext;
            }
            else
            {
                thing.SubSector!.Sector!.ThingList = thing.SectorNext;
            }
        }

        if ((thing.Flags & MapObjectFlag.MF_NOBLOCKMAP) == 0)
        {
            // inert things don't need to be in blockmap
            // unlink from block map
            if (thing.BlockNext != null)
            {
                thing.BlockNext.BlockPrev = thing.BlockPrev;
            }

            if (thing.BlockPrev != null)
            {
                thing.BlockPrev.BlockNext = thing.BlockNext;
            }
            else
            {
                var blockx = (thing.X - _blockMapOriginX).Value >> Constants.MapBlockShift;
                var blocky = (thing.Y - _blockMapOriginY).Value >> Constants.MapBlockShift;

                if (blockx >= 0 && blockx < _blockMapWidth && blocky >= 0 && blocky < _blockMapHeight)
                {
                    _blockLinks[blocky * _blockMapWidth + blockx] = thing.BlockNext;
                }
            }
        }
    }

    private void P_SetThingPosition(MapObject thing)
    {
        // link into subsector
        var ss = DoomGame.Instance.Renderer.PointInSubSector(thing.X, thing.Y);
        thing.SubSector = ss;

        if ((thing.Flags & MapObjectFlag.MF_NOSECTOR) == 0)
        {
            // invisible things don't go into the sector links
            var sec = ss.Sector!;

            thing.SectorPrev = null;
            thing.SectorNext = sec.ThingList;

            if (sec.ThingList != null)
            {
                sec.ThingList.SectorPrev = thing;
            }

            sec.ThingList = thing;
        }

        // link into blockmap
        if ((thing.Flags & MapObjectFlag.MF_NOBLOCKMAP) == 0)
        {
            // inert things don't need to be in blockmap		
            var blockx = (thing.X - _blockMapOriginX).Value >> Constants.MapBlockShift;
            var blocky = (thing.Y - _blockMapOriginY).Value >> Constants.MapBlockShift;

            if (blockx >= 0
                && blockx < _blockMapWidth
                && blocky >= 0
                && blocky < _blockMapHeight)
            {
                var link = _blockLinks[blocky * _blockMapWidth + blockx];
                thing.BlockPrev = null;
                thing.BlockNext = link;

                if (link != null)
                {
                    link.BlockPrev = thing;
                }

                _blockLinks[blocky * _blockMapWidth + blockx] = thing;
            }
            else
            {
                // thing is off the map
                thing.BlockNext = thing.BlockPrev = null;
            }
        }
    }

    private void P_ExplodeMissile(MapObject mo)
    {
        mo.MomX = mo.MomY = mo.MomZ = Fixed.Zero;
        mo.SetState(MapObjectInfo.GetByType(mo.Type).DeathState);

        mo.Tics -= DoomRandom.P_Random() & 3;

        if (mo.Tics < 1)
        {
            mo.Tics = 1;
        }

        mo.Flags &= ~MapObjectFlag.MF_MISSILE;

        if (mo.Info.DeathSound != SoundType.sfx_None)
        {
            DoomGame.Instance.Sound.StartSound(mo, mo.Info.DeathSound);
        }
    }

    //
    // P_LineOpening
    // Sets opentop and openbottom to the window
    // through a two sided line.
    // OPTIMIZE: keep this precalculated
    //
    private Fixed _openTop;
    private Fixed _openBottom;
    private Fixed _openRange;
    private Fixed _lowFloor;

    private void P_LineOpening(Line lineDef)
    {
        if (lineDef.SideNum[1] == -1)
        {
            // single sided line
            _openRange = Fixed.Zero;
            return;
        }

        var front = lineDef.FrontSector!;
        var back = lineDef.BackSector!;

        if (front.CeilingHeight < back.CeilingHeight)
        {
            _openTop = front.CeilingHeight;
        }
        else
        {
            _openTop = back.CeilingHeight;
        }

        if (front.FloorHeight > back.FloorHeight)
        {
            _openBottom = front.FloorHeight;
            _lowFloor = back.FloorHeight;
        }
        else
        {
            _openBottom = back.FloorHeight;
            _lowFloor = front.FloorHeight;
        }

        _openRange = _openTop - _openBottom;
    }

    //
    // MOVEMENT ITERATOR FUNCTIONS
    //

    /// <summary>
    /// Adjusts tmfloorz and tmceilingz as lines are contacted
    /// </summary>
    private bool PIT_CheckLine(Line line)
    {
        if (_tmBoundingBox[BoundingBox.BoxRight] <= line.BoundingBox[BoundingBox.BoxLeft]
            || _tmBoundingBox[BoundingBox.BoxLeft] >= line.BoundingBox[BoundingBox.BoxRight]
            || _tmBoundingBox[BoundingBox.BoxTop] <= line.BoundingBox[BoundingBox.BoxBottom]
            || _tmBoundingBox[BoundingBox.BoxBottom] >= line.BoundingBox[BoundingBox.BoxTop])
        {
            return true;
        }

        if (P_BoxOnLineSide(_tmBoundingBox, line) != -1)
        {
            return true;
        }

        // A line has been hit

        // The moving thing's destination position will cross
        // the given line.
        // If this should not be allowed, return false.
        // If the line is special, keep track of it
        // to process later if the move is proven ok.
        // NOTE: specials are NOT sorted by order,
        // so two special lines that are only 8 pixels apart
        // could be crossed in either order.

        if (line.BackSector == null)
        {
            return false; // one sided line
        }

        if ((_tmThing!.Flags & MapObjectFlag.MF_MISSILE) == 0)
        {
            if ((line.Flags & Constants.Line.Blocking) != 0)
            {
                return false; // Explicitly blocking everything
            }

            if (_tmThing.Player == null && (line.Flags & Constants.Line.BlockMonsters) != 0)
            {
                return false; // block monsters only
            }
        }

        // set openrange, opentop, openbottom
        P_LineOpening(line);

        // adjust floor / ceiling heights
        if (_openTop < _tmCeilingZ)
        {
            _tmCeilingZ = _openTop;
            _ceilingLine = line;
        }

        if (_openBottom > _tmFloorZ)
        {
            _tmFloorZ = _openBottom;
        }

        if (_lowFloor < _tmDropOffZ)
        {
            _tmDropOffZ = _lowFloor;
        }

        // if contacted a special line, add it to the list
        if (line.Special != 0)
        {
            _specHit[_numSpecHit++] = line;
        }

        return true;
    }

    private bool PIT_CheckThing(MapObject thing)
    {
        if ((thing.Flags & (MapObjectFlag.MF_SOLID | MapObjectFlag.MF_SPECIAL | MapObjectFlag.MF_SHOOTABLE)) == 0)
        {
            return true;
        }

        var blockDist = thing.Radius + _tmThing!.Radius;

        if (Fixed.Abs(thing.X - _tmX) >= blockDist ||
            Fixed.Abs(thing.Y - _tmY) >= blockDist)
        {
            // didn't hit it
            return true;
        }

        // don't clip against self
        if (thing == _tmThing)
        {
            return true;
        }

        // check for skulls slamming into things
        if ((_tmThing.Flags & MapObjectFlag.MF_SKULLFLY) != 0)
        {
            var damage = ((DoomRandom.P_Random() % 8) + 1) * _tmThing.Info.Damage;
            MapObject.DamageMapObject(thing, _tmThing, _tmThing, damage);

            _tmThing.Flags &= ~MapObjectFlag.MF_SKULLFLY;
            _tmThing.MomX = _tmThing.MomY = _tmThing.MomZ = Fixed.Zero;

            _tmThing.SetState(_tmThing.Info.SpawnState);
            
            return false; // stop moving
        }

        // missiles can hit other things
        if ((_tmThing.Flags & MapObjectFlag.MF_MISSILE) != 0)
        {
            // see if it went over / under
            if (_tmThing.Z > thing.Z + thing.Height)
            {
                return true; // overhead
            }

            if (_tmThing.Z + _tmThing.Height < thing.Z)
            {
                return true; // underneath
            }

            if (_tmThing.Target != null && (
                    _tmThing.Target.Type == thing.Type || (
                        _tmThing.Target.Type == MapObjectType.MT_KNIGHT && thing.Type == MapObjectType.MT_BRUISER) || (
                        _tmThing.Target.Type == MapObjectType.MT_BRUISER && thing.Type == MapObjectType.MT_KNIGHT)))
            {
                // Don't hit same species as originator.
                if (thing == _tmThing.Target)
                {
                    return true;
                }

                if (thing.Type != MapObjectType.MT_PLAYER)
                {
                    // Explode, but do no damage.
                    // Let players missile other players.
                    return false;
                }
            }

            if ((thing.Flags & MapObjectFlag.MF_SHOOTABLE) == 0)
            {
                // didn't do any damage
                return (thing.Flags & MapObjectFlag.MF_SOLID) == 0;
            }

            // damage / explode
            var damage = ((DoomRandom.P_Random() % 8) + 1) * _tmThing.Info.Damage;
            MapObject.DamageMapObject(thing, _tmThing, _tmThing.Target, damage);

            // don't traverse any more
            return false;
        }

        // check for special pickup
        if ((thing.Flags & MapObjectFlag.MF_SPECIAL) != 0)
        {
            var solid = (thing.Flags & MapObjectFlag.MF_SOLID) != 0;
            if ((_tmFlags & MapObjectFlag.MF_PICKUP) != 0)
            {
                // can remove thing
                P_TouchSpecialThing(thing, _tmThing);
            }
            return !solid;
        }

        return (thing.Flags & MapObjectFlag.MF_SOLID) == 0;
    }

    /// <summary>
    /// The validcount flags are used to avoid checking lines
    /// that are marked in multiple mapblocks,
    /// so increment validcount before the first call
    /// to P_BlockLinesIterator, then make one or more calls
    /// to it.
    /// </summary>
    private bool P_BlockLinesIterator(int x, int y, Func<Line, bool> lineFunction)
    {
        if (x < 0
            || y < 0
            || x >= _blockMapWidth
            || y >= _blockMapHeight)
        {
            return true;
        }

        var index = y * _blockMapWidth + x;
        var offset = _blockMapOffsets[index];

        for (var i = offset; _blockMapLump[i] != -1; i++)
        {
            var line = _lines[_blockMapLump[i]];

            if (line.ValidCount == DoomGame.Instance.Renderer.ValidCount)
            {
                continue; // line has already been checked;
            }

            line.ValidCount = DoomGame.Instance.Renderer.ValidCount;
            if (!lineFunction(line))
            {
                return false;
            }
        }

        return true; // everything was checked
    }

    private bool P_BlockThingsIterator(int x, int y, Func<MapObject, bool> thingFunction)
    {
        if (x < 0
            || y < 0
            || x >= _blockMapWidth
            || y >= _blockMapHeight)
        {
            return true;
        }

        var thing = _blockLinks[y * _blockMapWidth + x];
        for (; thing != null; thing = thing.BlockNext)
        {
            if (!thingFunction(thing))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// P_CheckPosition
    /// This is purely informative, nothing is modified
    /// (except things picked up).
    /// 
    /// in:
    ///  a mobj_t (can be valid or invalid)
    ///  a position to be checked
    ///   (doesn't need to be related to the mobj_t->x,y)
    ///
    /// during:
    ///  special things are touched if MF_PICKUP
    ///  early out on solid lines?
    ///
    /// out:
    ///  newsubsec
    ///  floorz
    ///  ceilingz
    ///  tmdropoffz
    ///   the lowest point contacted
    ///   (monsters won't move to a dropoff)
    ///  speciallines[]
    ///  numspeciallines
    /// </summary>
    private bool P_CheckPosition(MapObject thing, Fixed x, Fixed y)
    {
        _tmThing = thing;
        _tmFlags = thing.Flags;

        _tmX = x;
        _tmY = y;

        _tmBoundingBox[BoundingBox.BoxTop] = y + _tmThing.Radius;
        _tmBoundingBox[BoundingBox.BoxBottom] = y - _tmThing.Radius;
        _tmBoundingBox[BoundingBox.BoxRight] = x + _tmThing.Radius;
        _tmBoundingBox[BoundingBox.BoxLeft] = x - _tmThing.Radius;

        var newSubSector = DoomGame.Instance.Renderer.PointInSubSector(x, y);
        _ceilingLine = null;

        // The base floor / ceiling is from the subsector
        // that contains the point.
        // Any contacted lines the step closer together
        // will adjust them.
        _tmFloorZ = _tmDropOffZ = newSubSector.Sector!.FloorHeight;
        _tmCeilingZ = newSubSector.Sector.CeilingHeight;

        DoomGame.Instance.Renderer.ValidCount++;
        _numSpecHit = 0;

        if ((_tmFlags & MapObjectFlag.MF_NOCLIP) != 0)
        {
            return true;
        }

        // Check things first, possibly picking things up.
        // The bounding box is extended by MAXRADIUS
        // because mobj_ts are grouped into mapblocks
        // based on their origin point, and can overlap
        // into adjacent blocks by up to MAXRADIUS units.
        var xl = (_tmBoundingBox[BoundingBox.BoxLeft] - _blockMapOriginX - Constants.MaxRadius).Value >> Constants.MapBlockShift;
        var xh = (_tmBoundingBox[BoundingBox.BoxRight] - _blockMapOriginX + Constants.MaxRadius).Value >> Constants.MapBlockShift;
        var yl = (_tmBoundingBox[BoundingBox.BoxBottom] - _blockMapOriginY - Constants.MaxRadius).Value >> Constants.MapBlockShift;
        var yh = (_tmBoundingBox[BoundingBox.BoxTop] - _blockMapOriginY + Constants.MaxRadius).Value >> Constants.MapBlockShift;

        for (var bx = xl; bx <= xh; bx++)
        {
            for (var by = yl; by <= yh; by++)
            {
                if (!P_BlockThingsIterator(bx, by, PIT_CheckThing))
                {
                    return false;
                }
            }
        }

        // check lines
        xl = (_tmBoundingBox[BoundingBox.BoxLeft] - _blockMapOriginX).Value >> Constants.MapBlockShift;
        xh = (_tmBoundingBox[BoundingBox.BoxRight] - _blockMapOriginX).Value >> Constants.MapBlockShift;
        yl = (_tmBoundingBox[BoundingBox.BoxBottom] - _blockMapOriginY).Value >> Constants.MapBlockShift;
        yh = (_tmBoundingBox[BoundingBox.BoxTop] - _blockMapOriginY).Value >> Constants.MapBlockShift;

        for (var bx = xl; bx <= xh; bx++)
        {
            for (var by = yl; by <= yh; by++)
            {
                if (!P_BlockLinesIterator(bx, by, PIT_CheckLine))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Attempt to move to a new position,
    /// crossing special lines unless MF_TELEPORT is set.
    /// </summary>
    private bool P_TryMove(MapObject thing, Fixed x, Fixed y)
    {
        _floatOk = false;
        if (!P_CheckPosition(thing, x, y))
        {
            return false; // solid wall or thing
        }

        if ((thing.Flags & MapObjectFlag.MF_NOCLIP) == 0)
        {
            if (_tmCeilingZ - _tmFloorZ < thing.Height)
            {
                return false; // doesn't fit
            }

            _floatOk = true;

            if ((thing.Flags & MapObjectFlag.MF_TELEPORT) == 0 && _tmCeilingZ - thing.Z < thing.Height)
            {
                return false; // mapobject must lower itself to fit
            }

            if ((thing.Flags & MapObjectFlag.MF_TELEPORT) == 0 && _tmFloorZ - thing.Z > Fixed.FromInt(24))
            {
                return false; // too big a step up
            }

            if ((thing.Flags & (MapObjectFlag.MF_DROPOFF | MapObjectFlag.MF_FLOAT)) == 0 &&
                _tmFloorZ - _tmDropOffZ > Fixed.FromInt(24))
            {
                return false; // don't stand over a dropoff
            }
        }

        // the move is ok,
        // so link the thing into its new position
        P_UnsetThingPosition(thing);

        var oldX = thing.X;
        var oldY = thing.Y;
        thing.FloorZ = _tmFloorZ;
        thing.CeilingZ = _tmCeilingZ;
        thing.X = x;
        thing.Y = y;

        P_SetThingPosition(thing);

        // if any special lines were hit, do the effect
        if ((thing.Flags & (MapObjectFlag.MF_TELEPORT | MapObjectFlag.MF_NOCLIP)) == 0)
        {
            while (_numSpecHit-- != 0)
            {
                // see if the line was crossed
                var line = _specHit[_numSpecHit];
                var side = P_PointOnLineSide(thing.X, thing.Y, line);
                var oldSide = P_PointOnLineSide(oldX, oldY, line);
                if (side != oldSide)
                {
                    if (line.Special != 0)
                    {
                        P_CrossSpecialLine(line, oldSide, thing);
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Takes a valid thing and adjusts the thing->floorz,
    /// thing->ceilingz, and possibly thing->z.
    /// This is called for all nearby monsters
    /// whenever a sector changes height.
    /// If the thing doesn't fit,
    /// the z will be set to the lowest value
    /// and false will be returned.
    /// </summary>
    private bool P_ThingHeightClip(MapObject thing)
    {
        var onFloor = thing.Z == thing.FloorZ;

        P_CheckPosition(thing, thing.X, thing.Y);
        // what about stranding monster partially off an edge?

        thing.FloorZ = _tmFloorZ;
        thing.CeilingZ = _tmCeilingZ;

        if (onFloor)
        {
            // walking monsters rise and fall with the floor
            thing.Z = thing.FloorZ;
        }
        else
        {
            // don't adjust a floating monster unless forced to
            if (thing.Z + thing.Height > thing.CeilingZ)
            {
                thing.Z = thing.CeilingZ - thing.Height;
            }
        }

        return thing.CeilingZ - thing.FloorZ >= thing.Height;
    }

    //
    // RADIUS ATTACK
    //
    private MapObject? _bombSource;
    private MapObject? _bombSpot;
    private int _bombDamage;

    /// <summary>
    /// "bombsource" is the creature
    /// that caused the explosion at "bombspot".
    /// </summary>
    private bool PIT_RadiusAttack(MapObject thing)
    {
        if ((thing.Flags & MapObjectFlag.MF_SHOOTABLE) == 0)
        {
            return true;
        }

        // Boss spider and cyborg
        // take no damage from concussion.
        if (thing.Type is MapObjectType.MT_CYBORG or MapObjectType.MT_SPIDER)
        {
            return true;
        }

        var dx = Fixed.Abs(thing.X - _bombSpot!.X);
        var dy = Fixed.Abs(thing.Y - _bombSpot.Y);

        var dist = dx > dy ? dx : dy;
        dist = (dist - thing.Radius) >> Constants.FracBits;

        if (dist < Fixed.Zero)
        {
            dist = Fixed.Zero;
        }

        if (dist.Value >= _bombDamage)
        {
            return true; // out of range
        }

        if (P_CheckSight(thing, _bombSpot))
        {
            // must be in direct path
            MapObject.DamageMapObject(thing, _bombSpot, _bombSource, _bombDamage - dist.Value);
        }

        return true;
    }

    /// <summary>
    /// Source is the creature that caused the explosion at spot.
    /// </summary>
    public void P_RadiusAttack(MapObject spot, MapObject source, int damage)
    {
        var dist = Fixed.FromInt(damage + Constants.MaxRadius.Value);
        var yh = (spot.Y + dist - _blockMapOriginY).Value >> Constants.MapBlockShift;
        var yl = (spot.Y - dist - _blockMapOriginY).Value >> Constants.MapBlockShift;
        var xh = (spot.X + dist - _blockMapOriginX).Value >> Constants.MapBlockShift;
        var xl = (spot.X - dist - _blockMapOriginX).Value >> Constants.MapBlockShift;
        _bombSpot = spot;
        _bombSource = source;
        _bombDamage = damage;

        for (var y = yl; y < yh; y++)
        {
            for (var x = xl; x < xh; x++)
            {
                P_BlockThingsIterator(x, y, PIT_RadiusAttack);
            }
        }
    }

    //
    // SECTOR HEIGHT CHANGING
    // After modifying a sectors floor or ceiling height,
    // call this routine to adjust the positions
    // of all things that touch the sector.
    //
    // If anything doesn't fit anymore, true will be returned.
    // If crunch is true, they will take damage
    //  as they are being crushed.
    // If Crunch is false, you should set the sector height back
    //  the way it was and call P_ChangeSector again
    //  to undo the changes.
    //

    private bool _noFit;
    private bool _crushChange;

    private bool PIT_ChangeSector(MapObject thing)
    {
        if (P_ThingHeightClip(thing))
        {
            // keep checking
            return true;
        }

        // crunch bodies to giblets
        if (thing.Health <= 0)
        {
            thing.SetState(StateNum.S_GIBS);

            thing.Flags &= ~MapObjectFlag.MF_SOLID;
            thing.Height = Fixed.Zero;
            thing.Radius = Fixed.Zero;

            // keep checking
            return true;
        }

        // crunch dropped items
        if ((thing.Flags & MapObjectFlag.MF_DROPPED) != 0)
        {
            P_RemoveMapObject(thing);

            // keep checking
            return true;
        }

        if ((thing.Flags & MapObjectFlag.MF_SHOOTABLE) == 0)
        {
            // assume it is bloody gibs or something
            return true;
        }

        _noFit = true;

        if (_crushChange && (LevelTime & 3) == 0)
        {
            MapObject.DamageMapObject(thing, null, null, 10);

            // spray blood in a random direction
            var mo = P_SpawnMapObject(thing.X, thing.Y, thing.Z + thing.Height / 2, MapObjectType.MT_BLOOD);

            mo.MomX = new Fixed((DoomRandom.P_Random() - DoomRandom.P_Random()) << 12);
            mo.MomY = new Fixed((DoomRandom.P_Random() - DoomRandom.P_Random()) << 12);
        }

        // keep checking (crush other things)	
        return true;
    }

    public bool P_ChangeSector(Sector sector, bool crunch)
    {
        _noFit = false;
        _crushChange = crunch;

        // re-check heights for all things near the moving sector
        for (var x = sector.BlockBox[BoundingBox.BoxLeft]; x <= sector.BlockBox[BoundingBox.BoxRight]; x++)
        {
            for (var y = sector.BlockBox[BoundingBox.BoxBottom]; y <= sector.BlockBox[BoundingBox.BoxTop]; y++)
            {
                P_BlockThingsIterator(x, y, PIT_ChangeSector);
            }
        }

        return _noFit;
    }

    //
    // INTERCEPT ROUTINES
    //

    private const int MaxIntercepts = 128;
    private Intercept?[] _intercepts = new Intercept?[MaxIntercepts];
    private int _lastInterceptIdx = 0;

    public const int PT_AddLines = 1;
    private const int PT_AddThings = 2;
    private const int PT_EarlyOut = 4;

    private DividerLine _trace = new();
    private bool _earlyOut;
    private int _pathTraversalFlags;

    /// <summary>
    /// Looks for lines in the given block
    /// that intercept the given trace
    /// to add to the intercepts list.
    ///
    /// A line is crossed if its endpoints
    /// are on opposite sides of the trace.
    /// Returns true if earlyout and a solid line hit.
    /// </summary>
    private bool PIT_AddLineIntercepts(Line line)
    {
        int s1;
        int s2;
        var dl = new DividerLine();

        // avoid precision problems with two routines
        var fixed16 = Fixed.FromInt(16);
        if (_trace.Dx > fixed16
            || _trace.Dy > fixed16
            || _trace.Dx < -fixed16
            || _trace.Dy < -fixed16)
        {
            s1 = P_PointOnDivlineSide(line.V1.X, line.V1.Y, _trace);
            s2 = P_PointOnDivlineSide(line.V2.X, line.V2.Y, _trace);
        }
        else
        {
            s1 = P_PointOnLineSide(_trace.X, _trace.Y, line);
            s2 = P_PointOnLineSide(_trace.X + _trace.Dx, _trace.Y + _trace.Dy, line);
        }

        if (s1 == s2)
        {
            return true;    // line isn't crossed
        }

        // hit the line
        P_MakeDivline(line, dl);
        var frac = P_InterceptVector(_trace, dl);

        if (frac < Fixed.Zero)
        {
            return true;    // behind source
        }

        // try to early out the check
        if (_earlyOut && frac < Fixed.Unit && line.BackSector == null)
        {
            return false;   // stop checking
        }

        _intercepts[_lastInterceptIdx++] = new Intercept
        {
            Frac = frac,
            IsLine = true,
            Line = line
        };

        return true;	// continue
    }

    private bool PIT_AddThingIntercepts(MapObject thing)
    {
        Fixed x1;
        Fixed y1;
        Fixed x2;
        Fixed y2;

        var tracePositive = (_trace.Dx.Value ^ _trace.Dy.Value) > 0;

        // check a corner to corner crossection for hit
        if (tracePositive)
        {
            x1 = thing.X - thing.Radius;
            y1 = thing.Y + thing.Radius;

            x2 = thing.X + thing.Radius;
            y2 = thing.Y - thing.Radius;
        }
        else
        {
            x1 = thing.X - thing.Radius;
            y1 = thing.Y - thing.Radius;

            x2 = thing.X + thing.Radius;
            y2 = thing.Y + thing.Radius;
        }

        var s1 = P_PointOnDivlineSide(x1, y1, _trace);
        var s2 = P_PointOnDivlineSide(x2, y2, _trace);

        if (s1 == s2)
        {
            return true;        // line isn't crossed
        }

        var dl = new DividerLine
        {
            X = x1,
            Y = y1,
            Dx = x2 - x1,
            Dy = y2 - y1
        };

        var frac = P_InterceptVector(_trace, dl);
        if (frac < Fixed.Zero)
        {
            return true;        // behind source
        }

        _intercepts[_lastInterceptIdx++] = new Intercept
        {
            Frac = frac,
            IsLine = false,
            Thing = thing
        };

        return true;		// keep going
    }

    /// <summary>
    /// Returns true if the traverser function returns true
    /// for all lines.
    /// </summary>
    private bool P_TraverseIntercepts(Func<Intercept, bool> traverserFunction, Fixed maxFrac)
    {
        Intercept? intercept = null;

        var count = _lastInterceptIdx;
        while (count-- != 0)
        {
            var dist = Fixed.MaxValue;
            for (var i = 0; i < _lastInterceptIdx; i++)
            {
                var scan = _intercepts[i];
                if (scan != null && scan.Frac < dist)
                {
                    dist = scan.Frac;
                    intercept = scan;
                }
            }

            if (dist > maxFrac)
            {
                return true;    // checked everything in range		
            }
            
            if (!traverserFunction(intercept!))
            {
                return false;   // don't bother going farther
            }

            intercept!.Frac = Fixed.MaxValue;
        }

        return true;		// everything was traversed
    }

    public MapObject? UseThing { get; set; }

    public bool PTR_UseTraverse(Intercept intercept)
    {
        if (intercept.Line!.Special == 0)
        {
            P_LineOpening(intercept.Line);
            if (_openRange <= Fixed.Zero)
            {
                DoomGame.Instance.Sound.StartSound(UseThing, SoundType.sfx_noway);

                // can't use through a wall
                return false;
            }

            // not a special line, but keep checking
            return true;
        }

        var side = 0;
        if (P_PointOnLineSide(UseThing!.X, UseThing.Y, intercept.Line) == 1)
        {
            side = 1;
        }

        //	return false;		// don't use back side

        P_UseSpecialLine(UseThing, intercept.Line, side);

        // can't use for than one special line in a row
        return false;
    }

    /// <summary>
    /// Traces a line from x1,y1 to x2,y2,
    /// calling the traverser function for each.
    /// Returns true if the traverser function returns true
    /// for all lines.
    /// </summary>
    public bool P_PathTraverse(Fixed x1, Fixed y1, Fixed x2, Fixed y2, int flags, Func<Intercept, bool> traverserFunction)
    {
        Fixed xStep;
        Fixed yStep;

        Fixed partial;

        int mapXStep;
        int mapYStep;

        int count;

        _earlyOut = (flags & PT_EarlyOut) != 0;

        DoomGame.Instance.Renderer.ValidCount++;
        _lastInterceptIdx = 0;

        if (((x1 - _blockMapOriginX).Value & (Constants.MapBlockSize.Value - 1)) == 0)
        {
            x1 += Fixed.Unit; // don't side exactly on a line
        }

        if (((y1 - _blockMapOriginY).Value & (Constants.MapBlockSize.Value - 1)) == 0)
        {
            y1 += Fixed.Unit; // don't side exactly on a line
        }

        _trace.X = x1;
        _trace.Y = y1;
        _trace.Dx = x2 - x1;
        _trace.Dy = y2 - y1;

        x1 -= _blockMapOriginX;
        y1 -= _blockMapOriginY;
        var xt1 = x1.Value >> Constants.MapBlockShift;
        var yt1 = y1.Value >> Constants.MapBlockShift;

        x2 -= _blockMapOriginX;
        y2 -= _blockMapOriginY;
        var xt2 = x2.Value >> Constants.MapBlockShift;
        var yt2 = y2.Value >> Constants.MapBlockShift;

        if (xt2 > xt1)
        {
            mapXStep = 1;
            partial = new Fixed(Constants.FracUnit - ((x1.Value >> Constants.MapBlockToFrac) & (Constants.FracUnit - 1)));
            yStep = (y2 - y1) / Fixed.Abs(x2 - x1);
        }
        else if (xt2 < xt1)
        {
            mapXStep = -1;
            partial = new Fixed((x1.Value >> Constants.MapBlockToFrac) & (Constants.FracUnit - 1));
            yStep = (y2 - y1) / Fixed.Abs(x2 - x1);
        }
        else
        {
            mapXStep = 0;
            partial = Fixed.Unit;
            yStep = Fixed.FromInt(256);
        }

        var yIntercept = (y1 >> Constants.MapBlockToFrac) + (partial * yStep);


        if (yt2 > yt1)
        {
            mapYStep = 1;
            partial = new Fixed(Constants.FracUnit - ((y1.Value >> Constants.MapBlockToFrac) & (Constants.FracUnit - 1)));
            xStep = (x2 - x1) / Fixed.Abs(y2 - y1);
        }
        else if (yt2 < yt1)
        {
            mapYStep = -1;
            partial = new Fixed((y1.Value >> Constants.MapBlockToFrac) & (Constants.FracUnit - 1));
            xStep = (x2 - x1) / Fixed.Abs(y2 - y1);
        }
        else
        {
            mapYStep = 0;
            partial = Fixed.Unit;
            xStep = Fixed.FromInt(256);
        }
        var xIntercept = (x1 >> Constants.MapBlockToFrac) + (partial * xStep);

        // Step through map blocks.
        // Count is present to prevent a round off error
        // from skipping the break.
        var mapX = xt1;
        var mapY = yt1;

        for (count = 0; count < 64; count++)
        {
            if ((flags & PT_AddLines) != 0)
            {
                if (!P_BlockLinesIterator(mapX, mapY, PIT_AddLineIntercepts))
                {
                    return false;   // early out
                }
            }

            if ((flags & PT_AddThings) != 0)
            {
                if (!P_BlockThingsIterator(mapX, mapY, PIT_AddThingIntercepts))
                {
                    return false;   // early out
                }
            }

            if (mapX == xt2 && mapY == yt2)
            {
                break;
            }

            if ((yIntercept.Value >> Constants.FracBits) == mapY)
            {
                yIntercept += yStep;
                mapX += mapXStep;
            }
            else if ((xIntercept.Value >> Constants.FracBits) == mapX)
            {
                xIntercept += xStep;
                mapY += mapYStep;
            }
        }

        // go through the sorted list
        return P_TraverseIntercepts(traverserFunction, Fixed.Unit);
    }

    //
    // SLIDE MOVE
    // Allows the player to slide along any angled walls.
    //
    private Fixed _bestSlideFrac;
    private Fixed _secondSlideFrac;

    private Line? _bestSlideLine;
    private Line? _secondSlideLine;

    private MapObject? _slideMapObject;

    private Fixed _tmXMove;
    private Fixed _tmYMove;

    /// <summary>
    /// Adjusts the xmove / ymove
    /// so that the next move will slide along the wall.
    /// </summary>
    private void P_HitSlideLine(Line line)
    {
        if (line.SlopeType == SlopeType.Horizontal)
        {
            _tmYMove = Fixed.Zero;
            return;
        }

        if (line.SlopeType == SlopeType.Vertical)
        {
            _tmXMove = Fixed.Zero;
            return;
        }

        var side = P_PointOnLineSide(_slideMapObject!.X, _slideMapObject.Y, line);
        var lineAngle = DoomGame.Instance.Renderer.PointToAngle2(Fixed.Zero, Fixed.Zero, line.Dx, line.Dy);

        if (side == 1)
        {
            lineAngle += Angle.Angle180;
        }

        var moveAngle = DoomGame.Instance.Renderer.PointToAngle2(Fixed.Zero, Fixed.Zero, _tmXMove, _tmYMove);
        var deltaAngle = moveAngle - lineAngle;

        if (deltaAngle > Angle.Angle180)
        {
            deltaAngle += Angle.Angle180;
        }
        //	I_Error ("SlideLine: ang>ANG180");

        var moveLen = P_AproxDistance(_tmXMove, _tmYMove);
        var newLen = (moveLen * DoomMath.Cos(deltaAngle));

        _tmXMove = (newLen * DoomMath.Cos(lineAngle));
        _tmYMove = (newLen * DoomMath.Sin(lineAngle));
    }

    private bool PTR_SlideTraverse(Intercept intercept)
    {
        if (!intercept.IsLine)
        {
            DoomGame.Error("PTR_SlideTraverse: not a line?");
            return false;
        }

        var line = intercept.Line!;
        if ((line.Flags & Constants.Line.TwoSided) == 0)
        {
            if (P_PointOnLineSide(_slideMapObject!.X, _slideMapObject.Y, line) != 0)
            {
                // don't hit the back side
                return true;
            }

            IsBlocking();
            return false;
        }

        // set openrange, opentop, openbottom
        P_LineOpening(line);

        if (_openRange < _slideMapObject!.Height)
        {
            IsBlocking();        // doesn't fit
            return false;
        }

        if (_openTop - _slideMapObject.Z < _slideMapObject.Height)
        {
            IsBlocking();        // mobj is too high
            return false;
        }
        
        if (_openBottom - _slideMapObject.Z > Fixed.FromInt(24))
        {
            IsBlocking();        // too big a step up
            return false;
        }

        // this line doesn't block movement
        return true;

        // the line does block movement,
        // see if it is closer than best so far
        void IsBlocking()
        {
            if (intercept.Frac < _bestSlideFrac)
            {
                _secondSlideFrac = _bestSlideFrac;
                _secondSlideLine = _bestSlideLine;
                _bestSlideFrac = intercept.Frac;
                _bestSlideLine = line;
            }
        }
    }

    /// <summary>
    /// The momx / momy move is bad, so try to slide
    /// along a wall.
    /// Find the first line hit, move flush to it,
    /// and slide along it
    ///
    /// This is a kludgy mess. 
    /// </summary>
    private void P_SlideMove(MapObject mo)
    {
        Fixed leadx;
        Fixed leady;
        Fixed trailx;
        Fixed traily;
        Fixed newx;
        Fixed newy;
        var hitcount = 0;

        _slideMapObject = mo;

        Retry();

        // trace along the three leading corners
        if (mo.MomX > Fixed.Zero)
        {
            leadx = mo.X + mo.Radius;
            trailx = mo.X - mo.Radius;
        }
        else
        {
            leadx = mo.X - mo.Radius;
            trailx = mo.X + mo.Radius;
        }

        if (mo.MomY > Fixed.Zero)
        {
            leady = mo.Y + mo.Radius;
            traily = mo.Y - mo.Radius;
        }
        else
        {
            leady = mo.Y - mo.Radius;
            traily = mo.Y + mo.Radius;
        }

        _bestSlideFrac = new Fixed(Constants.FracUnit + 1);

        P_PathTraverse(leadx, leady, leadx + mo.MomX, leady + mo.MomY,
            PT_AddLines, PTR_SlideTraverse);
        P_PathTraverse(trailx, leady, trailx + mo.MomX, leady + mo.MomY,
            PT_AddLines, PTR_SlideTraverse);
        P_PathTraverse(leadx, traily, leadx + mo.MomX, traily + mo.MomY,
            PT_AddLines, PTR_SlideTraverse);

        // move up to the wall
        if (_bestSlideFrac == new Fixed(Constants.FracUnit + 1))
        {
            // the move most have hit the middle, so stairstep
            StairStep();
            return;
        }

        // fudge a bit to make sure it doesn't hit
        _bestSlideFrac -= new Fixed(0x800);
        if (_bestSlideFrac > Fixed.Zero)
        {
            newx = (mo.MomX * _bestSlideFrac);
            newy = (mo.MomY * _bestSlideFrac);

            if (!P_TryMove(mo, mo.X + newx, mo.Y + newy))
            {
                StairStep();
            }
        }

        // Now continue along the wall.
        // First calculate remainder.
        _bestSlideFrac = Fixed.Unit - (_bestSlideFrac + new Fixed(0x800));

        if (_bestSlideFrac > Fixed.Unit)
        {
            _bestSlideFrac = Fixed.Unit;
        }

        if (_bestSlideFrac <= Fixed.Zero)
        {
            return;
        }

        _tmXMove = (mo.MomX * _bestSlideFrac);
        _tmYMove = (mo.MomY * _bestSlideFrac);

        P_HitSlideLine(_bestSlideLine!);  // clip the moves

        mo.MomX = _tmXMove;
        mo.MomY = _tmYMove;

        if (!P_TryMove(mo, mo.X + _tmXMove, mo.Y + _tmYMove))
        {
            Retry();
        }

        void Retry()
        {
            if (++hitcount == 3)
            {
                StairStep(); // don't loop forever
            }
        }

        void StairStep()
        {
            if (!P_TryMove(mo, mo.X, mo.Y + mo.MomY))
            {
                P_TryMove(mo, mo.X + mo.MomX, mo.Y);
            }
        }
    }

    private static readonly Fixed StopSpeed = new(0x1000);
    private static readonly Fixed Friction = new(0xe800);

    private void P_XYMovement(MapObject mo)
    {
        if (mo.MomX == Fixed.Zero && mo.MomY == Fixed.Zero)
        {
            if ((mo.Flags & MapObjectFlag.MF_SKULLFLY) != 0)
            {
                // the skull slammed into something
                mo.Flags &= ~MapObjectFlag.MF_SKULLFLY;
                mo.MomX = mo.MomY = mo.MomZ = Fixed.Zero;

                mo.SetState(mo.Info.SpawnState);
            }
            return;
        }

        var player = mo.Player;

        if (mo.MomX > Constants.MaxMove)
        {
            mo.MomX = Constants.MaxMove;
        }
        else if (mo.MomX < -Constants.MaxMove)
        {
            mo.MomX = -Constants.MaxMove;
        }

        if (mo.MomY > Constants.MaxMove)
        {
            mo.MomY = Constants.MaxMove;
        }
        else if (mo.MomY < -Constants.MaxMove)
        {
            mo.MomY = -Constants.MaxMove;
        }

        var xmove = mo.MomX;
        var ymove = mo.MomY;
        Fixed ptryx, ptryy;

        do
        {
            if (xmove > Constants.MaxMove / 2 || ymove > Constants.MaxMove / 2)
            {
                ptryx = mo.X + xmove / 2;
                ptryy = mo.Y + ymove / 2;
                xmove >>= 1;
                ymove >>= 1;
            }
            else
            {
                ptryx = mo.X + xmove;
                ptryy = mo.Y + ymove;
                xmove = ymove = Fixed.Zero;
            }

            if (!P_TryMove(mo, ptryx, ptryy))
            {
                // blocked move
                if (mo.Player != null)
                {   // try to slide along it
                    P_SlideMove(mo);
                }
                else if ((mo.Flags & MapObjectFlag.MF_MISSILE) != 0)
                {
                    // explode a missile
                    if (_ceilingLine?.BackSector != null &&
                        _ceilingLine.BackSector.CeilingPic == DoomGame.Instance.Renderer.Sky.FlatNum)
                    {
                        // Hack to prevent missiles exploding
                        // against the sky.
                        // Does not handle sky floors.
                        P_RemoveMapObject(mo);
                        return;
                    }
                    P_ExplodeMissile(mo);
                }
                else
                {
                    mo.MomX = mo.MomY = Fixed.Zero;
                }
            }
        } while (xmove != Fixed.Zero || ymove != Fixed.Zero);

        // slow down
        if (player != null && (player.Cheats & Cheat.NoMomentum) != 0)
        {
            // debug option for no sliding at all
            mo.MomX = mo.MomY = Fixed.Zero;
            return;
        }

        if ((mo.Flags & (MapObjectFlag.MF_MISSILE | MapObjectFlag.MF_SKULLFLY)) != 0)
        {
            return;     // no friction for missiles ever
        }

        if (mo.Z > mo.FloorZ)
        {
            return;		// no friction when airborne
        }

        if ((mo.Flags & MapObjectFlag.MF_CORPSE) != 0)
        {
            // do not stop sliding
            //  if halfway off a step with some momentum
            var unitOver4 = Fixed.Unit / 4;
            if (mo.MomX > unitOver4
                || mo.MomX < -unitOver4
                || mo.MomY > unitOver4
                || mo.MomY < -unitOver4)
            {
                if (mo.FloorZ != mo.SubSector!.Sector!.FloorHeight)
                {
                    return;
                }
            }
        }

        if (mo.MomX > -StopSpeed
            && mo.MomX < StopSpeed
            && mo.MomY > -StopSpeed
            && mo.MomY < StopSpeed
            && (player == null || (player.Command.ForwardMove == 0 && player.Command.SideMove == 0)))
        {
            // if in a walking frame, stop moving
            if (player != null && (State.GetStateNum(player.MapObject!.State!) - StateNum.S_PLAY_RUN1) < 4)
            {
                player.MapObject.SetState(StateNum.S_PLAY);
            }

            mo.MomX = Fixed.Zero;
            mo.MomY = Fixed.Zero;
        }
        else
        {
            mo.MomX *= Friction;
            mo.MomY *= Friction;
        }
    }

    public Fixed P_AproxDistance(Fixed dx, Fixed dy)
    {
        dx = Fixed.Abs(dx);
        dy = Fixed.Abs(dy);
        if (dx < dy)
        {
            return dx + dy - (dx >> 1);
        }

        return dx + dy - (dy >> 1);
    }

    private int P_PointOnLineSide(Fixed x, Fixed y, Line line)
    {
        if (line.Dx == Fixed.Zero)
        {
            if (x <= line.V1.X)
            {
                return line.Dy > Fixed.Zero ? 1 : 0;
            }

            return line.Dy < Fixed.Zero ? 1 : 0;
        }

        if (line.Dy == Fixed.Zero)
        {
            if (y <= line.V1.Y)
            {
                return line.Dx < Fixed.Zero ? 1 : 0;
            }

            return line.Dx > Fixed.Zero ? 1 : 0;
        }

        var dx = (x - line.V1.X);
        var dy = (y - line.V1.Y);

        var left = new Fixed(line.Dy.Value >> Constants.FracBits) * dx;
        var right = dy * new Fixed(line.Dx.Value >> Constants.FracBits);

        if (right < left)
        {
            return 0;       // front side
        }

        return 1;			// back side
    }

    /// <summary>
    /// Considers the line to be infinite
    /// Returns side 0 or 1, -1 if box crosses the line.
    /// </summary>
    private int P_BoxOnLineSide(Fixed[] boundingBox, Line line)
    {
        int p1 = -1, p2 = -1;

        switch (line.SlopeType)
        {
            case SlopeType.Horizontal:
                p1 = boundingBox[BoundingBox.BoxTop] > line.V1.Y ? 1 : 0;
                p2 = boundingBox[BoundingBox.BoxBottom] > line.V1.Y ? 1 : 0;
                if (line.Dx < Fixed.Zero)
                {
                    p1 ^= 1;
                    p2 ^= 1;
                }
                break;

            case SlopeType.Vertical:
                p1 = boundingBox[BoundingBox.BoxRight] < line.V1.X ? 1 : 0;
                p2 = boundingBox[BoundingBox.BoxLeft] < line.V1.X ? 1 : 0;
                if (line.Dy < Fixed.Zero)
                {
                    p1 ^= 1;
                    p2 ^= 1;
                }
                break;

            case SlopeType.Positive:
                p1 = P_PointOnLineSide(boundingBox[BoundingBox.BoxLeft], boundingBox[BoundingBox.BoxTop], line);
                p2 = P_PointOnLineSide(boundingBox[BoundingBox.BoxRight], boundingBox[BoundingBox.BoxBottom], line);
                break;

            case SlopeType.Negative:
                p1 = P_PointOnLineSide(boundingBox[BoundingBox.BoxRight], boundingBox[BoundingBox.BoxTop], line);
                p2 = P_PointOnLineSide(boundingBox[BoundingBox.BoxLeft], boundingBox[BoundingBox.BoxBottom], line);
                break;
        }

        return p1 == p2 ? p1 : -1;
    }

    private int P_PointOnDivlineSide(Fixed x, Fixed y, DividerLine line)
    {
        if (line.Dx == Fixed.Zero)
        {
            if (x <= line.X)
            {
                return line.Dy > Fixed.Zero ? 1 : 0;
            }

            return line.Dy < Fixed.Zero ? 1 : 0;
        }
        
        if (line.Dy == Fixed.Zero)
        {
            if (y <= line.Y)
            {
                return line.Dx < Fixed.Zero ? 1 : 0;
            }

            return line.Dx > Fixed.Zero ? 1 : 0;
        }

        var dx = (x - line.X);
        var dy = (y - line.Y);

        // try to quickly decide by looking at sign bits
        if (((line.Dy.Value ^ line.Dx.Value ^ dx.Value ^ dy.Value) & 0x80000000) != 0)
        {
            if (((line.Dy.Value ^ dx.Value) & 0x80000000) != 0)
            {
                return 1;       // (left is negative)
            }
            
            return 0;
        }

        var left = new Fixed(line.Dy.Value >> 8) * new Fixed(dx.Value >> 8);
        var right = new Fixed(dy.Value >> 8) * new Fixed(line.Dx.Value >> 8);

        if (right < left)
        {
            return 0;       // front side
        }

        return 1;			// back side
    }

    private void P_MakeDivline(Line li, DividerLine dl)
    {
        dl.X = li.V1.X;
        dl.Y = li.V1.Y;
        dl.Dx = li.Dx;
        dl.Dy = li.Dy;
    }

    /// <summary>
    /// Returns the fractional intercept point
    /// along the first divline.
    /// This is only called by the addthings
    /// and addlines traversers.
    /// </summary>
    private Fixed P_InterceptVector(DividerLine v2, DividerLine v1)
    {
        var den = ((v1.Dy >> 8) * v2.Dx) - ((v1.Dx >> 8) * v2.Dy);

        if (den == Fixed.Zero)
        {
            return Fixed.Zero;
        }

        //	I_Error ("P_InterceptVector: parallel");

        var num = (((v1.X - v2.X) >> 8) * v1.Dy) + (((v2.Y - v1.Y) >> 8) * v1.Dx);
        return num / den;
    }

    private void P_ZMovement(MapObject mo)
    {
        // check for smooth step up
        if (mo.Player != null && mo.Z < mo.FloorZ)
        {
            mo.Player.ViewHeight -= (mo.FloorZ - mo.Z);
            mo.Player.DeltaViewHeight = (Constants.ViewHeight - mo.Player.ViewHeight) >> 3;
        }

        // adjust height
        mo.Z += mo.MomZ;

        if ((mo.Flags & MapObjectFlag.MF_FLOAT) != 0 && mo.Target != null)
        {
            // float down towards target if too close
            if ((mo.Flags & MapObjectFlag.MF_SKULLFLY) == 0 && (mo.Flags & MapObjectFlag.MF_INFLOAT) == 0)
            {
                var dist = P_AproxDistance(mo.X - mo.Target.X, mo.Y - mo.Target.Y);
                var delta = (mo.Target.Z + (mo.Height >> 1)) - mo.Z;

                if (delta < Fixed.Zero && dist < -(delta * 3))
                {
                    mo.Z -= Constants.FloatSpeed;
                }
                else if (delta > Fixed.Zero && dist < (delta * 3))
                {
                    mo.Z += Constants.FloatSpeed;
                }
            }
        }

        // clip movement
        if (mo.Z <= mo.FloorZ)
        {
            // hit the floor

            // Note (id):
            //  somebody left this after the setting momz to 0,
            //  kinda useless there.
            if ((mo.Flags & MapObjectFlag.MF_SKULLFLY) != 0)
            {
                // the skull slammed into something
                mo.MomZ = -mo.MomZ;
            }

            if (mo.MomZ < Fixed.Zero)
            {
                if (mo.Player != null && mo.MomZ < -Constants.Gravity * 8)
                {
                    // Squat down.
                    // Decrease viewheight for a moment
                    // after hitting the ground (hard),
                    // and utter appropriate sound.
                    mo.Player.DeltaViewHeight = mo.MomZ >> 3;
                    DoomGame.Instance.Sound.StartSound(mo, SoundType.sfx_oof);
                }
                mo.MomZ = Fixed.Zero;
            }
            mo.Z = mo.FloorZ;

            if ((mo.Flags & MapObjectFlag.MF_MISSILE) != 0 && (mo.Flags & MapObjectFlag.MF_NOCLIP) == 0)
            {
                P_ExplodeMissile(mo);
                return;
            }
        }
        else if ((mo.Flags & MapObjectFlag.MF_NOGRAVITY) == 0)
        {
            if (mo.MomZ == Fixed.Zero)
            {
                mo.MomZ = -Constants.Gravity * 2;
            }
            else
            {
                mo.MomZ -= Constants.Gravity;
            }
        }

        if (mo.Z + mo.Height > mo.CeilingZ)
        {
            // hit the ceiling
            if (mo.MomZ > Fixed.Zero)
            {
                mo.MomZ = Fixed.Zero;
            }
            
            mo.Z = mo.CeilingZ - mo.Height;
            
            if ((mo.Flags & MapObjectFlag.MF_SKULLFLY) != 0)
            {   // the skull slammed into something
                mo.MomZ = -mo.MomZ;
            }

            if ((mo.Flags & MapObjectFlag.MF_MISSILE) != 0 && (mo.Flags & MapObjectFlag.MF_NOCLIP) == 0)
            {
                P_ExplodeMissile(mo);
                return;
            }
        }
    }

    private bool PIT_StompThing(MapObject thing)
    {
        if ((thing.Flags & MapObjectFlag.MF_SHOOTABLE) == 0)
        {
            return true;
        }

        var blockDist = thing.Radius + _tmThing!.Radius;

        if (Fixed.Abs(thing.X - _tmX) >= blockDist ||
            Fixed.Abs(thing.Y - _tmY) >= blockDist)
        {
            // didn't hit it
            return true;
        }

        // don't clip against self
        if (thing == _tmThing)
        {
            return true;
        }

        // monsters don't stomp things except on boss level
        if (_tmThing.Player == null && GameMap != 30)
        {
            return false;
        }

        MapObject.DamageMapObject(thing, _tmThing, _tmThing, 10000);

        return true;
    }

    public bool P_TeleportMove(MapObject thing, Fixed x, Fixed y)
    {
        // kill anything occupying the position
        _tmThing = thing;
        _tmFlags = thing.Flags;

        _tmX = x;
        _tmY = y;

        _tmBoundingBox[BoundingBox.BoxTop] = y + _tmThing.Radius;
        _tmBoundingBox[BoundingBox.BoxBottom] = y - _tmThing.Radius;
        _tmBoundingBox[BoundingBox.BoxRight] = x + _tmThing.Radius;
        _tmBoundingBox[BoundingBox.BoxLeft] = x - _tmThing.Radius;

        var newSubSec = DoomGame.Instance.Renderer.PointInSubSector(x, y);
        _ceilingLine = null;

        // The base floor/ceiling is from the subsector
        // that contains the point.
        // Any contacted lines the step closer together
        // will adjust them.
        _tmFloorZ = _tmDropOffZ = newSubSec.Sector!.FloorHeight;
        _tmCeilingZ = newSubSec.Sector.CeilingHeight;

        DoomGame.Instance.Renderer.ValidCount++;
        _numSpecHit = 0;

        // stomp on any things contacted
        var xl = (_tmBoundingBox[BoundingBox.BoxLeft] - _blockMapOriginX - Constants.MaxRadius).Value >> Constants.MapBlockShift;
        var xh = (_tmBoundingBox[BoundingBox.BoxRight] - _blockMapOriginX + Constants.MaxRadius).Value >> Constants.MapBlockShift;
        var yl = (_tmBoundingBox[BoundingBox.BoxBottom] - _blockMapOriginY - Constants.MaxRadius).Value >> Constants.MapBlockShift;
        var yh = (_tmBoundingBox[BoundingBox.BoxTop] - _blockMapOriginY + Constants.MaxRadius).Value >> Constants.MapBlockShift;

        for (var bx = xl; bx <= xh; bx++)
        {
            for (var by = yl; by <= yh; by++)
            {
                if (!P_BlockThingsIterator(bx, by, PIT_StompThing))
                {
                    return false;
                }
            }
        }

        // the move is ok,
        // so link the thing into its new position
        P_UnsetThingPosition(thing);

        thing.FloorZ = _tmFloorZ;
        thing.CeilingZ = _tmCeilingZ;
        thing.X = x;
        thing.Y = y;

        P_SetThingPosition(thing);

        return true;
    }

    public void P_MapObjectThinker(ActionParams actionParams)
    {
        var mobj = actionParams.MapObject;
        if (mobj is null)
        {
            return;
        }

        // momentum movement
        if (mobj.MomX != Fixed.Zero || mobj.MomY != Fixed.Zero || (mobj.Flags & MapObjectFlag.MF_SKULLFLY) != 0)
        {
            P_XYMovement(mobj);

            if (mobj.Action == null)
            {
                return;     // mobj was removed
            }
        }

        if ((mobj.Z != mobj.FloorZ) || mobj.MomZ != Fixed.Zero)
        {
            P_ZMovement(mobj);

            if (mobj.Action == null)
            {
                return;     // mobj was removed
            }
        }

        // cycle through states,
        // calling action functions at transitions
        if (mobj.Tics != -1)
        {
            mobj.Tics--;

            // you can cycle through multiple states in a tic
            if (mobj.Tics == 0)
            {
                if (!mobj.SetState(mobj.State!.NextState))
                {
                    return;     // freed itself
                }
            }
        }
        else
        {
            // check for nightmare respawn
            if ((mobj.Flags & MapObjectFlag.MF_COUNTKILL) == 0)
            {
                return;
            }

            // if (!respawnmonsters)
                // return;

            mobj.MoveCount++;

            if (mobj.MoveCount < 12 * 35)
            { 
                return; 
            }

            if ((LevelTime & 31) != 0)
            {
                return;
            }

            if (DoomRandom.P_Random() > 4)
            {
                return;
            }

            // P_NightmareRespawn(mobj);
        }
    }

    public MapObject P_SpawnMapObject(Fixed x, Fixed y, Fixed z, MapObjectType type)
    {
        var info = MapObjectInfo.GetByType(type);
        var mobj = new MapObject(info)
        {
            X = x,
            Y = y
        };

        if (GameSkill != SkillLevel.Nightmare)
        {
            mobj.ReactionTime = info.ReactionTime;
        }

        mobj.LastLook = DoomRandom.P_Random() % Constants.MaxPlayers;
        // do not set the state with P_SetMobjState,
        // because action routines can not be called yet
        var st = State.GetSpawnState(info.SpawnState);
        mobj.State = st;
        mobj.Tics = st.Tics;
        mobj.Sprite = st.Sprite;
        mobj.Frame = st.Frame;

        // set subsector and/or block links
        P_SetThingPosition(mobj);

        mobj.FloorZ = mobj.SubSector!.Sector!.FloorHeight;
        mobj.CeilingZ = mobj.SubSector.Sector.CeilingHeight;

        if (z == Constants.OnFloorZ)
        {
            mobj.Z = mobj.FloorZ;
        }
        else if (z == Constants.OnCeilingZ)
        {
            mobj.Z = new Fixed(mobj.CeilingZ.Value - mobj.Info.Height);
        }
        else
        {
            mobj.Z = z;
        }

        mobj.Action = P_MapObjectThinker;

        AddThinker(mobj);

        return mobj;
    }

    public void P_RemoveMapObject(MapObject mobj)
    {
        if ((mobj.Flags & MapObjectFlag.MF_SPECIAL) != 0
            && (mobj.Flags & MapObjectFlag.MF_DROPPED) == 0
            && (mobj.Type != MapObjectType.MT_INV)
            && (mobj.Type != MapObjectType.MT_INS))
        {
            //itemrespawnque[iquehead] = mobj->spawnpoint;
            //itemrespawntime[iquehead] = leveltime;
            //iquehead = (iquehead + 1) & (ITEMQUESIZE - 1);

            //// lose one off the end?
            //if (iquehead == iquetail)
            //    iquetail = (iquetail + 1) & (ITEMQUESIZE - 1);
        }

        // unlink from sector and block lists
        P_UnsetThingPosition(mobj);

        // stop any playing sound
        DoomGame.Instance.Sound.StopSound(mobj);

        // free block
        RemoveThinker(mobj);
    }

    /// <summary>
    /// P_LoadLineDefs. Also counts secret lines for intermissions.
    /// </summary>
    private void P_LoadLineDefs(int lump)
    {
        const int sizeOfMapLineDef = 2 + 2 + 2 + 2 + 2 + 2 + 2;

        _numLines = DoomGame.Instance.WadData.LumpLength(lump) / sizeOfMapLineDef;
        _lines = new Line[_numLines];
        
        var data = DoomGame.Instance.WadData.GetLumpNum(lump, PurgeTag.Static)!;
        using var stream = new MemoryStream(data, false);
        using var reader = new BinaryReader(stream);

        for (var i = 0; i < _numLines; i++)
        {
            var ld = Line.ReadFromWadData(reader, _vertices);
            _lines[i] = ld;

            if (ld.Dx == Fixed.Zero)
            {
                ld.SlopeType = SlopeType.Vertical;
            }
            else if (ld.Dy == Fixed.Zero)
            {
                ld.SlopeType = SlopeType.Horizontal;
            }
            else
            {
                ld.SlopeType = ld.Dy / ld.Dx > Fixed.Zero ? SlopeType.Positive : SlopeType.Negative;
            }

            if (ld.V1.X < ld.V2.X)
            {
                ld.BoundingBox[BoundingBox.BoxLeft] = ld.V1.X;
                ld.BoundingBox[BoundingBox.BoxRight] = ld.V2.X;
            }
            else
            {
                ld.BoundingBox[BoundingBox.BoxLeft] = ld.V2.X;
                ld.BoundingBox[BoundingBox.BoxRight] = ld.V1.X;
            }

            if (ld.V1.Y < ld.V2.Y)
            {
                ld.BoundingBox[BoundingBox.BoxBottom] = ld.V1.Y;
                ld.BoundingBox[BoundingBox.BoxTop] = ld.V2.Y;
            }
            else
            {
                ld.BoundingBox[BoundingBox.BoxBottom] = ld.V2.Y;
                ld.BoundingBox[BoundingBox.BoxTop] = ld.V1.Y;
            }

            if (ld.SideNum[0] != -1)
            {
                ld.FrontSector = _sides[ld.SideNum[0]].Sector;
            }

            if (ld.SideNum[1] != -1)
            {
                ld.BackSector = _sides[ld.SideNum[1]].Sector;
            }
        }
    }
    
    private void P_LoadSideDefs(int lump)
    {
        const int mapSideDefSize = 2 + 2 + 8 + 8 + 8 + 2; // size of mapsidedef_t struct
        _numSides = DoomGame.Instance.WadData.LumpLength(lump) / mapSideDefSize;
        _sides = new SideDef[_numSides];
        
        var data = DoomGame.Instance.WadData.GetLumpNum(lump, PurgeTag.Static)!;
        using var stream = new MemoryStream(data, false);
        using var reader = new BinaryReader(stream);

        for (var i = 0; i < _numSides; i++)
        {
            _sides[i] = SideDef.ReadFromWadData(reader, _sectors);
        }
    }

    private void P_LoadBlockMap(int lump)
    {
        var data = DoomGame.Instance.WadData.GetLumpNum(lump, PurgeTag.Level)!;
        using var stream = new MemoryStream(data, false);
        using var reader = new BinaryReader(stream);

        var count = DoomGame.Instance.WadData.LumpLength(lump) / 2;
        _blockMapLump = new short[count];
        for (var i = 0; i < _blockMapLump.Length; i++)
        {
            _blockMapLump[i] = reader.ReadInt16();
        }

        
        // blockmap = blockmaplump+4;
        _blockMap = new short[count - 4];
        for (var i = 4; i < _blockMapLump.Length; i++)
        {
            _blockMap[i - 4] = _blockMapLump[i];
        }

        _blockMapOriginX = Fixed.FromInt(_blockMapLump[0]);
        _blockMapOriginY = Fixed.FromInt(_blockMapLump[1]);
        _blockMapWidth = _blockMapLump[2];
        _blockMapHeight = _blockMapLump[3];

        _blockMapOffsets = new ushort[_blockMapWidth * _blockMapHeight];
        for (var i = 4; i < _blockMapWidth * _blockMapHeight + 4; i++)
        {
            _blockMapOffsets[i - 4] = (ushort)_blockMapLump[i];
        }

        // clear out mobj chains
        //count = _blockMapWidth * _blockMapHeight;
        //_blockLinks = Z_Malloc(count, PurgeTag.Level, 0);
        //memset(_blockLinks, 0, count);
        _blockLinks = new MapObject?[_blockMapWidth * _blockMapHeight];
        //for (var i = 0; i < _blockMapWidth; i++)
        //{
        //    _blockLinks[i] = new MapObject[_blockMapHeight];
        //}
    }

    private void P_GroupLines()
    {
        // look up sector number for each subsector
        for (var i = 0; i < _numSubSectors; i++)
        {
            var subSector = _subSectors[i];
            var seg = _segments[subSector.FirstLine];
            subSector.Sector = seg.SideDef.Sector;
        }

        // count number of lines in each sector
        var total = 0;
        for (var i = 0; i < _numLines; i++)
        {
            total++;
            var line = _lines[i];
            line.FrontSector!.LineCount++;

            if (line.BackSector != null && line.BackSector != line.FrontSector)
            {
                line.BackSector.LineCount++;
                total++;
            }
        }

        // build line tables for each sector	
        var boundingBox = new Fixed[4];
        for (var i = 0; i < _numSectors; i++)
        {
            BoundingBox.ClearBox(boundingBox);

            var lineIdx = 0;
            var sector = _sectors[i];
            sector.Lines = new Line[sector.LineCount];

            for (var j = 0; j < _numLines; j++)
            {
                var line = _lines[j];
                if (line.FrontSector == sector || line.BackSector == sector)
                {
                    sector.Lines[lineIdx++] = line;
                    BoundingBox.AddToBox(boundingBox, line.V1.X, line.V1.Y);
                    BoundingBox.AddToBox(boundingBox, line.V2.X, line.V2.Y);
                }
            }

            if (lineIdx != sector.LineCount)
            {
                DoomGame.Error("P_GroupLines: miscounted");
                return;
            }

            // set the degenmobj_t to the middle of the bounding box
            sector.SoundOrigin.X = (boundingBox[BoundingBox.BoxRight] + boundingBox[BoundingBox.BoxLeft]) / 2;
            sector.SoundOrigin.Y = (boundingBox[BoundingBox.BoxTop] + boundingBox[BoundingBox.BoxBottom]) / 2;

            // adjust bounding box to map blocks
            var block = (boundingBox[BoundingBox.BoxTop] - _blockMapOriginY + Constants.MaxRadius).Value >> Constants.MapBlockShift;
            block = block >= _blockMapHeight ? _blockMapHeight - 1 : block;
            sector.BlockBox[BoundingBox.BoxTop] = block;

            block = (boundingBox[BoundingBox.BoxBottom] - _blockMapOriginY - Constants.MaxRadius).Value >> Constants.MapBlockShift;
            block = block < 0 ? 0 : block;
            sector.BlockBox[BoundingBox.BoxBottom] = block;

            block = (boundingBox[BoundingBox.BoxRight] - _blockMapOriginX + Constants.MaxRadius).Value >> Constants.MapBlockShift;
            block = block >= _blockMapWidth ? _blockMapWidth - 1 : block;
            sector.BlockBox[BoundingBox.BoxRight] = block;

            block = (boundingBox[BoundingBox.BoxLeft] - _blockMapOriginX - Constants.MaxRadius).Value >> Constants.MapBlockShift;
            block = block < 0 ? 0 : block;
            sector.BlockBox[BoundingBox.BoxLeft] = block;
        }
    }
    
    private void SetupLevel(int episode, int map, int playerMask, SkillLevel skillLevel)
    {
        TotalKills = TotalItems = TotalSecrets = _wmInfo.MaxFrags = 0;
        _wmInfo.ParTime = 180;

        for (var i = 0; i < Constants.MaxPlayers; i++)
        {
            Players[i].KillCount = Players[i].SecretCount = Players[i].ItemCount = 0;
        }

        // Initial height of PointOfView
        // will be set by player think.
        Players[ConsolePlayer].ViewZ = new Fixed(1);

        // Make sure all sounds are stopped before Z_FreeTags.
        DoomGame.Instance.Sound.Start();

        // TODO Free up memory?
        // Z_FreeTags (PU_LEVEL, PU_PURGELEVEL-1);

        InitThinkers();

        // if working with a development map, reload it
        // W_Reload();

        // find map name
        var lumpName = DoomGame.Instance.GameMode == GameMode.Commercial ? $"map{map:00}" : $"E{episode}M{map}";
        var lumpNum = DoomGame.Instance.WadData.GetNumForName(lumpName);

        LevelTime = 0;

        // note: most of this ordering is important	
        P_LoadBlockMap(lumpNum + MapLumps.BlockMap);
        P_LoadVertexes(lumpNum + MapLumps.Vertices);
        P_LoadSectors(lumpNum + MapLumps.Sectors);
        P_LoadSideDefs(lumpNum + MapLumps.SideDefs);

        P_LoadLineDefs(lumpNum + MapLumps.LineDefs);
        P_LoadSubSectors(lumpNum + MapLumps.SubSectors);
        P_LoadNodes(lumpNum + MapLumps.Nodes);
        P_LoadSegments(lumpNum + MapLumps.Segs);

        _rejectMatrix = DoomGame.Instance.WadData.GetLumpNum(lumpNum + MapLumps.Reject, PurgeTag.Level)!;
        P_GroupLines();

        _bodyQueueSlot = 0;
        _deathMatchStartIdx = 0;
        P_LoadThings(lumpNum + MapLumps.Things);

        // if deathmatch, randomly spawn the active players
        if (DeathMatch)
        {
            for (var i = 0; i < Constants.MaxPlayers; i++)
            {
                if (PlayerInGame[i])
                {
                    Players[i].MapObject = null;
                    DeathMatchSpawnPlayer(i);
                }
            }
        }

        // clear special respawning que
        // iquehead = iquetail = 0;

        // set up world state
        P_SpawnSpecials();

        // build subsector connect matrix
        //	UNUSED P_ConnectSubsectors ();

        // preload graphics
        if (PreCache)
        {
            DoomGame.Instance.Renderer.PreCacheLevel();
        }
    }

    private void P_InitSwitchList()
    {
        var episode = 1;
        if (DoomGame.Instance.GameMode == GameMode.Registered)
        {
            episode = 2;
        }
        else if (DoomGame.Instance.GameMode == GameMode.Commercial)
        {
            episode = 3;
        }

        for (int i = 0, index = 0; i < Constants.MaxSwitches; i++)
        {
            if (SwitchControl.PredefinedSwitchList[i].Episode == 0)
            {
                _numSwitches = index / 2;
                _switchList[index] = -1;
                break;
            }

            if (SwitchControl.PredefinedSwitchList[i].Episode <= episode)
            {
                _switchList[index++] = DoomGame.Instance.Renderer.TextureNumForName(SwitchControl.PredefinedSwitchList[i].Name1);
                _switchList[index++] = DoomGame.Instance.Renderer.TextureNumForName(SwitchControl.PredefinedSwitchList[i].Name2);
            }
        }
    }

    public void P_Init()
    {
        P_InitSwitchList();
        P_InitPicAnims();
        DoomGame.Instance.Renderer.InitSprites(Constants.SpriteNames);
    }

    /// <summary>
    /// Function that changes wall texture.
    /// Tell it if switch is ok to use again (1=yes, it's a button).
    /// </summary>
    private void P_ChangeSwitchTexture(Line line, bool useAgain)
    {
        if (!useAgain)
        {
            line.Special = 0;
        }

        var texTop = _sides[line.SideNum[0]].TopTexture;
        var texMid = _sides[line.SideNum[0]].MidTexture;
        var texBot = _sides[line.SideNum[0]].BottomTexture;

        var sound = SoundType.sfx_swtchn;

        // EXIT SWITCH?
        if (line.Special == 11)
        {
            sound = SoundType.sfx_swtchx;
        }

        for (var i = 0; i < _numSwitches * 2; i++)
        {
            if (_switchList[i] == texTop)
            {
                DoomGame.Instance.Sound.StartSound(_buttonList[0].SoundOrigin, sound);
                _sides[line.SideNum[0]].TopTexture = _switchList[i ^ 1];

                if (useAgain)
                {
                    P_StartButton(line, ButtonWhere.Top, _switchList[i], Constants.ButtonTime);
                }

                return;
            }

            if (_switchList[i] == texMid)
            {
                DoomGame.Instance.Sound.StartSound(_buttonList[0].SoundOrigin, sound);
                _sides[line.SideNum[0]].MidTexture = _switchList[i ^ 1];

                if (useAgain)
                {
                    P_StartButton(line, ButtonWhere.Middle, _switchList[i], Constants.ButtonTime);
                }

                return;
            }
                
            if (_switchList[i] == texBot)
            {
                DoomGame.Instance.Sound.StartSound(_buttonList[0].SoundOrigin, sound);
                _sides[line.SideNum[0]].BottomTexture = _switchList[i ^ 1];

                if (useAgain)
                {
                    P_StartButton(line, ButtonWhere.Bottom, _switchList[i], Constants.ButtonTime);
                }

                return;
            }
        }
    }

    /// <summary>
    /// Called when a thing uses a special line.
    /// Only the front sides of lines are usable.
    /// </summary>
    public bool P_UseSpecialLine(MapObject thing, Line line, int side)
    {
        // Err...
        // Use the back sides of VERY SPECIAL lines...
        if (side != 0)
        {
            switch (line.Special)
            {
                case 124:
                    // Sliding door open&close
                    // UNUSED?
                    break;

                default:
                    return false;
            }
        }

        // Switches that other things can activate.
        if (thing.Player == null)
        {
            // never open secret doors
            if ((line.Flags & Constants.Line.Secret) != 0)
            {
                return false;
            }

            switch (line.Special)
            {
                case 1:     // MANUAL DOOR RAISE
                case 32:    // MANUAL BLUE
                case 33:    // MANUAL RED
                case 34:    // MANUAL YELLOW
                    break;

                default:
                    return false;
            }
        }

        // do something  
        switch (line.Special)
        {
            // MANUALS
            case 1:     // Vertical Door
            case 26:        // Blue Door/Locked
            case 27:        // Yellow Door /Locked
            case 28:        // Red Door /Locked

            case 31:        // Manual door open
            case 32:        // Blue locked door open
            case 33:        // Red locked door open
            case 34:        // Yellow locked door open

            case 117:       // Blazing door raise
            case 118:       // Blazing door open
                Trigger.VerticalDoorEvent(line, thing);
                break;

            //UNUSED - Door Slide Open&Close
            // case 124:
            // EV_SlidingDoor (line, thing);
            // break;

            // SWITCHES
            case 7:
                // Build Stairs
                if (Trigger.BuildStairsEvent(line, StaircaseType.Build8))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 9:
                // Change Donut
                if (Trigger.DonutEvent(line))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 11:
                // Exit level
                P_ChangeSwitchTexture(line, false);
                ExitLevel();
                break;

            case 14:
                // Raise Floor 32 and change texture
                if (Trigger.PlatformEvent(line, PlatformType.RaiseAndChange, 32))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 15:
                // Raise Floor 24 and change texture
                if (Trigger.PlatformEvent(line, PlatformType.RaiseAndChange, 24))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 18:
                // Raise Floor to next highest floor
                if (Trigger.FloorEvent(line, FloorType.RaiseFloorToNearest))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 20:
                // Raise Plat next highest floor and change texture
                if (Trigger.PlatformEvent(line, PlatformType.RaiseToNearestAndChange, 0))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 21:
                // PlatDownWaitUpStay
                if (Trigger.PlatformEvent(line, PlatformType.DownWaitUpStay, 0))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 23:
                // Lower Floor to Lowest
                if (Trigger.FloorEvent(line, FloorType.LowerFloorToLowest))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 29:
                // Raise Door
                if (Trigger.DoorEvent(line, DoorType.Normal))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 41:
                // Lower Ceiling to Floor
                if (Trigger.CeilingEvent(line, CeilingType.LowerToFloor))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 71:
                // Turbo Lower Floor
                if (Trigger.FloorEvent(line, FloorType.TurboLower))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 49:
                // Ceiling Crush And Raise
                if (Trigger.CeilingEvent(line, CeilingType.CrushAndRaise))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 50:
                // Close Door
                if (Trigger.DoorEvent(line, DoorType.Close))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 51:
                // Secret EXIT
                P_ChangeSwitchTexture(line, false);
                SecretExitLevel();
                break;

            case 55:
                // Raise Floor Crush
                if (Trigger.FloorEvent(line, FloorType.RaiseFloorCrush))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 101:
                // Raise Floor
                if (Trigger.FloorEvent(line, FloorType.RaiseFloor))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 102:
                // Lower Floor to Surrounding floor height
                if (Trigger.FloorEvent(line, FloorType.LowerFloor))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 103:
                // Open Door
                if (Trigger.DoorEvent(line, DoorType.Open))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 111:
                // Blazing Door Raise (faster than TURBO!)
                if (Trigger.DoorEvent(line, DoorType.BlazeRaise))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 112:
                // Blazing Door Open (faster than TURBO!)
                if (Trigger.DoorEvent(line, DoorType.BlazeOpen))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 113:
                // Blazing Door Close (faster than TURBO!)
                if (Trigger.DoorEvent(line, DoorType.BlazeClose))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 122:
                // Blazing PlatDownWaitUpStay
                if (Trigger.PlatformEvent(line, PlatformType.BlazeDWUS, 0))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 127:
                // Build Stairs Turbo 16
                if (Trigger.BuildStairsEvent(line, StaircaseType.Turbo16))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 131:
                // Raise Floor Turbo
                if (Trigger.FloorEvent(line, FloorType.RaiseFloorTurbo))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 133:
            // BlzOpenDoor BLUE
            case 135:
            // BlzOpenDoor RED
            case 137:
                // BlzOpenDoor YELLOW
                if (Trigger.LockedDoorEvent(line, DoorType.BlazeOpen, thing))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            case 140:
                // Raise Floor 512
                if (Trigger.FloorEvent(line, FloorType.RaiseFloor512))
                {
                    P_ChangeSwitchTexture(line, false);
                }
                break;

            // BUTTONS
            case 42:
                // Close Door
                if (Trigger.DoorEvent(line, DoorType.Close))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 43:
                // Lower Ceiling to Floor
                if (Trigger.CeilingEvent(line, CeilingType.LowerToFloor))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 45:
                // Lower Floor to Surrounding floor height
                if (Trigger.FloorEvent(line, FloorType.LowerFloor))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 60:
                // Lower Floor to Lowest
                if (Trigger.FloorEvent(line, FloorType.LowerFloorToLowest))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 61:
                // Open Door
                if (Trigger.DoorEvent(line, DoorType.Open))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 62:
                // PlatDownWaitUpStay
                if (Trigger.PlatformEvent(line, PlatformType.DownWaitUpStay, 1))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 63:
                // Raise Door
                if (Trigger.DoorEvent(line, DoorType.Normal))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 64:
                // Raise Floor to ceiling
                if (Trigger.FloorEvent(line, FloorType.RaiseFloor))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 66:
                // Raise Floor 24 and change texture
                if (Trigger.PlatformEvent(line, PlatformType.RaiseAndChange, 24))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 67:
                // Raise Floor 32 and change texture
                if (Trigger.PlatformEvent(line, PlatformType.RaiseAndChange, 32))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 65:
                // Raise Floor Crush
                if (Trigger.FloorEvent(line, FloorType.RaiseFloorCrush))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 68:
                // Raise Plat to next highest floor and change texture
                if (Trigger.PlatformEvent(line, PlatformType.RaiseToNearestAndChange, 0))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 69:
                // Raise Floor to next highest floor
                if (Trigger.FloorEvent(line, FloorType.RaiseFloorToNearest))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 70:
                // Turbo Lower Floor
                if (Trigger.FloorEvent(line, FloorType.TurboLower))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 114:
                // Blazing Door Raise (faster than TURBO!)
                if (Trigger.DoorEvent(line, DoorType.BlazeRaise))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 115:
                // Blazing Door Open (faster than TURBO!)
                if (Trigger.DoorEvent(line, DoorType.BlazeOpen))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 116:
                // Blazing Door Close (faster than TURBO!)
                if (Trigger.DoorEvent(line, DoorType.BlazeClose))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 123:
                // Blazing PlatDownWaitUpStay
                if (Trigger.PlatformEvent(line, PlatformType.BlazeDWUS, 0))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 132:
                // Raise Floor Turbo
                if (Trigger.FloorEvent(line, FloorType.RaiseFloorTurbo))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 99:
            // BlzOpenDoor BLUE
            case 134:
            // BlzOpenDoor RED
            case 136:
                // BlzOpenDoor YELLOW
                if (Trigger.LockedDoorEvent(line, DoorType.BlazeOpen, thing))
                {
                    P_ChangeSwitchTexture(line, true);
                }
                break;

            case 138:
                // Light Turn On
                Trigger.LightTurnOnEvent(line, 255);
                P_ChangeSwitchTexture(line, true);
                break;

            case 139:
                // Light Turn Off
                Trigger.LightTurnOnEvent(line, 35);
                P_ChangeSwitchTexture(line, true);
                break;

        }

        return true;
    }

    /// <summary>
    /// Start a button counting down until it turns off.
    /// </summary>
    private void P_StartButton(Line line, ButtonWhere w, int texture, int time)
    {
        // See if button is already pressed
        for (var i = 0; i < Constants.MaxButtons; i++)
        {
            if (_buttonList[i].Timer != 0 && _buttonList[i].Line == line)
            {
                return;
            }
        }

        for (var i = 0; i < Constants.MaxButtons; i++)
        {
            if (_buttonList[i].Timer == 0)
            {
                _buttonList[i].Line = line;
                _buttonList[i].Where = w;
                _buttonList[i].Texture = texture;
                _buttonList[i].Timer = time;
                _buttonList[i].SoundOrigin = line.FrontSector?.SoundOrigin;
                return;
            }
        }

        DoomGame.Error("P_StartButton: no button slots left!");
    }

    private void P_InitPicAnims()
    {
        //	Init animation
        for (var i = 0; i < Constants.MaxAnimations; i++)
        {
            _animations[i] = new AnimatingItem();
        }

        for (var i = 0; i < AnimationDefinition.Definitions.Length; i++)
        {
            _lastAnimation = _animations[i];
            var animDef = AnimationDefinition.Definitions[i];
            if (animDef.IsTexture)
            {
                // different episode ?
                if (DoomGame.Instance.Renderer.CheckTextureNumForName(animDef.StartName) == -1)
                {
                    continue;
                }

                _lastAnimation.PicNum = DoomGame.Instance.Renderer.TextureNumForName(animDef.EndName);
                _lastAnimation.BasePic = DoomGame.Instance.Renderer.TextureNumForName(animDef.StartName);
            }
            else
            {
                if (DoomGame.Instance.WadData.CheckNumForName(animDef.StartName) == -1)
                {
                    continue;
                }

                _lastAnimation.PicNum = DoomGame.Instance.Renderer.FlatNumForName(animDef.EndName);
                _lastAnimation.BasePic = DoomGame.Instance.Renderer.FlatNumForName(animDef.StartName);
            }

            _lastAnimation.IsTexture = animDef.IsTexture;
            _lastAnimation.NumPics = _lastAnimation.PicNum - _lastAnimation.BasePic + 1;

            if (_lastAnimation.NumPics < 2)
            {
                DoomGame.Error($"P_InitPicAnims: bad cycle from {animDef.StartName} to {animDef.EndName}");
                return;
            }

            _lastAnimation.Speed = animDef.Speed;
        }
    }

    private void P_SpawnSpecials()
    {
        var episode = 1;
        if (DoomGame.Instance.WadData.CheckNumForName("TEXTURE2") >= 0)
        {
            episode = 2;
        }

        //// See if -TIMER needs to be used;
        //LevelTimer = false;

        //i = M_CheckParm("-avg");
        //if (i && deathmatch)
        //{
        //    levelTimer = true;
        //    levelTimeCount = 20 * 60 * 35;
        //}

        //i = M_CheckParm("-timer");
        //if (i && deathmatch)
        //{
        //    int time;
        //    time = atoi(myargv[i + 1]) * 60 * 35;
        //    levelTimer = true;
        //    levelTimeCount = time;
        //}

        //	Init special SECTORs.
        foreach (var sector in Sectors.Where(x => x.Special != 0))
        {
            switch (sector.Special)
            {
                case 1:
                    // FLICKERING LIGHTS
                    Trigger.SpawnLightFlash(sector);
                    break;

                case 2:
                    // STROBE FAST
                    Trigger.SpawnStrobeFlash(sector, StrobeFlash.FastDark, 0);
                    break;

                case 3:
                    // STROBE SLOW
                    Trigger.SpawnStrobeFlash(sector, StrobeFlash.SlowDark, 0);
                    break;

                case 4:
                    // STROBE FAST/DEATH SLIME
                    Trigger.SpawnStrobeFlash(sector, StrobeFlash.FastDark, 0);
                    sector.Special = 4;
                    break;

                case 8:
                    // GLOWING LIGHT
                    Trigger.SpawnGlowingLight(sector);
                    break;
                case 9:
                    // SECRET SECTOR
                    TotalSecrets++;
                    break;

                case 10:
                    // DOOR CLOSE IN 30 SECONDS
                    Trigger.SpawnDoorCloseIn30(sector);
                    break;

                case 12:
                    // SYNC STROBE SLOW
                    Trigger.SpawnStrobeFlash(sector, StrobeFlash.SlowDark, 1);
                    break;

                case 13:
                    // SYNC STROBE FAST
                    Trigger.SpawnStrobeFlash(sector, StrobeFlash.FastDark, 1);
                    break;

                case 14:
                    // DOOR RAISE IN 5 MINUTES
                    Trigger.SpawnDoorRaiseIn5Mins(sector);
                    break;

                case 17:
                    Trigger.SpawnFireFlicker(sector);
                    break;
            }
        }


        //	Init line EFFECTs
        _numLineSpecials = 0;
        foreach (var line in _lines.Where(x => x.Special == 48))
        {
            // EFFECT FIRSTCOL SCROLL+
            _lineSpecialList[_numLineSpecials++] = line;
            break;
        }

        //	Init other misc stuff
        for (var i = 0; i < Ceiling.MaxCeilings; i++)
        {
            _activeCeilings[i] = null;
        }

        for (var i = 0; i < Platform.MaxPlats; i++)
        {
            _activePlats[i] = null;
        }

        for (var i = 0; i < Constants.MaxButtons; i++)
        {
            _buttonList[i] = new Button();
        }

        // UNUSED: no horizonal sliders.
        //	P_InitSlidingDoorFrames();
    }

    public void P_AddActiveCeiling(Ceiling ceiling)
    {
        for (var i = 0; i < Ceiling.MaxCeilings; i++)
        {
            if (_activeCeilings[i] == null)
            {
                _activeCeilings[i] = ceiling;
                return;
            }
        }
    }

    public void P_RemoveActiveCeiling(Ceiling ceiling)
    {
        for (var i = 0; i < Ceiling.MaxCeilings; i++)
        {
            if (_activeCeilings[i] != ceiling)
            {
                continue;
            }

            ceiling.Sector!.SpecialData = null;
            RemoveThinker(ceiling);
            _activeCeilings[i] = null;
            return;
        }
    }

    /// <summary>
    /// Restart a ceiling that's in-stasis
    /// </summary>
    public void P_ActivateInStasisCeiling(Line line)
    {
        for (var i = 0; i < Ceiling.MaxCeilings; i++)
        {
            var ceiling = _activeCeilings[i];
            if (ceiling == null ||
                ceiling.Tag != line.Tag ||
                ceiling.Direction != 0)
            {
                continue;
            }

            ceiling.Direction = ceiling.OldDirection;
            ceiling.Action = Trigger.MoveCeiling;
            return;
        }
    }

    public int CeilingCrushStopEvent(Line line)
    {
        var rtn = 0;
        for (var i = 0; i < Ceiling.MaxCeilings; i++)
        {
            var ceiling = _activeCeilings[i];
            if (ceiling != null &&
                ceiling.Tag == line.Tag &&
                ceiling.Direction != 0)
            {
                ceiling.OldDirection = ceiling.Direction;
                ceiling.Action = null;
                ceiling.Direction = 0; // in stasis
                rtn = 1;
            }
        }

        return rtn;
    }

    public void StopPlatformEvent(Line line)
    {
        foreach (var platform in _activePlats.Where(x => x != null && x.Status != PlatformState.InStatis && x.Tag == line.Tag))
        {
            platform!.OldStatus = platform.Status;
            platform.Status = PlatformState.InStatis;
            platform.Action = null;
        }
    }

    public void P_AddActivePlatform(Platform platform)
    {
        for (var i = 0; i < Platform.MaxPlats; i++)
        {
            if (_activePlats[i] == null)
            {
                _activePlats[i] = platform;
                return;
            }
        }

        DoomGame.Error("P_AddActivePlat: no more plats!");
    }

    public void P_RemoveActivePlatform(Platform platform)
    {
        for (var i = 0; i < Platform.MaxPlats; i++)
        { 
            if (_activePlats[i] != platform)
            {
                continue;
            }

            platform.Sector!.SpecialData = null;
            RemoveThinker(platform);
            _activePlats[i] = null;
            return;
        }

        DoomGame.Error("P_RemoveActivePlat: can't find plat!");
    }

    public void P_ActivateInStasis(int tag)
    {
        for (var i = 0; i < Platform.MaxPlats; i++)
        {
            var platform = _activePlats[i];
            if (platform == null ||
                platform.Tag != tag ||
                platform.Status != PlatformState.InStatis)
            {
                continue;
            }

            platform.Status = platform.OldStatus;
            platform.Action = Trigger.PlatformRaise;
            return;
        }
    }

    private void P_TouchSpecialThing(MapObject special, MapObject toucher)
    {
        var delta = special.Z - toucher.Z;

        if (delta > toucher.Height || delta < Fixed.FromInt(-8))
        {
            // out of reach
            return;
        }

        var sound = SoundType.sfx_itemup;
        var player = toucher.Player!;

        // Dead thing touching.
        // Can happen with a sliding player corpse.
        if (toucher.Health <= 0)
        {
            return;
        }

        // Identify the sprite.
        switch (special.Sprite)
        {
            // armor
            case SpriteNum.SPR_ARM1:
                if (!player.GiveArmor(1))
                {
                    return;
                }

                player.Message = Messages.GotArmor;
                break;

            case SpriteNum.SPR_ARM2:
                if (!player.GiveArmor(2))
                {
                    return;
                }

                player.Message = Messages.GotMega;
                break;

            case SpriteNum.SPR_BON1:
                player.Health++; // can go over 100%
                if (player.Health > 200)
                {
                    player.Health = 200;
                }

                player.MapObject!.Health = player.Health;
                player.Message = Messages.GotHealthBonus;

                break;

            case SpriteNum.SPR_BON2:
                player.ArmorPoints++; // can go over 100%
                if (player.ArmorPoints > 200)
                {
                    player.ArmorPoints = 200;
                }

                if (player.ArmorType == 0)
                {
                    player.ArmorType = 1;
                }

                player.Message = Messages.GotArmorBonus;

                break;

            case SpriteNum.SPR_SOUL:
                player.Health += 100;
                if (player.Health > 200)
                {
                    player.Health = 200;
                }

                player.MapObject!.Health = player.Health;
                player.Message = Messages.GotSuper;
                sound = SoundType.sfx_getpow;

                break;

            case SpriteNum.SPR_MEGA:
                if (DoomGame.Instance.GameMode != GameMode.Commercial)
                {
                    return;
                }

                player.Health = 200;
                player.MapObject!.Health = player.Health;
                player.GiveArmor(2);
                player.Message = Messages.GotMegaSphere;
                sound = SoundType.sfx_getpow;

                break;

            // cards
            // leave cards for everyone
            case SpriteNum.SPR_BKEY:
                if (!player.Cards[(int)KeyCardType.BlueCard])
                {
                    player.Message = Messages.GotBlueCard;
                }
                player.GiveCard(KeyCardType.BlueCard);
                if (!NetGame)
                {
                    break;
                }

                return;

            case SpriteNum.SPR_YKEY:
                if (!player.Cards[(int)KeyCardType.YellowCard])
                {
                    player.Message = Messages.GotYellowCard;
                }
                player.GiveCard(KeyCardType.YellowCard);
                if (!NetGame)
                {
                    break;
                }

                return;

            case SpriteNum.SPR_RKEY:
                if (!player.Cards[(int)KeyCardType.RedCard])
                {
                    player.Message = Messages.GotRedCard;
                }
                player.GiveCard(KeyCardType.RedCard);
                if (!NetGame)
                {
                    break;
                }

                return;

            case SpriteNum.SPR_BSKU:
                if (!player.Cards[(int)KeyCardType.BlueSkull])
                {
                    player.Message = Messages.GotBlueSkull;
                }
                player.GiveCard(KeyCardType.BlueSkull);
                if (!NetGame)
                {
                    break;
                }

                return;

            case SpriteNum.SPR_YSKU:
                if (!player.Cards[(int)KeyCardType.YellowSkull])
                {
                    player.Message = Messages.GotYellowSkull;
                }
                player.GiveCard(KeyCardType.YellowSkull);
                if (!NetGame)
                {
                    break;
                }

                return;

            case SpriteNum.SPR_RSKU:
                if (!player.Cards[(int)KeyCardType.RedSkull])
                {
                    player.Message = Messages.GotRedSkull;
                }
                player.GiveCard(KeyCardType.RedSkull);
                if (!NetGame)
                {
                    break;
                }

                return;

            // medikits, heals
            case SpriteNum.SPR_STIM:
                if (!player.GiveBody(10))
                {
                    return;
                }

                player.Message = Messages.GotStim;
                break;

            case SpriteNum.SPR_MEDI:
                if (!player.GiveBody(25))
                {
                    return;
                }

                if (player.Health < 25)
                {
                    player.Message = Messages.GotMedikitAndItWasNeeded;
                }
                else
                {
                    player.Message = Messages.GotMedikit;
                }

                break;

            case SpriteNum.SPR_PINV:
                if (!player.GivePower(PowerUpType.Invulnerability))
                {
                    return;
                }

                player.Message = Messages.GotInvulnerability;
                sound = SoundType.sfx_getpow;

                break;

            case SpriteNum.SPR_PSTR:
                if (!player.GivePower(PowerUpType.Strength))
                {
                    return;
                }

                player.Message = Messages.GotBerserk;
                if (player.ReadyWeapon != WeaponType.Fist)
                {
                    player.PendingWeapon = WeaponType.Fist;
                }
                sound = SoundType.sfx_getpow;

                break;

            case SpriteNum.SPR_PINS:
                if (!player.GivePower(PowerUpType.Invisibility))
                {
                    return;
                }

                player.Message = Messages.GotInvisibility;
                sound = SoundType.sfx_getpow;

                break;

            case SpriteNum.SPR_SUIT:
                if (!player.GivePower(PowerUpType.IronFeet))
                {
                    return;
                }

                player.Message = Messages.GotSuit;
                sound = SoundType.sfx_getpow;

                break;

            case SpriteNum.SPR_PMAP:
                if (!player.GivePower(PowerUpType.AllMap))
                {
                    return;
                }

                player.Message = Messages.GotMap;
                sound = SoundType.sfx_getpow;

                break;

            case SpriteNum.SPR_PVIS:
                if (!player.GivePower(PowerUpType.InfraRed))
                {
                    return;
                }

                player.Message = Messages.GotVisor;
                sound = SoundType.sfx_getpow;

                break;

            // ammo
            case SpriteNum.SPR_CLIP:
                if ((special.Flags & MapObjectFlag.MF_DROPPED) != 0)
                {
                    if (!player.GiveAmmo(AmmoType.Clip, 0))
                    {
                        return;
                    }
                }
                else
                {
                    if (!player.GiveAmmo(AmmoType.Clip, 1))
                    {
                        return;
                    }
                }

                player.Message = Messages.GotClip;
                break;

            case SpriteNum.SPR_AMMO:
                if (!player.GiveAmmo(AmmoType.Clip, 5))
                {
                    return;
                }

                player.Message = Messages.GotClipBox;
                break;

            case SpriteNum.SPR_ROCK:
                if (!player.GiveAmmo(AmmoType.Missile, 1))
                {
                    return;
                }

                player.Message = Messages.GotRocket;
                break;

            case SpriteNum.SPR_BROK:
                if (!player.GiveAmmo(AmmoType.Missile, 5))
                {
                    return;
                }

                player.Message = Messages.GotRocketBox;
                break;

            case SpriteNum.SPR_CELL:
                if (!player.GiveAmmo(AmmoType.Cell, 1))
                {
                    return;
                }

                player.Message = Messages.GotCell;
                break;

            case SpriteNum.SPR_CELP:
                if (!player.GiveAmmo(AmmoType.Cell, 5))
                {
                    return;
                }

                player.Message = Messages.GotCellBox;
                break;

            case SpriteNum.SPR_SHEL:
                if (!player.GiveAmmo(AmmoType.Shell, 1))
                {
                    return;
                }

                player.Message = Messages.GotShells;
                break;

            case SpriteNum.SPR_SBOX:
                if (!player.GiveAmmo(AmmoType.Shell, 5))
                {
                    return;
                }

                player.Message = Messages.GotShellBox;
                break;

            case SpriteNum.SPR_BPAK:
                if (!player.Backpack)
                {
                    for (var i = 0; i < (int)AmmoType.NumAmmo; i++)
                    {
                        player.MaxAmmo[i] *= 2;
                    }

                    player.Backpack = true;
                }

                for (var i = 0; i < (int)AmmoType.NumAmmo; i++)
                {
                    player.GiveAmmo((AmmoType)i, 1);
                }

                player.Message = Messages.GotBackpack;
                break;

            // weapons
            case SpriteNum.SPR_BFUG:
                if (!player.GiveWeapon(WeaponType.Bfg, false))
                {
                    return;
                }

                player.Message = Messages.GotBfg9000;
                sound = SoundType.sfx_wpnup;
                break;

            case SpriteNum.SPR_MGUN:
                if (!player.GiveWeapon(WeaponType.Chaingun, (special.Flags & MapObjectFlag.MF_DROPPED) != 0))
                {
                    return;
                }

                player.Message = Messages.GotChaingun;
                sound = SoundType.sfx_wpnup;
                break;

            case SpriteNum.SPR_CSAW:
                if (!player.GiveWeapon(WeaponType.Chainsaw, false))
                {
                    return;
                }

                player.Message = Messages.GotChainsaw;
                sound = SoundType.sfx_wpnup;
                break;

            case SpriteNum.SPR_LAUN:
                if (!player.GiveWeapon(WeaponType.Missile, false))
                {
                    return;
                }

                player.Message = Messages.GotRocketLauncher;
                sound = SoundType.sfx_wpnup;
                break;

            case SpriteNum.SPR_PLAS:
                if (!player.GiveWeapon(WeaponType.Plasma, false))
                {
                    return;
                }

                player.Message = Messages.GotPlasmaRifle;
                sound = SoundType.sfx_wpnup;
                break;

            case SpriteNum.SPR_SHOT:
                if (!player.GiveWeapon(WeaponType.Shotgun, (special.Flags & MapObjectFlag.MF_DROPPED) != 0))
                {
                    return;
                }

                player.Message = Messages.GotShotgun;
                sound = SoundType.sfx_wpnup;
                break;

            case SpriteNum.SPR_SGN2:
                if (!player.GiveWeapon(WeaponType.SuperShotgun, (special.Flags & MapObjectFlag.MF_DROPPED) != 0))
                {
                    return;
                }

                player.Message = Messages.GotShotgun2;
                sound = SoundType.sfx_wpnup;
                break;

            default:
                DoomGame.Error("P_SpecialThing: Unknown gettable thing");
                return;
        }

        if ((special.Flags & MapObjectFlag.MF_COUNTITEM) != 0)
        {
            player.ItemCount++;
        }

        P_RemoveMapObject(special);
        player.BonusCount += BonusAdd;
        if (player == Players[ConsolePlayer])
        {
            DoomGame.Instance.Sound.StartSound(null, sound);
        }
    }

    public Fixed P_FindLowestFloorSurrounding(Sector sec)
    {
        var floor = sec.FloorHeight;

        for (var i = 0; i < sec.LineCount; i++)
        {
            var check = sec.Lines[i];
            var other = check.GetNextSector(sec);

            if (other == null)
            {
                continue;
            }

            if (other.FloorHeight < floor)
            {
                floor = other.FloorHeight;
            }
        }

        return floor;
    }

    public Fixed P_FindHighestFloorSurrounding(Sector sec)
    {
        var floor = Fixed.FromInt(-500);

        for (var i = 0; i < sec.LineCount; i++)
        {
            var check = sec.Lines[i];
            var other = check.GetNextSector(sec);

            if (other == null)
            {
                continue;
            }

            if (other.FloorHeight > floor)
            {
                floor = other.FloorHeight;
            }
        }

        return floor;
    }

    private const int MaxAdjoiningSectors = 20;

    public Fixed P_FindNextHighestFloor(Sector sec, Fixed currentHeight)
    {
        var heightList = new List<Fixed>();
        var h = 0;

        for (var i = 0; i < sec.LineCount; i++)
        {
            var check = sec.Lines[i];
            var other = check.GetNextSector(sec);

            if (other == null)
            {
                continue;
            }

            if (other.FloorHeight > currentHeight)
            {
                heightList.Add(other.FloorHeight);
                h++;
            }

            // Check for overflow. Exit.
            if (h >= MaxAdjoiningSectors)
            {
                DoomGame.Console.WriteLine("Sector with more than 20 adjoining sectors");
                break;
            }
        }

        // Find lowest height in list
        if (h == 0)
        {
            return currentHeight;
        }

        return heightList.Min();
    }

    public Fixed P_FindLowestCeilingSurrounding(Sector sec)
    {
        var height = Fixed.MaxValue;

        for (var i = 0; i < sec.LineCount; i++)
        {
            var check = sec.Lines[i];
            var other = check.GetNextSector(sec);

            if (other == null)
            {
                continue;
            }

            if (other.CeilingHeight < height)
            {
                height = other.CeilingHeight;
            }
        }

        return height;
    }

    public Fixed P_FindHighestCeilingSurrounding(Sector sec)
    {
        var height = Fixed.Zero;

        for (var i = 0; i < sec.LineCount; i++)
        {
            var check = sec.Lines[i];
            var other = check.GetNextSector(sec);

            if (other == null)
            {
                continue;
            }

            if (other.CeilingHeight > height)
            {
                height = other.CeilingHeight;
            }
        }

        return height;
    }

    public int P_FindSectorFromLineTag(Line line, int start)
    {
        for (var i = start + 1; i < NumSectors; i++)
        {
            if (Sectors[i].Tag == line.Tag)
            {
                return i;
            }
        }

        return -1;
    }

    public int P_FindMinSurroundingLight(Sector sector, int max)
    {
        var min = max;

        for (var i = 0; i < sector.LineCount; i++)
        {
            var line = sector.Lines[i];
            var check = line.GetNextSector(sector);

            if (check == null)
            {
                continue;
            }

            if (check.LightLevel < min)
            {
                min = check.LightLevel;
            }
        }

        return min;
    }

    /// <summary>
    /// Called every time a thing origin is about
    ///  to cross a line with a non 0 special.
    /// </summary>
    private void P_CrossSpecialLine(Line line, int side, MapObject thing)
    {
        var ok = false;

        // Triggers that other things can activate
        if (thing.Player == null)
        {
            // Things that should NOT trigger specials...
            switch (thing.Type)
            {
                case MapObjectType.MT_ROCKET:
                case MapObjectType.MT_PLASMA:
                case MapObjectType.MT_BFG:
                case MapObjectType.MT_TROOPSHOT:
                case MapObjectType.MT_HEADSHOT:
                case MapObjectType.MT_BRUISERSHOT:
                    return;
            }

            switch (line.Special)
            {
                case 39:    // TELEPORT TRIGGER
                case 97:    // TELEPORT RETRIGGER
                case 125:   // TELEPORT MONSTERONLY TRIGGER
                case 126:   // TELEPORT MONSTERONLY RETRIGGER
                case 4: // RAISE DOOR
                case 10:    // PLAT DOWN-WAIT-UP-STAY TRIGGER
                case 88:    // PLAT DOWN-WAIT-UP-STAY RETRIGGER
                    ok = true;
                    break;
            }

            if (!ok)
            {
                return;
            }
        }

        Trigger.Handle(line, side, thing);
    }

    /// <summary>
    /// Called when a thing shoots a special line.
    /// </summary>
    private void P_ShootSpecialLine(MapObject thing, Line line)
    {
        var ok = false;

        // Impacts that other things can activate.
        if (thing.Player == null)
        {
            if (line.Special == 46)
            {
                // open door impact
                ok = true;
            }

            if (!ok)
            {
                return;
            }
        }

        switch (line.Special)
        {
            case 24:
                // RAISE FLOOR
                Trigger.FloorEvent(line, FloorType.RaiseFloor);
                P_ChangeSwitchTexture(line, false);
                break;

            case 46:
                // OPEN DOOR
                Trigger.DoorEvent(line, DoorType.Open);
                P_ChangeSwitchTexture(line, true);
                break;

            case 47:
                // RAISE FLOOR NEAR AND CHANGE
                Trigger.PlatformEvent(line, PlatformType.RaiseToNearestAndChange, 0);
                P_ChangeSwitchTexture(line, false);
                break;
        }
    }

    /// <summary>
    /// Get info needed to make ticcmd_ts for the players
    /// </summary>
    public bool HandleEvent(InputEvent currentEvent)
    {
        // allow spy mode changes even during the demo
        if (GameState == GameState.Level && 
            currentEvent.Type == EventType.KeyDown &&
            currentEvent.Data1 == (int)Keys.F12 && 
            (SingleDemo || !DeathMatch))
        {
            // spy mode 
            do
            {
                DisplayPlayer++;
                if (DisplayPlayer == Constants.MaxPlayers)
                {
                    DisplayPlayer = 0;
                }
            } while (!PlayerInGame[DisplayPlayer] && DisplayPlayer != ConsolePlayer);
            
            return true;
        }

        // any other key pops up menu if in demos
        if (GameAction == GameAction.Nothing && 
            !SingleDemo &&
            (DemoPlayback || GameState == GameState.DemoScreen)
        )
        {
            if (currentEvent.Type == EventType.KeyDown ||
                (currentEvent.Type == EventType.Mouse && currentEvent.Data1 != 0) ||
                (currentEvent.Type == EventType.Joystick && currentEvent.Data1 != 0))
            {
                DoomGame.Instance.Menu.StartControlPanel();
                return true;
            }
            return false;
        }

        if (GameState == GameState.Level)
        {
            if (DoomGame.Instance.Hud.HandleEvent(currentEvent))
            {
                return true;    // chat ate it 
            }
            if (DoomGame.Instance.StatusBar.HandleEvent(currentEvent))
            {
                return true;    // status window ate it 
            }
            //if (AM_Responder(currentEvent))
            //    return true;    // automap ate it 
        }

        if (GameState == GameState.Finale)
        {
            //if (F_Responder(currentEvent))
            //    return true;    // finale ate the event 
        }

        switch (currentEvent.Type)
        {
            case EventType.KeyDown:
                if (currentEvent.Data1 == (int)Keys.Pause)
                {
                    SendPause = true;
                    return true;
                }

                if (currentEvent.Data1 < NumKeys)
                {
                    _gameKeyDown[currentEvent.Data1] = true;
                }
                
                return true;    // eat key down events 

            case EventType.KeyUp:
                if (currentEvent.Data1 < NumKeys)
                {
                    _gameKeyDown[currentEvent.Data1] = false;
                }

                return false;   // always let key up events filter down 

            case EventType.Mouse:
                _mouseButtons[0] = (currentEvent.Data1 & 1) != 0;
                _mouseButtons[1] = (currentEvent.Data1 & 2) != 0;
                _mouseButtons[2] = (currentEvent.Data1 & 4) != 0;
                _mouseX = currentEvent.Data2 * (DoomGame.Instance.Menu.MouseSensitivity + 5) / 10;
                _mouseY = currentEvent.Data3 * (DoomGame.Instance.Menu.MouseSensitivity + 5) / 10;
                return true;    // eat events 

            case EventType.Joystick:
                _joyButtons[0] = (currentEvent.Data1 & 1) != 0;
                _joyButtons[1] = (currentEvent.Data1 & 2) != 0;
                _joyButtons[2] = (currentEvent.Data1 & 4) != 0;
                _joyButtons[3] = (currentEvent.Data1 & 8) != 0;
                _joyXMove = currentEvent.Data2;
                _joyYMove = currentEvent.Data3;
                return true;    // eat events 
        }

        return false;
    }

    /// <summary>
    /// Builds a ticcmd from all of the available inputs
    /// or reads it from the demo buffer. 
    /// If recording a demo, write it out 
    /// </summary>
    public TicCommand BuildTicCommand()
    {
        var cmd = new TicCommand(); // empty, or external driver

        cmd.Consistency = _consistency[ConsolePlayer][DoomGame.Instance.MakeTic % Constants.BackupTics];

        var strafe = _gameKeyDown[_keyStrafe] || _mouseButtons[_mouseButtonStrafe] || _joyButtons[_joyButtonStrafe];
        var speed = (_gameKeyDown[_keySpeed] || _joyButtons[_joyButtonSpeed]) ? 1 : 0;

        int turnSpeed;
        var forward = 0;
        var side = 0;

        // use two stage accelerative turning
        // on the keyboard and joystick
        if (_joyXMove is < 0 or > 0 
            || _gameKeyDown[_keyRight] 
            || _gameKeyDown[_keyLeft])
        {
            _turnHeld += DoomGame.Instance.TicDup;
        }
        else
        {
            _turnHeld = 0;
        }

        if (_turnHeld < SlowTurnTics)
        {
            turnSpeed = 2;             // slow turn 
        }
        else
        {
            turnSpeed = speed;
        }

        // let movement keys cancel each other out
        if (strafe)
        {
            if (_gameKeyDown[_keyRight])
            {
                // fprintf(stderr, "strafe right\n");
                side += SideMove[speed];
            }

            if (_gameKeyDown[_keyLeft])
            {
                //	fprintf(stderr, "strafe left\n");
                side -= SideMove[speed];
            }

            if (_joyXMove > 0)
            {
                side += SideMove[speed];
            }

            if (_joyXMove < 0)
            {
                side -= SideMove[speed];
            }

        }
        else
        {
            if (_gameKeyDown[_keyRight])
            {
                cmd.AngleTurn -= (short)AngleTurn[turnSpeed];
            }

            if (_gameKeyDown[_keyLeft])
            {
                cmd.AngleTurn += (short)AngleTurn[turnSpeed];
            }

            if (_joyXMove > 0)
            {
                cmd.AngleTurn -= (short)AngleTurn[turnSpeed];
            }

            if (_joyXMove < 0)
            {
                cmd.AngleTurn += (short)AngleTurn[turnSpeed];
            }
        }

        if (_gameKeyDown[_keyUp])
        {
            // fprintf(stderr, "up\n");
            forward += ForwardMove[speed];
        }
        
        if (_gameKeyDown[_keyDown])
        {
            // fprintf(stderr, "down\n");
            forward -= ForwardMove[speed];
        }

        if (_joyYMove < 0)
        {
            forward += ForwardMove[speed];
        }

        if (_joyYMove > 0)
        {
            forward -= ForwardMove[speed];
        }

        if (_gameKeyDown[_keyStrafeRight])
        {
            side += SideMove[speed];
        }

        if (_gameKeyDown[_keyStrafeLeft])
        {
            side -= SideMove[speed];
        }

        // buttons
        cmd.ChatChar = (char)0; // TODO HU_dequeueChatChar();

        if (_gameKeyDown[_keyFire] || _mouseButtons[_mouseButtonFire]
                                   || _joyButtons[_joyButtonFire])
        {
            cmd.Buttons |= ButtonCode.Attack;
        }

        if (_gameKeyDown[_keyUse] || _joyButtons[_joyButtonUse])
        {
            cmd.Buttons |= ButtonCode.Use;
            // clear double clicks if hit use button 
            _doubleClicks = 0;
        }

        // chainsaw overrides 
        for (var i = 0; i < (int)WeaponType.NumberOfWeapons - 1; i++)
        {
            if (!_gameKeyDown['1' + i])
            {
                continue;
            }

            cmd.Buttons |= ButtonCode.Change;
            cmd.Buttons |= (ButtonCode)(i << (int)ButtonCode.WeaponShift);
            break;
        }

        // mouse
        if (_mouseButtons[_mouseButtonForward])
        {
            forward += ForwardMove[speed];
        }

        // forward double click
        if (_mouseButtons[_mouseButtonForward] != _doubleClickState && _doubleClickTime > 1)
        {
            _doubleClickState = _mouseButtons[_mouseButtonForward];
            if (_doubleClickState)
            {
                _doubleClicks++;
            }

            if (_doubleClicks == 2)
            {
                cmd.Buttons |= ButtonCode.Use;
                _doubleClicks = 0;
            }
            else
            {
                _doubleClickTime = 0;
            }
        }
        else
        {
            _doubleClickTime += DoomGame.Instance.TicDup;
            if (_doubleClickTime > 20)
            {
                _doubleClicks = 0;
                _doubleClickState = false;
            }
        }

        // strafe double click
        var bstrafe = _mouseButtons[_mouseButtonStrafe] || _joyButtons[_joyButtonStrafe];
        if (bstrafe != _doubleClickState2 && _doubleClickTime2 > 1)
        {
            _doubleClickState2 = bstrafe;
            if (_doubleClickState2)
            {
                _doubleClicks2++;
            }

            if (_doubleClicks2 == 2)
            {
                cmd.Buttons |= ButtonCode.Use;
                _doubleClicks2 = 0;
            }
            else
            {
                _doubleClickTime2 = 0;
            }
        }
        else
        {
            _doubleClickTime2 += DoomGame.Instance.TicDup;
            if (_doubleClickTime2 > 20)
            {
                _doubleClicks2 = 0;
                _doubleClickState2 = false;
            }
        }

        forward += _mouseY;
        if (strafe)
        {
            side += _mouseX * 2;
        }
        else
        {
            cmd.AngleTurn -= (short)(_mouseX * 0x8);
        }

        _mouseX = _mouseY = 0;

        if (forward > ForwardMove[1]) // MAXPLMOVE = forwardmove[1]
        {
            forward = ForwardMove[1];
        }
        else if (forward < -ForwardMove[1])
        {
            forward = -ForwardMove[1];
        }

        if (side > ForwardMove[1])
        {
            side = ForwardMove[1];
        }
        else if (side < -ForwardMove[1])
        {
            side = -ForwardMove[1];
        }

        cmd.ForwardMove += (sbyte)forward;
        cmd.SideMove += (sbyte)side;

        // special buttons
        if (SendPause)
        {
            SendPause = false;
            cmd.Buttons = ButtonCode.Special | ButtonCode.Pause;
        }

        if (SendSave)
        {
            SendSave = false;
            cmd.Buttons = ButtonCode.Special | ButtonCode.SaveGame | (ButtonCode)(_saveGameSlot << (int)ButtonCode.SaveShift);
        }

        return cmd;
    }

    // DOOM Par Times
    private static readonly int[][] ParTimes =
    {
        new[] { 0 },
        new[] { 0, 30, 75, 120, 90, 165, 180, 180, 30, 165 },
        new[] { 0, 90, 90, 90, 120, 90, 360, 240, 30, 170 },
        new[] { 0, 90, 45, 90, 150, 90, 90, 165, 30, 135 }
    };

    // DOOM II Par Times
    private static readonly int[] CommercialParTimes =
    {
        30, 90, 120, 120, 90, 150, 120, 120, 270, 90, //  1-10
        210, 150, 150, 150, 210, 150, 420, 150, 210, 150, // 11-20
        240, 150, 180, 150, 150, 300, 330, 420, 300, 180, // 21-30
        120, 30 // 31-32
    };

    public void ExitLevel()
    {
        _secretExit = false;
        GameAction = GameAction.Completed;
    }

    public void SecretExitLevel()
    {
        // IF NO WOLF3D LEVELS, NO SECRET EXIT!
        if (DoomGame.Instance.GameMode == GameMode.Commercial &&
            DoomGame.Instance.WadData.CheckNumForName("map31") < 0)
        {
            _secretExit = false;
        }
        else
        {
            _secretExit = true;
        }

        GameAction = GameAction.Completed;
    }

    public void DoCompleted()
    {
        GameAction = GameAction.Nothing;

        for (var i = 0; i < Constants.MaxPlayers; i++)
        {
            if (PlayerInGame[i])
            {
                PlayerFinishLevel(i);
            }
        }

        if (AutomapActive)
        {
            // AM_Stop();
        }

        if (DoomGame.Instance.GameMode != GameMode.Commercial)
        {
            switch (GameMap)
            {
                case 8:
                    GameAction = GameAction.Victory;
                    return;
                case 9:
                    for (var i = 0; i < Constants.MaxPlayers; i++)
                    {
                        Players[i].DidSecret = true;
                    }

                    break;
            }
        }

        _wmInfo.DidSecret = Players[ConsolePlayer].DidSecret;
        _wmInfo.Episode = GameEpisode - 1;
        _wmInfo.Last = GameMap - 1;

        // wminfo.next is 0 based, unlike gamemap
        if (DoomGame.Instance.GameMode == GameMode.Commercial)
        {
            if (_secretExit)
            {
                switch (GameMap)
                {
                    case 15:
                        _wmInfo.Next = 30;
                        break;
                    case 31:
                        _wmInfo.Next = 31;
                        break;
                }
            }
            else
            {
                switch (GameMap)
                {
                    case 31:
                    case 32:
                        _wmInfo.Next = 15;
                        break;
                    default:
                        _wmInfo.Next = GameMap;
                        break;
                }
            }
        }
        else
        {
            if (_secretExit)
            {
                _wmInfo.Next = 9; // go to secret level
            }
            else if (GameMap == 9)
            {
                // returning from secret level
                switch (GameEpisode)
                {
                    case 1:
                        _wmInfo.Next = 3;
                        break;
                    case 2:
                        _wmInfo.Next = 5;
                        break;
                    case 3:
                        _wmInfo.Next = 6;
                        break;
                    case 4:
                        _wmInfo.Next = 2;
                        break;
                }
            }
            else
            {
                _wmInfo.Next = GameMap; // go to next level
            }
        }

        _wmInfo.MaxKills = TotalKills;
        _wmInfo.MaxItems = TotalItems;
        _wmInfo.MaxSecret = TotalSecrets;
        _wmInfo.MaxFrags = 0;

        if (DoomGame.Instance.GameMode == GameMode.Commercial)
        {
            _wmInfo.ParTime = 35 * CommercialParTimes[GameMap - 1];
        }
        else
        {
            _wmInfo.ParTime = 35 * ParTimes[GameEpisode][GameMap];
        }

        _wmInfo.PlayerNum = ConsolePlayer;

        for (var i = 0; i < Constants.MaxPlayers; i++)
        {
            _wmInfo.Players[i].In = PlayerInGame[i];
            _wmInfo.Players[i].Kills = Players[i].KillCount;
            _wmInfo.Players[i].Items = Players[i].ItemCount;
            _wmInfo.Players[i].Secret = Players[i].SecretCount;
            _wmInfo.Players[i].Time = LevelTime;

            Array.Copy(Players[i].Frags, 0, _wmInfo.Players[i].Frags, 0, _wmInfo.Players[i].Frags.Length);
        }

        GameState = GameState.Intermission;
        ViewActive = false;
        AutomapActive = false;

        //if (statcopy != null)
        //{
        //    // copy wminfo to statcopy 
        //}

        DoomGame.Instance.Intermission.Start(_wmInfo);
    }

    public void WorldDone()
    {
        GameAction = GameAction.WorldDone;

        if (_secretExit)
        {
            Players[ConsolePlayer].DidSecret = true;
        }

        if (DoomGame.Instance.GameMode == GameMode.Commercial)
        {
            switch (GameMap)
            {
                case 15 or 31:
                    if (!_secretExit)
                    {
                        break;
                    }
                    // F_StartFinale();
                    break;
                case 6:
                case 11:
                case 20:
                case 30:
                    // F_StartFinale();
                    break;
            }
        }
    }

    public void DoWorldDone()
    {
        GameState = GameState.Level;
        GameMap = _wmInfo.Next + 1;
        DoLoadLevel();
        GameAction = GameAction.Nothing;
        ViewActive = true;
    }

    public int GetPlayerIndex(Player player)
    {
        for (var i = 0; i < Players.Length; i++)
        {
            if (Players[i] == player)
            {
                return i;
            }
        }

        return -1;
    }

    //
    // P_BulletSlope
    // Sets a slope so a near miss is at approximately
    // the height of the intended target
    //
    private Fixed _bulletSlope;
    
    public void P_BulletSlope(MapObject mo)
    {
        // see which target is to be aimed at
        var an = mo.Angle;
        _bulletSlope = P_AimLineAttack(mo, an, Fixed.FromInt(16 * 64));

        if (_lineTarget == null)
        {
            an += new Angle(1 << 26);
            _bulletSlope = P_AimLineAttack(mo, an, Fixed.FromInt(16 * 64));
            if (_lineTarget == null)
            {
                an -= new Angle(2 << 26);
                _bulletSlope = P_AimLineAttack(mo, an, Fixed.FromInt(16 * 64));
            }
        }
    }

    //
    // P_GunShot
    //
    public void P_GunShot(MapObject mo, bool accurate)
    {
        var damage = 5 * (DoomRandom.P_Random() % 3 + 1);
        var angle = mo.Angle;

        if (!accurate)
        {
            angle += new Angle((DoomRandom.P_Random() - DoomRandom.P_Random()) << 18);
        }

        P_LineAttack(mo, angle, Constants.MissileRange, _bulletSlope, damage);
    }


    /// <summary>
    /// Sets line target and aim slope when a target is aimed at.
    /// </summary>
    private bool PTR_AimTraverse(Intercept intercept)
    {
        Fixed dist;

        if (intercept.IsLine)
        {
            var li = intercept.Line!;
            if ((li.Flags & Constants.Line.TwoSided) == 0)
            {
                return false; // stop
            }

            // Crosses a two sided line.
            // A two sided line will restrict
            // the possible target ranges.
            P_LineOpening(li);

            if (_openBottom >= _openTop)
            {
                return false; // stop
            }

            dist = _attackRange * intercept.Frac;

            if (li.FrontSector!.FloorHeight != li.BackSector!.FloorHeight)
            {
                var slope = (_openBottom - _shootZ) / dist;
                if (slope > _bottomSlope)
                {
                    _bottomSlope = slope;
                }
            }

            if (li.FrontSector!.CeilingHeight != li.BackSector!.CeilingHeight)
            {
                var slope = (_openTop - _shootZ) / dist;
                if (slope < _topSlope)
                {
                    _topSlope = slope;
                }
            }

            if (_topSlope <= _bottomSlope)
            {
                return false; // stop
            }

            return true; // shot continues
        }

        // shoot a thing
        var th = intercept.Thing!;
        if (th == _shootThing)
        {
            return true; // can't shoot self
        }

        if ((th.Flags & MapObjectFlag.MF_SHOOTABLE) == 0)
        {
            return true; // corpse or something
        }

        // check angles to see if the thing can be aimed at
        dist = _attackRange * intercept.Frac;
        var thingTopSlope = (th.Z + th.Height - _shootZ) / dist;

        if (thingTopSlope < _bottomSlope)
        {
            return true;			// shot over the thing
        }

        var thingBottomSlope = (th.Z - _shootZ) / dist;

        if (thingBottomSlope > _topSlope)
        {
            return true;			// shot under the thing
        }

        // this thing can be hit
        if (thingTopSlope > _topSlope)
        {
            thingTopSlope = _topSlope;
        }

        if (thingBottomSlope < _bottomSlope)
        {
            thingBottomSlope = _bottomSlope;
        }

        _aimSlope = (thingTopSlope + thingBottomSlope) / 2;
        _lineTarget = th;

        return false;
    }

    private bool PTR_ShootTraverse(Intercept intercept)
    {
        Fixed dist;

        if (intercept.IsLine)
        {
            var li = intercept.Line!;
            if (li.Special != 0)
            {
                P_ShootSpecialLine(_shootThing!, li);
            }

            if ((li.Flags & Constants.Line.TwoSided) == 0)
            {
                return HitLine(li);
            }

            // crosses a two sided line
            P_LineOpening(li);

            dist = _attackRange * intercept.Frac;

            if (li.FrontSector!.FloorHeight != li.BackSector!.FloorHeight)
            {
                var slope = (_openBottom - _shootZ) / dist;
                if (slope > _aimSlope)
                {
                    return HitLine(li);
                }
            }

            if (li.FrontSector!.CeilingHeight != li.BackSector!.CeilingHeight)
            {
                var slope = (_openTop - _shootZ) / dist;
                if (slope < _aimSlope)
                {
                    return HitLine(li);
                }
            }

            // shot continues
            return true;
        }

        // shoot a thing
        var th = intercept.Thing!;
        if (th == _shootThing)
        {
            return true; // can't shoot self
        }

        if ((th.Flags & MapObjectFlag.MF_SHOOTABLE) == 0)
        {
            return true; // corpse or something
        }

        // check angles to see if the thing can be aimed at
        dist = _attackRange * intercept.Frac;
        var thingTopSlope = (th.Z + th.Height - _shootZ) / dist;

        if (thingTopSlope < _aimSlope)
        {
            return true; // shot over the thing
        }

        var thingBottomSlope = (th.Z - _shootZ) / dist;

        if (thingBottomSlope > _aimSlope)
        {
            return true; // shot under the thing
        }

        // hit thing
        // position a bit closer
        var frac = intercept.Frac - Fixed.FromInt(10) / _attackRange;

        var x = _trace.X + (_trace.Dx * frac);
        var y = _trace.Y + (_trace.Dy * frac);
        var z = _shootZ + (_aimSlope * (frac * _attackRange));

        // Spawn bullet puffs or blod spots,
        // depending on target type.
        if ((th.Flags & MapObjectFlag.MF_NOBLOOD) != 0)
        {
            P_SpawnPuff(x, y, z);
        }
        else
        {
            P_SpawnBlood(x, y, z, _laDamage);
        }

        if (_laDamage != 0)
        {
            MapObject.DamageMapObject(th, _shootThing, _shootThing, _laDamage);
        }

        // don't go any farther
        return false;

        bool HitLine(Line li)
        {
            // position a bit closer
            var frac = intercept.Frac - Fixed.FromInt(4) / _attackRange;
            var x = _trace.X + (_trace.Dx * frac);
            var y = _trace.Y + (_trace.Dy * frac);
            var z = _shootZ + (_aimSlope * (frac * _attackRange));

            if (li.FrontSector!.CeilingPic == DoomGame.Instance.Renderer.Sky.FlatNum)
            {
                // don't shoot the sky!
                if (z > li.FrontSector.CeilingHeight)
                {
                    return false;
                }

                // it's a sky hack wall
                if (li.BackSector != null && li.BackSector.CeilingPic == DoomGame.Instance.Renderer.Sky.FlatNum)
                {
                    return false;
                }
            }

            // Spawn bullet puffs.
            P_SpawnPuff(x, y, z);

            // don't go any farther.
            return false;
        }
    }

    public Fixed P_AimLineAttack(MapObject thing, Angle angle, Fixed distance)
    {
        _shootThing = thing;

        var x2 = thing.X + (distance.Value >> Constants.FracBits) * DoomMath.Cos(angle);
        var y2 = thing.Y + (distance.Value >> Constants.FracBits) * DoomMath.Sin(angle);
        _shootZ = new Fixed(thing.Z.Value + (thing.Height.Value >> 1) + 8 * Constants.FracUnit);

        // Can't shoot outside view angles
        _topSlope = Fixed.FromInt(100) / 160;
        _bottomSlope = Fixed.FromInt(-100) / 160;

        _attackRange = distance;
        _lineTarget = null;

        P_PathTraverse(thing.X, thing.Y, x2, y2, PT_AddLines | PT_AddThings, PTR_AimTraverse);

        if (LineTarget != null)
        {
            return _aimSlope;
        }

        return Fixed.Zero;
    }

    /// <summary>
    /// If damage == 0, it is just a test trace that will leave linetarget set
    /// </summary>
    public void P_LineAttack(MapObject thing, Angle angle, Fixed distance, Fixed slope, int damage)
    {
        _shootThing = thing;
        _laDamage = damage;

        var x2 = thing.X + (distance.Value >> Constants.FracBits) * DoomMath.Cos(angle);
        var y2 = thing.Y + (distance.Value >> Constants.FracBits) * DoomMath.Sin(angle);
        _shootZ = new Fixed(thing.Z.Value + (thing.Height.Value >> 1) + 8 * Constants.FracUnit);
        _attackRange = distance;
        _aimSlope = slope;

        P_PathTraverse(thing.X, thing.Y, x2, y2, PT_AddLines | PT_AddThings, PTR_ShootTraverse);
    }

    public void P_SpawnPuff(Fixed x, Fixed y, Fixed z)
    {
        z += new Fixed((DoomRandom.P_Random() - DoomRandom.P_Random()) << 10);

        var th = P_SpawnMapObject(x, y, z, MapObjectType.MT_PUFF);
        th.MomZ = Fixed.Unit;
        th.Tics -= DoomRandom.P_Random() & 3;

        if (th.Tics < 1)
        {
            th.Tics = 1;
        }

        // don't make punches spark on the wall
        if (_attackRange == Constants.MeleeRange)
        {
            th.SetState(StateNum.S_PUFF3);
        }
    }

    private void P_SpawnBlood(Fixed x, Fixed y, Fixed z, int damage)
    {
        z += new Fixed((DoomRandom.P_Random() - DoomRandom.P_Random()) << 10);

        var th = P_SpawnMapObject(x, y, z, MapObjectType.MT_BLOOD);
        th.MomZ = Fixed.FromInt(2);
        th.Tics -= DoomRandom.P_Random() & 3;

        if (th.Tics < 1)
        {
            th.Tics = 1;
        }

        if (damage is <= 12 and >= 9)
        {
            th.SetState(StateNum.S_BLOOD2);
        }
        else if (damage < 9)
        {
            th.SetState(StateNum.S_BLOOD3);
        }
    }

    /// <summary>
    /// Moves the missile forward a bit
    ///  and possibly explodes it right there.
    /// </summary>
    private void P_CheckMissileSpawn(MapObject th)
    {
        th.Tics -= DoomRandom.P_Random() & 3;
        if (th.Tics < 1)
        {
            th.Tics = 1;
        }

        // move a little forward so an angle can
        // be computed if it immediately explodes
        th.X += (th.MomX >> 1);
        th.Y += (th.MomY >> 1);
        th.Z += (th.MomZ >> 1);

        if (!P_TryMove(th, th.X, th.Y))
        {
            P_ExplodeMissile(th);
        }
    }

    public MapObject P_SpawnMissile(MapObject source, MapObject dest, MapObjectType type)
    {
        var th = P_SpawnMapObject(source.X, source.Y, source.Z + Fixed.FromInt(4 * 8), type);

        if (th.Info.SeeSound != SoundType.sfx_None)
        {
            DoomGame.Instance.Sound.StartSound(th, th.Info.SeeSound);
        }

        th.Target = source; // where it came from
        var an = DoomGame.Instance.Renderer.PointToAngle2(source.X, source.Y, dest.X, dest.Y);

        // fuzzy player
        if ((dest.Flags & MapObjectFlag.MF_SHADOW) != 0)
        {
            an += new Angle((DoomRandom.P_Random() - DoomRandom.P_Random()) << 20);
        }

        th.Angle = an;
        th.MomX = new Fixed(th.Info.Speed) * DoomMath.Cos(an);
        th.MomY = new Fixed(th.Info.Speed) * DoomMath.Sin(an);

        var dist = P_AproxDistance(dest.X - source.X, dest.Y - source.Y);
        dist /= th.Info.Speed;

        if (dist < new Fixed(1))
        {
            dist = new Fixed(1);
        }

        th.MomZ = (dest.Z - source.Z) / dist.Value;
        P_CheckMissileSpawn(th);

        return th;
    }

    /// <summary>
    /// Tries to aim at a nearby monster
    /// </summary>
    public void P_SpawnPlayerMissile(MapObject source, MapObjectType type)
    {
        // see which target is to be aimed at
        var an = source.Angle;
        var slope = P_AimLineAttack(source, an, Fixed.FromInt(16 * 64));

        if (_lineTarget == null)
        {
            an += new Angle(1 << 26);
            slope = P_AimLineAttack(source, an, Fixed.FromInt(16 * 64));

            if (_lineTarget == null)
            {
                an -= new Angle(2 << 26);
                slope = P_AimLineAttack(source, an, Fixed.FromInt(16 * 64));
            }

            if (_lineTarget == null)
            {
                an = source.Angle;
                slope = Fixed.Zero;
            }
        }

        var x = source.X;
        var y = source.Y;
        var z = source.Z + Fixed.FromInt(4 * 8);

        var th = P_SpawnMapObject(x, y, z, type);
        if (th.Info.SeeSound != SoundType.sfx_None)
        {
            DoomGame.Instance.Sound.StartSound(th, th.Info.SeeSound);
        }

        th.Target = source;
        th.Angle = an;
        th.MomX = th.Info.Speed * DoomMath.Cos(an);
        th.MomY = th.Info.Speed * DoomMath.Sin(an);
        th.MomZ = th.Info.Speed * slope;

        P_CheckMissileSpawn(th);
    }

    //
    // P_CheckSight
    //
    private Fixed _sightZStart;        // eye z of looker
    private Fixed _topSlope;
    private Fixed _bottomSlope;        // slopes to top and bottom of target

    private readonly DividerLine _sTrace = new();           // from t1 to t2
    private Fixed _t2x;
    private Fixed _t2y;

    private readonly int[] _sightCounts = { 0, 0 };

    /// <summary>
    /// Returns side 0 (front), 1 (back), or 2 (on).
    /// </summary>
    private int P_DivlineSide(Fixed x, Fixed y, DividerLine node)
    {
        if (node.Dx == Fixed.Zero)
        {
            if (x == node.X)
            {
                return 2;
            }

            if (x <= node.X)
            {
                return node.Dy > Fixed.Zero ? 1 : 0;
            }

            return node.Dy < Fixed.Zero ? 1 : 0;
        }

        if (node.Dy == Fixed.Zero)
        {
            // This is a known bug, let's keep it for accuracy
            if (x == node.Y)
            {
                return 2;
            }

            if (y <= node.Y)
            {
                return node.Dx < Fixed.Zero ? 1 : 0;
            }

            return node.Dx > Fixed.Zero ? 1 : 0;
        }

        var dx = (x - node.X);
        var dy = (y - node.Y);

        var left = new Fixed(node.Dy.Value >> Constants.FracBits) * (dx.Value >> Constants.FracBits);
        var right = new Fixed(dy.Value >> Constants.FracBits) * (node.Dx.Value >> Constants.FracBits);

        if (right < left)
        {
            return 0; // front side
        }

        if (left == right)
        {
            return 2;
        }

        return 1; // back side
    }

    /// <summary>
    /// Returns true
    ///  if strace crosses the given subsector successfully.
    /// </summary>
    private bool P_CrossSubSector(int num)
    {
        var sub = SubSectors[(ushort)num];

        // check lines
        var count = sub.NumLines;
        var i = sub.FirstLine;
        for (; count != 0; count--, i++)
        {
            var seg = Segments[i];
            var line = seg.LineDef;

            // already checked other side?
            if (line.ValidCount == DoomGame.Instance.Renderer.ValidCount)
            {
                continue;
            }

            line.ValidCount = DoomGame.Instance.Renderer.ValidCount;

            var v1 = line.V1;
            var v2 = line.V2;
            var s1 = P_DivlineSide(v1.X, v1.Y, _sTrace);
            var s2 = P_DivlineSide(v2.X, v2.Y, _sTrace);

            // line isn't crossed?
            if (s1 == s2)
            {
                continue;
            }

            var divl = new DividerLine
            {
                X = v1.X,
                Y = v1.Y,
                Dx = v2.X - v1.X,
                Dy = v2.Y - v1.Y
            };

            s1 = P_DivlineSide(_sTrace.X, _sTrace.Y, divl);
            s2 = P_DivlineSide(_t2x, _t2y, divl);

            // line isn't crossed?
            if (s1 == s2)
            {
                continue;
            }

            // stop because it is not two sided anyway
            // might do this after updating validcount?
            if ((line.Flags & Constants.Line.TwoSided) == 0)
            {
                return false;
            }

            // crosses a two sided line
            var front = seg.FrontSector;
            var back = seg.BackSector!;

            // no wall to block sight with?
            if (front.FloorHeight == back.FloorHeight &&
                front.CeilingHeight == back.CeilingHeight)
            {
                continue;
            }

            // possible occluder
            // because of ceiling height differences
            if (front.CeilingHeight < back.CeilingHeight)
            {
                _openTop = front.CeilingHeight;
            }
            else
            {
                _openTop = back.CeilingHeight;
            }

            // because of ceiling height differences
            if (front.FloorHeight > back.FloorHeight)
            {
                _openBottom = front.FloorHeight;
            }
            else
            {
                _openBottom = back.FloorHeight;
            }

            // quick test for totally closed doors
            if (_openBottom >= _openTop)
            {
                return false;		// stop
            }

            var frac = P_InterceptVector(_sTrace, divl);
            Fixed slope;

            if (front.FloorHeight != back.FloorHeight)
            {
                slope = (_openBottom - _sightZStart) / frac;
                if (slope > _bottomSlope)
                {
                    _bottomSlope = slope;
                }
            }

            if (front.CeilingHeight != back.CeilingHeight)
            {
                slope = (_openTop - _sightZStart) / frac;
                if (slope < _topSlope)
                {
                    _topSlope = slope;
                }
            }

            if (_topSlope <= _bottomSlope)
            {
                return false;		// stop	
            }
        }

        // passed the subsector ok
        return true;
    }

    /// <summary>
    /// Returns true
    ///  if strace crosses the given node successfully.
    /// </summary>
    private bool P_CrossBSPNode(int bspnum)
    {
        if ((bspnum & Constants.NodeLeafSubSector) != 0)
        {
            if (bspnum == -1)
            {
                return P_CrossSubSector(0);
            }
            else
            {
                return P_CrossSubSector(bspnum & (~Constants.NodeLeafSubSector));
            }
        }

        var bsp = Nodes[bspnum];

        // decide which side the start point is on
        var side = P_DivlineSide(_sTrace.X, _sTrace.Y, bsp);
        if (side == 2)
        {
            side = 0; // an "on" should cross both sides
        }

        // cross the starting side
        if (!P_CrossBSPNode(bsp.Children[side]))
        {
            return false;
        }

        // the partition plane is crossed here
        if (side == P_DivlineSide(_t2x, _t2y, bsp))
        {
            // the line doesn't touch the other side
            return true;
        }

        // cross the ending side
        return P_CrossBSPNode(bsp.Children[side ^ 1]);
    }

    /// <summary>
    /// Returns true
    ///  if a straight line between t1 and t2 is unobstructed.
    /// Uses REJECT.
    /// </summary>
    public bool P_CheckSight(MapObject t1, MapObject t2)
    {
        // First check for trivial rejection.

        // Determine subsector entries in REJECT table.
        var s1 = Sectors.IndexOf(t1.SubSector!.Sector!);
        var s2 = Sectors.IndexOf(t2.SubSector!.Sector!);
        var pnum = s1 * NumSectors + s2;
        var bytenum = pnum >> 3;
        var bitnum = 1 << (pnum & 7);

        // Check in REJECT table.
        if ((_rejectMatrix[bytenum] & bitnum) != 0)
        {
            _sightCounts[0]++;

            // Can't possibly be connected
            return false;
        }

        // An unobstructed LOS is possible.
        // Now look from eyes of t1 to any part of t2.
        _sightCounts[1]++;

        DoomGame.Instance.Renderer.ValidCount++;

        _sightZStart = t1.Z + t1.Height - (t1.Height >> 2);
        _topSlope = (t2.Z + t2.Height) - _sightZStart;
        _bottomSlope = (t2.Z) - _sightZStart;

        _sTrace.X = t1.X;
        _sTrace.Y = t1.Y;
        _t2x = t2.X;
        _t2y = t2.Y;
        _sTrace.Dx = t2.X - t1.X;
        _sTrace.Dy = t2.Y - t1.Y;

        // the head node is the last node output
        return P_CrossBSPNode(NumNodes - 1);
    }

    /// <summary>
    /// If allaround is false, only look 180 degrees in front.
    /// Returns true if a player is targeted. 
    /// </summary>
    public bool P_LookForPlayers(MapObject actor, bool allAround)
    {
        var c = 0;
        var stop = (actor.LastLook - 1) & 3;

        for (;; actor.LastLook = (actor.LastLook + 1) & 3)
        {
            if (!PlayerInGame[actor.LastLook])
            {
                continue;
            }

            if (c++ == 2 || actor.LastLook == stop)
            {
                // done looking
                return false;
            }

            var player = Players[actor.LastLook];
            if (player.Health <= 0)
            {
                continue; // dead
            }

            if (!P_CheckSight(actor, player.MapObject!))
            {
                continue; // out of sight
            }

            if (!allAround)
            {
                var angle = DoomGame.Instance.Renderer.PointToAngle2
                    (actor.X, actor.Y, player.MapObject!.X, player.MapObject.Y)
                            - actor.Angle;

                if (angle > Angle.Angle90 && angle < Angle.Angle270)
                {
                    var dist = P_AproxDistance(
                        player.MapObject.X - actor.X,
                        player.MapObject.Y - actor.Y);

                    // if real close, react anyway
                    if (dist > Constants.MeleeRange)
                    {
                        continue; // behind back
                    }
                }
            }

            actor.Target = player.MapObject;
            return true;
        }
    }

    //
    // P_NewChaseDir related LUT.
    //
    private static readonly DirectionType[] Opposite =
    {
        DirectionType.West, DirectionType.SouthWest, DirectionType.South, DirectionType.SouthEast,
        DirectionType.East, DirectionType.NorthEast, DirectionType.North, DirectionType.NorthWest, DirectionType.NoDirection
    };

    private static readonly DirectionType[] Diagonals = 
    {
        DirectionType.NorthWest, DirectionType.NorthEast, 
        DirectionType.SouthWest, DirectionType.SouthEast
    };

    public void P_NewChaseDir(MapObject actor)
    {
        if (actor.Target == null)
        {
            DoomGame.Error("P_NewChaseDir: called with no target");
            return;
        }

        var oldDir = actor.MoveDir;
        var turnAround = Opposite[(int)oldDir];

        var deltaX = actor.Target.X - actor.X;
        var deltaY = actor.Target.Y - actor.Y;

        var directions = new DirectionType[3];
        var fixed10 = Fixed.FromInt(10);
        if (deltaX > fixed10)
        {
            directions[1] = DirectionType.East;
        }
        else if (deltaX < -fixed10)
        {
            directions[1] = DirectionType.West;
        }
        else
        {
            directions[1] = DirectionType.NoDirection;
        }

        if (deltaY < -fixed10) 
        {
            directions[2] = DirectionType.South;
        }
        else if (deltaY > fixed10)
        {
            directions[2] = DirectionType.North;
        }
        else
        {
            directions[2] = DirectionType.NoDirection;
        }

        // try direct route
        if (directions[1] != DirectionType.NoDirection &&
            directions[2] != DirectionType.NoDirection)
        {
            var di = ((deltaY < Fixed.Zero ? 1 : 0) << 1) + ((deltaX > Fixed.Zero) ? 1 : 0);
            actor.MoveDir = Diagonals[di];
            if (actor.MoveDir != turnAround && P_TryWalk(actor))
            {
                return;
            }
        }

        // try other directions
        if (DoomRandom.P_Random() > 200 || Fixed.Abs(deltaY) > Fixed.Abs(deltaX))
        {
            (directions[1], directions[2]) = (directions[2], directions[1]);
        }

        if (directions[1] == turnAround)
        {
            directions[1] = DirectionType.NoDirection;
        }
        if (directions[2] == turnAround)
        {
            directions[2] = DirectionType.NoDirection;
        }

        if (directions[1] != DirectionType.NoDirection)
        {
            actor.MoveDir = directions[1];
            if (P_TryWalk(actor))
            {
                // either moved forward or attacked
                return;
            }
        }

        if (directions[2] != DirectionType.NoDirection)
        {
            actor.MoveDir = directions[2];
            if (P_TryWalk(actor))
            {
                return;
            }
        }

        // there is no direct path to the player,
        // so pick another direction.
        if (oldDir != DirectionType.NoDirection)
        {
            actor.MoveDir = oldDir;

            if (P_TryWalk(actor))
            {
                return;
            }
        }

        // randomly determine direction of search
        if ((DoomRandom.P_Random() & 1) != 0)
        {
            for (var dir = DirectionType.East; dir < DirectionType.SouthEast; dir++)
            {
                if (dir != turnAround)
                {
                    actor.MoveDir = dir;
                    if (P_TryWalk(actor))
                    {
                        return;
                    }
                }
            }
        }
        else
        {
            for (var dir = DirectionType.SouthEast; dir != DirectionType.East - 1; dir--)
            {
                if (dir != turnAround)
                {
                    actor.MoveDir = dir;
                    if (P_TryWalk(actor))
                    {
                        return;
                    }
                }
            }
        }

        if (turnAround != DirectionType.NoDirection)
        {
            actor.MoveDir = turnAround;
            if (P_TryWalk(actor))
            {
                return;
            }
        }

        actor.MoveDir = DirectionType.NoDirection;	// can not move
    }

    /// <summary>
    /// Attempts to move actor on
    /// in its current (ob->moveangle) direction.
    /// If blocked by either a wall or an actor
    /// returns FALSE
    /// If move is either clear or blocked only by a door,
    /// returns TRUE and sets...
    /// If a door is in the way,
    /// an OpenDoor call is made to start it opening.
    /// </summary>
    public bool P_TryWalk(MapObject actor)
    {
        if (!P_Move(actor))
        {
            return false;
        }

        actor.MoveCount = DoomRandom.P_Random() & 15;
        return true;
    }

    private static readonly Fixed[] XSpeed =
    {
        Fixed.Unit, new(47000), Fixed.Zero, new(-47000), -Fixed.Unit, new(-47000), Fixed.Zero, new(47000)
    };

    private static readonly Fixed[] YSpeed =
    {
        Fixed.Zero, new(47000), Fixed.Unit, new(47000), Fixed.Zero, new(-47000), -Fixed.Unit, new(-47000)
    };

    public bool P_Move(MapObject actor)
    {
        if (actor.MoveDir == DirectionType.NoDirection)
        {
            return false;
        }

        if ((uint)actor.MoveDir >= 8)
        {
            DoomGame.Error("Weird actor->movedir!");
            return false;
        }

        var tryX = actor.X + actor.Info.Speed * XSpeed[(int)actor.MoveDir];
        var tryY = actor.Y + actor.Info.Speed * YSpeed[(int)actor.MoveDir];

        var tryOk = P_TryMove(actor, tryX, tryY);

        if (!tryOk)
        {
            // open any specials
            if ((actor.Flags & MapObjectFlag.MF_FLOAT) != 0 && _floatOk)
            {
                // must adjust height
                if (actor.Z < _tmFloorZ)
                {
                    actor.Z += Constants.FloatSpeed;
                }
                else
                {
                    actor.Z -= Constants.FloatSpeed;
                }

                actor.Flags |= MapObjectFlag.MF_INFLOAT;
                return true;
            }

            if (_numSpecHit == 0)
            {
                return false;
            }

            actor.MoveDir = DirectionType.NoDirection;
            var good = false;
            while (_numSpecHit-- != 0)
            {
                var ld = _specHit[_numSpecHit];
                // if the special is not a door
                // that can be opened,
                // return false
                if (P_UseSpecialLine(actor, ld, 0))
                {
                    good = true;
                }
            }

            return good;
        }
     
        actor.Flags &= ~MapObjectFlag.MF_INFLOAT;
        if ((actor.Flags & MapObjectFlag.MF_FLOAT) == 0)
        {
            actor.Z = actor.FloorZ;
        }

        return true;
    }

    // Called by P_NoiseAlert
    // Recursively traverse adjacent sectors,
    // sound blocking lines cut off traversal
    private MapObject? _soundTarget;

    private void P_RecursiveSound(Sector sec, int soundBlocks) 
    {
        // wake up all monsters in this sector
        if (sec.ValidCount == DoomGame.Instance.Renderer.ValidCount && sec.SoundTraversed <= soundBlocks + 1)
        {
            return;     // already flooded
        }

        sec.ValidCount = DoomGame.Instance.Renderer.ValidCount;
        sec.SoundTraversed = soundBlocks + 1;
        sec.SoundTarget = _soundTarget;

        for (var i = 0; i < sec.LineCount; i++)
        {
            var check = sec.Lines[i];
            if ((check.Flags & Constants.Line.TwoSided) == 0)
            {
                continue;
            }

            P_LineOpening(check);

            if (_openRange <= Fixed.Zero)
            {
                continue;   // closed door
            }

            Sector? other;
            if (_sides[check.SideNum[0]].Sector == sec)
            {
                other = _sides[check.SideNum[1]].Sector;
            }
            else
            {
                other = _sides[check.SideNum[0]].Sector;
            }

            if ((check.Flags & Constants.Line.SoundBlock) != 0)
            {
                if (soundBlocks == 0)
                {
                    P_RecursiveSound(other, 1);
                }
            }
            else
            {
                P_RecursiveSound(other, soundBlocks);
            }
        }
    }

    /// <summary>
    /// If a monster yells at a player, it will alert other monsters to the player.
    /// </summary>
    public void P_NoiseAlert(MapObject target, MapObject emitter)
    {
        _soundTarget = target;
        DoomGame.Instance.Renderer.ValidCount++;
        P_RecursiveSound(emitter.SubSector!.Sector!, 0);
    }

    public bool P_CheckMeleeRange(MapObject actor)
    {
        if (actor.Target is null)
        {
            return false;
        }

        var player = actor.Target;
        var dist = P_AproxDistance(player.X - actor.X, player.Y - actor.Y);

        var meleeRange = Constants.MeleeRange - Fixed.FromInt(20);
        if (dist >= meleeRange.AddRadius(player.Info.Radius))
        {
            return false;
        }

        if (!P_CheckSight(actor, actor.Target))
        {
            return false;
        }

        return true;
    }

    public bool P_CheckMissileRange(MapObject actor)
    {
        if (actor.Target is null)
        {
            return false;
        }

        if (!P_CheckSight(actor, actor.Target))
        {
            return false;
        }

        if ((actor.Flags & MapObjectFlag.MF_JUSTHIT) != 0)
        {
            // the target just hit the enemy,
            // so fight back!
            actor.Flags &= ~MapObjectFlag.MF_JUSTHIT;
            return true;
        }

        if (actor.ReactionTime != 0)
        {
            return false;   // do not attack yet
        }

        // OPTIMIZE: get this from a global checksight
        var dist = P_AproxDistance(actor.X - actor.Target.X,
            actor.Y - actor.Target.Y) - Fixed.FromInt(64);

        if (actor.Info.MeleeState == StateNum.S_NULL)
        {
            dist -= Fixed.FromInt(128); // no melee attack, so fire more
        }

        var distValue = dist.Value >> 16;

        if (actor.Type == MapObjectType.MT_VILE)
        {
            if (distValue > 14 * 64)
            {
                return false;   // too far away
            }
        }

        if (actor.Type == MapObjectType.MT_UNDEAD)
        {
            if (distValue < 196)
            {
                return false;   // close for fist attack
            }
            distValue >>= 1;
        }


        if (actor.Type is MapObjectType.MT_CYBORG 
            or MapObjectType.MT_SPIDER 
            or MapObjectType.MT_SKULL)
        {
            distValue >>= 1;
        }

        if (distValue > 200)
        {
            distValue = 200;
        }

        if (actor.Type == MapObjectType.MT_CYBORG && distValue > 160)
        {
            distValue = 160;
        }

        if (DoomRandom.P_Random() < distValue)
        {
            return false;
        }

        return true;
    }
}