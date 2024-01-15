
using System.Collections.Generic;
using System;
using ManagedDoom;
using SFML.System;
using Time = System.TimeSpan;
namespace SFML.Audio
{
    public class Sound
    {
        public SoundStatus Status { get; internal set; }
        public SoundBuffer SoundBuffer { get; internal set; }
        public float Pitch { get; internal set; }
        public float Volume { get; internal set; }
        public Time PlayingOffset { get; internal set; }
        public Vector3f Position { get; internal set; }

        internal void Stop()
        {
            // TODO: implement
        }

        internal void Play()
        {
        }

        public override string ToString()
        {
            return $"{SoundBuffer.samples.Length} samples. Pitch: {Pitch}, Volume: {Volume}, Position: {Position}";
        }

        internal void Pause()
        {
            // TODO: implement
        }

        internal void Dispose()
        {
            // TODO: implement
        }
    }
}