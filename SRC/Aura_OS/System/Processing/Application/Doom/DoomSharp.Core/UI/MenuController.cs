using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using DoomSharp.Core.Data;
using DoomSharp.Core.Input;
using DoomSharp.Core.Sound;

namespace DoomSharp.Core.UI;

public class MenuController
{
    private static readonly string[] SkullNames = { "M_SKULL1", "M_SKULL2" };
    
    private const int MaxSaveFiles = 6;
    private const int SaveStringSize = 24;
    private const int LineHeight = 16;
    private const int SkullXOff = -32;

    private int _whichSkull = 0;
    private int _skullAnimationCounter = 10;
    
    private readonly List<MenuItem> _mainMenuItems;
    private readonly List<MenuItem> _episodeMenuItems;
    private readonly List<MenuItem> _newGameMenuItems;
    private readonly List<MenuItem> _optionsMenuItems;
    private readonly List<MenuItem> _readMenuItems;
    private readonly List<MenuItem> _readMenuItems2;
    private readonly List<MenuItem> _soundMenuItems;
    private readonly List<MenuItem> _loadMenuItems;
    private readonly List<MenuItem> _saveMenuItems;

    private readonly Menu _mainMenu;
    private readonly Menu _episodeMenu;
    private readonly Menu _newGameMenu;
    private readonly Menu _optionsMenu;
    private readonly Menu _readMenu;
    private readonly Menu _readMenu2;
    private readonly Menu _soundMenu;
    private readonly Menu _loadMenu;
    private readonly Menu _saveMenu;

    private readonly string[] _saveGameStrings = new string[MaxSaveFiles];
    private int _quickSaveSlot = -1;
    private int _saveSlot = 0;
    private int _saveCharIndex = 0;
    private bool _saveStringEnter = false;
    private string _oldSaveString = "";

    private Menu _currentMenu;
    private int _itemOn;

    private bool _messageLastMenuActive = false;
    private bool _messageToPrint = false;
    private string _messageString = string.Empty;
    private Action<char>? _messageRoutine;
    private bool _messageNeedsInput = false;

    private enum MainMenuItemIndexes
    {
        NewGame = 0,
        Options,
        LoadGame,
        SaveGame,
        ReadThis,
        QuitDOOM
    };

    private enum Episodes
    {
        Episode1,
        Episode2,
        Episode3,
        Episode4
    };

    private enum NewGameMenuItemIndexes
    {
        KillThings,
        TooRough,
        HurtMe,
        Violence,
        Nightmare
    };

    private enum OptionsMenuItemIndexes
    {
        EndGame,
        Messages,
        Detail,
        ScreenSize,
        Empty1,
        MouseSensitivity,
        Empty2,
        SoundVolume
    };

    private enum SoundMenuItemIndexes
    {
        SoundFxVolume,
        Empty1,
        MusicVolume,
        Empty2
    }

