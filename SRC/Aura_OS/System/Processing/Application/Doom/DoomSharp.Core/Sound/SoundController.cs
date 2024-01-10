using DoomSharp.Core.Data;
using DoomSharp.Core.GameLogic;
using System;
using System.Linq;

namespace DoomSharp.Core.Sound;

public class SoundController
{
	private ISoundDriver _driver;

    // the set of channels available
    private int _numChannels = 8;
    private SoundChannel[] _channels = Array.Empty<SoundChannel>();

    // These are not used, but should be (menu).
    // Maximum volume of a sound effect.
    // Internal default is max out of 0-15.
    private int _sfxVolume = 15;
    
    private int _musicVolume = 15;

	// whether songs are mus_paused
	private bool _musicPaused = false;

	// music currently being played
	private MusicInfo? _musicPlaying = null;

	private int _nextCleanup;

	// when to clip out sounds
	// Does not fit the large outdoor areas.
	public static readonly Fixed S_CLIPPING_DIST = new Fixed(1200 * 0x10000);

	// Distance tp origin when sounds should be maxed out.
	// This should relate to movement clipping resolution
	// (see BLOCKMAP handling).
	// Originally: (200*0x10000).
	public static readonly Fixed S_CLOSE_DIST = new Fixed(160 * 0x10000);

	public static readonly int S_ATTENUATOR = ((S_CLIPPING_DIST - S_CLOSE_DIST).Value >> Constants.FracBits);

	// Adjustable by menu.
	public const int NormalVolume = 127; // TODO snd_MaxVolume; ?

	public const int NormalPitch = 128;
	public const int NormalPriority = 64;
	public const int NormalStereoSeparation = 128;

	public const int S_PITCH_PERTURB = 1;
	public static readonly Fixed S_STEREO_SWING = new Fixed(96 * 0x10000);

	// percent attenuation from front to back
	public const int S_IFRACVOL = 30;

	public const int NA = 0;
	public const int S_NUMCHANNELS = 2;

	private static readonly SfxInfo PistolSfx = new("pistol", false, 64, null, -1, -1);

