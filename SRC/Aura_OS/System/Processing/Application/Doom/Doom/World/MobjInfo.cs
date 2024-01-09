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

namespace ManagedDoom
{
    public sealed class MobjInfo
    {
        private int doomEdNum;
        private MobjState spawnState;
        private int spawnHealth;
        private MobjState seeState;
        private int reactionTime;
        private MobjState painState;
        private int painChance;
        private MobjState meleeState;
        private MobjState missileState;
        private MobjState deathState;
        private MobjState xdeathState;
        private int speed;
        private Fixed radius;
        private Fixed height;
        private int mass;
        private int damage;
        private MobjFlags flags;
        private MobjState raiseState;

        public MobjInfo(
            int doomEdNum,
            MobjState spawnState,
            int spawnHealth,
            MobjState seeState,
            int reactionTime,
            MobjState painState,
            int painChance,
            MobjState meleeState,
            MobjState missileState,
            MobjState deathState,
            MobjState xdeathState,
            int speed,
            Fixed radius,
            Fixed height,
            int mass,
            int damage,
            MobjFlags flags,
            MobjState raiseState)
        {
            this.doomEdNum = doomEdNum;
            this.spawnState = spawnState;
            this.spawnHealth = spawnHealth;
            this.seeState = seeState;
            this.reactionTime = reactionTime;
            this.painState = painState;
            this.painChance = painChance;
            this.meleeState = meleeState;
            this.missileState = missileState;
            this.deathState = deathState;
            this.xdeathState = xdeathState;
            this.speed = speed;
            this.radius = radius;
            this.height = height;
            this.mass = mass;
            this.damage = damage;
            this.flags = flags;
            this.raiseState = raiseState;
        }

        public int DoomEdNum
        {
            get => doomEdNum;
            set => doomEdNum = value;
        }

        public MobjState SpawnState
        {
            get => spawnState;
            set => spawnState = value;
        }

        public int SpawnHealth
        {
            get => spawnHealth;
            set => spawnHealth = value;
        }

        public MobjState SeeState
        {
            get => seeState;
            set => seeState = value;
        }

        public int ReactionTime
        {
            get => reactionTime;
            set => reactionTime = value;
        }

        public MobjState PainState
        {
            get => painState;
            set => painState = value;
        }

        public int PainChance
        {
            get => painChance;
            set => painChance = value;
        }

        public MobjState MeleeState
        {
            get => meleeState;
            set => meleeState = value;
        }

        public MobjState MissileState
        {
            get => missileState;
            set => missileState = value;
        }

        public MobjState DeathState
        {
            get => deathState;
            set => deathState = value;
        }

        public MobjState XdeathState
        {
            get => xdeathState;
            set => xdeathState = value;
        }

        public int Speed
        {
            get => speed;
            set => speed = value;
        }

        public Fixed Radius
        {
            get => radius;
            set => radius = value;
        }

        public Fixed Height
        {
            get => height;
            set => height = value;
        }

        public int Mass
        {
            get => mass;
            set => mass = value;
        }

        public int Damage
        {
            get => damage;
            set => damage = value;
        }

        public MobjFlags Flags
        {
            get => flags;
            set => flags = value;
        }

        public MobjState Raisestate
        {
            get => raiseState;
            set => raiseState = value;
        }
    }
}
