namespace DoomSharp.Core.Sound;

internal class NullSoundDriver : ISoundDriver
{
    public void SetChannels() { }

    public int RegisterSong(byte[] data) => 1;
    public void PlaySong(int handle, bool looping) { }
    public void PauseSong(int handle) { }
    public void ResumeSong(int handle) { }
    public void StopSong(int handle) { }
    public void UnregisterSong(int handle) { }

    public void SetMusicVolume(int volume) { }

    public bool SoundIsPlaying(int handle) => false;
    public int StartSound(SoundType soundType, byte[] data, int volume, int stereoSeparation, int pitch, int priority) => 1;
    public void UpdateSoundParams(int handle, int volume, int stereoSeparation, int pitch) { }
    public void StopSound(int handle) { }

    public void SubmitSound() { }
}