	//
	// Information about all the sfx
	//
	private static readonly SfxInfo[] Sfx = 
	{
		// S_sfx[0] needs to be a dummy for odd reasons.
		new("none", false,  0, null, -1, -1),
        PistolSfx,
		new("shotgn", false, 64, null, -1, -1),
		new("sgcock", false, 64, null, -1, -1),
		new("dshtgn", false, 64, null, -1, -1),
		new("dbopn", false, 64, null, -1, -1),
		new("dbcls", false, 64, null, -1, -1),
		new("dbload", false, 64, null, -1, -1),
		new("plasma", false, 64, null, -1, -1),
		new("bfg", false, 64, null, -1, -1),
		new("sawup", false, 64, null, -1, -1),
		new("sawidl", false, 118, null, -1, -1),
		new("sawful", false, 64, null, -1, -1),
		new("sawhit", false, 64, null, -1, -1),
		new("rlaunc", false, 64, null, -1, -1),
		new("rxplod", false, 70, null, -1, -1),
		new("firsht", false, 70, null, -1, -1),
		new("firxpl", false, 70, null, -1, -1),
		new("pstart", false, 100, null, -1, -1),
		new("pstop", false, 100, null, -1, -1),
		new("doropn", false, 100, null, -1, -1),
		new("dorcls", false, 100, null, -1, -1),
		new("stnmov", false, 119, null, -1, -1),
		new("swtchn", false, 78, null, -1, -1),
		new("swtchx", false, 78, null, -1, -1),
		new("plpain", false, 96, null, -1, -1),
		new("dmpain", false, 96, null, -1, -1),
		new("popain", false, 96, null, -1, -1),
		new("vipain", false, 96, null, -1, -1),
		new("mnpain", false, 96, null, -1, -1),
		new("pepain", false, 96, null, -1, -1),
		new("slop", false, 78, null, -1, -1),
		new("itemup", true, 78, null, -1, -1),
		new("wpnup", true, 78, null, -1, -1),
		new("oof", false, 96, null, -1, -1),
		new("telept", false, 32, null, -1, -1),
		new("posit1", true, 98, null, -1, -1),
		new("posit2", true, 98, null, -1, -1),
		new("posit3", true, 98, null, -1, -1),
		new("bgsit1", true, 98, null, -1, -1),
		new("bgsit2", true, 98, null, -1, -1),
		new("sgtsit", true, 98, null, -1, -1),
		new("cacsit", true, 98, null, -1, -1),
		new("brssit", true, 94, null, -1, -1),
		new("cybsit", true, 92, null, -1, -1),
		new("spisit", true, 90, null, -1, -1),
		new("bspsit", true, 90, null, -1, -1),
		new("kntsit", true, 90, null, -1, -1),
		new("vilsit", true, 90, null, -1, -1),
		new("mansit", true, 90, null, -1, -1),
		new("pesit", true, 90, null, -1, -1),
		new("sklatk", false, 70, null, -1, -1),
		new("sgtatk", false, 70, null, -1, -1),
		new("skepch", false, 70, null, -1, -1),
		new("vilatk", false, 70, null, -1, -1),
		new("claw", false, 70, null, -1, -1),
		new("skeswg", false, 70, null, -1, -1),
		new("pldeth", false, 32, null, -1, -1),
		new("pdiehi", false, 32, null, -1, -1),
		new("podth1", false, 70, null, -1, -1),
		new("podth2", false, 70, null, -1, -1),
		new("podth3", false, 70, null, -1, -1),
		new("bgdth1", false, 70, null, -1, -1),
		new("bgdth2", false, 70, null, -1, -1),
		new("sgtdth", false, 70, null, -1, -1),
		new("cacdth", false, 70, null, -1, -1),
		new("skldth", false, 70, null, -1, -1),
		new("brsdth", false, 32, null, -1, -1),
		new("cybdth", false, 32, null, -1, -1),
		new("spidth", false, 32, null, -1, -1),
		new("bspdth", false, 32, null, -1, -1),
		new("vildth", false, 32, null, -1, -1),
		new("kntdth", false, 32, null, -1, -1),
		new("pedth", false, 32, null, -1, -1),
		new("skedth", false, 32, null, -1, -1),
		new("posact", true, 120, null, -1, -1),
		new("bgact", true, 120, null, -1, -1),
		new("dmact", true, 120, null, -1, -1),
		new("bspact", true, 100, null, -1, -1),
		new("bspwlk", true, 100, null, -1, -1),
		new("vilact", true, 100, null, -1, -1),
		new("noway", false, 78, null, -1, -1),
		new("barexp", false, 60, null, -1, -1),
		new("punch", false, 64, null, -1, -1),
		new("hoof", false, 70, null, -1, -1),
		new("metal", false, 70, null, -1, -1),
		new("chgun", false, 64, PistolSfx, 150, 0),
		new("tink", false, 60, null, -1, -1),
		new("bdopn", false, 100, null, -1, -1),
		new("bdcls", false, 100, null, -1, -1),
		new("itmbk", false, 100, null, -1, -1),
		new("flame", false, 32, null, -1, -1),
		new("flamst", false, 32, null, -1, -1),
		new("getpow", false, 60, null, -1, -1),
		new("bospit", false, 70, null, -1, -1),
		new("boscub", false, 70, null, -1, -1),
		new("bossit", false, 70, null, -1, -1),
		new("bospn", false, 70, null, -1, -1),
		new("bosdth", false, 70, null, -1, -1),
		new("manatk", false, 70, null, -1, -1),
		new("mandth", false, 70, null, -1, -1),
		new("sssit", false, 70, null, -1, -1),
		new("ssdth", false, 70, null, -1, -1),
		new("keenpn", false, 70, null, -1, -1),
		new("keendt", false, 70, null, -1, -1),
		new("skeact", false, 70, null, -1, -1),
		new("skesit", false, 70, null, -1, -1),
		new("skeatk", false, 70, null, -1, -1),
		new("radio", false, 60, null, -1, -1)
    };

