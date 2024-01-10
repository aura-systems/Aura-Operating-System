namespace DoomSharp.Core.Sound;

public interface ISoundDriver
{
    void SetChannels();

    int RegisterSong(byte[] data);
    void PlaySong(int handle, bool looping);
    void PauseSong(int handle);
    void ResumeSong(int handle);
    void StopSong(int handle);
    void UnregisterSong(int handle);

    void SetMusicVolume(int volume);
    
    bool SoundIsPlaying(int handle);
    int StartSound(SoundType soundType, byte[] data, int volume, int stereoSeparation, int pitch, int priority);
    void UpdateSoundParams(int handle, int volume, int stereoSeparation, int pitch);
    void StopSound(int handle);

    void SubmitSound();
}