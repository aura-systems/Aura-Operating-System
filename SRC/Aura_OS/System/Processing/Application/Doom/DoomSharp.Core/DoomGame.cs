using DoomSharp.Core.Data;
using DoomSharp.Core.Graphics;
using DoomSharp.Core.Input;
using DoomSharp.Core.Networking;
using DoomSharp.Core.UI;
using System.Runtime.InteropServices;
using DoomSharp.Core.GameLogic;
using DoomSharp.Core.Sound;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace DoomSharp.Core;

public class DoomGame : IDisposable
{
    public const int Version = 109;
    private static IConsole _console = new NullConsole();
    private static IGraphics _graphics = new NullGraphics();
    private static ISoundDriver _soundDriver = new NullSoundDriver();

    public static readonly DoomGame Instance = new();

    private readonly List<string> _wadFileNames = new();
    private WadFileCollection? _wadFiles;

    private readonly RenderEngine _renderer = new();
    private readonly Video _video = new(_graphics);
    private readonly SoundController _sound = new(_soundDriver);
    private readonly Zone _zone = new();

    private readonly GameController _game = new();
    private readonly NetworkController _network = new();
    private MenuController? _menu;
    private HudController? _hud;
    private readonly StatusBar _statusBar = new();
    private readonly IntermissionController _intermission = new();

    private bool _singleTics = false; // debug flag to cancel adaptiveness

    private int[] _frametics = new int[4];
    private int _frameOn;
    private bool[] _frameskip = new bool[4];
    
    private long _baseTime = 0;
    private int _skiptics = 0;
    private int _gametime;

    private TicCommand[] _localCommands = new TicCommand[Constants.BackupTics];
    private TicCommand[][] _netCommands = new TicCommand[Constants.MaxPlayers][];
    private int[] _netTics = new int[Constants.MaxNetNodes];
    private bool[] _nodeInGame = new bool[Constants.MaxNetNodes];
    private bool[] _remoteResend = new bool[Constants.MaxNetNodes];
    private int[] _resendTo = new int[Constants.MaxNetNodes];
    private int[] _resendCount = new int[Constants.MaxNetNodes];

    private int[] _nodeForPlayer = new int[Constants.MaxPlayers];
    private int _maxSend;

    private int _oldnettics;
    private int _oldEnterTics = 0;
    private DoomData? _reboundStore;
    private bool _reboundPacket;

    private readonly Queue<InputEvent> _events = new(Constants.MaxEvents);

    private int _demoSequence;
    private bool _advancedemo;
    private int _demoPageTic = 0;
    private string _demoPageName = "";

    // Display method "statics"
    private bool _displayViewActiveState = false;
    private bool _displayMenuActiveState = false;
    private bool _displayInHelpScreensState = false;
    private bool _displayFullscreen = false;
    private GameState _oldDisplayGameState = GameState.Wipe;
    private int _displayBorderDrawCount;
    
    private DoomGame()
    {
        for (var i = 0; i < _netCommands.Length; i++)
        {
            _netCommands[i] = new TicCommand[Constants.BackupTics];
        }
    }

    /// <summary>
    /// To be used for platforms where the filesystem is not widely available, like mobile platforms
    /// </summary>
    /// <param name="gameMode"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public void RunAsync(GameMode gameMode, string fileName)
    {
        GameMode = gameMode;
        _wadFileNames.Add(fileName);

        RunAsync();
    }