    //
    // Information about all the music
    //
    private static readonly MusicInfo[] Music = 
	{
        new(),
		new("e1m1"),
		new("e1m2"),
		new("e1m3"),
		new("e1m4"),
		new("e1m5"),
		new("e1m6"),
		new("e1m7"),
		new("e1m8"),
		new("e1m9"),
		new("e2m1"),
		new("e2m2"),
		new("e2m3"),
		new("e2m4"),
		new("e2m5"),
		new("e2m6"),
		new("e2m7"),
		new("e2m8"),
		new("e2m9"),
		new("e3m1"),
		new("e3m2"),
		new("e3m3"),
		new("e3m4"),
		new("e3m5"),
		new("e3m6"),
		new("e3m7"),
		new("e3m8"),
		new("e3m9"),
		new("inter"),
		new("intro"),
		new("bunny"),
		new("victor"),
		new("introa"),
		new("runnin"),
		new("stalks"),
		new("countd"),
		new("betwee"),
		new("doom"),
		new("the_da"),
		new("shawn"),
		new("ddtblu"),
		new("in_cit"),
		new("dead"),
		new("stlks2"),
		new("theda2"),
		new("doom2"),
		new("ddtbl2"),
		new("runni2"),
		new("dead2"),
		new("stlks3"),
		new("romero"),
		new("shawn2"),
		new("messag"),
		new("count2"),
		new("ddtbl3"),
		new("ampie"),
		new("theda3"),
		new("adrian"),
		new("messg2"),
		new("romer2"),
		new("tense"),
		new("shawn3"),
		new("openin"),
		new("evil"),
		new("ultima"),
		new("read_m"),
		new("dm2ttl"),
		new("dm2int")
    };

	public SoundController(ISoundDriver driver)
	{
		_driver = driver;
	}

	public void SetDriver(ISoundDriver driver)
    {
		_driver = driver;
    }

	/// <summary>
	/// Initializes sound stuff, including volume.
	/// Sets channels, SFX and music volume,
	/// allocates channel buffer, sets S_sfx lookup.
	/// </summary>
	public void Initialize(int sfxVolume, int musicVolume)
	{
		DoomGame.Console.WriteLine($"S_Init: default sfx volume {sfxVolume}");

		// Whatever these did with DMX, these are rather dummies now.
		_driver.SetChannels();

		SetSfxVolume(sfxVolume);
		SetMusicVolume(musicVolume);

		// Allocating the internal channels for mixing
		// (the maximum number of sounds rendered
		// simultaneously) within zone memory.
		_channels = new SoundChannel[_numChannels];

		// Free all channels for user
        for (var i = 0; i < _channels.Length; i++)
        {
            _channels[i] = new SoundChannel
            {
                SfxInfo = null
            };
        }

		// No sounds are playing, and they are not mus_paused
		_musicPaused = false;

		// Note that sounds have not been cached (yet).
		for (var i = 0; i < (int)SoundType.NUMSFX; i++)
		{
			Sfx[i].LumpNum = Sfx[i].Usefulness = -1;
        }
	}

	/// <summary>
	/// Per-level startup code.
	/// Kills playing sounds at start of level, determines music if any, changes music.
	/// </summary>
    public void Start()
	{
		// kill all playing sounds at start of level
		//  (trust me - a good idea)
		foreach (var channel in _channels.Where(x => x.SfxInfo != null))
		{
			StopChannel(channel);
		}

		// start new music for the level
		_musicPaused = false;
		var musicNum = MusicType.mus_None;
		if (DoomGame.Instance.GameMode == GameMode.Commercial)
		{
			musicNum = MusicType.mus_runnin + DoomGame.Instance.Game.GameMap - 1;
		}
		else
		{
			MusicType[] spmus =
			{
				// Song - Who? - Where?
      
				MusicType.mus_e3m4,	// American	e4m1
				MusicType.mus_e3m2,	// Romero	e4m2
				MusicType.mus_e3m3,	// Shawn	e4m3
				MusicType.mus_e1m5,	// American	e4m4
				MusicType.mus_e2m7,	// Tim 	e4m5
				MusicType.mus_e2m4,	// Romero	e4m6
				MusicType.mus_e2m6,	// J.Anderson	e4m7 CHIRON.WAD
				MusicType.mus_e2m5,	// Shawn	e4m8
				MusicType.mus_e1m9	// Tim		e4m9
			};

			if (DoomGame.Instance.Game.GameEpisode < 4)
			{
				musicNum = MusicType.mus_e1m1 + (DoomGame.Instance.Game.GameEpisode - 1) * 9 + DoomGame.Instance.Game.GameMap - 1;
			}
			else
			{
				musicNum = spmus[DoomGame.Instance.Game.GameMap - 1];
			}
		}

		// HACK FOR COMMERCIAL
		//  if (commercial && mnum > mus_e3m9)	
		//      mnum -= mus_e3m9;

		ChangeMusic(musicNum, true);

		_nextCleanup = 15;
    }

