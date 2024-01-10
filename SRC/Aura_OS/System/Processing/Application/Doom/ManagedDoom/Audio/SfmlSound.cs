//
// Copyright (C) 1993-1996 Id Software, Inc.
// Copyright (C) 2019-2020 Nobuaki Tanaka
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//



using System;
using System.Runtime.ExceptionServices;
using SFML.Audio;
using SFML.System;

using Time = System.TimeSpan;

namespace ManagedDoom.Audio
{
    public sealed class SfmlSound : ISound, IDisposable
    {
        private static readonly int channelCount = 8;

        private static readonly float fastDecay = (float)Math.Pow(0.5, 1.0 / (35 / 5));
        private static readonly float slowDecay = (float)Math.Pow(0.5, 1.0 / 35);

        private static readonly float clipDist = 1200;
        private static readonly float closeDist = 160;
        private static readonly float attenuator = clipDist - closeDist;

        private Config config;

        private SoundBuffer[] buffers;
        private float[] amplitudes;

        private DoomRandom random;

        private Sound[] channels;
        private ChannelInfo[] infos;

        private Sound uiChannel;
        private Sfx uiReserved;

        private Mobj listener;

        private float masterVolumeDecay;

        private DateTime lastUpdate;

        public SfmlSound(Config config, Wad wad)
        {
            
        }

        private static short[] GetSamples(Wad wad, string name, out int sampleRate, out int sampleCount)
        {
            sampleRate = 0;
            sampleCount = 0;
            return null;
        }

        // Check if the data contains pad bytes.
        // If the first and last 16 samples are the same,
        // the data should contain pad bytes.
        // https://doomwiki.org/wiki/Sound
        private static bool ContainsDmxPadding(byte[] data)
        {
            return false;
        }

        private static float GetAmplitude(short[] samples, int sampleRate, int sampleCount)
        {
            return 0;
        }

        public void SetListener(Mobj listener)
        {
        }

        public void Update()
        {
        }

        public void StartSound(Sfx sfx)
        {
        }

        public void StartSound(Mobj mobj, Sfx sfx, SfxType type)
        {
        }

        public void StartSound(Mobj mobj, Sfx sfx, SfxType type, int volume)
        {
        }

        public void StopSound(Mobj mobj)
        {
        }

        public void Reset()
        {
        }

        public void Pause()
        {
        }

        public void Resume()
        {
        }

        private void SetParam(Sound sound, ChannelInfo info)
        {
        }

        private float GetDistanceDecay(float dist)
        {
            return 0;
        }

        private float GetPitch(SfxType type, Sfx sfx)
        {
            return 0;
        }

        public void Dispose()
        {
        }

        public int MaxVolume
        {
            get
            {
                return 15;
            }
        }

        public int Volume
        {
            get
            {
                return 0;
            }

            set
            {
            }
        }



        private class ChannelInfo
        {
            public Sfx Reserved;
            public Sfx Playing;
            public float Priority;

            public Mobj Source;
            public SfxType Type;
            public int Volume;
            public Fixed LastX;
            public Fixed LastY;

            public void Clear()
            {
                Reserved = Sfx.NONE;
                Playing = Sfx.NONE;
                Priority = 0;
                Source = null;
                Type = 0;
                Volume = 0;
                LastX = Fixed.Zero;
                LastY = Fixed.Zero;
            }
        }
    }
}