    public void RunAsync()
    {
        try
        {
            if (GameMode == GameMode.Indetermined)
            {
                IdentifyVersion();
            }

            ModifiedGame = false;

            var titleFormat = GameMode switch
            {
                GameMode.Retail => "The Ultimate DOOM Startup v{0}.{1}",
                GameMode.Shareware => "DOOM Shareware Startup v{0}.{1}",
                GameMode.Registered => "DOOM Registered Startup v{0}.{1}",
                GameMode.Commercial => "DOOM 2: Hell on Earth v{0}.{1}",
                _ => "Public DOOM - v{0}.{1}"
            };

            _console.SetTitle(string.Format(titleFormat, Version / 100, Version % 100));

            // init subsystems
            _console.WriteLine("V_Init: allocate screens.");
            _video.Initialize();

            _console.WriteLine("M_LoadDefaults: Load system defaults.");
            // M_LoadDefaults();              // load before initing other systems

            _console.WriteLine("Z_Init: Init zone memory allocation daemon.");
            _zone.Initialize();

            _console.WriteLine("W_Init: Init WADfiles.");
            _wadFiles = WadFileCollection.InitializeMultipleFiles(_wadFileNames);

            switch (GameMode)
            {
                case GameMode.Shareware:
                case GameMode.Indetermined:
                    _console.Write(
                        "===========================================================================" +
                        Environment.NewLine +
                        "                                Shareware!" + Environment.NewLine +
                        "===========================================================================" +
                        Environment.NewLine
                    );
                    break;
                case GameMode.Registered:
                case GameMode.Retail:
                case GameMode.Commercial:
                    _console.Write(
                        "===========================================================================" +
                        Environment.NewLine +
                        "                 Commercial product - do not distribute!" + Environment.NewLine +
                        "         Please report software piracy to the SPA: 1-800-388-PIR8" + Environment.NewLine +
                        "===========================================================================" +
                        Environment.NewLine
                    );
                    break;

                default:
                    // Ouch.
                    break;
            }

            _console.WriteLine("M_Init: Init miscellaneous info.");
            _menu = new MenuController();
            
            _console.Write("R_Init: Init DOOM refresh daemon - ");
            _renderer.Initialize();

            _console.WriteLine(Environment.NewLine + "P_Init: Init Playloop state.");
            _game.P_Init();

            _console.WriteLine("I_Init: Setting up machine state.");
            // I_Init ();

            _console.WriteLine("D_CheckNetGame: Checking network game status.");
            CheckNetGame();

            _console.WriteLine("S_Init: Setting up sound.");
            Sound.Initialize(Menu.SoundFxVolume * 8, Menu.MusicVolume * 8);

            _console.WriteLine("HU_Init: Setting up heads up display.");
            _hud = new HudController();
            
            _console.WriteLine("ST_Init: Init status bar.");
            _statusBar.Init();

            if (_game.GameAction != GameAction.LoadGame)
            {
                if (AutoStart || _game.NetGame)
                {
                    _game.InitNew(StartSkill, StartEpisode, StartMap);
                }
                else
                {
                    StartTitle(); // start up intro loop
                }
            }

            // demo recording
            // debug
            _graphics.Initialize();

            DoomLoop();  // never returns
        }
        catch (DoomErrorException)
        {
            // TODO Cleanup?
        }
    }

    public bool ModifiedGame { get; private set; } = false;
    public GameMode GameMode { get; private set; } = GameMode.Indetermined;
    public GameState WipeGameState { get; private set; } = GameState.Wipe;
    public GameLanguage Language { get; private set; } = GameLanguage.English;

    public SkillLevel StartSkill { get; set; } = SkillLevel.Medium;
    public int StartEpisode { get; set; } = 1;
    public int StartMap { get; set; } = 1;
    public bool AutoStart { get; set; } = false;

    public TicCommand[][] NetCommands => _netCommands;

    public int TicDup { get; private set; } = 1; // tic duplication // 1 = no duplication, 2-5 = dup for slow nets

    /// <summary>
    /// maketic is the tick that hasn't had control made for it yet
    /// </summary>
    public int MakeTic { get; private set; } = 0;

    public bool StatusBarActive { get; set; } = false;

    public bool AutoMapActive { get; set; } = false;   // In AutoMap mode?
    public bool MenuActive { get; set; } = false;  // Menu overlayed?
    public bool InHelpScreensActive { get; set; } = false;

    public RenderEngine Renderer => _renderer;
    public GameController Game => _game;
    public Video Video => _video;
    public SoundController Sound => _sound;
    public HudController Hud => _hud!;
    public WadFileCollection WadData => _wadFiles!;
    public MenuController Menu => _menu!;
    public StatusBar StatusBar => _statusBar;
    public IntermissionController Intermission => _intermission;

    public static void SetConsole(IConsole console)
    {
        _console = console;
    }

    public static void SetOutputRenderer(IGraphics renderer)
    {
        _graphics = renderer;
        Instance.Video.SetOutputRenderer(renderer);
    }

    public static void SetSoundDriver(ISoundDriver driver)
    {
        _soundDriver = driver;
        Instance.Sound.SetDriver(driver);
    }

    public static IConsole Console => _console;