    public void StartSound(MapObject? origin, SoundType soundType)
	{
		StartSoundAtVolume(origin, soundType, _sfxVolume);
	}

	public void StartSoundAtVolume(MapObject? origin, SoundType soundType, int volume)
	{
		if (!Enum.IsDefined(soundType))
		{
			DoomGame.Error($"Bad sfx #: {soundType}");
			return;
		}

		var sfx = Sfx[(int)soundType];

		// Initialize sound parameters
		int pitch, priority;
		if (sfx.Link != null)
		{
			pitch = sfx.Pitch;
			priority = sfx.Priority;
			volume += sfx.Volume;

			if (volume < 1)
			{
				return;
			}

			if (volume > _sfxVolume)
			{
				volume = _sfxVolume;
            }
		}
		else
		{
			pitch = NormalPitch;
			priority = NormalPriority;
		}

		// Check to see if it is audible,
		//  and if not, modify the params
		int sep = 0;
		var playerOrigin = DoomGame.Instance.Game.Players[DoomGame.Instance.Game.ConsolePlayer].MapObject;
        if (origin != null && origin != playerOrigin)
		{
			var rc = AdjustSoundParams(playerOrigin!, origin, ref volume, ref sep);
			
			if (origin.X == playerOrigin!.X && origin.Y == playerOrigin.Y)
			{
				sep = NormalStereoSeparation;
			}

			if (!rc)
			{
				return;
			}
		}
		else
		{
            sep = NormalStereoSeparation;
        }

        // hacks to vary the sfx pitches
        if (soundType >= SoundType.sfx_sawup && soundType <= SoundType.sfx_sawhit)
        {
            pitch += 8 - (DoomRandom.M_Random() & 15);

			if (pitch < 0)
			{
				pitch = 0;
			}
			else if (pitch > 255)
			{
				pitch = 255;
			}
        }
        else if (soundType != SoundType.sfx_itemup && soundType != SoundType.sfx_tink)
        {
            pitch += 16 - (DoomRandom.M_Random() & 31);

			if (pitch < 0)
			{
				pitch = 0;
			}
			else if (pitch > 255)
			{
				pitch = 255;
			}
        }

        // kill old sound
        StopSound(origin);

		// try to find a channel
        var channel = GetChannel(origin, sfx);

		if (channel == null)
		{
			return;
		}

		//
		// This is supposed to handle the loading/caching.
		// For some odd reason, the caching is done nearly
		//  each time the sound is needed?
		//

		// get lumpnum if necessary
		if (sfx.LumpNum < 0)
		{
			sfx.LumpNum = DoomGame.Instance.WadData.GetNumForName($"DS{sfx.Name}");
            if (sfx.LumpNum == -1)
            {
                sfx.LumpNum = DoomGame.Instance.WadData.GetNumForName("DSPISTOL"); // fix for loading sounds that don't exist
            }
			sfx.Data = DoomGame.Instance.WadData.GetLumpNum(sfx.LumpNum, PurgeTag.Sound)!;
        }

		// increase the usefulness
		if (sfx.Usefulness++ < 0)
		{
			sfx.Usefulness = 1;
		}

        // Assigns the handle to one of the channels in the
        //  mix/output buffer.
        channel.Handle = _driver.StartSound(soundType, sfx.Data, volume, sep, pitch, priority);
    }

