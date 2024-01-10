using System;
using System.Collections.Generic;

namespace DoomSharp.Core.UI;

public static class Messages
{
    public const int NumberOfQuitMessages = 22;

    public static bool IsEnglish => DoomGame.Instance.Language == GameLanguage.English;

    public static string PressAnyKey =>
        IsEnglish
            ? "press a key."
            : "APPUYEZ SUR UNE TOUCHE.";

    public static string PressYesOrNo =>
        IsEnglish
            ? "press y or n."
            : "APPUYEZ SUR Y OU N.";

    public static string QuickSaveSpot =>
        IsEnglish
            ? $"you haven't picked a quicksave slot yet!{Environment.NewLine}{Environment.NewLine}" + PressAnyKey
            : $"VOUS N'AVEZ PAS CHOISI UN EMPLACEMENT!{Environment.NewLine}{Environment.NewLine}" + PressAnyKey;

    public static string QuickSavePrompt =>
        IsEnglish
            ? $"quicksave over your game named{Environment.NewLine}{Environment.NewLine}'{{0}}'?{Environment.NewLine}" + PressYesOrNo
            : $"SAUVEGARDE RAPIDE DANS LE FICHIER {Environment.NewLine}{Environment.NewLine}'{{0}}'?{Environment.NewLine}" + PressYesOrNo;

    public static string QuickLoadPrompt =>
        IsEnglish
            ? $"do you want to quickload the game named{Environment.NewLine}{Environment.NewLine}'{{0}}'?{Environment.NewLine}" + PressYesOrNo
            : $"VOULEZ-VOUS CHARGER LA SAUVEGARDE{Environment.NewLine}{Environment.NewLine}'{{0}}'?{Environment.NewLine}" + PressYesOrNo;

    public static string NewGame =>
        IsEnglish
            ? $"you can't start a new game{Environment.NewLine}while in a network game{Environment.NewLine}{Environment.NewLine}" + PressAnyKey
            : $"VOUS NE POUVEZ PAS LANCER{Environment.NewLine}UN NOUVEAU JEU SUR RESEAU.{Environment.NewLine}{Environment.NewLine}" + PressAnyKey;

    public static string Nightmare =>
        IsEnglish
            ? $"are you sure? this skill level{Environment.NewLine}isn't even remotely fair.{Environment.NewLine}{Environment.NewLine}" + PressYesOrNo
            : $"VOUS CONFIRMEZ? CE NIVEAU EST{Environment.NewLine}VRAIMENT IMPITOYABLE!{Environment.NewLine}" + PressYesOrNo;

    public static string Shareware =>
        IsEnglish
            ? $"this is the shareware version of doom.{Environment.NewLine}{Environment.NewLine}you need to order the entire trilogy.{Environment.NewLine}{Environment.NewLine}" + PressAnyKey
            : $"CECI EST UNE VERSION SHAREWARE DE DOOM.{Environment.NewLine}{Environment.NewLine}VOUS DEVRIEZ COMMANDER LA TRILOGIE COMPLETE.{Environment.NewLine}{Environment.NewLine}" + PressAnyKey;

    public static string GenericQuitMessage =>
        IsEnglish 
            ? $"are you sure you want to{Environment.NewLine}quit this great game?"
            : $"VOUS VOULEZ VRAIMENT{Environment.NewLine}QUITTER CE SUPER JEU?";

    public static string DosY =>
        IsEnglish
            ? "(press y to quit)"
            : "(APPUYEZ SUR Y POUR REVENIR AU OS.)";

    public static string DetailHigh =>
        IsEnglish
            ? "High detail"
            : "GRAPHISMES MAXIMUM";

    public static string DetailLow =>
        IsEnglish
            ? "Low detail"
            : "GRAPHISMES MINIMUM";

    public static string GotArmor => IsEnglish ? "Picked up the armor." : "";
    public static string GotMega => IsEnglish ? "Picked up the MegaArmor!" : "";
    public static string GotHealthBonus => IsEnglish ? "Picked up a health bonus." : "";
    public static string GotArmorBonus => IsEnglish ? "Picked up an armor bonus." : "";
    public static string GotStim => IsEnglish ? "Picked up a stimpack." : "";
    public static string GotMedikitAndItWasNeeded => IsEnglish ? "Picked up a medikit that you REALLY need!" : "";
    public static string GotMedikit => IsEnglish ? "Picked up a medikit." : "";
    public static string GotSuper => IsEnglish ? "Supercharge!" : "";

    public static string GotBlueCard => IsEnglish ? "Picked up the blue keycard" : "";
    public static string GotYellowCard => IsEnglish ? "Picked up the yellow keycard" : "";
    public static string GotRedCard => IsEnglish ? "Picked up the red keycard" : "";
    public static string GotBlueSkull => IsEnglish ? "Picked up the blue skull key" : "";
    public static string GotYellowSkull => IsEnglish ? "Picked up the yellow skull key" : "";
    public static string GotRedSkull => IsEnglish ? "Picked up the red skull key" : "";