    private void Display()
    {
        bool wipe;

        if (Game.NoDrawers)
        {
            return;                    // for comparative timing / profiling
        }

        var redrawsbar = false;

        // change the view size if needed
        if (Renderer.SetSizeNeeded)
        {
            Renderer.ExecuteSetViewSize();
            _oldDisplayGameState = GameState.Wipe; // force background redraw
            _displayBorderDrawCount = 3;
        }

        // save the current screen if about to wipe
        if (_game.GameState != _game.WipeGameState)
        {
            wipe = true;
            _video.WipeStartScreen(0, 0, Constants.ScreenWidth, Constants.ScreenHeight);
        }
        else
        {
            wipe = false;
        }

        if (_game.GameState == GameState.Level && _game.GameTic != 0)
        {
            DoomGame.Instance.Hud.Erase();
        }

        // do buffered drawing
        switch (_game.GameState)
        {
            case GameState.Level:
                if (_game.GameTic == 0)
                {
                    break;
                }

                if (AutoMapActive)
                {
                    // AM_Drawer();
                }

                if (wipe || (Renderer.ViewHeight != 200 && _displayFullscreen))
                {
                    redrawsbar = true;
                }

                if (_displayInHelpScreensState && !Menu.InHelpScreens)
                {
                    redrawsbar = true;              // just put away the help screen
                }

                _statusBar.Drawer(Renderer.ViewHeight == 200, redrawsbar);
                _displayFullscreen = Renderer.ViewHeight == 200;
                break;

            case GameState.Intermission:
                Intermission.Drawer();
                break;
            case GameState.Finale:
                // F_Drawer();
                break;
            case GameState.DemoScreen:
                PageDrawer();
                break;
        }

        // draw buffered stuff to screen
        // I_UpdateNoBlit(); // Doesn't do anything

        // draw the view directly
        if (_game.GameState == GameState.Level && !AutoMapActive && _game.GameTic != 0)
        {
            Renderer.RenderPlayerView(Game.Players[Game.DisplayPlayer]);
        }

        if (_game.GameState == GameState.Level && _game.GameTic != 0)
        {
            Hud.Drawer();
        }

        // clean up border stuff
        if (_game.GameState != _oldDisplayGameState && _game.GameState != GameState.Level)
        {
            _video.SetPalette("PLAYPAL");
        }

        // see if the border needs to be initially drawn
        if (_game.GameState == GameState.Level && _oldDisplayGameState != GameState.Level)
        {
            _displayViewActiveState = false; // view was not active
            Renderer.FillBackScreen(); // draw the pattern into the back screen
        }

        // see if the border needs to be updated to the screen
        if (_game.GameState == GameState.Level && !AutoMapActive && Renderer.ScaledViewWidth != 320)
        {
            if (_menu!.IsActive || MenuActive || !_displayViewActiveState)
            {
                _displayBorderDrawCount = 3;
            }
            if (_displayBorderDrawCount != 0)
            {
                Renderer.DrawViewBorder(); // erase old menu stuff
                _displayBorderDrawCount--;
            }
        }

        _displayMenuActiveState = _menu!.IsActive;
        _displayViewActiveState = _game.ViewActive;
        _displayInHelpScreensState = _menu!.InHelpScreens;
        _oldDisplayGameState = _game.WipeGameState = _game.GameState;

        // draw pause pic
        if (_game.Paused)
        {
            int y;
            if (AutoMapActive)
            {
                y = 4;
            }
            else
            {
                y = Renderer.ViewWindowY + 4;
            }

            _video.DrawPatchDirect(Renderer.ViewWindowX + (Renderer.ScaledViewWidth - 68) / 2, y, 0, WadData.GetLumpName("M_PAUSE", PurgeTag.Cache));
        }

        // menus go directly to the screen
        _menu!.Drawer(); // menu is drawn even on top of everything
        NetUpdate(); // send out any new accumulation

        // normal update
        if (!wipe)
        {
            _video.SignalOutputReady(); // page flip or blit buffer
            return;
        }

        // wipe update
        _video.WipeEndScreen(0, 0, Constants.ScreenWidth, Constants.ScreenHeight);

        var wipeStart = GetTime() - 1;
        bool done;

        do
        {
            int nowTime;
            int tics;
            do
            {
                nowTime = GetTime();
                tics = nowTime - wipeStart;
            } while (tics == 0);

            wipeStart = nowTime;
            done = _video.WipeScreenEffect(WipeMethod.Melt, 0, 0, Constants.ScreenWidth, Constants.ScreenHeight, tics);
            // I_UpdateNoBlit(); // Doesn't do anything
            _menu!.Drawer();                       // menu is drawn even on top of wipes
            _video.SignalOutputReady();            // page flip or blit buffer
        } while (!done);
    }

    public void DoomLoop()
    {
        // frame syncronous IO operations
        //I_StartFrame(); // not needed

        // process one or more tics
        if (_singleTics)
        {
            _graphics.StartTic();
            ProcessEvents();
            _netCommands[Game.ConsolePlayer][MakeTic % Constants.BackupTics] = Game.BuildTicCommand();
            if (_advancedemo)
            {
                DoAdvanceDemo();
            }

            _menu!.Ticker();
            _game.Ticker();

            _game.GameTic++;
            MakeTic++;

            Thread.Sleep(1000 / Constants.TicRate);
        }
        else
        {
            TryRunTics(); // will run at least one tic
        }

        Sound.UpdateSounds(Game.Players[Game.ConsolePlayer].MapObject!); // move positional sounds

        // Update display, next frame, with current state.
        Display();

        Sound.Submit();
    }

