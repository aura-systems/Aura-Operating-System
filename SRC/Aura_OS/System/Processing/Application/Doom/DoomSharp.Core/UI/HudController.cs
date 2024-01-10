using DoomSharp.Core.Data;
using DoomSharp.Core.GameLogic;
using DoomSharp.Core.Graphics;
using DoomSharp.Core.Input;
using System.Collections.Generic;

namespace DoomSharp.Core.UI;

public class HudController
{
    public const char HuFontStart = '!';
    public const char HuFontEnd = '_';
    public const int HuFontSize = HuFontEnd - HuFontStart + 1;

    public const int Broadcast = 5;

    public const int MessageRefresh = (int)Keys.Enter;
    public const int MessageX = 0;
    public const int MessageY = 0;
    public const int MessageWidth = 64; // width in characters
    public const int MessageHeight = 1; // in lines

    public const int MessageTimeout = Constants.TicRate * 4;

    // DOOM shareware/registered/retail (Ultimate) names.
    public static readonly List<string> MapNames;

    // DOOM 2 map names.
    public static readonly List<string> MapNames2;

    private Player? _player;
    private bool _chatOn;

    private bool _messageOn;
    private bool _messageDontFuckWithMe;
    private bool _messageNotToBeFuckedWith;

    private int _messageCounter;

    static HudController()
    {
        MapNames = new List<string>(new[]
        {
            Messages.HUSTR_E1M1,
            Messages.HUSTR_E1M2,
            Messages.HUSTR_E1M3,
            Messages.HUSTR_E1M4,
            Messages.HUSTR_E1M5,
            Messages.HUSTR_E1M6,
            Messages.HUSTR_E1M7,
            Messages.HUSTR_E1M8,
            Messages.HUSTR_E1M9,

            Messages.HUSTR_E2M1,
            Messages.HUSTR_E2M2,
            Messages.HUSTR_E2M3,
            Messages.HUSTR_E2M4,
            Messages.HUSTR_E2M5,
            Messages.HUSTR_E2M6,
            Messages.HUSTR_E2M7,
            Messages.HUSTR_E2M8,
            Messages.HUSTR_E2M9,

            Messages.HUSTR_E3M1,
            Messages.HUSTR_E3M2,
            Messages.HUSTR_E3M3,
            Messages.HUSTR_E3M4,
            Messages.HUSTR_E3M5,
            Messages.HUSTR_E3M6,
            Messages.HUSTR_E3M7,
            Messages.HUSTR_E3M8,
            Messages.HUSTR_E3M9,

            Messages.HUSTR_E4M1,
            Messages.HUSTR_E4M2,
            Messages.HUSTR_E4M3,
            Messages.HUSTR_E4M4,
            Messages.HUSTR_E4M5,
            Messages.HUSTR_E4M6,
            Messages.HUSTR_E4M7,
            Messages.HUSTR_E4M8,
            Messages.HUSTR_E4M9,

            "NEWLEVEL",
            "NEWLEVEL",
            "NEWLEVEL",
            "NEWLEVEL",
            "NEWLEVEL",
            "NEWLEVEL",
            "NEWLEVEL",
            "NEWLEVEL",
            "NEWLEVEL"
        });

        MapNames2 = new List<string>(new[]
        {
            Messages.HUSTR_1,
            Messages.HUSTR_2,
            Messages.HUSTR_3,
            Messages.HUSTR_4,
            Messages.HUSTR_5,
            Messages.HUSTR_6,
            Messages.HUSTR_7,
            Messages.HUSTR_8,
            Messages.HUSTR_9,
            Messages.HUSTR_10,
            Messages.HUSTR_11,

            Messages.HUSTR_12,
            Messages.HUSTR_13,
            Messages.HUSTR_14,
            Messages.HUSTR_15,
            Messages.HUSTR_16,
            Messages.HUSTR_17,
            Messages.HUSTR_18,
            Messages.HUSTR_19,
            Messages.HUSTR_20,
            
            Messages.HUSTR_21,
            Messages.HUSTR_22,
            Messages.HUSTR_23,
            Messages.HUSTR_24,
            Messages.HUSTR_25,
            Messages.HUSTR_26,
            Messages.HUSTR_27,
            Messages.HUSTR_28,
            Messages.HUSTR_29,
            Messages.HUSTR_30,
            Messages.HUSTR_31,
            Messages.HUSTR_32
        });
    }

    public HudController()
    {
        // Load the heads-up font
        var j = (int)HuFontStart;
        for (var i = 0; i < HuFontSize; i++)
        {
            var lump = DoomGame.Instance.WadData.GetLumpName($"STCFN{j++:000}", PurgeTag.Cache)!;
            Font[i] = Patch.FromBytes(lump);
        }
    }

    public Patch[] Font { get; } = new Patch[HuFontSize];

    public bool HeadsUpActive { get; private set; }