    public static string GotInvulnerability => IsEnglish ? "Invulnerability!" : "";
    public static string GotBerserk => IsEnglish ? "Berserk!" : "";
    public static string GotInvisibility => IsEnglish ? "Partial invisibility" : "";
    public static string GotSuit => IsEnglish ? "Radiation Shielding Suit" : "";
    public static string GotMap => IsEnglish ? "Computer Area Map" : "";
    public static string GotVisor => IsEnglish ? "Light Amplification Visor" : "";
    public static string GotMegaSphere => IsEnglish ? "MegaSphere!" : "";

    public static string GotClip => IsEnglish ? "Picked up a clip." : "";
    public static string GotClipBox => IsEnglish ? "Picked up a box of bullets." : "";
    public static string GotRocket => IsEnglish ? "Picked up a rocket." : "";
    public static string GotRocketBox => IsEnglish ? "Picked up a box of rockets." : "";
    public static string GotCell => IsEnglish ? "Picked up an energy cell." : "";
    public static string GotCellBox => IsEnglish ? "Picked up an energy cell pack." : "";
    public static string GotShells => IsEnglish ? "Picked up 4 shotgun shells." : "";
    public static string GotShellBox => IsEnglish ? "Picked up a box of shotgun shells." : "";
    public static string GotBackpack  => IsEnglish ? "Picked up a backpack full of ammo!" : "";

    public static string GotBfg9000 => IsEnglish ? "You got the BFG9000! Oh, yes." : "";
    public static string GotChaingun => IsEnglish ? "You got the chaingun!" : "";
    public static string GotChainsaw => IsEnglish ? "A chainsaw! Find some meat!" : "";
    public static string GotRocketLauncher => IsEnglish ? "You got the rocket launcher!" : "";
    public static string GotPlasmaRifle => IsEnglish ? "You got the plasma rifle!" : "";
    public static string GotShotgun => IsEnglish ? "You got the shotgun!" : "";
    public static string GotShotgun2 => IsEnglish ? "You got the super shotgun!" : "";

    public static string PD_BLUEO => IsEnglish ? "You need a blue key to activate this object" : "";
    public static string PD_REDO => IsEnglish ? "You need a red key to activate this object" : "";
    public static string PD_YELLOWO => IsEnglish ? "You need a yellow key to activate this object" : "";

    public static string PD_BLUEK => IsEnglish ? "You need a blue key to open this door" : "";
    public static string PD_REDK => IsEnglish ? "You need a red key to open this door" : "";
    public static string PD_YELLOWK => IsEnglish ? "You need a yellow key to open this door" : "";

    public static IEnumerable<string> QuitMessages 
    {
        get
        {
            // DOOM1
            yield return GenericQuitMessage;
            yield return $"please don't leave, there's more{Environment.NewLine}demons to toast!";
            yield return $"let's beat it -- this is turning{Environment.NewLine}into a bloodbath!";
            yield return $"i wouldn't leave if i were you.{Environment.NewLine}dos is much worse.";
            yield return $"you're trying to say you like dos{Environment.NewLine}better than me, right?";
            yield return $"don't leave yet -- there's a{Environment.NewLine}demon around that corner!";
            yield return $"ya know, next time you come in here{Environment.NewLine}i'm gonna toast ya.";
            yield return "go ahead and leave. see if i care.";

            // QuitDOOM II messages
            yield return $"you want to quit?{Environment.NewLine}then, thou hast lost an eighth!";
            yield return $"don't go now, there's a {Environment.NewLine}dimensional shambler waiting{Environment.NewLine}at the dos prompt!";
            yield return $"get outta here and go back{Environment.NewLine}to your boring programs.";
            yield return $"if i were your boss, i'd {Environment.NewLine} deathmatch ya in a minute!";
            yield return $"look, bud. you leave now{Environment.NewLine}and you forfeit your body count!";
            yield return $"just leave. when you come{Environment.NewLine}back, i'll be waiting with a bat.";
            yield return $"you're lucky i don't smack{Environment.NewLine}you for thinking about leaving.";

            // FinalDOOM?
            yield return $"fuck you, pussy!{Environment.NewLine}get the fuck out!";
            yield return $"you quit and i'll jizz{Environment.NewLine}in your cystholes!";
            yield return $"if you leave, i'll make{Environment.NewLine}the lord drink my jizz.";
            yield return $"hey, ron! can we say{Environment.NewLine}'fuck' in the game?";
            yield return $"i'd leave: this is just{Environment.NewLine}more monsters and levels.{Environment.NewLine}what a load.";
            yield return $"suck it down, asshole!{Environment.NewLine}you're a fucking wimp!";
            yield return $"don't quit now! we're {Environment.NewLine}still spending your money!";

            // Internal debug. Different style, too.
            yield return $"THIS IS NO MESSAGE!{Environment.NewLine}Page intentionally left blank.";
        }
    }

