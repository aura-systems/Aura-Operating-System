
using System;
using SFML.System;

using Time = System.TimeSpan;

namespace SFML.Audio
{
    public class SoundBuffer
    {
        public short[] samples;
        private int v;
        public uint sampleRate;

        public SoundBuffer(short[] samples, int v, uint sampleRate)
        {
            this.samples = samples;
            this.v = v;
            this.sampleRate = sampleRate;
        }

        public Time Duration { get; internal set; }

        internal void Dispose()
        {
            // TODO: implement
        }
    }
}