    public void Stop()
    {
        HeadsUpActive = false;
    }

    public void Start()
    {
        var game = DoomGame.Instance.Game;

        if (HeadsUpActive)
        {
            Stop();
        }

        _player = game.Players[DoomGame.Instance.Game.ConsolePlayer];
        _messageOn = false;
        _messageDontFuckWithMe = false;
        _messageNotToBeFuckedWith = false;
        _chatOn = false;

        //// create the message widget
        //HUlib_initSText(&w_message,
        //    HU_MSGX, HU_MSGY, HU_MSGHEIGHT,
        //    hu_font,
        //    HU_FONTSTART, &message_on);

        //// create the map title widget
        //HUlib_initTextLine(&w_title,
        //    HU_TITLEX, HU_TITLEY,
        //    hu_font,
        //    HU_FONTSTART);

        var mapName = "";
        switch (DoomGame.Instance.GameMode)
        {
            case GameMode.Shareware:
            case GameMode.Registered:
            case GameMode.Retail:
                mapName = MapNames[(game.GameEpisode - 1) * 9 + game.GameMap - 1];
                break;
            case GameMode.Commercial:
            default:
                mapName = MapNames2[game.GameMap - 1]; ;
                break;
        }

        foreach (var c in mapName)
        {
            // HUlib_addCharToTextLine(&w_title, c);
        }

        //// create the chat widget
        //HUlib_initIText(&w_chat,
        //    HU_INPUTX, HU_INPUTY,
        //    hu_font,
        //    HU_FONTSTART, &chat_on);

        // create the inputbuffer widgets
        for (var i = 0; i < Constants.MaxPlayers; i++)
        {
            // HUlib_initIText(&w_inputbuffer[i], 0, 0, 0, 0, &always_off);
        }

        HeadsUpActive = true;
    }

    public void Drawer()
    {
        //HUlib_drawSText(&w_message);
        //HUlib_drawIText(&w_chat);
        if (DoomGame.Instance.Game.AutomapActive)
        {
            // HUlib_drawTextLine(&w_title, false);
        }

    }

    public void Erase()
    {
        //HUlib_eraseSText(&w_message);
        //HUlib_eraseIText(&w_chat);
        //HUlib_eraseTextLine(&w_title);
    }

    public void Ticker()
    {
        int rc;
        char c;

        var game = DoomGame.Instance.Game;

        // tick down message counter if message is up
        if (_messageCounter != 0 && --_messageCounter == 0)
        {
            _messageOn = false;
            _messageNotToBeFuckedWith = false;
        }

        if (DoomGame.Instance.Menu.ShowMessages || _messageDontFuckWithMe)
        {
            // display message if necessary
            if ((_player!.Message != null && !_messageNotToBeFuckedWith)
                || (_player.Message != null && _messageDontFuckWithMe))
            {
                //HUlib_addMessageToSText(&w_message, 0, plr->message);
                _player.Message = null;
                _messageOn = true;
                _messageCounter = MessageTimeout;
                _messageNotToBeFuckedWith = _messageDontFuckWithMe;
                _messageDontFuckWithMe = false;
            }

        } // else message_on = false;

        // check for incoming chat characters
        //if (game.NetGame)
        //{
        //    for (var i = 0; i < Constants.MaxPlayers; i++)
        //    {
        //        if (!game.PlayerInGame[i])
        //        {
        //            continue;
        //        }

        //        if (i != game.ConsolePlayer && (c = game.Players[i].Command.ChatChar) != 0)
        //        {
        //            if (c <= Broadcast)
        //            {
        //                chat_dest[i] = c;
        //            }
        //            else
        //            {
        //                if (c >= 'a' && c <= 'z')
        //                {
        //                    c = (char)shiftxform[(unsigned char) c];
        //                }
        //                rc = HUlib_keyInIText(&w_inputbuffer[i], c);
        //                if (rc != 0 && c == (int)Keys.Enter)
        //                {
        //                    if (w_inputbuffer[i].l.len
        //                        && (chat_dest[i] == consoleplayer + 1
        //                            || chat_dest[i] == HU_BROADCAST))
        //                    {
        //                        HUlib_addMessageToSText(&w_message,
        //                            player_names[i],
        //                            w_inputbuffer[i].l.l);

        //                        message_nottobefuckedwith = true;
        //                        message_on = true;
        //                        message_counter = HU_MSGTIMEOUT;
        //                        if (gamemode == commercial)
        //                            S_StartSound(0, sfx_radio);
        //                        else
        //                            S_StartSound(0, sfx_tink);
        //                    }
        //                    HUlib_resetIText(&w_inputbuffer[i]);
        //                }
        //            }
        //            game.Players[i].Command.ChatChar = (char)0;
        //        }
        //    }
        //}
    }

    public bool HandleEvent(InputEvent currentEvent)
    {
        return false;
    }
}