    private void QuitNetGame()
    {
        //if (debugfile)
        //    fclose(debugfile);

        if (!_game.NetGame || !_game.UserGame || _game.ConsolePlayer == -1 || _game.DemoPlayback)
        {
            return;
        }

        // send a bunch of packets for security
        var netBuffer = _network.DoomCom.Data;
        netBuffer.Player = (byte)_game.ConsolePlayer;
        netBuffer.NumTics = 0;
        for (var i = 0; i < 4; i++)
        {
            for (var j = 1; j < _network.DoomCom.NumNodes; j++)
            {
                if (_nodeInGame[j])
                {
                    HSendPacket(j, Constants.NetCommands.Exit);
                }
            }
            WaitVBL(1);
        }
    }

    private void TryRunTics()
    {
        var enterTic = GetTime() / TicDup;
        var realTics = enterTic - _oldEnterTics;
        _oldEnterTics = enterTic;

        NetUpdate();

        var lowTic = int.MaxValue;
        var numplaying = 0;
        for (var i = 0; i < _network.DoomCom.NumNodes; i++)
        {
            if (_nodeInGame[i])
            {
                numplaying++;
                if (_netTics[i] < lowTic)
                {
                    lowTic = _netTics[i];
                }
            }
        }
        var availableTics = lowTic - (_game.GameTic / TicDup);

        // decide how many tics to run
        int counts;
        if (realTics < (availableTics - 1))
        {
            counts = realTics + 1;
        }
        else if (realTics < availableTics)
        {
            counts = realTics;
        }
        else
        {
            counts = availableTics;
        }

        if (counts < 1)
        {
            counts = 1;
        }

        _frameOn++;

        //if (debugfile)
        //    fprintf(debugfile,
        //        "=======real: %i  avail: %i  game: %i\n",
        //        realtics, availabletics, counts);

        if (!_game.DemoPlayback)
        {
            // ideally nettics[0] should be 1 - 3 tics above lowtic
            // if we are consistantly slower, speed up time
            int i;
            for (i = 0; i < Constants.MaxPlayers; i++)
            {
                if (_game.PlayerInGame[i])
                    break;
            }

            if (_game.ConsolePlayer == i)
            {
                // the key player does not adapt
            }
            else
            {
                if (_netTics[0] <= _netTics[_nodeForPlayer[i]])
                {
                    _gametime--;
                    // printf ("-");
                }
                _frameskip[_frameOn & 3] = (_oldnettics > _netTics[_nodeForPlayer[i]]);
                _oldnettics = _netTics[0];
                if (_frameskip[0] && _frameskip[1] && _frameskip[2] && _frameskip[3])
                {
                    _skiptics = 1;
                    // printf ("+");
                }
            }
        }// demoplayback

        // wait for new tics if needed
        while (lowTic < ((_game.GameTic / TicDup) + counts))
        {
            NetUpdate();
            lowTic = int.MaxValue;

            for (var i = 0; i < _network.DoomCom.NumNodes; i++)
            {
                if (_nodeInGame[i] && _netTics[i] < lowTic)
                {
                    lowTic = _netTics[i];
                }
            }

            if (lowTic < (_game.GameTic / TicDup))
            {
                Error("TryRunTics: lowtic < gametic");
            }

            // don't stay in here forever -- give the menu a chance to work
            var currentTime = GetTime() / TicDup;
            if ((currentTime - enterTic) >= 20)
            {
                _menu!.Ticker();
                return;
            }
        }

        // run the count * ticdup tics
        while (counts > 0)
        {
            counts--;
            for (var i = 0; i < TicDup; i++)
            {
                if ((_game.GameTic / TicDup) > lowTic)
                {
                    Error("gametic>lowtic");
                }

                if (_advancedemo)
                {
                    DoAdvanceDemo();
                }

                _menu!.Ticker();
                _game.Ticker();
                _game.GameTic++;

                // modify command for duplicated tics
                if (i != (TicDup - 1))
                {
                    var buf = (_game.GameTic / TicDup) % Constants.BackupTics;
                    for (var j = 0; j < Constants.MaxPlayers; j++)
                    {
                        var cmd = _netCommands[j][buf];
                        cmd.ChatChar = (char)0;
                        if ((cmd.Buttons & ButtonCode.Special) != 0)
                        {
                            cmd.Buttons = 0;
                        }
                    }
                }
            }
            NetUpdate();    // check for new console commands
        }
    }

