using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DoomSharp.Core.Sound;
using FMOD;
using Channel = FMOD.Channel;

namespace DoomSharp.Core.Internals;

public class SoundDriver : ISoundDriver, IDisposable
{
    private FMOD.System _fmodSystem;
    private bool _fmodInitialized = false;

    private ChannelGroup _soundFxChannelGroup;
    private ChannelGroup _musicChannelGroup;

    private readonly Dictionary<SoundType, FMOD.Sound> _sounds = new();

    private readonly FMOD.Sound?[] _loadedMusic = new FMOD.Sound?[2];
    private FMOD.Sound? _currentMusic = null;
    private Channel? _currentMusicChannel = null;
    private float _musicVolume = 1f;

    private string? _soundBankPath;
    
    private readonly Dictionary<SoundType, int> _soundChannelMap = new();
    private readonly Dictionary<int, SoundType> _channelSoundMap = new();
    
    // The number of internal mixing channels,
    //  the samples calculated for each mixing step,
    //  the size of the 16bit, 2 hardware channel (stereo)
    //  mixing buffer, and the samplerate of the raw data.

    // Needed for calling the actual sound output.
    public const int SAMPLECOUNT = 512;
    public const int NUM_CHANNELS = 8;
    // It is 2 for 16bit, and 2 for two channels.
    public const int BUFMUL = 4;
    public const int MIXBUFFERSIZE = (SAMPLECOUNT * BUFMUL);

    public const int SAMPLERATE = 11025;	// Hz
    public const SOUND_FORMAT SoundFormat = SOUND_FORMAT.PCM8;   	// 8-bit PCM

    /// <summary>
    /// Constructor which takes a <paramref name="soundBankPath"/>
    /// </summary>
    /// <param name="soundBankPath">Path to a DLS file, used for MIDI playback.</param>
    public SoundDriver(string soundBankPath) : this()
    {
        _soundBankPath = soundBankPath;
    }

    public SoundDriver()
    {
        var result = Factory.System_Create(out var fmodSystem);
        if (result != RESULT.OK)
        {
            DoomGame.Error($"FMOD error while creating the FMOD system! ({result}) {Error.String(result)}");
        }
        else
        {
            _fmodSystem = fmodSystem;
            result = _fmodSystem.init(NUM_CHANNELS, INITFLAGS.NORMAL, IntPtr.Zero);
            if (result != RESULT.OK)
            {
                DoomGame.Error($"FMOD error while initializing the FMOD system! ({result}) {Error.String(result)}");
            }
            else
            {
                _fmodInitialized = true;
            }
        }
    }

    public void SetChannels()
    {
        _fmodSystem.createChannelGroup("SoundFX", out var channelGroup);
        _soundFxChannelGroup = channelGroup;

        _fmodSystem.createChannelGroup("Music", out var channelGroup2);
        _musicChannelGroup = channelGroup2;
    }

    public int RegisterSong(byte[] data)
    {
        var dlsName = _soundBankPath is null
            ? IntPtr.Zero
            : Marshal.StringToHGlobalAuto(_soundBankPath); // TODO Might need to use a more specific string to pointer method

        var soundInfo = new CREATESOUNDEXINFO
        {
            cbsize = MarshalHelper.SizeOf(typeof(CREATESOUNDEXINFO)),
            length = (uint)data.Length,
            suggestedsoundtype = SOUND_TYPE.MIDI,
            dlsname = dlsName
        };

        var result = _fmodSystem.createSound(data, MODE.CREATESAMPLE | MODE.OPENMEMORY | MODE.LOOP_NORMAL, ref soundInfo, out var music);
        if (result != RESULT.OK)
        {
            DoomGame.Console.WriteLine($"RegisterSong failed: {Error.String(result)}");
        }
        
        return AddMusicToLoadedList(music);
    }

    private int AddMusicToLoadedList(FMOD.Sound music)
    {
        for (var i = 0; i < _loadedMusic.Length; i++)
        {
            if (_loadedMusic[i] != null)
            {
                continue;
            }
            
            _loadedMusic[i] = music;
            return i;
        }

        // You shouldn't get here
        DoomGame.Error("No more music slots available!");
        return -1;
    }

    public void PlaySong(int handle, bool looping)
    {
        var music = GetCurrentMusic(handle);
        if (music is null)
        {
            return;
        }

        music.Value.setLoopCount(looping ? -1 : 0);
        var result = _fmodSystem.playSound(music.Value, _musicChannelGroup, false, out var channel);
        _currentMusicChannel = channel;
        channel.setVolume(_musicVolume);
    }

