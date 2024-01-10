using System.Collections.Generic;

namespace DoomSharp.Core.GameLogic;

public record WeaponInfo(AmmoType Ammo, StateNum UpState, StateNum DownState, StateNum ReadyState, StateNum AttackState, StateNum FlashState)
{
    public static readonly Fixed LowerSpeed = Fixed.FromInt(6);
    public static readonly Fixed RaiseSpeed = Fixed.FromInt(6);

    public static readonly Fixed WeaponBottom = Fixed.FromInt(128);
    public static readonly Fixed WeaponTop = Fixed.FromInt(32);

    public const int BFGCells = 40;

    private static readonly List<WeaponInfo> _predefined = new();

    static WeaponInfo()
    {
        // fist
        _predefined.Add(new WeaponInfo(AmmoType.NoAmmo, StateNum.S_PUNCHUP, StateNum.S_PUNCHDOWN, StateNum.S_PUNCH, StateNum.S_PUNCH1, StateNum.S_NULL));

        // pistol
        _predefined.Add(new WeaponInfo(AmmoType.Clip, StateNum.S_PISTOLUP, StateNum.S_PISTOLDOWN, StateNum.S_PISTOL, StateNum.S_PISTOL1, StateNum.S_PISTOLFLASH));

        // shotgun
        _predefined.Add(new WeaponInfo(AmmoType.Shell, StateNum.S_SGUNUP, StateNum.S_SGUNDOWN, StateNum.S_SGUN, StateNum.S_SGUN1, StateNum.S_SGUNFLASH1));

        // chaingun
        _predefined.Add(new WeaponInfo(AmmoType.Clip, StateNum.S_CHAINUP, StateNum.S_CHAINDOWN, StateNum.S_CHAIN, StateNum.S_CHAIN1, StateNum.S_CHAINFLASH1));

        // missile launcher
        _predefined.Add(new WeaponInfo(AmmoType.Missile, StateNum.S_MISSILEUP, StateNum.S_MISSILEDOWN, StateNum.S_MISSILE, StateNum.S_MISSILE1, StateNum.S_MISSILEFLASH1));

        // plasma rifle
        _predefined.Add(new WeaponInfo(AmmoType.Cell, StateNum.S_PLASMAUP, StateNum.S_PLASMADOWN, StateNum.S_PLASMA, StateNum.S_PLASMA1, StateNum.S_PLASMAFLASH1));

        // bfg 9000
        _predefined.Add(new WeaponInfo(AmmoType.Cell, StateNum.S_BFGUP, StateNum.S_BFGDOWN, StateNum.S_BFG, StateNum.S_BFG1, StateNum.S_BFGFLASH1));

        // chainsaw
        _predefined.Add(new WeaponInfo(AmmoType.NoAmmo, StateNum.S_SAWUP, StateNum.S_SAWDOWN, StateNum.S_SAW, StateNum.S_SAW1, StateNum.S_NULL));

        // super shotgun
        _predefined.Add(new WeaponInfo(AmmoType.Shell, StateNum.S_DSGUNUP, StateNum.S_DSGUNDOWN, StateNum.S_DSGUN, StateNum.S_DSGUN1, StateNum.S_DSGUNFLASH1));
    }

    public static WeaponInfo GetByType(WeaponType type)
    {
        return _predefined[(int)type];
    }
}