    public MenuController()
    {
        _mainMenuItems = new(new[]
        {
            new MenuItem(MenuItemStatus.Ok, "M_NGAME", NewGame, 'n'),
            new MenuItem(MenuItemStatus.Ok, "M_OPTION", Options, 'o'),
            new MenuItem(MenuItemStatus.Ok, "M_LOADG", LoadGame, 'l'),
            new MenuItem(MenuItemStatus.Ok, "M_SAVEG", SaveGame, 's'),
            new MenuItem(MenuItemStatus.Ok, "M_RDTHIS", ReadThis, 'r'),
            new MenuItem(MenuItemStatus.Ok, "M_QUITG", QuitDOOM, 'q')
        });

        _episodeMenuItems = new(new[]
        {
            new MenuItem(MenuItemStatus.Ok, "M_EPI1", SelectEpisode, 'k'),
            new MenuItem(MenuItemStatus.Ok, "M_EPI2", SelectEpisode, 't'),
            new MenuItem(MenuItemStatus.Ok, "M_EPI3", SelectEpisode, 'i'),
            new MenuItem(MenuItemStatus.Ok, "M_EPI4", SelectEpisode, 't')
        });

        _newGameMenuItems = new(new[]
        {
            new MenuItem(MenuItemStatus.Ok, "M_JKILL", ChooseSkill, 'i'),
            new MenuItem(MenuItemStatus.Ok, "M_ROUGH", ChooseSkill, 'h'),
            new MenuItem(MenuItemStatus.Ok, "M_HURT", ChooseSkill, 'h'),
            new MenuItem(MenuItemStatus.Ok, "M_ULTRA", ChooseSkill, 'u'),
            new MenuItem(MenuItemStatus.Ok, "M_NMARE", ChooseSkill, 'n')
        });

        _optionsMenuItems = new(new[]
        {
            new MenuItem(MenuItemStatus.Ok, "M_ENDGAM", EndGame, 'e'),
            new MenuItem(MenuItemStatus.Ok, "M_MESSG", ChangeMessages, 'm'),
            new MenuItem(MenuItemStatus.Ok, "M_DETAIL", ChangeDetail, 'g'),
            new MenuItem(MenuItemStatus.ArrowsOk, "M_SCRNSZ", SizeDisplay, 's'),
            new MenuItem(MenuItemStatus.Empty, ""),
            new MenuItem(MenuItemStatus.ArrowsOk, "M_MSENS", ChangeSensitivity, 'm'),
            new MenuItem(MenuItemStatus.Empty, ""),
            new MenuItem(MenuItemStatus.Ok, "M_SVOL", Sound, 's'),
        });

        _readMenuItems = new(new[]
        {
            new MenuItem(MenuItemStatus.Ok, "", ReadThis2)
        });

        _readMenuItems2 = new(new[]
        {
            new MenuItem(MenuItemStatus.Ok, "", FinishReadThis)
        });

        _soundMenuItems = new(new[]
        {
            new MenuItem(MenuItemStatus.ArrowsOk, "M_SFXVOL", ChangeSoundFxVolume, 's'),
            new MenuItem(MenuItemStatus.Empty, ""),
            new MenuItem(MenuItemStatus.ArrowsOk, "M_MUSVOL", ChangeMusicVolume, 'm'),
            new MenuItem(MenuItemStatus.Empty, "")
        });

        _loadMenuItems = new(new[]
        {
            new MenuItem(MenuItemStatus.Ok, "", LoadSelect, '1'),
            new MenuItem(MenuItemStatus.Ok, "", LoadSelect, '2'),
            new MenuItem(MenuItemStatus.Ok, "", LoadSelect, '3'),
            new MenuItem(MenuItemStatus.Ok, "", LoadSelect, '4'),
            new MenuItem(MenuItemStatus.Ok, "", LoadSelect, '5'),
            new MenuItem(MenuItemStatus.Ok, "", LoadSelect, '6')
        });

        _saveMenuItems = new(new[]
        {
            new MenuItem(MenuItemStatus.Ok, "", SaveSelect, '1'),
            new MenuItem(MenuItemStatus.Ok, "", SaveSelect, '2'),
            new MenuItem(MenuItemStatus.Ok, "", SaveSelect, '3'),
            new MenuItem(MenuItemStatus.Ok, "", SaveSelect, '4'),
            new MenuItem(MenuItemStatus.Ok, "", SaveSelect, '5'),
            new MenuItem(MenuItemStatus.Ok, "", SaveSelect, '6')
        });

        _mainMenu = new Menu(_mainMenuItems.Count, null, _mainMenuItems, DrawMainMenu, 97, 64, 0);
        _episodeMenu = new Menu(_episodeMenuItems.Count, _mainMenu, _episodeMenuItems, DrawEpisodesMenu, 48, 63, (int)Episodes.Episode1);
        _newGameMenu = new Menu(_newGameMenuItems.Count, _episodeMenu, _newGameMenuItems, DrawNewGameMenu, 48, 63, (int)NewGameMenuItemIndexes.HurtMe);
        _optionsMenu = new Menu(_optionsMenuItems.Count, _mainMenu, _optionsMenuItems, DrawOptionsMenu, 60, 37, 0);
        _readMenu = new Menu(_readMenuItems.Count, _mainMenu, _readMenuItems, DrawReadThisMenu, 330, 175, 0);
        _readMenu2 = new Menu(_readMenuItems2.Count, _readMenu, _readMenuItems2, DrawReadThisMenu2, 330, 175, 0);
        _soundMenu = new Menu(_soundMenuItems.Count, _optionsMenu, _soundMenuItems, DrawSoundMenu, 80, 64, 0);
        _loadMenu = new Menu(_loadMenuItems.Count, _mainMenu, _loadMenuItems, DrawLoadMenu, 80, 54, 0);
        _saveMenu = new Menu(_saveMenuItems.Count, _mainMenu, _saveMenuItems, DrawSaveMenu, 80, 54, 0);

        _currentMenu = _mainMenu;

        _itemOn = _currentMenu.LastOn;

        ScreenSize = ScreenBlocks - 3;
        
        switch (DoomGame.Instance.GameMode)
        {
            case GameMode.Commercial:
                // This is used because DOOM 2 had only one HELP
                //  page. I use CREDIT as second page now, but
                //  kept this hack for educational purposes.

                _mainMenuItems[(int)MainMenuItemIndexes.ReadThis] = _mainMenuItems[(int)MainMenuItemIndexes.QuitDOOM];
                _mainMenu.NumItems--;
                _mainMenu.Y += 8;
                _newGameMenu.PreviousMenu = _mainMenu;
                _readMenu.DrawRoutine = DrawReadThisMenu;
                _readMenu.X = 330;
                _readMenu.Y = 165;
                _readMenuItems[0].ChoiceRoutine = FinishReadThis;
                break;

            case GameMode.Shareware:
                // Episode 2 and 3 are handled,
                //  branching to an ad screen.
                break;

            case GameMode.Registered:
                // We need to remove the fourth episode.
                _episodeMenu.NumItems--;
                break;

            case GameMode.Retail:
                // We are fine.
                break;

            default:
                break;
        }
    }

    public bool IsActive { get; private set; } = false;
    public bool InHelpScreens { get; private set; } = false;

    public bool ShowMessages { get; private set; } = true;
    public bool DetailLevel { get; private set; } = true; // High detail by default, we have modern PC's now :D
    public int MouseSensitivity { get; private set; } = 5;
    public int ScreenSize { get; private set; }
    public int ScreenBlocks { get; private set; } = 10;
    public int SoundFxVolume { get; private set; } = 13;
    public int MusicVolume { get; private set; } = 5;

    public int SelectedEpisode { get; private set; } = 0;