    public void NetUpdate()
    {
        // check time
        var nowTime = GetTime() / TicDup;
        var newTics = nowTime - _gametime;
        _gametime = nowTime;

        if (newTics <= 0) // nothing new to update
        {
            // listen for other packets
            GetPackets();
            return;
        }

        if (_skiptics <= newTics)
        {
            newTics -= _skiptics;
            _skiptics = 0;
        }
        else
        {
            _skiptics -= newTics;
            newTics = 0;
        }

        var netBuffer = _network.DoomCom.Data;
        netBuffer.Player = (byte)_game.ConsolePlayer;

        // build new ticcmds for console player
        var gameTicDiv = _game.GameTic / TicDup;
        for (var i = 0; i < newTics; i++)
        {
            _graphics.StartTic();
            ProcessEvents();
            if ((MakeTic - gameTicDiv) >= ((Constants.BackupTics / 2) - 1))
            {
                break;          // can't hold any more
            }

            _localCommands[MakeTic % Constants.BackupTics] = Game.BuildTicCommand();

            MakeTic++;
        }

        if (_singleTics)
        {
            return; // singletic update is synchronous
        }

        // send the packet to the other nodes
        for (var i = 0; i < _network.DoomCom.NumNodes; i++)
        {
            if (!_nodeInGame[i])
            {
                continue;
            }

            var realStart = _resendTo[i];
            netBuffer.StartTic = (byte)_resendTo[i];
            netBuffer.NumTics = (byte)(MakeTic - realStart);
            if (netBuffer.NumTics > Constants.BackupTics)
            {
                Error("NetUpdate: netbuffer->numtics > BACKUPTICS");
            }

            _resendTo[i] = MakeTic - _network.DoomCom.ExtraTics;

            for (var j = 0; j < netBuffer.NumTics; j++)
            {
                netBuffer.Commands[j] = _localCommands[(realStart + j) % Constants.BackupTics];
            }

            if (_remoteResend[i])
            {
                netBuffer.RetransmitFrom = (byte)_netTics[i];
                HSendPacket(i, Constants.NetCommands.Retransmit);
            }
            else
            {
                netBuffer.RetransmitFrom = 0;
                HSendPacket(i, 0);
            }
        }

        // listen for other packets
        GetPackets();
    }

    private int NetbufferSize()
    {
        return 5 * 4 + Marshal.SizeOf<TicCommand>() * Constants.BackupTics;
    }

    private uint NetbufferChecksum()
    {
        uint c = 0x1234567;
        var netbuffer = _network.DoomCom.Data;
        var checksumOffset = 1;

        c += (uint)(netbuffer.RetransmitFrom * (checksumOffset++));
        c += (uint)(netbuffer.StartTic * (checksumOffset++));
        c += (uint)(netbuffer.Player * (checksumOffset++));
        c += (uint)(netbuffer.NumTics * (checksumOffset++));

        foreach (var cmd in netbuffer.Commands)
        {
            c += (uint)(cmd.ForwardMove * (checksumOffset++));
            c += (uint)(cmd.SideMove * (checksumOffset++));
            c += (uint)(cmd.AngleTurn * (checksumOffset++));
            c += (uint)(cmd.Consistency * (checksumOffset++));
            c += (uint)(cmd.ChatChar * (checksumOffset++));
            c += (uint)((int)cmd.Buttons * (checksumOffset++));
        }

        return c & Constants.NetCommands.CheckSum;
    }

    private int ExpandTics(int low)
    {
        var delta = low - (MakeTic & 0xff);

        switch (delta)
        {
            case >= -64 and <= 64:
                return (MakeTic & ~0xff) + low;
            case > 64:
                return (MakeTic & ~0xff) - 256 + low;
            case < -64:
                return (MakeTic & ~0xff) + 256 + low;
            default:
                Error($"ExpandTics: strange value {low} at maketic {MakeTic}");
                return 0;
        }
    }

    private void HSendPacket(int node, uint flags)
    {
        var netBuffer = _network.DoomCom.Data;

        netBuffer.CheckSum = NetbufferChecksum() | flags;
        if (node == 0)
        {
            _reboundStore = netBuffer;
            _reboundPacket = true;
            return;
        }

        if (_game.DemoPlayback)
        {
            return;
        }

        if (!_game.NetGame)
        {
            Error("Tried to transmit to another node");
        }

        _network.DoomCom.Command = Command.Send;
        _network.DoomCom.RemoteNode = node;
        _network.DoomCom.DataLength = NetbufferSize();

        //if (debugfile)
        //{
        //    int i;
        //    int realretrans;
        //    if (netbuffer->checksum & NCMD_RETRANSMIT)
        //        realretrans = ExpandTics(netbuffer->retransmitfrom);
        //    else
        //        realretrans = -1;

        //    fprintf(debugfile, "send (%i + %i, R %i) [%i] ",
        //        ExpandTics(netbuffer->starttic),
        //        netbuffer->numtics, realretrans, doomcom->datalength);

        //    for (i = 0; i < doomcom->datalength; i++)
        //        fprintf(debugfile, "%i ", ((byte*)netbuffer)[i]);

        //    fprintf(debugfile, "\n");
        //}

        _network.NetworkCommand();
    }