    public void PauseSong(int handle)
    {
        _currentMusicChannel?.setPaused(true);
    }

    public void ResumeSong(int handle)
    {
        _currentMusicChannel?.setPaused(false);
    }

    public void StopSong(int handle)
    {
        _currentMusicChannel?.stop();
    }

    public void UnregisterSong(int handle)
    {
        var music = GetCurrentMusic(handle);
        music?.release();
        
        _loadedMusic[handle] = null;
    }
    
    public void SetMusicVolume(int volume)
    {
        _musicVolume = volume < 0 ? 1 : (volume / 128f);
        _currentMusicChannel?.setVolume(_musicVolume);
    }

    private FMOD.Sound? GetCurrentMusic(int handle)
    {
        if (handle < 0 || handle >= _loadedMusic.Length)
        {
            return null;
        }

        return _loadedMusic[handle];
    }

    public bool SoundIsPlaying(int handle)
    {
        if (handle < 0)
        {
            return false;
        }

        var channel = GetChannel(handle);
        channel.isPlaying(out var isPlaying);
        
        return isPlaying;
    }

    public int StartSound(SoundType soundType, byte[] data, int volume, int stereoSeparation, int pitch, int priority)
    {
        // play these sound effects only one at a time
        if (soundType is SoundType.sfx_sawup or SoundType.sfx_sawidl or SoundType.sfx_sawful or SoundType.sfx_sawhit or SoundType.sfx_stnmov or SoundType.sfx_pistol)
        {
            if (_soundChannelMap.ContainsKey(soundType))
            {
                return _soundChannelMap[soundType];
            }
        }

        FMOD.Sound sound;
        if (_sounds.ContainsKey(soundType))
        {
            sound = _sounds[soundType];
        }
        else
        {
            using var ms = new MemoryStream(data, false);
            using var reader = new BinaryReader(ms);

            var _ = reader.ReadUInt16();
            var sampleRate = reader.ReadUInt16();
            var sampleCount = reader.ReadUInt32();

            // Read the sample including the pre/post 16 byte padding
            var sampleData = reader.ReadBytes((int)sampleCount);

            var soundInfo = new CREATESOUNDEXINFO
            {
                cbsize = MarshalHelper.SizeOf(typeof(CREATESOUNDEXINFO)),
                format = SoundFormat,
                numchannels = 1,
                defaultfrequency = sampleRate,
                length = (uint)sampleData.Length
            };

            _fmodSystem.createSound(sampleData, MODE.OPENRAW | MODE.CREATESAMPLE | MODE.OPENMEMORY, ref soundInfo, out sound);
            _sounds.Add(soundType, sound);
        }

        _fmodSystem.playSound(sound, _soundFxChannelGroup, false, out var channel);

        channel.getIndex(out var handle);
        // _soundChannelMap.Add(soundType, handle);
        // _channelSoundMap.Add(handle, soundType);

        channel.setPriority(priority);
        UpdateSoundParams(handle, volume, stereoSeparation, pitch);
        
        return handle;
    }

    public void UpdateSoundParams(int handle, int volume, int stereoSeparation, int pitch)
    {
        if (handle < 0)
        {
            return;
        }

        var channel = GetChannel(handle);
        channel.setPitch(pitch < 0 ? 1f : pitch / 128f);
        channel.setPan((stereoSeparation - 128f) / 128f);
        channel.setVolume(volume < 0 ? 1f : volume / 128f);
    }

    public void StopSound(int handle)
    {
        if (handle < 0)
        {
            return;
        }

        var channel = GetChannel(handle);
        channel.stop();

        if (_channelSoundMap.TryGetValue(handle, out var soundType))
        {
            _channelSoundMap.Remove(handle);
            _soundChannelMap.Remove(soundType);
        }
    }

    private Channel GetChannel(int handle)
    {
        _fmodSystem.getChannel(handle, out var channel);
        return channel;
    }

    public void SubmitSound()
    {
        _fmodSystem.update();
    }

    public void Dispose()
    {
        if (!_fmodInitialized)
        {
            return;
        }

        foreach (var sound in _sounds)
        {
            sound.Value.release();
        }
        _sounds.Clear();

        _fmodInitialized = false;
        
        _fmodSystem.release();
    }
}