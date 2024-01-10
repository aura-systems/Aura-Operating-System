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
using System.IO;
using ManagedDoom.Audio;
using SFML.Window;

namespace ManagedDoom
{
    public static class ConfigUtilities
    {

        static string[] names = new string[]
            {
                "DOOM2.WAD",
                "PLUTONIA.WAD",
                "TNT.WAD",
                "DOOM.WAD",
                "DOOM1.WAD"
            };

        public static SfmlMusic GetSfmlMusicInstance(Config config, Wad wad)
        {
            return null;
            // TODO: load music
            /*var sfPath = Path.Combine(GetExeDirectory(), "TimGM6mb.sf2");
            if (File.Exists(sfPath))
            {
                return new SfmlMusic(config, wad, sfPath);
            }
            else
            {
                return null;
            }*/
        }
    }
}