    private bool HGetPacket()
    {
        var netBuffer = _network.DoomCom.Data;
        if (_reboundPacket && _reboundStore != null)
        {
            _network.DoomCom.Data = _reboundStore;
            _network.DoomCom.RemoteNode = 0;
            _reboundPacket = false;
            return true;
        }

        if (!_game.NetGame)
        {
            return false;
        }

        if (_game.DemoPlayback)
        {
            return false;
        }

        _network.DoomCom.Command = Command.Get;
        _network.NetworkCommand();

        if (_network.DoomCom.RemoteNode == -1)
        {
            return false;
        }

        if (_network.DoomCom.DataLength != NetbufferSize())
        {
            //if (debugfile)
            //    fprintf(debugfile, "bad packet length %i\n", doomcom->datalength);
            return false;
        }

        if (NetbufferChecksum() != (netBuffer.CheckSum & Constants.NetCommands.CheckSum))
        {
            //if (debugfile)
            //    fprintf(debugfile, "bad packet checksum\n");
            return false;
        }

        //if (debugfile)
        //{
        //    int realretrans;
        //    int i;

        //    if (netbuffer->checksum & NCMD_SETUP)
        //        fprintf(debugfile, "setup packet\n");
        //    else
        //    {
        //        if (netbuffer->checksum & NCMD_RETRANSMIT)
        //            realretrans = ExpandTics(netbuffer->retransmitfrom);
        //        else
        //            realretrans = -1;

        //        fprintf(debugfile, "get %i = (%i + %i, R %i)[%i] ",
        //            doomcom->remotenode,
        //            ExpandTics(netbuffer->starttic),
        //            netbuffer->numtics, realretrans, doomcom->datalength);

        //        for (i = 0; i < doomcom->datalength; i++)
        //            fprintf(debugfile, "%i ", ((byte*)netbuffer)[i]);
        //        fprintf(debugfile, "\n");
        //    }
        //}
        return true;
    }

    private void GetPackets()
    {
        var netbuffer = _network.DoomCom.Data;
        while (HGetPacket())
        {
            if ((netbuffer.CheckSum & Constants.NetCommands.Setup) > 0)
            {
                continue;       // extra setup packet
            }

            var netconsole = netbuffer.Player & ~Constants.PlayerDrone;
            var netnode = _network.DoomCom.RemoteNode;

            // to save bytes, only the low byte of tic numbers are sent
            // Figure out what the rest of the bytes are
            var realstart = ExpandTics(netbuffer.StartTic);
            var realend = (realstart + netbuffer.NumTics);

            // check for exiting the game
            if ((netbuffer.CheckSum & Constants.NetCommands.Exit) > 0)
            {
                if (!_nodeInGame[netnode])
                {
                    continue;
                }

                _nodeInGame[netnode] = false;
                _game.PlayerInGame[netconsole] = false;

                Game.Players[Game.ConsolePlayer].Message = $"Player {netconsole} left the game";

                if (_game.DemoRecording)
                {
                    _game.CheckDemoStatus();
                }
                continue;
            }

            // check for a remote game kill
            if ((netbuffer.CheckSum & Constants.NetCommands.Kill) > 0)
            {
                Error("Killed by network driver");
                return;
            }

            _nodeForPlayer[netconsole] = netnode;

            // check for retransmit request
            if (_resendCount[netnode] <= 0 && (netbuffer.CheckSum & Constants.NetCommands.Retransmit) > 0)
            {
                _resendTo[netnode] = ExpandTics(netbuffer.RetransmitFrom);
                //if (debugfile)
                //    fprintf(debugfile, "retransmit from %i\n", resendto[netnode]);
                _resendCount[netnode] = Constants.ResendCount;
            }
            else
            {
                _resendCount[netnode]--;
            }

            // check for out of order / duplicated packet		
            if (realend == _netTics[netnode])
            {
                continue;
            }

            if (realend < _netTics[netnode])
            {
                //if (debugfile)
                //    fprintf(debugfile,
                //         "out of order packet (%i + %i)\n",
                //         realstart, netbuffer->numtics);
                continue;
            }

            // check for a missed packet
            if (realstart > _netTics[netnode])
            {
                // stop processing until the other system resends the missed tics
                //if (debugfile)
                //    fprintf(debugfile,
                //         "missed tics from %i (%i - %i)\n",
                //         netnode, realstart, nettics[netnode]);
                _remoteResend[netnode] = true;
                continue;
            }

            // update command store from the packet
            {
                _remoteResend[netnode] = false;

                var start = _netTics[netnode] - realstart;

                while (_netTics[netnode] < realend)
                {
                    _netCommands[netconsole][_netTics[netnode] % Constants.BackupTics] = netbuffer.Commands[start++];
                    _netTics[netnode]++;
                }
            }
        }
    }