    public static string HUSTR_E1M1 => IsEnglish ? "E1M1: Hangar" : "E1M1: HANGAR";
    public static string HUSTR_E1M2 => IsEnglish ? "E1M2: Nuclear Plant" : "E1M2: USINE NUCLEAIRE ";
    public static string HUSTR_E1M3 => IsEnglish ? "E1M3: Toxin Refinery" : "E1M3: RAFFINERIE DE TOXINES ";
    public static string HUSTR_E1M4 => IsEnglish ? "E1M4: Command Control" : "E1M4: CENTRE DE CONTROLE ";
    public static string HUSTR_E1M5 => IsEnglish ? "E1M5: Phobos Lab" : "E1M5: LABORATOIRE PHOBOS ";
    public static string HUSTR_E1M6 => IsEnglish ? "E1M6: Central Processing" : "E1M6: TRAITEMENT CENTRAL ";
    public static string HUSTR_E1M7 => IsEnglish ? "E1M7: Computer Station" : "E1M7: CENTRE INFORMATIQUE ";
    public static string HUSTR_E1M8 => IsEnglish ? "E1M8: Phobos Anomaly" : "E1M8: ANOMALIE PHOBOS ";
    public static string HUSTR_E1M9 => IsEnglish ? "E1M9: Military Base" : "E1M9: BASE MILITAIRE ";

    public static string HUSTR_E2M1 => IsEnglish ? "E2M1: Deimos Anomaly" : "E2M1: ANOMALIE DEIMOS ";
    public static string HUSTR_E2M2 => IsEnglish ? "E2M2: Containment Area" : "E2M2: ZONE DE CONFINEMENT ";
    public static string HUSTR_E2M3 => IsEnglish ? "E2M3: Refinery" : "E2M3: RAFFINERIE";
    public static string HUSTR_E2M4 => IsEnglish ? "E2M4: Deimos Lab" : "E2M4: LABORATOIRE DEIMOS ";
    public static string HUSTR_E2M5 => IsEnglish ? "E2M5: Command Center" : "E2M5: CENTRE DE CONTROLE ";
    public static string HUSTR_E2M6 => IsEnglish ? "E2M6: Halls of the Damned" : "E2M6: HALLS DES DAMNES ";
    public static string HUSTR_E2M7 => IsEnglish ? "E2M7: Spawning Vats" : "E2M7: CUVES DE REPRODUCTION ";
    public static string HUSTR_E2M8 => IsEnglish ? "E2M8: Tower of Babel" : "E2M8: TOUR DE BABEL ";
    public static string HUSTR_E2M9 => IsEnglish ? "E2M9: Fortress of Mystery" : "E2M9: FORTERESSE DU MYSTERE ";

    public static string HUSTR_E3M1 => IsEnglish ? "E3M1: Hell Keep" : "E3M1: DONJON DE L'ENFER ";
    public static string HUSTR_E3M2 => IsEnglish ? "E3M2: Slough of Despair" : "E3M2: BOURBIER DU DESESPOIR ";
    public static string HUSTR_E3M3 => IsEnglish ? "E3M3: Pandemonium" : "E3M3: PANDEMONIUM";
    public static string HUSTR_E3M4 => IsEnglish ? "E3M4: House of Pain" : "E3M4: MAISON DE LA DOULEUR ";
    public static string HUSTR_E3M5 => IsEnglish ? "E3M5: Unholy Cathedral" : "E3M5: CATHEDRALE PROFANE ";
    public static string HUSTR_E3M6 => IsEnglish ? "E3M6: Mt. Erebus" : "E3M6: MONT EREBUS";
    public static string HUSTR_E3M7 => IsEnglish ? "E3M7: Limbo" : "E3M7: LIMBES";
    public static string HUSTR_E3M8 => IsEnglish ? "E3M8: Dis" : "E3M8: DIS";
    public static string HUSTR_E3M9 => IsEnglish ? "E3M9: Warrens" : "E3M9: CLAPIERS";

    public static string HUSTR_E4M1 => "E4M1: Hell Beneath";
    public static string HUSTR_E4M2 => "E4M2: Perfect Hatred";
    public static string HUSTR_E4M3 => "E4M3: Sever The Wicked";
    public static string HUSTR_E4M4 => "E4M4: Unruly Evil";
    public static string HUSTR_E4M5 => "E4M5: They Will Repent";
    public static string HUSTR_E4M6 => "E4M6: Against Thee Wickedly";
    public static string HUSTR_E4M7 => "E4M7: And Hell Followed";
    public static string HUSTR_E4M8 => "E4M8: Unto The Cruel";
    public static string HUSTR_E4M9 => "E4M9: Fear";