    //
    // Stop and resume music, during game PAUSE.
    //
    public void StopSound(MapObject? origin)
	{
		foreach (var channel in _channels.Where(x => x.SfxInfo != null && x.Origin == origin))
		{
			StopChannel(channel);
		}
	}

	public void PauseSound()
	{
		if (_musicPlaying != null && !_musicPaused)
		{
			_driver.PauseSong(_musicPlaying.Handle);
			_musicPaused = true;
		}
	}

	public void ResumeSound()
	{
        if (_musicPlaying != null && _musicPaused)
        {
            _driver.ResumeSong(_musicPlaying.Handle);
            _musicPaused = false;
        }
    }

	/// <summary>
	/// Updates music & sounds
	/// </summary>
    public void UpdateSounds(MapObject listener)
    {
        foreach (var channel in _channels.Where(x => x.SfxInfo != null))
        {
            var sfx = channel.SfxInfo!;
            if (_driver.SoundIsPlaying(channel.Handle))
            {
				// initialize parameters
                var volume = _sfxVolume;
                var pitch = NormalPitch;
                var sep = NormalStereoSeparation;

                if (sfx.Link != null)
                {
                    pitch = sfx.Pitch;
                    volume += sfx.Volume;
                    if (volume < 1)
                    {
						StopChannel(channel);
						continue;
                    }
					
                    if (volume > _sfxVolume)
                    {
                        volume = _sfxVolume;
                    }
                }

				// check non-local sounds for distance clipping
				// or modify their params
                if (channel.Origin != null && listener != channel.Origin)
                {
                    var audible = AdjustSoundParams(listener, channel.Origin, ref volume, ref sep);
                    if (!audible)
                    {
						StopChannel(channel);
                    }
                    else
                    {
                        _driver.UpdateSoundParams(channel.Handle, volume, sep, pitch);
                    }
                }
            }
            else
            {
				// if channel is allocated but sound has stopped, free it
				StopChannel(channel);
            }
        }
    }

    public void SetMusicVolume(int volume)
    {
        if (volume is < 0 or > 127)
        {
            DoomGame.Error($"Attempt to set music volume at {volume}");
            return;
        }

        _driver.SetMusicVolume(volume);
        _musicVolume = volume;
    }

    public void SetSfxVolume(int volume)
    {
        if (volume is < 0 or > 127)
        {
            DoomGame.Error($"Attempt to set sfx volume at {volume}");
            return;
        }

        _sfxVolume = volume;
    }

	/// <summary>
	/// Starts some music with music id found in sounds.h
	/// </summary>
	public void StartMusic(MusicType musicType)
	{
		ChangeMusic(musicType, false);
	}

	public void ChangeMusic(MusicType musicType, bool looping)
	{
		if (!Enum.IsDefined(musicType))
		{
			DoomGame.Error($"Bad music number {musicType}");
			return;
		}

		var music = Music[(int)musicType];
		if (_musicPlaying == music)
		{
			return;
		}

		// shutdown old music
		StopMusic();

		// get lumpnum if necessary
		if (music.LumpNum == 0)
		{
			music.LumpNum = DoomGame.Instance.WadData.GetNumForName($"d_{music.Name}");
		}

		// load & register it
		var musicLumpData = DoomGame.Instance.WadData.GetLumpNum(music.LumpNum, Data.PurgeTag.Music)!;
        music.Data = Mus2MidiConverter.TryConvert(musicLumpData, out var convertedData) ? convertedData : musicLumpData;
        music.Handle = _driver.RegisterSong(music.Data);

		// play it
		_driver.PlaySong(music.Handle, looping);

		_musicPlaying = music;
	}

	public void StopMusic()
	{
		if (_musicPlaying != null)
		{
			if (_musicPaused)
			{
				_driver.ResumeSong(_musicPlaying.Handle);
			}

			_driver.StopSong(_musicPlaying.Handle);
			_driver.UnregisterSong(_musicPlaying.Handle);
			// Z_ChangeTag(_musicPlaying.Data, PU_CACHE);

			Array.Clear(_musicPlaying.Data);
            _musicPlaying = null;
		}
	}