    private void CheckNetGame()
    {
        for (var i = 0; i < Constants.MaxNetNodes; i++)
        {
            _nodeInGame[i] = false;
            _netTics[i] = 0;
            _remoteResend[i] = false;    // set when local needs tics
            _resendTo[i] = 0;        // which tic to start sending
        }

        _network.Initialize();
        if (_network.DoomCom.Id != Constants.DoomComId)
        {
            Error("Doomcom buffer invalid!");
            return;
        }

        _game.ConsolePlayer = _game.DisplayPlayer = _network.DoomCom.ConsolePlayer;
        if (_game.NetGame)
        {
            ArbitrateNetStart();
        }

        Console.WriteLine($"startskill {StartSkill}  deathmatch: {_game.DeathMatch}  startmap: {StartMap}  startepisode: {StartEpisode}");

        // read values out of doomcom
        TicDup = _network.DoomCom.TicDup;
        _maxSend = Constants.BackupTics / (2 * TicDup) - 1;
        if (_maxSend < 1)
        {
            _maxSend = 1;
        }

        for (var i = 0; i < _network.DoomCom.NumPlayers; i++)
        {
            _game.PlayerInGame[i] = true;
        }

        for (var i = 0; i < _network.DoomCom.NumNodes; i++)
        {
            _nodeInGame[i] = true;
        }

        Console.WriteLine($"player {_game.ConsolePlayer + 1} of {_network.DoomCom.NumPlayers} ({_network.DoomCom.NumNodes} nodes)");
    }

    private void CheckAbort()
    {
        var stopTic = GetTime() + 2;
        while (GetTime() < stopTic)
        {
            _graphics.StartTic();
        }

        _graphics.StartTic();
        while (_events.TryDequeue(out var ev))
        {
            if (ev.Type == EventType.KeyDown && ev.Data1 == (int)Keys.Escape)
            {
                Error("Network game synchronization aborted.");
            }
        }
    }

    private void ArbitrateNetStart()
    {
        AutoStart = true;

        var netBuffer = _network.DoomCom.Data;

        if (_network.DoomCom.ConsolePlayer != 0)
        {
            // listen for setup info from key player
            Console.WriteLine("Listening for network start info ...");
            
            while (true)
            {
                CheckAbort();
                if (!HGetPacket())
                {
                    continue;
                }
                if ((netBuffer.CheckSum & Constants.NetCommands.Setup) != 0)
                {
                    if (netBuffer.Player != Version)
                    {
                        Error("Different DOOM versions cannot play a net game!");
                        return;
                    }

                    StartSkill = (SkillLevel)(netBuffer.RetransmitFrom & 15);
                    _game.DeathMatch = ((netBuffer.RetransmitFrom & 0xc0) >> 6) != 0;
                    var nomonsters = (netBuffer.RetransmitFrom & 0x20) > 0;
                    var respawnparm = (netBuffer.RetransmitFrom & 0x10) > 0;
                    StartMap = netBuffer.StartTic & 0x3f;
                    StartEpisode = netBuffer.StartTic >> 6;
                    return;
                }
            }
        }
        else
        {
            // key player, send the setup info
            Console.WriteLine("Sending network start info ...");

            int i;
            var gotInfo = new bool[Constants.MaxNetNodes];
            do
            {
                CheckAbort();
                for (i = 0; i < _network.DoomCom.NumNodes; i++)
                {
                    netBuffer.RetransmitFrom = (byte)StartSkill;
                    if (_game.DeathMatch)
                    {
                        netBuffer.RetransmitFrom |= (byte)((_game.DeathMatch ? 1 : 0) << 6);
                    }
                    //if (nomonsters)
                    //    netbuffer->retransmitfrom |= 0x20;
                    //if (respawnparm)
                    //    netbuffer->retransmitfrom |= 0x10;
                    netBuffer.StartTic = (byte)(StartEpisode * 64 + StartMap);
                    netBuffer.Player = Version;
                    netBuffer.NumTics = 0;
                    HSendPacket(i, Constants.NetCommands.Setup);
                }

                for (i = 10 ; i != 0 && HGetPacket(); --i)
                {
                    if ((netBuffer.Player & 0x7f) < Constants.MaxNetNodes)
                    {
                        gotInfo[netBuffer.Player & 0x7f] = true;
                    }
                }

                for (i = 1; i < _network.DoomCom.NumNodes; i++)
                {
                    if (!gotInfo[i])
                    {
                        break;
                    }
                }
            } while (i < _network.DoomCom.NumNodes);
        }
    }

    public void PostEvent(InputEvent ev)
    {
        _events.Enqueue(ev);
    }

    private void ProcessEvents()
    {
        // process events and dispatch them to the menu and the game logic (the latter only if the menu didn't eat the event)

        // IF STORE DEMO, DO NOT ACCEPT INPUT
        if (GameMode == GameMode.Commercial && WadData.GetNumForName("map01") < 0)
        {
            return;
        }

        while (_events.TryDequeue(out var currentEvent))
        {
            if (_menu!.HandleEvent(currentEvent))
            {
                continue;
            }

            Game.HandleEvent(currentEvent);
        }
    }