    public void Drawer()
    {
        int start = 0, x = 0, y = 0, i = 0;

        InHelpScreens = false;
        
        if (_messageToPrint)
        {
            start = 0;
            y = 100 - StringHeight(_messageString) / 2;

            while (start < _messageString.Length)
            {
                var currentString = "";
                for (i = start; i < _messageString.Length; i++)
                {
                    var c = _messageString[i];
                    if (c == '\r')
                    {
                        continue;
                    }

                    if (c == '\n')
                    {
                        currentString = _messageString.Substring(start, i - start);
                        start = i + 1;
                    }
                }

                if (i == _messageString.Length)
                {
                    currentString = _messageString[start..];
                    start = _messageString.Length;
                }

                x = 160 - StringWidth(currentString) / 2;
                WriteText(x, y, currentString);
                y += DoomGame.Instance.Hud.Font[0].Height;
            }

            return;
        }

        if (!IsActive)
        {
            return;
        }

        // Call current menu's draw routine
        _currentMenu.DrawRoutine();
        
        // Draw menu
        x = _currentMenu.X;
        y = _currentMenu.Y;
        var max = _currentMenu.NumItems;

        for (i = 0; i < max; i++)
        {
            if (_currentMenu.Items[i].Name != "")
            {
                var lump = DoomGame.Instance.WadData.GetLumpName(_currentMenu.Items[i].Name, PurgeTag.Cache);
                DoomGame.Instance.Video.DrawPatchDirect(x, y, 0, lump);
            }

            y += LineHeight;
        }

        // Draw skull
        var skullLump = DoomGame.Instance.WadData.GetLumpName(SkullNames[_whichSkull], PurgeTag.Cache);
        DoomGame.Instance.Video.DrawPatchDirect(x + SkullXOff, _currentMenu.Y - 5 + _itemOn * LineHeight, 0, skullLump);
    }

    public void Ticker()
    {
        if (--_skullAnimationCounter <= 0)
        {
            _whichSkull ^= 1;
            _skullAnimationCounter = 8;
        }
    }

    private int _joyWait = 0;
    private int _mouseWait = 0;
    private int _mouseX = 0;
    private int _mouseY = 0;
    private int _lastX = 0;
    private int _lastY = 0;

    public bool HandleEvent(InputEvent ev)
    {
        var ch = -1;

        if (ev.Type == EventType.Joystick && _joyWait < DoomGame.Instance.GetTime())
        {
            if (ev.Data3 == -1)
            {
                ch = (int)Keys.UpArrow;
                _joyWait = DoomGame.Instance.GetTime() + 5;
            }
            else if (ev.Data3 == 1)
            {
                ch = (int)Keys.DownArrow;
                _joyWait = DoomGame.Instance.GetTime() + 5;
            }

            if (ev.Data2 == -1)
            {
                ch = (int)Keys.LeftArrow;
                _joyWait = DoomGame.Instance.GetTime() + 2;
            }
            else if (ev.Data2 == 1)
            {
                ch = (int)Keys.RightArrow;
                _joyWait = DoomGame.Instance.GetTime() + 2;
            }

            if ((ev.Data1 & 1) != 0)
            {
                ch = (int)Keys.Enter;
                _joyWait = DoomGame.Instance.GetTime() + 5;
            }
            else if ((ev.Data1 & 2) != 0)
            {
                ch = (int)Keys.Backspace;
                _joyWait = DoomGame.Instance.GetTime() + 5;
            }
        }
        else if (ev.Type == EventType.Mouse && _mouseWait < DoomGame.Instance.GetTime())
        {
            _mouseY += ev.Data3;
            if (_mouseY < _lastY - 30)
            {
                ch = (int)Keys.DownArrow;
                _mouseWait = DoomGame.Instance.GetTime() + 5;
                _mouseY = _lastY -= 30;
            }
            else if (_mouseY > _lastY + 30)
            {
                ch = (int)Keys.UpArrow;
                _mouseWait = DoomGame.Instance.GetTime() + 5;
                _mouseY = _lastY += 30;
            }

            _mouseX += ev.Data2;
            if (_mouseX < _lastX - 30)
            {
                ch = (int)Keys.LeftArrow;
                _mouseWait = DoomGame.Instance.GetTime() + 5;
                _mouseX = _lastX -= 30;
            }
            else if (_mouseX > _lastX + 30)
            {
                ch = (int)Keys.RightArrow;
                _mouseWait = DoomGame.Instance.GetTime() + 5;
                _mouseX = _lastX += 30;
            }

            if ((ev.Data1 & 1) != 0)
            {
                ch = (int)Keys.Enter;
                _mouseWait = DoomGame.Instance.GetTime() + 15;
            }
            else if ((ev.Data1 & 2) != 0)
            {
                ch = (int)Keys.Backspace;
                _mouseWait = DoomGame.Instance.GetTime() + 15;
            }
        }
        else if (ev.Type == EventType.KeyDown)
        {
            ch = ev.Data1;
        }

        if (ch == -1)
        {
            return false;
        }

        // Save game string input
        if (_saveStringEnter)
        {
            switch (ch)
            {
                case (int)Keys.Backspace:
                    if (_saveCharIndex > 0)
                    {
                        _saveCharIndex--;
                        _saveGameStrings[_saveSlot] = _saveGameStrings[_saveSlot][..(_saveGameStrings[_saveSlot].Length - 1)];
                    }
                    break;

                case (int)Keys.Escape:
                    _saveStringEnter = false;
                    _saveGameStrings[_saveSlot] = _oldSaveString;
                    break;

                case (int)Keys.Enter:
                    _saveStringEnter = false;
                    if (!string.IsNullOrWhiteSpace(_saveGameStrings[_saveSlot]))
                    {
                        DoSave(_saveSlot);
                    }
                    break;

                default:
                    var saveChar = char.ToUpper((char)ch);
                    if (saveChar != 32)
                    {
                        if ((saveChar - HudController.HuFontStart) < 0 || (saveChar - HudController.HuFontStart) >= HudController.HuFontSize)
                        {
                            break; // invalid char (not in font)
                        }
                    }

                    if (((int)saveChar is >= 32 and <= 127) &&
                        _saveCharIndex < SaveStringSize - 1 &&
                        StringWidth(_saveGameStrings[_saveSlot]) < (SaveStringSize - 2) * 8)
                    {
                        _saveGameStrings[_saveSlot] += saveChar;
                        _saveCharIndex++;
                    }

                    break;
            }

            return true;
        }

        // Take care of any messages that need input
        if (_messageToPrint)
        {
            if (_messageNeedsInput && ch is not (' ' or 'n' or 'y' or (char)Keys.Escape))
            {
                return false;
            }

            IsActive = _messageLastMenuActive;
            _messageToPrint = false;
            
            _messageRoutine?.Invoke((char)ch);
            
            IsActive = false;
            DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_swtchx);
            return true;
        }

