/*
* PROJECT:          Aura Operating System Development
* CONTENT:          System sounds
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Cosmos.Kernel.System.Audio;

namespace Aura_OS.System.Audio
{
    /// <summary>
    /// System sounds, embedded under Resources/ as 48 kHz stereo 16-bit PCM .wav: the HD Audio driver
    /// plays only stereo 16-bit, and 48 kHz is the rate it programs at probe.
    /// </summary>
    public static class Sounds
    {
        public static void PlayBoot()
        {
            Play("lovelyboot.wav");
        }

        /// <summary>
        /// Plays an embedded .wav on a thread of its own and returns at once. No output (QEMU without
        /// -device intel-hda), an output already playing or an unreadable file is logged and skipped:
        /// a sound is never worth failing the caller.
        /// </summary>
        private static void Play(string relPath)
        {
            if (AudioManager.Primary == null)
            {
                CustomConsole.WriteLineWarning("No audio output, " + relPath + " not played.");
                return;
            }

            try
            {
                MemoryAudioStream stream = MemoryAudioStream.FromWave(Files.Get(relPath));

                // The playback thread holds the stream until it ends; nothing to keep here.
                if (AudioManager.TryStartPlayback(stream, null, out _))
                {
                    CustomConsole.WriteLineOK("Playing " + relPath + ".");
                }
                else
                {
                    CustomConsole.WriteLineWarning("Audio output busy or no scheduler, " + relPath + " not played.");
                }
            }
            catch (Exception ex)
            {
                CustomConsole.WriteLineError("Cannot play " + relPath + ": " + ex.Message);
            }
        }
    }
}