    public void StopChannel(SoundChannel channel)
    {
		if (channel.SfxInfo != null)
		{
			// stop the sound playing
			if (_driver.SoundIsPlaying(channel.Handle))
			{
				_driver.StopSound(channel.Handle);
			}

			// check to see if other channels are playing the sound
			foreach (var c in _channels.Where(x => x != channel))
			{
				if (c.SfxInfo == channel.SfxInfo)
				{
					break;
				}
			}

			// degrade the usefulness of the sound data
			channel.SfxInfo.Usefulness--;
			channel.SfxInfo = null;
		}
    }

    public void Submit()
    {
		_driver.SubmitSound();
    }

	/// <summary>
	/// Changes volume and stereo-separation variables from the norm of a sound effect to be played.
	/// If the sound is not audible, returns false. Otherwise, modifies parameters and returns true.
	/// </summary>
	private bool AdjustSoundParams(MapObject listener, MapObject source, ref int vol, ref int sep)
	{
		// calculate the distance to sound origin and clip it if necessary
        var adx = Fixed.Abs(listener.X - source.X);
        var ady = Fixed.Abs(listener.Y - source.Y);

		// From _GG1_ p.428. Appox. eucledian distance fast.
		var approxDist = adx + ady - ((adx < ady ? adx : ady) >> 1);

		if (DoomGame.Instance.Game.GameMap != 8 && approxDist > S_CLIPPING_DIST)
		{
			return false;
		}

		// angle of source to listener
        var angle = DoomGame.Instance.Renderer.PointToAngle2(listener.X, listener.Y, source.X, source.Y);
		
		if (angle > listener.Angle)
		{
			angle -= listener.Angle;
		}
		else
		{
			angle += new Angle(0xffffffff - listener.Angle.Value);
		}

		// stereo separation
		sep = 128 - ((S_STEREO_SWING * DoomMath.Sin(angle)).Value >> Constants.FracBits);

		// volume calculation
		if (approxDist < S_CLOSE_DIST)
		{
			vol = _sfxVolume;
		}
		else if (DoomGame.Instance.Game.GameMap == 8)
		{
			if (approxDist > S_CLIPPING_DIST)
			{
				approxDist = S_CLIPPING_DIST;
			}

            vol = 15 + ((_sfxVolume - 15)
                    * ((S_CLIPPING_DIST - approxDist).Value >> Constants.FracBits)) / S_ATTENUATOR;
        }
		else
		{
			// distance effect
			vol = (_sfxVolume
				* ((S_CLIPPING_DIST - approxDist).Value >> Constants.FracBits)) / S_ATTENUATOR;
		}

		return vol > 0;
    }

    /// <summary>
    /// If none available, return <c>null</c>. Otherwhise, return a channel.
    /// </summary>
    private SoundChannel? GetChannel(MapObject? origin, SfxInfo sfxInfo)
	{
		// Find an open channel
		var cnum = 0;

		for (cnum = 0; cnum < _numChannels; cnum++)
		{
			if (_channels[cnum].SfxInfo == null)
			{
				break;
			}
			else if (origin != null && _channels[cnum].Origin == origin)
			{
				StopChannel(_channels[cnum]);
				break;
			}
		}

		// None available?
		if (cnum >= _numChannels)
		{
			// Look for lower priority
			for (cnum = 0; cnum < _numChannels; cnum++)
			{
				if (_channels[cnum].SfxInfo!.Priority >= sfxInfo.Priority)
				{
					break;
				}
			}

            if (cnum == _numChannels)
            {
                // FUCK!  No lower priority.  Sorry, Charlie.    
                return null;
            }
            else
            {
                // Otherwise, kick out lower priority.
                StopChannel(_channels[cnum]);
            }
        }

		var c = _channels[cnum];

		// Channel is decided to be cnum
		c.SfxInfo = sfxInfo;
		c.Origin = origin;

		return c;
	}
}