        //if (devparm && ch == KEY_F1)
        //{
        //    G_ScreenShot();
        //    return true;
        //}

        if (!IsActive)
        {
            switch (ch)
            {
                case (int)Keys.Minus: // Screen size down
                    //if (automapactive || chat_on)
                    //{
                    //    return false;
                    //}
                    SizeDisplay(0);
                    DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_stnmov);
                    return true;

                case (int)Keys.Equals: // Screen size up
                    //if (automapactive || chat_on)
                    //{
                    //    return false;
                    //}
                    SizeDisplay(1);
                    DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_stnmov);
                    return true;

                case (int)Keys.F1: // Help key
                    StartControlPanel();

                    _currentMenu = DoomGame.Instance.GameMode == GameMode.Retail ? _readMenu2 : _readMenu;

                    _itemOn = 0;
                    DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_swtchn);
                    return true;

                case (int)Keys.F2:            // Save
                    StartControlPanel();
                    DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_swtchn);
                    SaveGame(0);
                    return true;

                case (int)Keys.F3:            // Load
                    StartControlPanel();
                    DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_swtchn);
                    LoadGame(0);
                    return true;

                case (int)Keys.F4:            // Sound Volume
                    StartControlPanel();
                    _currentMenu = _soundMenu;
                    _itemOn = (int)SoundMenuItemIndexes.SoundFxVolume;
                    DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_swtchn);
                    return true;

                case (int)Keys.F5:            // Detail toggle
                    ChangeDetail(0);
                    DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_swtchn);
                    return true;

                case (int)Keys.F6:            // Quicksave
                    DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_swtchn);
                    QuickSave();
                    return true;

                case (int)Keys.F7:            // End game
                    DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_swtchn);
                    EndGame(0);
                    return true;

                case (int)Keys.F8:            // Toggle messages
                    ChangeMessages(0);
                    DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_swtchn);
                    return true;

                case (int)Keys.F9:            // Quickload
                    DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_swtchn);
                    QuickLoad();
                    return true;

                case (int)Keys.F10:           // Quit DOOM
                    DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_swtchn);
                    QuitDOOM(0);
                    return true;

                case (int)Keys.F11:           // gamma toggle
                    DoomGame.Instance.Video.ToggleGamma();
                    // DoomGame.Instance.Game.Players[DoomGame.Instance.Game.ConsolePlayer].Message = gammamsg[usegamma];
                    return true;
            }
        }

        // Pop-up menu?
        if (!IsActive)
        {
            if (ch == (int)Keys.Escape)
            {
                StartControlPanel();
                DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_swtchn);
                return true;
            }
            return false;
        }

        // Keys usable within menu
        switch (ch)
        {
            case (int)Keys.DownArrow:
                do
                {
                    if (_itemOn + 1 > _currentMenu.NumItems - 1)
                    {
                        _itemOn = 0;
                    }
                    else
                    {
                        _itemOn++;
                    }
                    DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_pstop);
                } while (_currentMenu.Items[_itemOn].Status == MenuItemStatus.Empty);
                return true;

            case (int)Keys.UpArrow:
                do
                {
                    if (_itemOn == 0)
                    {
                        _itemOn = _currentMenu.NumItems - 1;
                    }
                    else
                    {
                        _itemOn--;
                    }
                    DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_pstop);
                } while (_currentMenu.Items[_itemOn].Status == MenuItemStatus.Empty);
                return true;

            case (int)Keys.LeftArrow:
                if (_currentMenu.Items[_itemOn].ChoiceRoutine is not null && _currentMenu.Items[_itemOn].Status == MenuItemStatus.ArrowsOk)
                {
                    DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_stnmov);
                    _currentMenu.Items[_itemOn].ChoiceRoutine?.Invoke(0);
                }
                return true;

            case (int)Keys.RightArrow:
                if (_currentMenu.Items[_itemOn].ChoiceRoutine is not null && _currentMenu.Items[_itemOn].Status == MenuItemStatus.ArrowsOk)
                {
                    DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_stnmov);
                    _currentMenu.Items[_itemOn].ChoiceRoutine?.Invoke(1);
                }
                return true;

            case (int)Keys.Enter:
                if (_currentMenu.Items[_itemOn].ChoiceRoutine is not null && _currentMenu.Items[_itemOn].Status > 0)
                {
                    _currentMenu.LastOn = _itemOn;
                    if (_currentMenu.Items[_itemOn].Status == MenuItemStatus.ArrowsOk)
                    {
                        DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_stnmov);
                        _currentMenu.Items[_itemOn].ChoiceRoutine?.Invoke(1); // right arrow
                    }
                    else
                    {
                        _currentMenu.Items[_itemOn].ChoiceRoutine?.Invoke(_itemOn);
                        DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_pistol);
                    }
                }
                return true;
                
            case (int)Keys.Escape:
                _currentMenu.LastOn = _itemOn;
                ClearMenus();
                DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_swtchx);
                return true;

            case (int)Keys.Backspace:
                _currentMenu.LastOn = _itemOn;
                if (_currentMenu.PreviousMenu is not null)
                {
                    _currentMenu = _currentMenu.PreviousMenu;
                    _itemOn = _currentMenu.LastOn;
                    DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_swtchn);
                }
                return true;

            default:
                for (var i = _itemOn + 1; i < _currentMenu.NumItems; i++)
                {
                    if (_currentMenu.Items[i].HotKey == ch)
                    {
                        _itemOn = i;
                        DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_pstop);
                        return true;
                    }
                }

                for (var i = 0; i <= _itemOn; i++)
                {
                    if (_currentMenu.Items[i].HotKey == ch)
                    {
                        _itemOn = i;
                        DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_pstop);
                        return true;
                    }
                }

                break;

        }

        return false;
    }

    public void StartControlPanel()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        _currentMenu = _mainMenu;
        _itemOn = _currentMenu.LastOn;
    }

    private void DrawMainMenu()
    {
        var lump = DoomGame.Instance.WadData.GetLumpName("M_DOOM", PurgeTag.Cache);
        DoomGame.Instance.Video.DrawPatchDirect(94, 2, 0, lump);
    }

    private void DrawNewGameMenu()
    {
        var lump = DoomGame.Instance.WadData.GetLumpName("M_NEWG", PurgeTag.Cache);
        DoomGame.Instance.Video.DrawPatchDirect(96, 14, 0, lump);

        lump = DoomGame.Instance.WadData.GetLumpName("M_SKILL", PurgeTag.Cache);
        DoomGame.Instance.Video.DrawPatchDirect(54, 38, 0, lump);
    }

    private void DrawEpisodesMenu()
    {
        var lump = DoomGame.Instance.WadData.GetLumpName("M_EPISOD", PurgeTag.Cache);
        DoomGame.Instance.Video.DrawPatchDirect(54, 38, 0, lump);
    }

    private void DrawOptionsMenu()
    {
        var lump = DoomGame.Instance.WadData.GetLumpName("M_OPTTTL", PurgeTag.Cache);
        DoomGame.Instance.Video.DrawPatchDirect(108, 15, 0, lump);

        lump = DoomGame.Instance.WadData.GetLumpName(DetailLevel ? "M_GDHIGH" : "M_GDLOW", PurgeTag.Cache);
        DoomGame.Instance.Video.DrawPatchDirect(_optionsMenu.X + 175, _optionsMenu.Y + LineHeight * (int)OptionsMenuItemIndexes.Detail, 0, lump);

        lump = DoomGame.Instance.WadData.GetLumpName(ShowMessages ? "M_MSGON" : "M_MSGOFF", PurgeTag.Cache);
        DoomGame.Instance.Video.DrawPatchDirect(_optionsMenu.X + 120, _optionsMenu.Y + LineHeight * (int)OptionsMenuItemIndexes.Messages, 0, lump);

        DrawThermo(_optionsMenu.X, _optionsMenu.Y + LineHeight * ((int)OptionsMenuItemIndexes.MouseSensitivity + 1), 10, MouseSensitivity);
        DrawThermo(_optionsMenu.X, _optionsMenu.Y + LineHeight * ((int)OptionsMenuItemIndexes.ScreenSize + 1), 9, ScreenSize);
    }

    private void NewGame(int choice)
    {
        var game = DoomGame.Instance.Game;
        if (game.NetGame && !game.DemoPlayback)
        {
            StartMessage(Messages.NewGame, null, false);
            return;
        }

        SetupNextMenu(DoomGame.Instance.GameMode == GameMode.Commercial 
            ? _newGameMenu 
            : _episodeMenu);
    }

    private void ChooseSkill(int choice)
    {
        if (choice == (int)NewGameMenuItemIndexes.Nightmare)
        {
            StartMessage(Messages.Nightmare, VerifyNightmare, true);
            return;
        }

        DoomGame.Instance.Game.DeferedInitNew((SkillLevel)choice, SelectedEpisode + 1, 1);
        ClearMenus();
    }

    private void VerifyNightmare(char choice)
    {
        if (choice != 'y')
        {
            return;
        }

        DoomGame.Instance.Game.DeferedInitNew(SkillLevel.Nightmare, SelectedEpisode + 1, 1);
        ClearMenus();
    }

    private void SelectEpisode(int choice)
    {
        if (DoomGame.Instance.GameMode == GameMode.Shareware && choice != 0)
        {
            StartMessage(Messages.Shareware, null, false);
            SetupNextMenu(_readMenu);
            return;
        }

        if (DoomGame.Instance.GameMode == GameMode.Registered && choice > 2)
        {
            DoomGame.Console.WriteLine("M_Episode: 4th episode requires UltimateDOOM");
            choice = 0;
        }

        SelectedEpisode = choice;
        SetupNextMenu(_newGameMenu);
    }

    private void Options(int choice)
    {
        SetupNextMenu(_optionsMenu);
    }

    private void ChangeMessages(int choice)
    {
        ShowMessages = !ShowMessages;

        //if (!ShowMessages)
        //{
        //    DoomGame.Instance.Game.Players[DoomGame.Instance.Game.ConsolePlayer].Message = MSGOFF;
        //}
        //else
        //{
        //    DoomGame.Instance.Game.Players[DoomGame.Instance.Game.ConsolePlayer].Message = MSGON;
        //}

        //message_dontfuckwithme = true;
    }

    private void EndGame(int choice)
    {
        var game = DoomGame.Instance.Game;
        if (!game.UserGame)
        {
            DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_oof);
            return;
        }

        if (game.NetGame)
        {
            // M_StartMessage(NETEND, NULL, false);
            return;
        }

        // M_StartMessage(ENDGAME, M_EndGameResponse, true);
    }

    private void EndGameResponse(char choice)
    {
        if (choice != 'y')
        {
            return;
        }

        _currentMenu.LastOn = _itemOn;
        ClearMenus();
        DoomGame.Instance.StartTitle();
    }

    private void ChangeSensitivity(int choice)
    {
        switch (choice)
        {
            case 0:
                if (MouseSensitivity > 0)
                {
                    MouseSensitivity--;
                }
                break;
            case 1:
                if (MouseSensitivity < 9)
                {
                    MouseSensitivity++;
                }
                break;
        }
    }

    private void ChangeDetail(int choice)
    {
        DetailLevel = !DetailLevel;

        // FIXME - does not work. Remove anyway?
        // fprintf(stderr, "M_ChangeDetail: low detail mode n.a.\n");

        DoomGame.Instance.Renderer.SetViewSize(ScreenBlocks, DetailLevel);

        if (!DetailLevel)
        {
            DoomGame.Instance.Game.Players[DoomGame.Instance.Game.ConsolePlayer].Message = Messages.DetailHigh;
        }
        else
        {
            DoomGame.Instance.Game.Players[DoomGame.Instance.Game.ConsolePlayer].Message = Messages.DetailLow;
        }
    }

    private void SizeDisplay(int choice)
    {
        switch (choice)
        {
            case 0:
                if (ScreenSize > 0)
                {
                    ScreenBlocks--;
                    ScreenSize--;
                }
                break;
            case 1:
                if (ScreenSize < 8)
                {
                    ScreenBlocks++;
                    ScreenSize++;
                }
                break;
        }

        DoomGame.Instance.Renderer.SetViewSize(ScreenBlocks, DetailLevel);
    }

    private void DrawSoundMenu()
    {
        var lump = DoomGame.Instance.WadData.GetLumpName("M_SVOL", PurgeTag.Cache);
        DoomGame.Instance.Video.DrawPatchDirect(60, 38, 0, lump);

        DrawThermo(_soundMenu.X, _soundMenu.Y + LineHeight * ((int)SoundMenuItemIndexes.SoundFxVolume + 1), 16, SoundFxVolume);

        DrawThermo(_soundMenu.X, _soundMenu.Y + LineHeight * ((int)SoundMenuItemIndexes.MusicVolume + 1), 16, MusicVolume);
    }

    private void Sound(int choice)
    {
        SetupNextMenu(_soundMenu);
    }

    private void ChangeSoundFxVolume(int choice)
    {
        switch (choice)
        {
            case 0:
                if (SoundFxVolume > 0)
                {
                    SoundFxVolume--;
                }
                break;
            case 1:
                if (SoundFxVolume < 15)
                {
                    SoundFxVolume++;
                }
                break;
        }

        DoomGame.Instance.Sound.SetSfxVolume(SoundFxVolume * 8);
    }

    private void ChangeMusicVolume(int choice)
    {
        switch (choice)
        {
            case 0:
                if (MusicVolume > 0)
                {
                    MusicVolume--;
                }
                break;
            case 1:
                if (MusicVolume < 15)
                {
                    MusicVolume++;
                }
                break;
        }

        DoomGame.Instance.Sound.SetMusicVolume(MusicVolume * 8);
    }

    //
    // Read This Menus
    // Had a "quick hack to fix romero bug"
    //
    private void DrawReadThisMenu()
    {
        InHelpScreens = true;

        byte[]? lump;
        switch (DoomGame.Instance.GameMode)
        {
            case GameMode.Commercial:
                lump = DoomGame.Instance.WadData.GetLumpName("HELP", PurgeTag.Cache);
                break;
            case GameMode.Shareware:
            case GameMode.Registered:
            case GameMode.Retail:
                lump = DoomGame.Instance.WadData.GetLumpName("HELP1", PurgeTag.Cache);
                break;
            default:
                lump = null;
                break;
        }

        if (lump == null)
        {
            return;
        }

        DoomGame.Instance.Video.DrawPatchDirect(0, 0, 0, lump);
    }

    //
    // Read This Menus - optional second page.
    //
    private void DrawReadThisMenu2()
    {
        InHelpScreens = true;

        byte[]? lump;
        switch (DoomGame.Instance.GameMode)
        {
            case GameMode.Retail:
            case GameMode.Commercial:
                // This hack keeps us from having to change menus.
                lump = DoomGame.Instance.WadData.GetLumpName("CREDIT", PurgeTag.Cache);
                break;
            case GameMode.Shareware:
            case GameMode.Registered:
                lump = DoomGame.Instance.WadData.GetLumpName("HELP2", PurgeTag.Cache);
                break;
            default:
                lump = null;
                break;
        }

        if (lump == null)
        {
            return;
        }

        DoomGame.Instance.Video.DrawPatchDirect(0, 0, 0, lump);
    }

    private void ReadThis(int choice)
    {
        SetupNextMenu(_readMenu);
    }

    private void ReadThis2(int choice)
    {
        SetupNextMenu(_readMenu2);
    }

    private void FinishReadThis(int choice)
    {
        SetupNextMenu(_mainMenu);
    }

    private void DrawLoadMenu()
    {
        var lump = DoomGame.Instance.WadData.GetLumpName("M_LOADG", PurgeTag.Cache);
        DoomGame.Instance.Video.DrawPatchDirect(72, 28, 0, lump);
        
        for (var i = 0; i < MaxSaveFiles; i++)
        {
            DrawSaveLoadBorder(_loadMenu.X, _loadMenu.Y + LineHeight * i);
            WriteText(_loadMenu.X, _loadMenu.Y + LineHeight * i, _saveGameStrings[i]);
        }
    }

    private void DrawSaveLoadBorder(int x, int y)
    {
        var lump = DoomGame.Instance.WadData.GetLumpName("M_LSLEFT", PurgeTag.Cache);
        DoomGame.Instance.Video.DrawPatchDirect(x - 8, y + 7, 0, lump);

        lump = DoomGame.Instance.WadData.GetLumpName("M_LSCNTR", PurgeTag.Cache);
        for (var i = 0; i < 24; i++)
        {
            DoomGame.Instance.Video.DrawPatchDirect(x, y + 7, 0, lump);
            x += 8;
        }

        lump = DoomGame.Instance.WadData.GetLumpName("M_LSRGHT", PurgeTag.Cache);
        DoomGame.Instance.Video.DrawPatchDirect(x, y + 7, 0, lump);
    }

    private void LoadSelect(int choice)
    {
        var name = $"doomsav{choice}.dsg";
        //G_LoadGame(name);
        ClearMenus();
    }

    private void LoadGame(int choice)
    {
        //if (netgame)
        //{
        //    M_StartMessage(LOADNET, NULL, false);
        //    return;
        //}

        SetupNextMenu(_loadMenu);
        ReadSaveStrings();
    }

    private void DrawSaveMenu()
    {
        var lump = DoomGame.Instance.WadData.GetLumpName("M_SAVEG", PurgeTag.Cache);
        DoomGame.Instance.Video.DrawPatchDirect(72, 28, 0, lump);

        for (var i = 0; i < MaxSaveFiles; i++)
        {
            DrawSaveLoadBorder(_loadMenu.X, _loadMenu.Y + LineHeight * i);
            WriteText(_loadMenu.X, _loadMenu.Y + LineHeight * i, _saveGameStrings[i]);
        }

        if (_saveStringEnter)
        {
            var i = StringWidth(_saveGameStrings[_saveSlot]);
            WriteText(_loadMenu.X + i, _loadMenu.Y + LineHeight * _saveSlot, "_");
        }
    }

    private void DoSave(int slot)
    {
        // G_SaveGame(slot, _saveGameStrings[slot]);
        ClearMenus();

        if (_quickSaveSlot == -2)
        {
            _quickSaveSlot = slot;
        }
    }

    private void SaveSelect(int choice)
    {
        // we are going to be intercepting all chars
        _saveStringEnter = true;
        _saveSlot = choice;

        _oldSaveString = _saveGameStrings[choice];

        //if (!strcmp(savegamestrings[choice], EMPTYSTRING))
        //    savegamestrings[choice][0] = 0;

        _saveCharIndex = _oldSaveString.Length;
    }

    private void SaveGame(int choice)
    {
        //if (!usergame)
        //{
        //    M_StartMessage(SAVEDEAD, NULL, false);
        //    return;
        //}

        if (DoomGame.Instance.Game.GameState != GameState.Level)
        {
            return;
        }

        SetupNextMenu(_saveMenu);
        ReadSaveStrings();
    }

    private void QuickSaveResponse(char ch)
    {
        if (ch == 'y')
        {
            DoSave(_quickSaveSlot);
            DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_swtchx);
        }
    }

    private void QuickSave()
    {
        //if (!usergame)
        //{
        //    S_StartSound(NULL, sfx_oof);
        //    return;
        //}

        if (DoomGame.Instance.Game.GameState != GameState.Level)
        {
            return;
        }

        if (_quickSaveSlot < 0)
        {
            StartControlPanel();
            ReadSaveStrings();
            SetupNextMenu(_saveMenu);
            _quickSaveSlot = -2; // means to pick a slot now
            return;
        }

        var tempString = string.Format(Messages.QuickSavePrompt, _saveGameStrings[_quickSaveSlot]);
        StartMessage(tempString, QuickSaveResponse, true);
    }

    private void QuickLoadResponse(char ch)
    {
        if (ch == 'y')
        {
            LoadSelect(_quickSaveSlot);
            DoomGame.Instance.Sound.StartSound(null, SoundType.sfx_swtchx);
        }
    }

    private void QuickLoad()
    {
        //if (netgame)
        //{
        //    M_StartMessage(QLOADNET, NULL, false);
        //    return;
        //}

        if (_quickSaveSlot < 0)
        {
            StartMessage(Messages.QuickSaveSpot, null, false);
            return;
        }

        var tempString = string.Format(Messages.QuickLoadPrompt, _saveGameStrings[_quickSaveSlot]);
        StartMessage(tempString, QuickLoadResponse, true);
    }

    private void ReadSaveStrings()
    {
        for (var i = 0; i < MaxSaveFiles; i++)
        {
            var name = $"doomsav{i}.dsg";
            if (!File.Exists(name))
            {
                _saveGameStrings[i] = "";
                _loadMenuItems[i].Status = MenuItemStatus.NoCursorHere;
                continue;
            }
            
            using var sr = new StreamReader(name, Encoding.ASCII);
            var saveName = new char[SaveStringSize];
            sr.ReadBlock(saveName, 0, SaveStringSize);
            
            _saveGameStrings[i] = new string(saveName);
            _loadMenuItems[i].Status = MenuItemStatus.Ok;
        }
    }

    private void QuitDOOM(int choice)
    {
        // We pick index 0 which is language sensitive,
        //  or one at random, between 1 and maximum number.
        var endMessage = "";
        if (DoomGame.Instance.Language != GameLanguage.French)
        {
            endMessage = Messages.QuitMessages.First() + Environment.NewLine + Environment.NewLine + Messages.DosY;
        }
        else
        {
            endMessage = Messages.QuitMessages.ElementAt((DoomGame.Instance.Game.GameTic % (Messages.NumberOfQuitMessages - 2)) + 1) + Environment.NewLine + Environment.NewLine + Messages.DosY;
        }
        
        StartMessage(endMessage, QuitResponse, true);
    }

    private void QuitResponse(char choice)
    {
        if (choice != 'y')
        {
            return;
        }

        if (!DoomGame.Instance.Game.NetGame)
        {
            if (DoomGame.Instance.GameMode == GameMode.Commercial)
            {
                //DoomGame.Instance.Sound.StartSound(null, SoundType.quitsounds2[(gametic >> 2) & 7]);
            }
            else
            {
                //DoomGame.Instance.Sound.StartSound(null, SoundType.quitsounds[(gametic >> 2) & 7]);
            }
            DoomGame.Instance.WaitVBL(105);
        }

        DoomGame.Instance.Quit();
    }

    private void ClearMenus()
    {
        IsActive = false;
    }

    private void SetupNextMenu(Menu nextMenu)
    {
        _currentMenu = nextMenu;
        _itemOn = _currentMenu.LastOn;
    }

    private void StartMessage(string message, Action<char>? routine, bool input)
    {
        _messageLastMenuActive = IsActive;
        _messageToPrint = true;
        _messageString = message;
        _messageRoutine = routine;
        _messageNeedsInput = input;
        IsActive = true;
    }

    private void StopMessage()
    {
        IsActive = _messageLastMenuActive;
        _messageToPrint = false;
    }

    private void DrawThermo(int x, int y, int width, int value)
    {
        var xx = x;

        var lump = DoomGame.Instance.WadData.GetLumpName("M_THERML", PurgeTag.Cache);
        DoomGame.Instance.Video.DrawPatchDirect(xx, y, 0, lump);

        xx += 8;
        lump = DoomGame.Instance.WadData.GetLumpName("M_THERMM", PurgeTag.Cache);
        for (var i = 0; i < width; i++)
        {
            DoomGame.Instance.Video.DrawPatchDirect(xx, y, 0, lump);
            xx += 8;
        }

        lump = DoomGame.Instance.WadData.GetLumpName("M_THERMR", PurgeTag.Cache);
        DoomGame.Instance.Video.DrawPatchDirect(xx, y, 0, lump);

        lump = DoomGame.Instance.WadData.GetLumpName("M_THERMO", PurgeTag.Cache);
        DoomGame.Instance.Video.DrawPatchDirect(x + 8 + (value * 8), y, 0, lump);
    }

    private int StringHeight(string message)
    {
        var height = DoomGame.Instance.Hud.Font[0].Height;
        var lineCount = message.Count(x => x == '\n') + 1;

        return lineCount * height;
    }

    private int StringWidth(string message)
    {
        var width = 0;

        message = message.ToUpperInvariant();
        foreach (var letter in message)
        {
            var c = letter - HudController.HuFontStart;
            if (c is < 0 or >= HudController.HuFontSize)
            {
                width += 4;
            }
            else
            {
                width += DoomGame.Instance.Hud.Font[c].Width;
            }
        }

        return width;
    }

    private void WriteText(int x, int y, string message)
    {
        var cx = x;
        var cy = y;
        foreach (var c in message.ToUpperInvariant())
        {
            if (c == '\r')
            {
                continue;
            }

            if (c == '\n')
            {
                cx = x;
                cy += 12;
                continue;
            }

            var drawC = c - HudController.HuFontStart;
            if (drawC is < 0 or >= HudController.HuFontSize)
            {
                cx += 4;
                continue;
            }

            var width = DoomGame.Instance.Hud.Font[drawC].Width;
            if ((cx + width) > Constants.ScreenWidth)
            {
                break;
            }

            DoomGame.Instance.Video.DrawPatchDirect(cx, cy, 0, DoomGame.Instance.Hud.Font[drawC]);
            cx += width;
        }
    }
}