    public int GetTime()
    {
        var timeSinceEpoch = DateTime.UtcNow - DateTime.UnixEpoch;
        var secondsSinceEpoch = (long)timeSinceEpoch.TotalSeconds;
        if (_baseTime == 0)
        {
            _baseTime = secondsSinceEpoch;
        }

        return (int)(((secondsSinceEpoch - _baseTime) * Constants.TicRate)
                     + (timeSinceEpoch.Milliseconds * Constants.TicRate / 1000));
    }

    public void StartTitle()
    {
        _game.GameAction = GameAction.Nothing;
        _demoSequence = -1;
        AdvanceDemo();
    }

    public void AdvanceDemo()
    {
        _advancedemo = true;
    }

    private void DoAdvanceDemo()
    {
        _game.Players[_game.ConsolePlayer].PlayerState = PlayerState.Alive; // not reborn
        _advancedemo = false; 
        _game.UserGame = false;               // no save / end game here
        _game.Paused = false;
        _game.GameAction = GameAction.Nothing;

        if (GameMode == GameMode.Retail)
        {
            _demoSequence = (_demoSequence + 1) % 7;
        }
        else
        {
            _demoSequence = (_demoSequence + 1) % 6;
        }

        switch (_demoSequence)
        {
            case 0:
                if (GameMode == GameMode.Commercial)
                {
                    _demoPageTic = 35 * 11;
                }
                else
                {
                    _demoPageTic = 170;
                }
                _game.GameState = GameState.DemoScreen;
                _demoPageName = "TITLEPIC";
                Sound.StartMusic(GameMode == GameMode.Commercial ? MusicType.mus_dm2ttl : MusicType.mus_intro);
                break;
            case 1:
                _game.DeferedPlayDemo("demo1");
                break;
            case 2:
                _demoPageTic = 200;
                _game.GameState = GameState.DemoScreen;
                _demoPageName = "CREDIT";
                break;
            case 3:
                _game.DeferedPlayDemo("demo2");
                break;
            case 4:
                _game.GameState = GameState.DemoScreen;
                if (GameMode == GameMode.Commercial)
                {
                    _demoPageTic = 35 * 11;
                    _demoPageName = "TITLEPIC";
                    Sound.StartMusic(MusicType.mus_dm2ttl);
                }
                else
                {
                    _demoPageTic = 200;

                    if (GameMode == GameMode.Retail)
                    {
                        _demoPageName = "CREDIT";
                    }
                    else
                    {
                        _demoPageName = "HELP2";
                    }
                }
                break;
            case 5:
                _game.DeferedPlayDemo("demo3");
                break;
            // THE DEFINITIVE DOOM Special Edition demo
            case 6:
                _game.DeferedPlayDemo("demo4");
                break;
        }
    }

    private void PageDrawer()
    {
        _video.DrawPatch(0, 0, 0, WadData.GetLumpName(_demoPageName, PurgeTag.Cache));
    }

    public void PageTicker()
    {
        if (--_demoPageTic < 0)
        {
            AdvanceDemo();
        }
    }

    public void Quit()
    {
        QuitNetGame();
        
        //I_ShutdownSound();
        //I_ShutdownMusic();
        //M_SaveDefaults();
        //I_ShutdownGraphics();
        
        Console.Shutdown();
    }

    public void WaitVBL(int count)
    {
        Thread.Sleep((int)(count * (1000 / 70.0)));
    }

    private void IdentifyVersion()
    {
        var doomWadDir = Environment.GetEnvironmentVariable("DOOMWADDIR") ?? ".";
        
        // Commercial
        var wadFile = Path.Combine(doomWadDir, "doom2.wad");
        if (File.Exists(wadFile))
        {
            GameMode = GameMode.Commercial;
            _wadFileNames.Add(wadFile);
            return;
        }

        // Retail
        wadFile = Path.Combine(doomWadDir, "doomu.wad");
        if (File.Exists(wadFile))
        {
            GameMode = GameMode.Retail;
            _wadFileNames.Add(wadFile);
            return;
        }

        // Registered
        wadFile = Path.Combine(doomWadDir, "doom.wad");
        if (File.Exists(wadFile))
        {
            GameMode = GameMode.Registered;
            _wadFileNames.Add(wadFile);
            return;
        }

        // Shareware
        wadFile = Path.Combine(doomWadDir, "doom1.wad");
        if (File.Exists(wadFile))
        {
            GameMode = GameMode.Shareware;
            _wadFileNames.Add(wadFile);
        }
    }

    public static void Error(string message)
    {
        _console.WriteLine("Error: " + message);
        DoomGame.Instance.Quit();
    }

    public void Dispose()
    {
        _wadFiles?.Dispose();
    }
}