    public static string HUSTR_1 => IsEnglish ? "level 1: entryway" : "NIVEAU 1: ENTREE ";
    public static string HUSTR_2 => IsEnglish ? "level 2: underhalls" : "NIVEAU 2: HALLS SOUTERRAINS ";
    public static string HUSTR_3 => IsEnglish ? "level 3: the gantlet" : "NIVEAU 3: LE FEU NOURRI ";
    public static string HUSTR_4 => IsEnglish ? "level 4: the focus" : "NIVEAU 4: LE FOYER ";
    public static string HUSTR_5 => IsEnglish ? "level 5: the waste tunnels" : "NIVEAU 5: LES EGOUTS ";
    public static string HUSTR_6 => IsEnglish ? "level 6: the crusher" : "NIVEAU 6: LE BROYEUR ";
    public static string HUSTR_7 => IsEnglish ? "level 7: dead simple" : "NIVEAU 7: L'HERBE DE LA MORT";
    public static string HUSTR_8 => IsEnglish ? "level 8: tricks and traps" : "NIVEAU 8: RUSES ET PIEGES ";
    public static string HUSTR_9 => IsEnglish ? "level 9: the pit" : "NIVEAU 9: LE PUITS ";
    public static string HUSTR_10 => IsEnglish ? "level 10: refueling base" : "NIVEAU 10: BASE DE RAVITAILLEMENT ";
    public static string HUSTR_11 => IsEnglish ? "level 11: 'o' of destruction!" : "NIVEAU 11: LE CERCLE DE LA MORT!";

    public static string HUSTR_12 => IsEnglish ? "level 12: the factory" : "NIVEAU 12: L'USINE ";
    public static string HUSTR_13 => IsEnglish ? "level 13: downtown" : "NIVEAU 13: LE CENTRE VILLE";
    public static string HUSTR_14 => IsEnglish ? "level 14: the inmost dens" : "NIVEAU 14: LES ANTRES PROFONDES ";
    public static string HUSTR_15 => IsEnglish ? "level 15: industrial zone" : "NIVEAU 15: LA ZONE INDUSTRIELLE ";
    public static string HUSTR_16 => IsEnglish ? "level 16: suburbs" : "NIVEAU 16: LA BANLIEUE";
    public static string HUSTR_17 => IsEnglish ? "level 17: tenements" : "NIVEAU 17: LES IMMEUBLES";
    public static string HUSTR_18 => IsEnglish ? "level 18: the courtyard" : "NIVEAU 18: LA COUR ";
    public static string HUSTR_19 => IsEnglish ? "level 19: the citadel" : "NIVEAU 19: LA CITADELLE ";
    public static string HUSTR_20 => IsEnglish ? "level 20: gotcha!" : "NIVEAU 20: JE T'AI EU!";

    public static string HUSTR_21 => IsEnglish ? "level 21: nirvana" : "NIVEAU 21: LE NIRVANA";
    public static string HUSTR_22 => IsEnglish ? "level 22: the catacombs" : "NIVEAU 22: LES CATACOMBES ";
    public static string HUSTR_23 => IsEnglish ? "level 23: barrels o' fun" : "NIVEAU 23: LA GRANDE FETE ";
    public static string HUSTR_24 => IsEnglish ? "level 24: the chasm" : "NIVEAU 24: LE GOUFFRE ";
    public static string HUSTR_25 => IsEnglish ? "level 25: bloodfalls" : "NIVEAU 25: LES CHUTES DE SANG";
    public static string HUSTR_26 => IsEnglish ? "level 26: the abandoned mines" : "NIVEAU 26: LES MINES ABANDONNEES ";
    public static string HUSTR_27 => IsEnglish ? "level 27: monster condo" : "NIVEAU 27: CHEZ LES MONSTRES ";
    public static string HUSTR_28 => IsEnglish ? "level 28: the spirit world" : "NIVEAU 28: LE MONDE DE L'ESPRIT ";
    public static string HUSTR_29 => IsEnglish ? "level 29: the living end" : "NIVEAU 29: LA LIMITE ";
    public static string HUSTR_30 => IsEnglish ? "level 30: icon of sin" : "NIVEAU 30: L'ICONE DU PECHE ";

    public static string HUSTR_31 => IsEnglish ? "level 31: wolfenstein" : "NIVEAU 31: WOLFENSTEIN";
    public static string HUSTR_32 => IsEnglish ? "level 32: grosse" : "NIVEAU 32: LE MASSACRE";
}