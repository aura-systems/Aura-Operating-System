﻿//
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
    public static partial class DoomInfo
    {
        public static class Strings
        {
            public static readonly DoomString PRESSKEY = new DoomString("press a key.");
            public static readonly DoomString PRESSYN = new DoomString("press y or n.");
            public static readonly DoomString QUITMSG = new DoomString("are you sure you want to\nquit this great game?");
            public static readonly DoomString LOADNET = new DoomString("you can't do load while in a net game!\n\n" + PRESSKEY);
            public static readonly DoomString QLOADNET = new DoomString("you can't quickload during a netgame!\n\n" + PRESSKEY);
            public static readonly DoomString QSAVESPOT = new DoomString("you haven't picked a quicksave slot yet!\n\n" + PRESSKEY);
            public static readonly DoomString SAVEDEAD = new DoomString("you can't save if you aren't playing!\n\n" + PRESSKEY);
            public static readonly DoomString QSPROMPT = new DoomString("quicksave over your game named\n\n'%s'?\n\n" + PRESSYN);
            public static readonly DoomString QLPROMPT = new DoomString("do you want to quickload the game named\n\n'%s'?\n\n" + PRESSYN);

            public static readonly DoomString NEWGAME = new DoomString(
                "you can't start a new game\n" +
                "while in a network game.\n\n" + PRESSKEY);

            public static readonly DoomString NIGHTMARE = new DoomString(
                "are you sure? this skill level\n" +
                "isn't even remotely fair.\n\n" + PRESSYN);

            public static readonly DoomString SWSTRING = new DoomString(
                "this is the shareware version of doom.\n\n" +
                "you need to order the entire trilogy.\n\n" + PRESSKEY);

            public static readonly DoomString MSGOFF = new DoomString("Messages OFF");
            public static readonly DoomString MSGON = new DoomString("Messages ON");
            public static readonly DoomString NETEND = new DoomString("you can't end a netgame!\n\n" + PRESSKEY);
            public static readonly DoomString ENDGAME = new DoomString("are you sure you want to end the game?\n\n" + PRESSYN);
            public static readonly DoomString DOSY = new DoomString("(press y to quit)");
            public static readonly DoomString GAMMALVL0 = new DoomString("Gamma correction OFF");
            public static readonly DoomString GAMMALVL1 = new DoomString("Gamma correction level 1");
            public static readonly DoomString GAMMALVL2 = new DoomString("Gamma correction level 2");
            public static readonly DoomString GAMMALVL3 = new DoomString("Gamma correction level 3");
            public static readonly DoomString GAMMALVL4 = new DoomString("Gamma correction level 4");
            public static readonly DoomString EMPTYSTRING = new DoomString("empty slot");
            public static readonly DoomString GOTARMOR = new DoomString("Picked up the armor.");
            public static readonly DoomString GOTMEGA = new DoomString("Picked up the MegaArmor!");
            public static readonly DoomString GOTHTHBONUS = new DoomString("Picked up a health bonus.");
            public static readonly DoomString GOTARMBONUS = new DoomString("Picked up an armor bonus.");
            public static readonly DoomString GOTSTIM = new DoomString("Picked up a stimpack.");
            public static readonly DoomString GOTMEDINEED = new DoomString("Picked up a medikit that you REALLY need!");
            public static readonly DoomString GOTMEDIKIT = new DoomString("Picked up a medikit.");
            public static readonly DoomString GOTSUPER = new DoomString("Supercharge!");
            public static readonly DoomString GOTBLUECARD = new DoomString("Picked up a blue keycard.");
            public static readonly DoomString GOTYELWCARD = new DoomString("Picked up a yellow keycard.");
            public static readonly DoomString GOTREDCARD = new DoomString("Picked up a red keycard.");
            public static readonly DoomString GOTBLUESKUL = new DoomString("Picked up a blue skull key.");
            public static readonly DoomString GOTYELWSKUL = new DoomString("Picked up a yellow skull key.");
            public static readonly DoomString GOTREDSKULL = new DoomString("Picked up a red skull key.");
            public static readonly DoomString GOTINVUL = new DoomString("Invulnerability!");
            public static readonly DoomString GOTBERSERK = new DoomString("Berserk!");
            public static readonly DoomString GOTINVIS = new DoomString("Partial Invisibility");
            public static readonly DoomString GOTSUIT = new DoomString("Radiation Shielding Suit");
            public static readonly DoomString GOTMAP = new DoomString("Computer Area Map");
            public static readonly DoomString GOTVISOR = new DoomString("Light Amplification Visor");
            public static readonly DoomString GOTMSPHERE = new DoomString("MegaSphere!");
            public static readonly DoomString GOTCLIP = new DoomString("Picked up a clip.");
            public static readonly DoomString GOTCLIPBOX = new DoomString("Picked up a box of bullets.");
            public static readonly DoomString GOTROCKET = new DoomString("Picked up a rocket.");
            public static readonly DoomString GOTROCKBOX = new DoomString("Picked up a box of rockets.");
            public static readonly DoomString GOTCELL = new DoomString("Picked up an energy cell.");
            public static readonly DoomString GOTCELLBOX = new DoomString("Picked up an energy cell pack.");
            public static readonly DoomString GOTSHELLS = new DoomString("Picked up 4 shotgun shells.");
            public static readonly DoomString GOTSHELLBOX = new DoomString("Picked up a box of shotgun shells.");
            public static readonly DoomString GOTBACKPACK = new DoomString("Picked up a backpack full of ammo!");
            public static readonly DoomString GOTBFG9000 = new DoomString("You got the BFG9000!  Oh, yes.");
            public static readonly DoomString GOTCHAINGUN = new DoomString("You got the chaingun!");
            public static readonly DoomString GOTCHAINSAW = new DoomString("A chainsaw!  Find some meat!");
            public static readonly DoomString GOTLAUNCHER = new DoomString("You got the rocket launcher!");
            public static readonly DoomString GOTPLASMA = new DoomString("You got the plasma gun!");
            public static readonly DoomString GOTSHOTGUN = new DoomString("You got the shotgun!");
            public static readonly DoomString GOTSHOTGUN2 = new DoomString("You got the super shotgun!");
            public static readonly DoomString PD_BLUEO = new DoomString("You need a blue key to activate this object");
            public static readonly DoomString PD_REDO = new DoomString("You need a red key to activate this object");
            public static readonly DoomString PD_YELLOWO = new DoomString("You need a yellow key to activate this object");
            public static readonly DoomString PD_BLUEK = new DoomString("You need a blue key to open this door");
            public static readonly DoomString PD_REDK = new DoomString("You need a red key to open this door");
            public static readonly DoomString PD_YELLOWK = new DoomString("You need a yellow key to open this door");
            public static readonly DoomString GGSAVED = new DoomString("game saved.");
            public static readonly DoomString HUSTR_E1M1 = new DoomString("E1M1: Hangar");
            public static readonly DoomString HUSTR_E1M2 = new DoomString("E1M2: Nuclear Plant");
            public static readonly DoomString HUSTR_E1M3 = new DoomString("E1M3: Toxin Refinery");
            public static readonly DoomString HUSTR_E1M4 = new DoomString("E1M4: Command Control");
            public static readonly DoomString HUSTR_E1M5 = new DoomString("E1M5: Phobos Lab");
            public static readonly DoomString HUSTR_E1M6 = new DoomString("E1M6: Central Processing");
            public static readonly DoomString HUSTR_E1M7 = new DoomString("E1M7: Computer Station");
            public static readonly DoomString HUSTR_E1M8 = new DoomString("E1M8: Phobos Anomaly");
            public static readonly DoomString HUSTR_E1M9 = new DoomString("E1M9: Military Base");
            public static readonly DoomString HUSTR_E2M1 = new DoomString("E2M1: Deimos Anomaly");
            public static readonly DoomString HUSTR_E2M2 = new DoomString("E2M2: Containment Area");
            public static readonly DoomString HUSTR_E2M3 = new DoomString("E2M3: Refinery");
            public static readonly DoomString HUSTR_E2M4 = new DoomString("E2M4: Deimos Lab");
            public static readonly DoomString HUSTR_E2M5 = new DoomString("E2M5: Command Center");
            public static readonly DoomString HUSTR_E2M6 = new DoomString("E2M6: Halls of the Damned");
            public static readonly DoomString HUSTR_E2M7 = new DoomString("E2M7: Spawning Vats");
            public static readonly DoomString HUSTR_E2M8 = new DoomString("E2M8: Tower of Babel");
            public static readonly DoomString HUSTR_E2M9 = new DoomString("E2M9: Fortress of Mystery");
            public static readonly DoomString HUSTR_E3M1 = new DoomString("E3M1: Hell Keep");
            public static readonly DoomString HUSTR_E3M2 = new DoomString("E3M2: Slough of Despair");
            public static readonly DoomString HUSTR_E3M3 = new DoomString("E3M3: Pandemonium");
            public static readonly DoomString HUSTR_E3M4 = new DoomString("E3M4: House of Pain");
            public static readonly DoomString HUSTR_E3M5 = new DoomString("E3M5: Unholy Cathedral");
            public static readonly DoomString HUSTR_E3M6 = new DoomString("E3M6: Mt. Erebus");
            public static readonly DoomString HUSTR_E3M7 = new DoomString("E3M7: Limbo");
            public static readonly DoomString HUSTR_E3M8 = new DoomString("E3M8: Dis");
            public static readonly DoomString HUSTR_E3M9 = new DoomString("E3M9: Warrens");
            public static readonly DoomString HUSTR_E4M1 = new DoomString("E4M1: Hell Beneath");
            public static readonly DoomString HUSTR_E4M2 = new DoomString("E4M2: Perfect Hatred");
            public static readonly DoomString HUSTR_E4M3 = new DoomString("E4M3: Sever The Wicked");
            public static readonly DoomString HUSTR_E4M4 = new DoomString("E4M4: Unruly Evil");
            public static readonly DoomString HUSTR_E4M5 = new DoomString("E4M5: They Will Repent");
            public static readonly DoomString HUSTR_E4M6 = new DoomString("E4M6: Against Thee Wickedly");
            public static readonly DoomString HUSTR_E4M7 = new DoomString("E4M7: And Hell Followed");
            public static readonly DoomString HUSTR_E4M8 = new DoomString("E4M8: Unto The Cruel");
            public static readonly DoomString HUSTR_E4M9 = new DoomString("E4M9: Fear");
            public static readonly DoomString HUSTR_1 = new DoomString("level 1: entryway");
            public static readonly DoomString HUSTR_2 = new DoomString("level 2: underhalls");
            public static readonly DoomString HUSTR_3 = new DoomString("level 3: the gantlet");
            public static readonly DoomString HUSTR_4 = new DoomString("level 4: the focus");
            public static readonly DoomString HUSTR_5 = new DoomString("level 5: the waste tunnels");
            public static readonly DoomString HUSTR_6 = new DoomString("level 6: the crusher");
            public static readonly DoomString HUSTR_7 = new DoomString("level 7: dead simple");
            public static readonly DoomString HUSTR_8 = new DoomString("level 8: tricks and traps");
            public static readonly DoomString HUSTR_9 = new DoomString("level 9: the pit");
            public static readonly DoomString HUSTR_10 = new DoomString("level 10: refueling base");
            public static readonly DoomString HUSTR_11 = new DoomString("level 11: 'o' of destruction!");
            public static readonly DoomString HUSTR_12 = new DoomString("level 12: the factory");
            public static readonly DoomString HUSTR_13 = new DoomString("level 13: downtown");
            public static readonly DoomString HUSTR_14 = new DoomString("level 14: the inmost dens");
            public static readonly DoomString HUSTR_15 = new DoomString("level 15: industrial zone");
            public static readonly DoomString HUSTR_16 = new DoomString("level 16: suburbs");
            public static readonly DoomString HUSTR_17 = new DoomString("level 17: tenements");
            public static readonly DoomString HUSTR_18 = new DoomString("level 18: the courtyard");
            public static readonly DoomString HUSTR_19 = new DoomString("level 19: the citadel");
            public static readonly DoomString HUSTR_20 = new DoomString("level 20: gotcha!");
            public static readonly DoomString HUSTR_21 = new DoomString("level 21: nirvana");
            public static readonly DoomString HUSTR_22 = new DoomString("level 22: the catacombs");
            public static readonly DoomString HUSTR_23 = new DoomString("level 23: barrels o' fun");
            public static readonly DoomString HUSTR_24 = new DoomString("level 24: the chasm");
            public static readonly DoomString HUSTR_25 = new DoomString("level 25: bloodfalls");
            public static readonly DoomString HUSTR_26 = new DoomString("level 26: the abandoned mines");
            public static readonly DoomString HUSTR_27 = new DoomString("level 27: monster condo");
            public static readonly DoomString HUSTR_28 = new DoomString("level 28: the spirit world");
            public static readonly DoomString HUSTR_29 = new DoomString("level 29: the living end");
            public static readonly DoomString HUSTR_30 = new DoomString("level 30: icon of sin");
            public static readonly DoomString HUSTR_31 = new DoomString("level 31: wolfenstein");
            public static readonly DoomString HUSTR_32 = new DoomString("level 32: grosse");
            public static readonly DoomString PHUSTR_1 = new DoomString("level 1: congo");
            public static readonly DoomString PHUSTR_2 = new DoomString("level 2: well of souls");
            public static readonly DoomString PHUSTR_3 = new DoomString("level 3: aztec");
            public static readonly DoomString PHUSTR_4 = new DoomString("level 4: caged");
            public static readonly DoomString PHUSTR_5 = new DoomString("level 5: ghost town");
            public static readonly DoomString PHUSTR_6 = new DoomString("level 6: baron's lair");
            public static readonly DoomString PHUSTR_7 = new DoomString("level 7: caughtyard");
            public static readonly DoomString PHUSTR_8 = new DoomString("level 8: realm");
            public static readonly DoomString PHUSTR_9 = new DoomString("level 9: abattoire");
            public static readonly DoomString PHUSTR_10 = new DoomString("level 10: onslaught");
            public static readonly DoomString PHUSTR_11 = new DoomString("level 11: hunted");
            public static readonly DoomString PHUSTR_12 = new DoomString("level 12: speed");
            public static readonly DoomString PHUSTR_13 = new DoomString("level 13: the crypt");
            public static readonly DoomString PHUSTR_14 = new DoomString("level 14: genesis");
            public static readonly DoomString PHUSTR_15 = new DoomString("level 15: the twilight");
            public static readonly DoomString PHUSTR_16 = new DoomString("level 16: the omen");
            public static readonly DoomString PHUSTR_17 = new DoomString("level 17: compound");
            public static readonly DoomString PHUSTR_18 = new DoomString("level 18: neurosphere");
            public static readonly DoomString PHUSTR_19 = new DoomString("level 19: nme");
            public static readonly DoomString PHUSTR_20 = new DoomString("level 20: the death domain");
            public static readonly DoomString PHUSTR_21 = new DoomString("level 21: slayer");
            public static readonly DoomString PHUSTR_22 = new DoomString("level 22: impossible mission");
            public static readonly DoomString PHUSTR_23 = new DoomString("level 23: tombstone");
            public static readonly DoomString PHUSTR_24 = new DoomString("level 24: the final frontier");
            public static readonly DoomString PHUSTR_25 = new DoomString("level 25: the temple of darkness");
            public static readonly DoomString PHUSTR_26 = new DoomString("level 26: bunker");
            public static readonly DoomString PHUSTR_27 = new DoomString("level 27: anti-christ");
            public static readonly DoomString PHUSTR_28 = new DoomString("level 28: the sewers");
            public static readonly DoomString PHUSTR_29 = new DoomString("level 29: odyssey of noises");
            public static readonly DoomString PHUSTR_30 = new DoomString("level 30: the gateway of hell");
            public static readonly DoomString PHUSTR_31 = new DoomString("level 31: cyberden");
            public static readonly DoomString PHUSTR_32 = new DoomString("level 32: go 2 it");
            public static readonly DoomString THUSTR_1 = new DoomString("level 1: system control");
            public static readonly DoomString THUSTR_2 = new DoomString("level 2: human bbq");
            public static readonly DoomString THUSTR_3 = new DoomString("level 3: power control");
            public static readonly DoomString THUSTR_4 = new DoomString("level 4: wormhole");
            public static readonly DoomString THUSTR_5 = new DoomString("level 5: hanger");
            public static readonly DoomString THUSTR_6 = new DoomString("level 6: open season");
            public static readonly DoomString THUSTR_7 = new DoomString("level 7: prison");
            public static readonly DoomString THUSTR_8 = new DoomString("level 8: metal");
            public static readonly DoomString THUSTR_9 = new DoomString("level 9: stronghold");
            public static readonly DoomString THUSTR_10 = new DoomString("level 10: redemption");
            public static readonly DoomString THUSTR_11 = new DoomString("level 11: storage facility");
            public static readonly DoomString THUSTR_12 = new DoomString("level 12: crater");
            public static readonly DoomString THUSTR_13 = new DoomString("level 13: nukage processing");
            public static readonly DoomString THUSTR_14 = new DoomString("level 14: steel works");
            public static readonly DoomString THUSTR_15 = new DoomString("level 15: dead zone");
            public static readonly DoomString THUSTR_16 = new DoomString("level 16: deepest reaches");
            public static readonly DoomString THUSTR_17 = new DoomString("level 17: processing area");
            public static readonly DoomString THUSTR_18 = new DoomString("level 18: mill");
            public static readonly DoomString THUSTR_19 = new DoomString("level 19: shipping/respawning");
            public static readonly DoomString THUSTR_20 = new DoomString("level 20: central processing");
            public static readonly DoomString THUSTR_21 = new DoomString("level 21: administration center");
            public static readonly DoomString THUSTR_22 = new DoomString("level 22: habitat");
            public static readonly DoomString THUSTR_23 = new DoomString("level 23: lunar mining project");
            public static readonly DoomString THUSTR_24 = new DoomString("level 24: quarry");
            public static readonly DoomString THUSTR_25 = new DoomString("level 25: baron's den");
            public static readonly DoomString THUSTR_26 = new DoomString("level 26: ballistyx");
            public static readonly DoomString THUSTR_27 = new DoomString("level 27: mount pain");
            public static readonly DoomString THUSTR_28 = new DoomString("level 28: heck");
            public static readonly DoomString THUSTR_29 = new DoomString("level 29: river styx");
            public static readonly DoomString THUSTR_30 = new DoomString("level 30: last call");
            public static readonly DoomString THUSTR_31 = new DoomString("level 31: pharaoh");
            public static readonly DoomString THUSTR_32 = new DoomString("level 32: caribbean");
            public static readonly DoomString AMSTR_FOLLOWON = new DoomString("Follow Mode ON");
            public static readonly DoomString AMSTR_FOLLOWOFF = new DoomString("Follow Mode OFF");
            public static readonly DoomString AMSTR_GRIDON = new DoomString("Grid ON");
            public static readonly DoomString AMSTR_GRIDOFF = new DoomString("Grid OFF");
            public static readonly DoomString AMSTR_MARKEDSPOT = new DoomString("Marked Spot");
            public static readonly DoomString AMSTR_MARKSCLEARED = new DoomString("All Marks Cleared");
            public static readonly DoomString STSTR_MUS = new DoomString("Music Change");
            public static readonly DoomString STSTR_NOMUS = new DoomString("IMPOSSIBLE SELECTION");
            public static readonly DoomString STSTR_DQDON = new DoomString("Degreelessness Mode On");
            public static readonly DoomString STSTR_DQDOFF = new DoomString("Degreelessness Mode Off");
            public static readonly DoomString STSTR_KFAADDED = new DoomString("Very Happy Ammo Added");
            public static readonly DoomString STSTR_FAADDED = new DoomString("Ammo (no keys) Added");
            public static readonly DoomString STSTR_NCON = new DoomString("No Clipping Mode ON");
            public static readonly DoomString STSTR_NCOFF = new DoomString("No Clipping Mode OFF");
            public static readonly DoomString STSTR_BEHOLD = new DoomString("inVuln, Str, Inviso, Rad, Allmap, or Lite-amp");
            public static readonly DoomString STSTR_BEHOLDX = new DoomString("Power-up Toggled");
            public static readonly DoomString STSTR_CHOPPERS = new DoomString("... doesn't suck - GM");
            public static readonly DoomString STSTR_CLEV = new DoomString("Changing Level...");

            public static readonly DoomString E1TEXT = new DoomString(
                "Once you beat the big badasses and\n" +
                "clean out the moon base you're supposed\n" +
                "to win, aren't you? Aren't you? Where's\n" +
                "your fat reward and ticket home? What\n" +
                "the hell is this? It's not supposed to\n" +
                "end this way!\n" +
                "\n" +
                "It stinks like rotten meat, but looks\n" +
                "like the lost Deimos base.  Looks like\n" +
                "you're stuck on The Shores of Hell.\n" +
                "The only way out is through.\n" +
                "\n" +
                "To continue the DOOM experience, play\n" +
                "The Shores of Hell and its amazing\n" +
                "sequel, Inferno!\n");

            public static readonly DoomString E2TEXT = new DoomString(
                "You've done it! The hideous cyber-\n" +
                "demon lord that ruled the lost Deimos\n" +
                "moon base has been slain and you\n" +
                "are triumphant! But ... where are\n" +
                "you? You clamber to the edge of the\n" +
                "moon and look down to see the awful\n" +
                "truth.\n" +
                "\n" +
                "Deimos floats above Hell itself!\n" +
                "You've never heard of anyone escaping\n" +
                "from Hell, but you'll make the bastards\n" +
                "sorry they ever heard of you! Quickly,\n" +
                "you rappel down to  the surface of\n" +
                "Hell.\n" +
                "\n" +
                "Now, it's on to the final chapter of\n" +
                "DOOM! -- Inferno.");

            public static readonly DoomString E3TEXT = new DoomString(
                "The loathsome spiderdemon that\n" +
                "masterminded the invasion of the moon\n" +
                "bases and caused so much death has had\n" +
                "its ass kicked for all time.\n" +
                "\n" +
                "A hidden doorway opens and you enter.\n" +
                "You've proven too tough for Hell to\n" +
                "contain, and now Hell at last plays\n" +
                "fair -- for you emerge from the door\n" +
                "to see the green fields of Earth!\n" +
                "Home at last.\n" +
                "\n" +
                "You wonder what's been happening on\n" +
                "Earth while you were battling evil\n" +
                "unleashed. It's good that no Hell-\n" +
                "spawn could have come through that\n" +
                "door with you ...");

            public static readonly DoomString E4TEXT = new DoomString(
                "the spider mastermind must have sent forth\n" +
                "its legions of hellspawn before your\n" +
                "final confrontation with that terrible\n" +
                "beast from hell.  but you stepped forward\n" +
                "and brought forth eternal damnation and\n" +
                "suffering upon the horde as a true hero\n" +
                "would in the face of something so evil.\n" +
                "\n" +
                "besides, someone was gonna pay for what\n" +
                "happened to daisy, your pet rabbit.\n" +
                "\n" +
                "but now, you see spread before you more\n" +
                "potential pain and gibbitude as a nation\n" +
                "of demons run amok among our cities.\n" +
                "\n" +
                "next stop, hell on earth!");

            public static readonly DoomString C1TEXT = new DoomString(
                "YOU HAVE ENTERED DEEPLY INTO THE INFESTED\n" +
                "STARPORT. BUT SOMETHING IS WRONG. THE\n" +
                "MONSTERS HAVE BROUGHT THEIR OWN REALITY\n" +
                "WITH THEM, AND THE STARPORT'S TECHNOLOGY\n" +
                "IS BEING SUBVERTED BY THEIR PRESENCE.\n" +
                "\n" +
                "AHEAD, YOU SEE AN OUTPOST OF HELL, A\n" +
                "FORTIFIED ZONE. IF YOU CAN GET PAST IT,\n" +
                "YOU CAN PENETRATE INTO THE HAUNTED HEART\n" +
                "OF THE STARBASE AND FIND THE CONTROLLING\n" +
                "SWITCH WHICH HOLDS EARTH'S POPULATION\n" +
                "HOSTAGE.");

            public static readonly DoomString C2TEXT = new DoomString(
                "YOU HAVE WON! YOUR VICTORY HAS ENABLED\n" +
                "HUMANKIND TO EVACUATE EARTH AND ESCAPE\n" +
                "THE NIGHTMARE.  NOW YOU ARE THE ONLY\n" +
                "HUMAN LEFT ON THE FACE OF THE PLANET.\n" +
                "CANNIBAL MUTATIONS, CARNIVOROUS ALIENS,\n" +
                "AND EVIL SPIRITS ARE YOUR ONLY NEIGHBORS.\n" +
                "YOU SIT BACK AND WAIT FOR DEATH, CONTENT\n" +
                "THAT YOU HAVE SAVED YOUR SPECIES.\n" +
                "\n" +
                "BUT THEN, EARTH CONTROL BEAMS DOWN A\n" +
                "MESSAGE FROM SPACE: \"SENSORS HAVE LOCATED\n" +
                "THE SOURCE OF THE ALIEN INVASION. IF YOU\n" +
                "GO THERE, YOU MAY BE ABLE TO BLOCK THEIR\n" +
                "ENTRY.  THE ALIEN BASE IS IN THE HEART OF\n" +
                "YOUR OWN HOME CITY, NOT FAR FROM THE\n" +
                "STARPORT.\" SLOWLY AND PAINFULLY YOU GET\n" +
                "UP AND RETURN TO THE FRAY.");

            public static readonly DoomString C3TEXT = new DoomString(
                "YOU ARE AT THE CORRUPT HEART OF THE CITY,\n" +
                "SURROUNDED BY THE CORPSES OF YOUR ENEMIES.\n" +
                "YOU SEE NO WAY TO DESTROY THE CREATURES'\n" +
                "ENTRYWAY ON THIS SIDE, SO YOU CLENCH YOUR\n" +
                "TEETH AND PLUNGE THROUGH IT.\n" +
                "\n" +
                "THERE MUST BE A WAY TO CLOSE IT ON THE\n" +
                "OTHER SIDE. WHAT DO YOU CARE IF YOU'VE\n" +
                "GOT TO GO THROUGH HELL TO GET TO IT?");

            public static readonly DoomString C4TEXT = new DoomString(
                "THE HORRENDOUS VISAGE OF THE BIGGEST\n" +
                "DEMON YOU'VE EVER SEEN CRUMBLES BEFORE\n" +
                "YOU, AFTER YOU PUMP YOUR ROCKETS INTO\n" +
                "HIS EXPOSED BRAIN. THE MONSTER SHRIVELS\n" +
                "UP AND DIES, ITS THRASHING LIMBS\n" +
                "DEVASTATING UNTOLD MILES OF HELL'S\n" +
                "SURFACE.\n" +
                "\n" +
                "YOU'VE DONE IT. THE INVASION IS OVER.\n" +
                "EARTH IS SAVED. HELL IS A WRECK. YOU\n" +
                "WONDER WHERE BAD FOLKS WILL GO WHEN THEY\n" +
                "DIE, NOW. WIPING THE SWEAT FROM YOUR\n" +
                "FOREHEAD YOU BEGIN THE LONG TREK BACK\n" +
                "HOME. REBUILDING EARTH OUGHT TO BE A\n" +
                "LOT MORE FUN THAN RUINING IT WAS.\n");

            public static readonly DoomString C5TEXT = new DoomString(
                "CONGRATULATIONS, YOU'VE FOUND THE SECRET\n" +
                "LEVEL! LOOKS LIKE IT'S BEEN BUILT BY\n" +
                "HUMANS, RATHER THAN DEMONS. YOU WONDER\n" +
                "WHO THE INMATES OF THIS CORNER OF HELL\n" +
                "WILL BE.");

            public static readonly DoomString C6TEXT = new DoomString(
                "CONGRATULATIONS, YOU'VE FOUND THE\n" +
                "SUPER SECRET LEVEL!  YOU'D BETTER\n" +
                "BLAZE THROUGH THIS ONE!\n");

            public static readonly DoomString P1TEXT = new DoomString(
                "You gloat over the steaming carcass of the\n" +
                "Guardian.  With its death, you've wrested\n" +
                "the Accelerator from the stinking claws\n" +
                "of Hell.  You relax and glance around the\n" +
                "room.  Damn!  There was supposed to be at\n" +
                "least one working prototype, but you can't\n" +
                "see it. The demons must have taken it.\n" +
                "\n" +
                "You must find the prototype, or all your\n" +
                "struggles will have been wasted. Keep\n" +
                "moving, keep fighting, keep killing.\n" +
                "Oh yes, keep living, too.");

            public static readonly DoomString P2TEXT = new DoomString(
                "Even the deadly Arch-Vile labyrinth could\n" +
                "not stop you, and you've gotten to the\n" +
                "prototype Accelerator which is soon\n" +
                "efficiently and permanently deactivated.\n" +
                "\n" +
                "You're good at that kind of thing.");

            public static readonly DoomString P3TEXT = new DoomString(
                "You've bashed and battered your way into\n" +
                "the heart of the devil-hive.  Time for a\n" +
                "Search-and-Destroy mission, aimed at the\n" +
                "Gatekeeper, whose foul offspring is\n" +
                "cascading to Earth.  Yeah, he's bad. But\n" +
                "you know who's worse!\n" +
                "\n" +
                "Grinning evilly, you check your gear, and\n" +
                "get ready to give the bastard a little Hell\n" +
                "of your own making!");

            public static readonly DoomString P4TEXT = new DoomString(
                "The Gatekeeper's evil face is splattered\n" +
                "all over the place.  As its tattered corpse\n" +
                "collapses, an inverted Gate forms and\n" +
                "sucks down the shards of the last\n" +
                "prototype Accelerator, not to mention the\n" +
                "few remaining demons.  You're done. Hell\n" +
                "has gone back to pounding bad dead folks \n" +
                "instead of good live ones.  Remember to\n" +
                "tell your grandkids to put a rocket\n" +
                "launcher in your coffin. If you go to Hell\n" +
                "when you die, you'll need it for some\n" +
                "final cleaning-up ...");

            public static readonly DoomString P5TEXT = new DoomString(
                "You've found the second-hardest level we\n" +
                "got. Hope you have a saved game a level or\n" +
                "two previous.  If not, be prepared to die\n" +
                "aplenty. For master marines only.");

            public static readonly DoomString P6TEXT = new DoomString(
                "Betcha wondered just what WAS the hardest\n" +
                "level we had ready for ya?  Now you know.\n" +
                "No one gets out alive.");

            public static readonly DoomString T1TEXT = new DoomString(
                "You've fought your way out of the infested\n" +
                "experimental labs.   It seems that UAC has\n" +
                "once again gulped it down.  With their\n" +
                "high turnover, it must be hard for poor\n" +
                "old UAC to buy corporate health insurance\n" +
                "nowadays..\n" +
                "\n" +
                "Ahead lies the military complex, now\n" +
                "swarming with diseased horrors hot to get\n" +
                "their teeth into you. With luck, the\n" +
                "complex still has some warlike ordnance\n" +
                "laying around.");

            public static readonly DoomString T2TEXT = new DoomString(
                "You hear the grinding of heavy machinery\n" +
                "ahead.  You sure hope they're not stamping\n" +
                "out new hellspawn, but you're ready to\n" +
                "ream out a whole herd if you have to.\n" +
                "They might be planning a blood feast, but\n" +
                "you feel about as mean as two thousand\n" +
                "maniacs packed into one mad killer.\n" +
                "\n" +
                "You don't plan to go down easy.");

            public static readonly DoomString T3TEXT = new DoomString(
                "The vista opening ahead looks real damn\n" +
                "familiar. Smells familiar, too -- like\n" +
                "fried excrement. You didn't like this\n" +
                "place before, and you sure as hell ain't\n" +
                "planning to like it now. The more you\n" +
                "brood on it, the madder you get.\n" +
                "Hefting your gun, an evil grin trickles\n" +
                "onto your face. Time to take some names.");

            public static readonly DoomString T4TEXT = new DoomString(
                "Suddenly, all is silent, from one horizon\n" +
                "to the other. The agonizing echo of Hell\n" +
                "fades away, the nightmare sky turns to\n" +
                "blue, the heaps of monster corpses start \n" +
                "to evaporate along with the evil stench \n" +
                "that filled the air. Jeeze, maybe you've\n" +
                "done it. Have you really won?\n" +
                "\n" +
                "Something rumbles in the distance.\n" +
                "A blue light begins to glow inside the\n" +
                "ruined skull of the demon-spitter.");

            public static readonly DoomString T5TEXT = new DoomString(
                "What now? Looks totally different. Kind\n" +
                "of like King Tut's condo. Well,\n" +
                "whatever's here can't be any worse\n" +
                "than usual. Can it?  Or maybe it's best\n" +
                "to let sleeping gods lie..");

            public static readonly DoomString T6TEXT = new DoomString(
                "Time for a vacation. You've burst the\n" +
                "bowels of hell and by golly you're ready\n" +
                "for a break. You mutter to yourself,\n" +
                "Maybe someone else can kick Hell's ass\n" +
                "next time around. Ahead lies a quiet town,\n" +
                "with peaceful flowing water, quaint\n" +
                "buildings, and presumably no Hellspawn.\n" +
                "\n" +
                "As you step off the transport, you hear\n" +
                "the stomp of a cyberdemon's iron shoe.");

            public static readonly DoomString CC_ZOMBIE = new DoomString("ZOMBIEMAN");
            public static readonly DoomString CC_SHOTGUN = new DoomString("SHOTGUN GUY");
            public static readonly DoomString CC_HEAVY = new DoomString("HEAVY WEAPON DUDE");
            public static readonly DoomString CC_IMP = new DoomString("IMP");
            public static readonly DoomString CC_DEMON = new DoomString("DEMON");
            public static readonly DoomString CC_LOST = new DoomString("LOST SOUL");
            public static readonly DoomString CC_CACO = new DoomString("CACODEMON");
            public static readonly DoomString CC_HELL = new DoomString("HELL KNIGHT");
            public static readonly DoomString CC_BARON = new DoomString("BARON OF HELL");
            public static readonly DoomString CC_ARACH = new DoomString("ARACHNOTRON");
            public static readonly DoomString CC_PAIN = new DoomString("PAIN ELEMENTAL");
            public static readonly DoomString CC_REVEN = new DoomString("REVENANT");
            public static readonly DoomString CC_MANCU = new DoomString("MANCUBUS");
            public static readonly DoomString CC_ARCH = new DoomString("ARCH-VILE");
            public static readonly DoomString CC_SPIDER = new DoomString("THE SPIDER MASTERMIND");
            public static readonly DoomString CC_CYBER = new DoomString("THE CYBERDEMON");
            public static readonly DoomString CC_HERO = new DoomString("OUR HERO");
        }
    }
}
