using DoomSharp.Core.Data;
using DoomSharp.Core.Input;
using DoomSharp.Core.Sound;
using System;
using System.Collections.Generic;

namespace DoomSharp.Core.GameLogic;

public record State(SpriteNum Sprite, int Frame, int Tics, Action<ActionParams>? Action, StateNum NextState, int Misc1, int Misc2)
{
    private static readonly List<State> _states = new();

    static State()
    {
        AddPredefinedStates();
    }

    public static State GetSpawnState(StateNum stateNum)
    {
        return _states[(int)stateNum];
    }

    public static StateNum GetStateNum(State state)
    {
        return (StateNum)_states.IndexOf(state);
    }

    private static void ActionLight0(ActionParams actionParams)
    {
        actionParams.Player!.ExtraLight = 0;
    }

    private static void ActionLight1(ActionParams actionParams) 
    {
        actionParams.Player!.ExtraLight = 1;
    }

    private static void ActionLight2(ActionParams actionParams) 
    {
        actionParams.Player!.ExtraLight = 2;
    }

    // The player can fire the weapon
    // or change to another weapon at this time.
    // Follows after getting weapon up,
    // or after previous attack/fire sequence.
    private static void ActionWeaponReady(ActionParams actionParams)
    {
        var player = actionParams.Player!;
        var psp = actionParams.PlayerSprite!;
        var game = DoomGame.Instance.Game;

        if (player.MapObject!.State == _states[(int)StateNum.S_PLAY_ATK1] || player.MapObject.State == _states[(int)StateNum.S_PLAY_ATK2])
        {
            player.MapObject.SetState(StateNum.S_PLAY);
        }

        if (player.ReadyWeapon == WeaponType.Chainsaw && psp.State == _states[(int)StateNum.S_SAW])
        {
            DoomGame.Instance.Sound.StartSound(player.MapObject, SoundType.sfx_sawidl);
        }

        // check for change
        //  if player is dead, put the weapon away
        if (player.PendingWeapon != WeaponType.NoChange || player.Health == 0)
        {
            // change weapon
            //  (pending weapon should already be validated)
            var newState = WeaponInfo.GetByType(player.ReadyWeapon).DownState;
            game.P_SetPlayerSprite(player, PlayerSpriteType.Weapon, newState);
            return;
        }

        // check for fire
        //  the missile launcher and bfg do not auto fire
        if ((player.Command.Buttons & ButtonCode.Attack) != 0)
        {
            if (player.AttackDown == false
                 || (player.ReadyWeapon != WeaponType.Missile
                 && player.ReadyWeapon != WeaponType.Bfg))
            {
                player.AttackDown = true;
                game.P_FireWeapon(player);
                return;
            }
        }
        else
        {
            player.AttackDown = false;
        }

        // bob the weapon based on movement speed
        var angle = (128 * game.LevelTime) & DoomMath.FineMask;
        psp.SX = Fixed.Unit + (player.Bob * DoomMath.Cos(angle));
        angle &= DoomMath.FineAngleCount / 2 - 1;
        psp.SY = WeaponInfo.WeaponTop + (player.Bob * DoomMath.Sin(angle));
    }

    // Lowers current weapon,
    //  and changes weapon at bottom.
    private static void ActionLower(ActionParams actionParams)
    {
        var player = actionParams.Player!;
        var psp = actionParams.PlayerSprite!;

        psp.SY += WeaponInfo.LowerSpeed;

        // Is already down.
        if (psp.SY < WeaponInfo.WeaponBottom)
        {
            return;
        }

        // Player is dead.
        if (player.PlayerState == PlayerState.Dead)
        {
            psp.SY = WeaponInfo.WeaponBottom;

            // don't bring weapon back up
            return;
        }

        var game = DoomGame.Instance.Game;
        // The old weapon has been lowered off the screen,
        // so change the weapon and start raising it
        if (player.Health == 0)
        {
            // Player is dead, so keep the weapon off screen.
            game.P_SetPlayerSprite(player, PlayerSpriteType.Weapon, StateNum.S_NULL);
            return;
        }

        player.ReadyWeapon = player.PendingWeapon;

        game.P_BringUpWeapon(player);
    }
    
    private static void ActionRaise(ActionParams actionParams) 
    {
        var player = actionParams.Player!;
        var psp = actionParams.PlayerSprite!;
        psp.SY -= WeaponInfo.RaiseSpeed;

        if (psp.SY > WeaponInfo.WeaponTop)
        {
            return;
        }

        psp.SY = WeaponInfo.WeaponTop;

        // The weapon has been raised all the way,
        //  so change to the ready state.
        var newState = WeaponInfo.GetByType(player.ReadyWeapon).ReadyState;
        DoomGame.Instance.Game.P_SetPlayerSprite(player, PlayerSpriteType.Weapon, newState);
    }

    private static void ActionPunch(ActionParams actionParams)
    {
        var player = actionParams.Player!;

        var damage = (DoomRandom.P_Random() % 10 + 1) << 1;
        if (player.Powers[(int)PowerUpType.Strength] != 0)
        {
            damage *= 10;
        }

        var angle = player.MapObject!.Angle;
        angle += new Angle((DoomRandom.P_Random() - DoomRandom.P_Random()) << 18);
        var slope = DoomGame.Instance.Game.P_AimLineAttack(player.MapObject, angle, Constants.MeleeRange);
        DoomGame.Instance.Game.P_LineAttack(player.MapObject, angle, Constants.MeleeRange, slope, damage);

        // turn to face target
        var lineTarget = DoomGame.Instance.Game.LineTarget;
        if (lineTarget != null)
        {
            DoomGame.Instance.Sound.StartSound(player.MapObject, SoundType.sfx_punch);
            player.MapObject.Angle = DoomGame.Instance.Renderer.PointToAngle2(
                    player.MapObject.X,
                    player.MapObject.Y,
                    lineTarget.X,
                    lineTarget.Y);
        }
    }

    /// <summary>
    /// The player can re-fire the weapon
    /// without lowering it entirely.
    /// </summary>
    private static void ActionReFire(ActionParams actionParams)
    {
        // check for fire
        //  (if a weaponchange is pending, let it go through instead)
        var player = actionParams.Player!;
        if ((player.Command.Buttons & ButtonCode.Attack) != 0 &&
            player.PendingWeapon == WeaponType.NoChange &&
            player.Health != 0)
        {
            player.Refire++;
            DoomGame.Instance.Game.P_FireWeapon(player);
        }
        else
        {
            player.Refire = 0;
            DoomGame.Instance.Game.P_CheckAmmo(player);
        }
    }

    private static void ActionFirePistol(ActionParams actionParams)
    {
        var player = actionParams.Player!;
        var weapon = WeaponInfo.GetByType(player.ReadyWeapon);
        DoomGame.Instance.Sound.StartSound(player.MapObject, SoundType.sfx_pistol);
        
        player.MapObject!.SetState(StateNum.S_PLAY_ATK2);
        player.Ammo[(int)weapon.Ammo]--;

        var game = DoomGame.Instance.Game;
        game.P_SetPlayerSprite(player, PlayerSpriteType.Flash, weapon.FlashState);

        game.P_BulletSlope(player.MapObject);
        game.P_GunShot(player.MapObject, player.Refire == 0);
    }

    private static void ActionFireShotgun(ActionParams actionParams)
    {
        var player = actionParams.Player!;
        var weapon = WeaponInfo.GetByType(player.ReadyWeapon);
        DoomGame.Instance.Sound.StartSound(player.MapObject, SoundType.sfx_shotgn);
        
        player.MapObject!.SetState(StateNum.S_PLAY_ATK2);
        player.Ammo[(int)weapon.Ammo]--;

        var game = DoomGame.Instance.Game;
        game.P_SetPlayerSprite(player, PlayerSpriteType.Flash, weapon.FlashState);

        game.P_BulletSlope(player.MapObject);
        for (var i = 0; i < 7; i++)
        {
            game.P_GunShot(player.MapObject, false);
        }
    }

    private static void ActionFireShotgun2(ActionParams actionParams)
    {
        var player = actionParams.Player!;
        var weapon = WeaponInfo.GetByType(player.ReadyWeapon);
        DoomGame.Instance.Sound.StartSound(player.MapObject, SoundType.sfx_dshtgn);
        
        player.MapObject!.SetState(StateNum.S_PLAY_ATK2);
        player.Ammo[(int)weapon.Ammo] -= 2;

        var game = DoomGame.Instance.Game;
        game.P_SetPlayerSprite(player, PlayerSpriteType.Flash, weapon.FlashState);

        game.P_BulletSlope(player.MapObject);
        for (var i = 0; i < 20; i++)
        {
            var damage = 5 * (DoomRandom.P_Random() % 3 + 1);
            var angle = player.MapObject.Angle;
            angle += new Angle((DoomRandom.P_Random() - DoomRandom.P_Random()) << 19);

            game.P_LineAttack(
                player.MapObject, 
                angle, 
                Constants.MissileRange, 
                game.BulletSlope + new Fixed((DoomRandom.P_Random() - DoomRandom.P_Random()) << 5), 
                damage);
        }
    }

    private static void ActionCheckReload(ActionParams actionParams)
    {
        DoomGame.Instance.Game.P_CheckAmmo(actionParams.Player!);
    }

    private static void ActionOpenShotgun2(ActionParams actionParams) { }
    private static void ActionLoadShotgun2(ActionParams actionParams) { }
    private static void ActionCloseShotgun2(ActionParams actionParams) { }

    private static void ActionFireCGun(ActionParams actionParams)
    {
        var player = actionParams.Player!;
        var psp = actionParams.PlayerSprite!;

        var weapon = WeaponInfo.GetByType(player.ReadyWeapon);
        DoomGame.Instance.Sound.StartSound(player.MapObject, SoundType.sfx_pistol);
        
        if (player.Ammo[(int)weapon.Ammo] == 0)
        {
            return;
        }

        player.MapObject!.SetState(StateNum.S_PLAY_ATK2);
        player.Ammo[(int)weapon.Ammo]--;

        var game = DoomGame.Instance.Game;
        var stateNum = State.GetStateNum(psp.State!);
        game.P_SetPlayerSprite(player, PlayerSpriteType.Flash, (StateNum)(weapon.FlashState + (int)stateNum - StateNum.S_CHAIN1));

        game.P_BulletSlope(player.MapObject);
        game.P_GunShot(player.MapObject, player.Refire == 0);
    }

    private static void ActionGunFlash(ActionParams actionParams)
    {
        var player = actionParams.Player!;

        player.MapObject!.SetState(StateNum.S_PLAY_ATK2);
        DoomGame.Instance.Game.P_SetPlayerSprite(player, PlayerSpriteType.Flash,
            WeaponInfo.GetByType(player.ReadyWeapon).FlashState);
    }

    private static void ActionFireMissile(ActionParams actionParams)
    {
        var player = actionParams.Player!;
        var weapon = WeaponInfo.GetByType(player.ReadyWeapon);

        player.Ammo[(int)weapon.Ammo]--;

        var game = DoomGame.Instance.Game;
        game.P_SpawnPlayerMissile(player.MapObject!, MapObjectType.MT_ROCKET);
    }

    private static void ActionSaw(ActionParams actionParams)
    {
        var player = actionParams.Player!;

        var damage = 2 * (DoomRandom.P_Random() % 10 + 1);
        var angle = player.MapObject!.Angle;
        angle += new Angle((DoomRandom.P_Random() - DoomRandom.P_Random()) << 18);
        
        // use meleerange + 1 so the puff doesn't skip the flash
        var slope = DoomGame.Instance.Game.P_AimLineAttack(player.MapObject, angle, new Fixed(Constants.MeleeRange.Value + 1));
        DoomGame.Instance.Game.P_LineAttack(player.MapObject, angle, new Fixed(Constants.MeleeRange.Value + 1), slope, damage);

        // turn to face target
        var lineTarget = DoomGame.Instance.Game.LineTarget;
        if (lineTarget == null)
        {
            DoomGame.Instance.Sound.StartSound(player.MapObject, SoundType.sfx_sawful);
            return;
        }

        DoomGame.Instance.Sound.StartSound(player.MapObject, SoundType.sfx_sawhit);
        
        // turn to face target
        player.MapObject.Angle = DoomGame.Instance.Renderer.PointToAngle2(
                player.MapObject.X,
                player.MapObject.Y,
                lineTarget.X,
                lineTarget.Y);

        if (angle - player.MapObject.Angle > Angle.Angle180)
        {
            if (angle - player.MapObject.Angle < -Angle.Angle90 / 20)
                player.MapObject.Angle = angle + Angle.Angle90 / 21;
            else
                player.MapObject.Angle -= Angle.Angle90 / 20;
        }
        else
        {
            if (angle - player.MapObject.Angle > Angle.Angle90 / 20)
                player.MapObject.Angle = angle - Angle.Angle90 / 21;
            else
                player.MapObject.Angle += Angle.Angle90 / 20;
        }
        player.MapObject.Flags |= MapObjectFlag.MF_JUSTATTACKED;
    }

    private static void ActionFirePlasma(ActionParams actionParams)
    {
        var player = actionParams.Player!;
        var weapon = WeaponInfo.GetByType(player.ReadyWeapon);

        player.Ammo[(int)weapon.Ammo]--;

        var game = DoomGame.Instance.Game;
        
        game.P_SetPlayerSprite(player, PlayerSpriteType.Flash, weapon.FlashState + (DoomRandom.P_Random() & 1));
        game.P_SpawnPlayerMissile(player.MapObject!, MapObjectType.MT_PLASMA);
    }

    private static void ActionBFGsound(ActionParams actionParams)
    {
        var player = actionParams.Player!;
        DoomGame.Instance.Sound.StartSound(player.MapObject, SoundType.sfx_bfg);
    }

    private static void ActionFireBFG(ActionParams actionParams)
    {
        var player = actionParams.Player!;
        var weapon = WeaponInfo.GetByType(player.ReadyWeapon);

        player.Ammo[(int)weapon.Ammo] -= WeaponInfo.BFGCells;

        var game = DoomGame.Instance.Game;
        game.P_SpawnPlayerMissile(player.MapObject!, MapObjectType.MT_BFG);
    }

    private static void ActionBFGSpray(ActionParams actionParams)
    {
        var mo = actionParams.MapObject!;
        var game = DoomGame.Instance.Game;

        for (var i = 0; i < 40; i++)
        {
            var an = new Angle((uint)(mo.Angle.Value - Angle.Angle90.Value / 2 + Angle.Angle90.Value / 40 * i));

            // mo->target is the originator (player) of the missile
            game.P_AimLineAttack(mo.Target!, an, Fixed.FromInt(16 * 64));

            if (game.LineTarget == null)
            {
                continue;
            }

            game.P_SpawnMapObject(
                game.LineTarget.X,
                game.LineTarget.Y,
                game.LineTarget.Z + (game.LineTarget.Height >> 2),
                MapObjectType.MT_EXTRABFG);

            var damage = 0;
            for (var j = 0; j < 15; j++)
            {
                damage += (DoomRandom.P_Random() & 7) + 1;
            }

            MapObject.DamageMapObject(game.LineTarget, mo.Target, mo.Target, damage);
        }
    }

    private static void ActionExplode(ActionParams actionParams)
    {
        var thingy = actionParams.MapObject!;
        DoomGame.Instance.Game.P_RadiusAttack(thingy, thingy.Target!, 128);
    }

    private static void ActionPain(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;
        if (actor.Info.PainSound != SoundType.sfx_None)
        {
            DoomGame.Instance.Sound.StartSound(actor, actor.Info.PainSound);
        }
    }

    private static void ActionPlayerScream(ActionParams actionParams)
    {
        var mo = actionParams.MapObject!;

        // Default death sound.
        var sound = SoundType.sfx_pldeth;

        if (DoomGame.Instance.GameMode == GameMode.Commercial && mo.Health < -50)
        {
            // IF THE PLAYER DIES
            // LESS THAN -50% WITHOUT GIBBING
            sound = SoundType.sfx_pdiehi;
        }

        DoomGame.Instance.Sound.StartSound(mo, sound);
    }

    private static void ActionFall(ActionParams actionParams)
    {
        // actor is on the ground, it can be walked over
        actionParams.MapObject!.Flags &= ~MapObjectFlag.MF_SOLID;

        // So change this if corpse objects
        // are meant to be obstacles.
    }

    private static void ActionXScream(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;
        DoomGame.Instance.Sound.StartSound(actor, SoundType.sfx_slop);
    }

    /// <summary>
    /// Stay in state until a player is sighted.
    /// </summary>
    private static void ActionLook(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;

        actor.Threshold = 0; // any shot will wake up
        var target = actor.SubSector?.Sector?.SoundTarget;

        if (target != null && (target.Flags & MapObjectFlag.MF_SHOOTABLE) != 0)
        {
            actor.Target = target;
            if ((actor.Flags & MapObjectFlag.MF_AMBUSH) != 0)
            {
                if (DoomGame.Instance.Game.P_CheckSight(actor, actor.Target))
                {
                    SeeYou();
                    return;
                }
            }

            SeeYou();
            return;
        }

        if (!DoomGame.Instance.Game.P_LookForPlayers(actor, false))
        {
            return;
        }

        SeeYou();

        // go into chase state
        void SeeYou()
        {
            if (actor.Info.SeeSound != SoundType.sfx_None)
            {
                SoundType sound;

                switch (actor.Info.SeeSound)
                {
                    case SoundType.sfx_posit1:
                    case SoundType.sfx_posit2:
                    case SoundType.sfx_posit3:
                        sound = SoundType.sfx_posit1 + DoomRandom.P_Random() % 3;
                        break;

                    case SoundType.sfx_bgsit1:
                    case SoundType.sfx_bgsit2:
                        sound = SoundType.sfx_bgsit1 + DoomRandom.P_Random() % 2;
                        break;

                    default:
                        sound = actor.Info.SeeSound;
                        break;
                }

                if (actor.Type is MapObjectType.MT_SPIDER or MapObjectType.MT_CYBORG)
                {
                    // full volume
                    DoomGame.Instance.Sound.StartSound(null, sound);
                }
                else
                {
                    DoomGame.Instance.Sound.StartSound(actor, sound);
                }
            }

            actor.SetState(actor.Info.SeeState);
        }
    }

    /// <summary>
    /// Actor has a melee attack, so it tries to close as fast as possible
    /// </summary>
    private static void ActionChase(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;
        var game = DoomGame.Instance.Game;

        if (actor.ReactionTime != 0)
        {
            actor.ReactionTime--;
        }

        // modify target threshold
        if (actor.Threshold != 0)
        {
            if (actor.Target is not { Health: > 0 })
            {
                actor.Threshold = 0;
            }
            else
            {
                actor.Threshold--;
            }
        }

        // turn towards movement direction if not there yet
        if ((int)actor.MoveDir < 8)
        {
            actor.Angle = new Angle((uint)(actor.Angle.Value & (7 << 29)));
            var delta = (int)(actor.Angle.Value - ((int)actor.MoveDir << 29));

            if (delta > 0)
            {
                actor.Angle -= Angle.Angle90 / 2;
            }
            else if (delta < 0)
            {
                actor.Angle += Angle.Angle90 / 2;
            }
        }

        if (actor.Target == null || (actor.Target.Flags & MapObjectFlag.MF_SHOOTABLE) == 0)
        {
            // look for a new target
            if (game.P_LookForPlayers(actor, true))
            {
                return; // got a new target
            }

            actor.SetState(actor.Info.SpawnState);
        }

        // do not attack twice in a row
        if ((actor.Flags & MapObjectFlag.MF_JUSTATTACKED) != 0)
        {
            actor.Flags &= ~MapObjectFlag.MF_JUSTATTACKED;
            if (game.GameSkill != SkillLevel.Nightmare /* && !fastparm */)
            {
                game.P_NewChaseDir(actor);
            }

            return;
        }

        // check for melee attack
        if (actor.Info.MeleeState != StateNum.S_NULL && game.P_CheckMeleeRange(actor))
        {
            if (actor.Info.AttackSound != SoundType.sfx_None)
            {
                DoomGame.Instance.Sound.StartSound(actor, actor.Info.AttackSound);
            }

            actor.SetState(actor.Info.MeleeState);
            return;
        }

        // check for missile attack
        if (actor.Info.MissileState != StateNum.S_NULL)
        {
            if (game.GameSkill < SkillLevel.Nightmare && actor.MoveCount != 0 /* && !fastparm */)
            {
                NoMissile();
                return;
            }

            if (!game.P_CheckMissileRange(actor))
            {
                NoMissile();
                return;
            }

            actor.SetState(actor.Info.MissileState);
            actor.Flags |= MapObjectFlag.MF_JUSTATTACKED;
            return;
        }

        NoMissile();

        void NoMissile()
        {
            // possibly choose another target
            if (game.NetGame && actor.Threshold == 0 && !game.P_CheckSight(actor, actor.Target!))
            {
                if (game.P_LookForPlayers(actor, true))
                {
                    return; // got a new target
                }
            }

            // chase towards player
            if (--actor.MoveCount < 0 || !game.P_Move(actor))
            {
                game.P_NewChaseDir(actor);
            }

            // make active sound
            if (actor.Info.ActiveSound != SoundType.sfx_None && DoomRandom.P_Random() < 3)
            {
                DoomGame.Instance.Sound.StartSound(actor, actor.Info.ActiveSound);
            }
        }
    }

    private static void ActionFaceTarget(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;
        if (actor.Target is null)
        {
            return;
        }

        actor.Flags &= ~MapObjectFlag.MF_AMBUSH;
        actor.Angle = DoomGame.Instance.Renderer.PointToAngle2(actor.X, actor.Y, actor.Target.X, actor.Target.Y);

        if ((actor.Target.Flags & MapObjectFlag.MF_SHADOW) != 0)
        {
            actor.Angle += new Angle((DoomRandom.P_Random() - DoomRandom.P_Random()) << 21);
        }
    }

    private static void ActionPosAttack(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;
        if (actor.Target is null)
        {
            return;
        }

        var game = DoomGame.Instance.Game;
        
        ActionFaceTarget(actionParams);
        
        var angle = actor.Angle;
        var slope = game.P_AimLineAttack(actor, angle, Constants.MissileRange);

        DoomGame.Instance.Sound.StartSound(actor, SoundType.sfx_pistol);
        angle += new Angle((DoomRandom.P_Random() - DoomRandom.P_Random()) << 20);
        var damage = ((DoomRandom.P_Random() % 5) + 1) * 3;
        game.P_LineAttack(actor, angle, Constants.MissileRange, slope, damage);
    }

    private static void ActionScream(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;
        SoundType sound;

        switch (actor.Info.DeathSound)
        {
            case 0:
                return;

            case SoundType.sfx_podth1:
            case SoundType.sfx_podth2:
            case SoundType.sfx_podth3:
                sound = SoundType.sfx_podth1 + DoomRandom.P_Random() % 3;
                break;

            case SoundType.sfx_bgdth1:
            case SoundType.sfx_bgdth2:
                sound = SoundType.sfx_bgdth1 + DoomRandom.P_Random() % 2;
                break;

            default:
                sound = actor.Info.DeathSound;
                break;
        }

        // Check for bosses.
        if (actor.Type is MapObjectType.MT_SPIDER or MapObjectType.MT_CYBORG)
        {
            // full volume
            DoomGame.Instance.Sound.StartSound(null, sound);
        }
        else
        {
            DoomGame.Instance.Sound.StartSound(actor, sound);
        }
    }

    private static void ActionSPosAttack(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;
        if (actor.Target is null)
        {
            return;
        }

        var game = DoomGame.Instance.Game;
        DoomGame.Instance.Sound.StartSound(actor, SoundType.sfx_shotgn);
        ActionFaceTarget(actionParams);
        var bangle = actor.Angle;
        var slope = game.P_AimLineAttack(actor, bangle, Constants.MissileRange);

        for (var i = 0; i < 3; i++)
        {
            var angle = bangle + new Angle((DoomRandom.P_Random() - DoomRandom.P_Random()) << 20);
            var damage = ((DoomRandom.P_Random() % 5) + 1) * 3;
            game.P_LineAttack(actor, angle, Constants.MissileRange, slope, damage);
        }
    }

    private static void ActionCPosAttack(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;
        if (actor.Target is null)
        {
            return;
        }

        var game = DoomGame.Instance.Game;
        DoomGame.Instance.Sound.StartSound(actor, SoundType.sfx_shotgn);
        ActionFaceTarget(actionParams);
        var bangle = actor.Angle;
        var slope = game.P_AimLineAttack(actor, bangle, Constants.MissileRange);

        var angle = bangle + new Angle((DoomRandom.P_Random() - DoomRandom.P_Random()) << 20);
        var damage = ((DoomRandom.P_Random() % 5) + 1) * 3;
        game.P_LineAttack(actor, angle, Constants.MissileRange, slope, damage);
    }

    private static void ActionCPosRefire(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;
        var game = DoomGame.Instance.Game;

        // keep firing unless target got out of sight
        ActionFaceTarget(actionParams);

        if (DoomRandom.P_Random() < 40)
        {
            return;
        }

        if (actor.Target is null
            || actor.Target.Health <= 0
            || !game.P_CheckSight(actor, actor.Target))
        {
            actor.SetState(actor.Info.SeeState);
        }
    }

    private static void ActionSpidRefire(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;
        var game = DoomGame.Instance.Game;

        // keep firing unless target got out of sight
        ActionFaceTarget(actionParams);

        if (DoomRandom.P_Random() < 10)
        {
            return;
        }

        if (actor.Target is null
            || actor.Target.Health <= 0
            || !game.P_CheckSight(actor, actor.Target))
        {
            actor.SetState(actor.Info.SeeState);
        }
    }

    private static void ActionBspiAttack(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;
        var game = DoomGame.Instance.Game;

        if (actor.Target is null)
        {
            return;
        }

        ActionFaceTarget(actionParams);

        // launch a missile
        game.P_SpawnMissile(actor, actor.Target, MapObjectType.MT_ARACHPLAZ);
    }

    private static void ActionTroopAttack(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;
        var game = DoomGame.Instance.Game;

        if (actor.Target is null)
        {
            return;
        }

        ActionFaceTarget(actionParams);
        if (game.P_CheckMeleeRange(actor))
        {
            DoomGame.Instance.Sound.StartSound(actor, SoundType.sfx_claw);
            var damage = (DoomRandom.P_Random() % 8 + 1) * 3;
            MapObject.DamageMapObject(actor.Target, actor, actor, damage);
            return;
        }

        // launch a missile
        game.P_SpawnMissile(actor, actor.Target, MapObjectType.MT_TROOPSHOT);
    }

    private static void ActionSargAttack(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;
        var game = DoomGame.Instance.Game;

        if (actor.Target is null)
        {
            return;
        }

        ActionFaceTarget(actionParams);
        if (game.P_CheckMeleeRange(actor))
        {
            var damage = ((DoomRandom.P_Random() % 10) + 1) * 4;
            MapObject.DamageMapObject(actor.Target, actor, actor, damage);
        }
    }

    private static void ActionHeadAttack(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;
        var game = DoomGame.Instance.Game;

        if (actor.Target is null)
        {
            return;
        }

        ActionFaceTarget(actionParams);
        if (game.P_CheckMeleeRange(actor))
        {
            var damage = (DoomRandom.P_Random() % 6 + 1) * 10;
            MapObject.DamageMapObject(actor.Target, actor, actor, damage);
            return;
        }

        // launch a missile
        game.P_SpawnMissile(actor, actor.Target, MapObjectType.MT_HEADSHOT);
    }

    private static void ActionCyberAttack(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;
        var game = DoomGame.Instance.Game;

        if (actor.Target is null)
        {
            return;
        }

        ActionFaceTarget(actionParams);
        game.P_SpawnMissile(actor, actor.Target, MapObjectType.MT_ROCKET);
    }

    private static void ActionBruisAttack(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;
        var game = DoomGame.Instance.Game;

        if (actor.Target is null)
        {
            return;
        }

        ActionFaceTarget(actionParams);
        if (game.P_CheckMeleeRange(actor))
        {
            DoomGame.Instance.Sound.StartSound(actor, SoundType.sfx_claw);
            var damage = (DoomRandom.P_Random() % 8 + 1) * 10;
            MapObject.DamageMapObject(actor.Target, actor, actor, damage);
            return;
        }

        // launch a missile
        game.P_SpawnMissile(actor, actor.Target, MapObjectType.MT_BRUISERSHOT);
    }

    private static void ActionSkelMissile(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;
        var game = DoomGame.Instance.Game;

        if (actor.Target is null)
        {
            return;
        }

        ActionFaceTarget(actionParams);
        actor.Z += Fixed.FromInt(16); // so missile spawns higher
        var mo = game.P_SpawnMissile(actor, actor.Target, MapObjectType.MT_TRACER);
        actor.Z -= Fixed.FromInt(16); // back to normal

        mo.X += mo.MomX;
        mo.Y += mo.MomY;
        mo.Tracer = actor.Target;
    }

    private static void ActionTracer(ActionParams actionParams)
    {
        var actor = actionParams.MapObject!;
        var game = DoomGame.Instance.Game;

        if ((game.GameTic & 3) != 0)
        {
            return;
        }

        // spawn a puff of smoke behind the rocket		
        game.P_SpawnPuff(actor.X, actor.Y, actor.Z);

        var th = game.P_SpawnMapObject(actor.X - actor.MomX,
            actor.Y - actor.MomY,
            actor.Z, MapObjectType.MT_SMOKE);

        th.MomZ = Fixed.Unit;
        th.Tics -= DoomRandom.P_Random() & 3;
        if (th.Tics < 1)
        {
            th.Tics = 1;
        }

        // adjust direction
        var dest = actor.Tracer;

        if (dest is not { Health: > 0 })
        {
            return;
        }

        // change angle
        var exact = DoomGame.Instance.Renderer.PointToAngle2(actor.X, actor.Y, dest.X, dest.Y);

        if (exact != actor.Angle)
        {
            if (exact - actor.Angle > Angle.Angle180)
            {
                actor.Angle -= Angle.TraceAngle;
                if (exact - actor.Angle < Angle.Angle180)
                {
                    actor.Angle = exact;
                }
            }
            else
            {
                actor.Angle += Angle.TraceAngle;
                if (exact - actor.Angle > Angle.Angle180)
                {
                    actor.Angle = exact;
                }
            }
        }

        // BUG May need to convert speed into a Fixed first to get FixedMul behavior
        actor.MomX = actor.Info.Speed * DoomMath.Cos(exact);
        actor.MomY = actor.Info.Speed * DoomMath.Sin(exact);

        // change slope
        var dist = game.P_AproxDistance(dest.X - actor.X, dest.Y - actor.Y).Value;

        dist /= actor.Info.Speed;

        if (dist < 1)
        {
            dist = 1;
        }

        var slope = (dest.Z + Fixed.FromInt(40) - actor.Z) / dist;

        var fracOver8 = Fixed.Unit / 8;
        if (slope < actor.MomZ)
        {
            actor.MomZ -= fracOver8;
        }
        else
        {
            actor.MomZ += fracOver8;
        }
    }

    private static void ActionVileChase(ActionParams actionParams) { }
    private static void ActionVileStart(ActionParams actionParams) { }
    private static void ActionVileTarget(ActionParams actionParams) { }
    private static void ActionVileAttack(ActionParams actionParams) { }
    private static void ActionStartFire(ActionParams actionParams) { }
    private static void ActionFire(ActionParams actionParams) { }
    private static void ActionFireCrackle(ActionParams actionParams) { }
    
    private static void ActionSkelWhoosh(ActionParams actionParams) { }
    private static void ActionSkelFist(ActionParams actionParams) { }
    private static void ActionFatRaise(ActionParams actionParams) { }
    private static void ActionFatAttack1(ActionParams actionParams) { }
    private static void ActionFatAttack2(ActionParams actionParams) { }
    private static void ActionFatAttack3(ActionParams actionParams) { }
    private static void ActionBossDeath(ActionParams actionParams) { }
    private static void ActionSkullAttack(ActionParams actionParams) { }
    private static void ActionMetal(ActionParams actionParams) { }
    private static void ActionBabyMetal(ActionParams actionParams) { }
    private static void ActionHoof(ActionParams actionParams) { }
    private static void ActionPainAttack(ActionParams actionParams) { }
    private static void ActionPainDie(ActionParams actionParams) { }
    private static void ActionKeenDie(ActionParams actionParams) { }
    private static void ActionBrainPain(ActionParams actionParams) { }
    private static void ActionBrainScream(ActionParams actionParams) { }
    private static void ActionBrainDie(ActionParams actionParams) { }
    private static void ActionBrainAwake(ActionParams actionParams) { }
    private static void ActionBrainSpit(ActionParams actionParams) { }
    private static void ActionSpawnSound(ActionParams actionParams) { }
    private static void ActionSpawnFly(ActionParams actionParams) { }
    private static void ActionBrainExplode(ActionParams actionParams) { }

    private static void AddPredefinedStates()
    {
        _states.Add(new State(SpriteNum.SPR_TROO, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_NULL
        _states.Add(new State(SpriteNum.SPR_SHTG, 4, 0, ActionLight0, StateNum.S_NULL, 0, 0));  // S_LIGHTDONE
        _states.Add(new State(SpriteNum.SPR_PUNG, 0, 1, ActionWeaponReady, StateNum.S_PUNCH, 0, 0));    // S_PUNCH
        _states.Add(new State(SpriteNum.SPR_PUNG, 0, 1, ActionLower, StateNum.S_PUNCHDOWN, 0, 0));  // S_PUNCHDOWN
        _states.Add(new State(SpriteNum.SPR_PUNG, 0, 1, ActionRaise, StateNum.S_PUNCHUP, 0, 0)); // S_PUNCHUP
        _states.Add(new State(SpriteNum.SPR_PUNG, 1, 4, null, StateNum.S_PUNCH2, 0, 0));        // S_PUNCH1
        _states.Add(new State(SpriteNum.SPR_PUNG, 2, 4, ActionPunch, StateNum.S_PUNCH3, 0, 0)); // S_PUNCH2
        _states.Add(new State(SpriteNum.SPR_PUNG, 3, 5, null, StateNum.S_PUNCH4, 0, 0));     // S_PUNCH3
        _states.Add(new State(SpriteNum.SPR_PUNG, 2, 4, null, StateNum.S_PUNCH5, 0, 0));     // S_PUNCH4
        _states.Add(new State(SpriteNum.SPR_PUNG, 1, 5, ActionReFire, StateNum.S_PUNCH, 0, 0)); // S_PUNCH5
        _states.Add(new State(SpriteNum.SPR_PISG, 0, 1, ActionWeaponReady, StateNum.S_PISTOL, 0, 0));// S_PISTOL
        _states.Add(new State(SpriteNum.SPR_PISG, 0, 1, ActionLower, StateNum.S_PISTOLDOWN, 0, 0)); // S_PISTOLDOWN
        _states.Add(new State(SpriteNum.SPR_PISG, 0, 1, ActionRaise, StateNum.S_PISTOLUP, 0, 0));   // S_PISTOLUP
        _states.Add(new State(SpriteNum.SPR_PISG, 0, 4, null, StateNum.S_PISTOL2, 0, 0));   // S_PISTOL1
        _states.Add(new State(SpriteNum.SPR_PISG, 1, 6, ActionFirePistol, StateNum.S_PISTOL3, 0, 0));// S_PISTOL2
        _states.Add(new State(SpriteNum.SPR_PISG, 2, 4, null, StateNum.S_PISTOL4, 0, 0));   // S_PISTOL3
        _states.Add(new State(SpriteNum.SPR_PISG, 1, 5, ActionReFire, StateNum.S_PISTOL, 0, 0));    // S_PISTOL4
        _states.Add(new State(SpriteNum.SPR_PISF, 32768, 7, ActionLight1, StateNum.S_LIGHTDONE, 0, 0)); // S_PISTOLFLASH
        _states.Add(new State(SpriteNum.SPR_SHTG, 0, 1, ActionWeaponReady, StateNum.S_SGUN, 0, 0)); // S_SGUN
        _states.Add(new State(SpriteNum.SPR_SHTG, 0, 1, ActionLower, StateNum.S_SGUNDOWN, 0, 0));   // S_SGUNDOWN
        _states.Add(new State(SpriteNum.SPR_SHTG, 0, 1, ActionRaise, StateNum.S_SGUNUP, 0, 0)); // S_SGUNUP
        _states.Add(new State(SpriteNum.SPR_SHTG, 0, 3, null, StateNum.S_SGUN2, 0, 0)); // S_SGUN1
        _states.Add(new State(SpriteNum.SPR_SHTG, 0, 7, ActionFireShotgun, StateNum.S_SGUN3, 0, 0));    // S_SGUN2
        _states.Add(new State(SpriteNum.SPR_SHTG, 1, 5, null, StateNum.S_SGUN4, 0, 0)); // S_SGUN3
        _states.Add(new State(SpriteNum.SPR_SHTG, 2, 5, null, StateNum.S_SGUN5, 0, 0)); // S_SGUN4
        _states.Add(new State(SpriteNum.SPR_SHTG, 3, 4, null, StateNum.S_SGUN6, 0, 0)); // S_SGUN5
        _states.Add(new State(SpriteNum.SPR_SHTG, 2, 5, null, StateNum.S_SGUN7, 0, 0)); // S_SGUN6
        _states.Add(new State(SpriteNum.SPR_SHTG, 1, 5, null, StateNum.S_SGUN8, 0, 0)); // S_SGUN7
        _states.Add(new State(SpriteNum.SPR_SHTG, 0, 3, null, StateNum.S_SGUN9, 0, 0)); // S_SGUN8
        _states.Add(new State(SpriteNum.SPR_SHTG, 0, 7, ActionReFire, StateNum.S_SGUN, 0, 0));  // S_SGUN9
        _states.Add(new State(SpriteNum.SPR_SHTF, 32768, 4, ActionLight1, StateNum.S_SGUNFLASH2, 0, 0));    // S_SGUNFLASH1
        _states.Add(new State(SpriteNum.SPR_SHTF, 32769, 3, ActionLight2, StateNum.S_LIGHTDONE, 0, 0)); // S_SGUNFLASH2
        _states.Add(new State(SpriteNum.SPR_SHT2, 0, 1, ActionWeaponReady, StateNum.S_DSGUN, 0, 0));    // S_DSGUN
        _states.Add(new State(SpriteNum.SPR_SHT2, 0, 1, ActionLower, StateNum.S_DSGUNDOWN, 0, 0));  // S_DSGUNDOWN
        _states.Add(new State(SpriteNum.SPR_SHT2, 0, 1, ActionRaise, StateNum.S_DSGUNUP, 0, 0));    // S_DSGUNUP
        _states.Add(new State(SpriteNum.SPR_SHT2, 0, 3, null, StateNum.S_DSGUN2, 0, 0));    // S_DSGUN1
        _states.Add(new State(SpriteNum.SPR_SHT2, 0, 7, ActionFireShotgun2, StateNum.S_DSGUN3, 0, 0));  // S_DSGUN2
        _states.Add(new State(SpriteNum.SPR_SHT2, 1, 7, null, StateNum.S_DSGUN4, 0, 0));    // S_DSGUN3
        _states.Add(new State(SpriteNum.SPR_SHT2, 2, 7, ActionCheckReload, StateNum.S_DSGUN5, 0, 0));   // S_DSGUN4
        _states.Add(new State(SpriteNum.SPR_SHT2, 3, 7, ActionOpenShotgun2, StateNum.S_DSGUN6, 0, 0));  // S_DSGUN5
        _states.Add(new State(SpriteNum.SPR_SHT2, 4, 7, null, StateNum.S_DSGUN7, 0, 0));    // S_DSGUN6
        _states.Add(new State(SpriteNum.SPR_SHT2, 5, 7, ActionLoadShotgun2, StateNum.S_DSGUN8, 0, 0));  // S_DSGUN7
        _states.Add(new State(SpriteNum.SPR_SHT2, 6, 6, null, StateNum.S_DSGUN9, 0, 0));    // S_DSGUN8
        _states.Add(new State(SpriteNum.SPR_SHT2, 7, 6, ActionCloseShotgun2, StateNum.S_DSGUN10, 0, 0));    // S_DSGUN9
        _states.Add(new State(SpriteNum.SPR_SHT2, 0, 5, ActionReFire, StateNum.S_DSGUN, 0, 0)); // S_DSGUN10
        _states.Add(new State(SpriteNum.SPR_SHT2, 1, 7, null, StateNum.S_DSNR2, 0, 0)); // S_DSNR1
        _states.Add(new State(SpriteNum.SPR_SHT2, 0, 3, null, StateNum.S_DSGUNDOWN, 0, 0)); // S_DSNR2
        _states.Add(new State(SpriteNum.SPR_SHT2, 32776, 5, ActionLight1, StateNum.S_DSGUNFLASH2, 0, 0));   // S_DSGUNFLASH1
        _states.Add(new State(SpriteNum.SPR_SHT2, 32777, 4, ActionLight2, StateNum.S_LIGHTDONE, 0, 0)); // S_DSGUNFLASH2
        _states.Add(new State(SpriteNum.SPR_CHGG, 0, 1, ActionWeaponReady, StateNum.S_CHAIN, 0, 0));    // S_CHAIN
        _states.Add(new State(SpriteNum.SPR_CHGG, 0, 1, ActionLower, StateNum.S_CHAINDOWN, 0, 0));  // S_CHAINDOWN
        _states.Add(new State(SpriteNum.SPR_CHGG, 0, 1, ActionRaise, StateNum.S_CHAINUP, 0, 0));    // S_CHAINUP
        _states.Add(new State(SpriteNum.SPR_CHGG, 0, 4, ActionFireCGun, StateNum.S_CHAIN2, 0, 0));  // S_CHAIN1
        _states.Add(new State(SpriteNum.SPR_CHGG, 1, 4, ActionFireCGun, StateNum.S_CHAIN3, 0, 0));  // S_CHAIN2
        _states.Add(new State(SpriteNum.SPR_CHGG, 1, 0, ActionReFire, StateNum.S_CHAIN, 0, 0)); // S_CHAIN3
        _states.Add(new State(SpriteNum.SPR_CHGF, 32768, 5, ActionLight1, StateNum.S_LIGHTDONE, 0, 0)); // S_CHAINFLASH1
        _states.Add(new State(SpriteNum.SPR_CHGF, 32769, 5, ActionLight2, StateNum.S_LIGHTDONE, 0, 0)); // S_CHAINFLASH2
        _states.Add(new State(SpriteNum.SPR_MISG, 0, 1, ActionWeaponReady, StateNum.S_MISSILE, 0, 0));  // S_MISSILE
        _states.Add(new State(SpriteNum.SPR_MISG, 0, 1, ActionLower, StateNum.S_MISSILEDOWN, 0, 0));    // S_MISSILEDOWN
        _states.Add(new State(SpriteNum.SPR_MISG, 0, 1, ActionRaise, StateNum.S_MISSILEUP, 0, 0));  // S_MISSILEUP
        _states.Add(new State(SpriteNum.SPR_MISG, 1, 8, ActionGunFlash, StateNum.S_MISSILE2, 0, 0));    // S_MISSILE1
        _states.Add(new State(SpriteNum.SPR_MISG, 1, 12, ActionFireMissile, StateNum.S_MISSILE3, 0, 0));    // S_MISSILE2
        _states.Add(new State(SpriteNum.SPR_MISG, 1, 0, ActionReFire, StateNum.S_MISSILE, 0, 0));   // S_MISSILE3
        _states.Add(new State(SpriteNum.SPR_MISF, 32768, 3, ActionLight1, StateNum.S_MISSILEFLASH2, 0, 0)); // S_MISSILEFLASH1
        _states.Add(new State(SpriteNum.SPR_MISF, 32769, 4, null, StateNum.S_MISSILEFLASH3, 0, 0)); // S_MISSILEFLASH2
        _states.Add(new State(SpriteNum.SPR_MISF, 32770, 4, ActionLight2, StateNum.S_MISSILEFLASH4, 0, 0)); // S_MISSILEFLASH3
        _states.Add(new State(SpriteNum.SPR_MISF, 32771, 4, ActionLight2, StateNum.S_LIGHTDONE, 0, 0)); // S_MISSILEFLASH4
        _states.Add(new State(SpriteNum.SPR_SAWG, 2, 4, ActionWeaponReady, StateNum.S_SAWB, 0, 0)); // S_SAW
        _states.Add(new State(SpriteNum.SPR_SAWG, 3, 4, ActionWeaponReady, StateNum.S_SAW, 0, 0));  // S_SAWB
        _states.Add(new State(SpriteNum.SPR_SAWG, 2, 1, ActionLower, StateNum.S_SAWDOWN, 0, 0));    // S_SAWDOWN
        _states.Add(new State(SpriteNum.SPR_SAWG, 2, 1, ActionRaise, StateNum.S_SAWUP, 0, 0));  // S_SAWUP
        _states.Add(new State(SpriteNum.SPR_SAWG, 0, 4, ActionSaw, StateNum.S_SAW2, 0, 0)); // S_SAW1
        _states.Add(new State(SpriteNum.SPR_SAWG, 1, 4, ActionSaw, StateNum.S_SAW3, 0, 0)); // S_SAW2
        _states.Add(new State(SpriteNum.SPR_SAWG, 1, 0, ActionReFire, StateNum.S_SAW, 0, 0));   // S_SAW3
        _states.Add(new State(SpriteNum.SPR_PLSG, 0, 1, ActionWeaponReady, StateNum.S_PLASMA, 0, 0));   // S_PLASMA
        _states.Add(new State(SpriteNum.SPR_PLSG, 0, 1, ActionLower, StateNum.S_PLASMADOWN, 0, 0)); // S_PLASMADOWN
        _states.Add(new State(SpriteNum.SPR_PLSG, 0, 1, ActionRaise, StateNum.S_PLASMAUP, 0, 0));   // S_PLASMAUP
        _states.Add(new State(SpriteNum.SPR_PLSG, 0, 3, ActionFirePlasma, StateNum.S_PLASMA2, 0, 0));   // S_PLASMA1
        _states.Add(new State(SpriteNum.SPR_PLSG, 1, 20, ActionReFire, StateNum.S_PLASMA, 0, 0));   // S_PLASMA2
        _states.Add(new State(SpriteNum.SPR_PLSF, 32768, 4, ActionLight1, StateNum.S_LIGHTDONE, 0, 0)); // S_PLASMAFLASH1
        _states.Add(new State(SpriteNum.SPR_PLSF, 32769, 4, ActionLight1, StateNum.S_LIGHTDONE, 0, 0)); // S_PLASMAFLASH2
        _states.Add(new State(SpriteNum.SPR_BFGG, 0, 1, ActionWeaponReady, StateNum.S_BFG, 0, 0));  // S_BFG
        _states.Add(new State(SpriteNum.SPR_BFGG, 0, 1, ActionLower, StateNum.S_BFGDOWN, 0, 0));    // S_BFGDOWN
        _states.Add(new State(SpriteNum.SPR_BFGG, 0, 1, ActionRaise, StateNum.S_BFGUP, 0, 0));  // S_BFGUP
        _states.Add(new State(SpriteNum.SPR_BFGG, 0, 20, ActionBFGsound, StateNum.S_BFG2, 0, 0));   // S_BFG1
        _states.Add(new State(SpriteNum.SPR_BFGG, 1, 10, ActionGunFlash, StateNum.S_BFG3, 0, 0));   // S_BFG2
        _states.Add(new State(SpriteNum.SPR_BFGG, 1, 10, ActionFireBFG, StateNum.S_BFG4, 0, 0));    // S_BFG3
        _states.Add(new State(SpriteNum.SPR_BFGG, 1, 20, ActionReFire, StateNum.S_BFG, 0, 0));  // S_BFG4
        _states.Add(new State(SpriteNum.SPR_BFGF, 32768, 11, ActionLight1, StateNum.S_BFGFLASH2, 0, 0));    // S_BFGFLASH1
        _states.Add(new State(SpriteNum.SPR_BFGF, 32769, 6, ActionLight2, StateNum.S_LIGHTDONE, 0, 0)); // S_BFGFLASH2
        _states.Add(new State(SpriteNum.SPR_BLUD, 2, 8, null, StateNum.S_BLOOD2, 0, 0));    // S_BLOOD1
        _states.Add(new State(SpriteNum.SPR_BLUD, 1, 8, null, StateNum.S_BLOOD3, 0, 0));    // S_BLOOD2
        _states.Add(new State(SpriteNum.SPR_BLUD, 0, 8, null, StateNum.S_NULL, 0, 0));  // S_BLOOD3
        _states.Add(new State(SpriteNum.SPR_PUFF, 32768, 4, null, StateNum.S_PUFF2, 0, 0)); // S_PUFF1
        _states.Add(new State(SpriteNum.SPR_PUFF, 1, 4, null, StateNum.S_PUFF3, 0, 0)); // S_PUFF2
        _states.Add(new State(SpriteNum.SPR_PUFF, 2, 4, null, StateNum.S_PUFF4, 0, 0)); // S_PUFF3
        _states.Add(new State(SpriteNum.SPR_PUFF, 3, 4, null, StateNum.S_NULL, 0, 0));  // S_PUFF4
        _states.Add(new State(SpriteNum.SPR_BAL1, 32768, 4, null, StateNum.S_TBALL2, 0, 0));    // S_TBALL1
        _states.Add(new State(SpriteNum.SPR_BAL1, 32769, 4, null, StateNum.S_TBALL1, 0, 0));    // S_TBALL2
        _states.Add(new State(SpriteNum.SPR_BAL1, 32770, 6, null, StateNum.S_TBALLX2, 0, 0));   // S_TBALLX1
        _states.Add(new State(SpriteNum.SPR_BAL1, 32771, 6, null, StateNum.S_TBALLX3, 0, 0));   // S_TBALLX2
        _states.Add(new State(SpriteNum.SPR_BAL1, 32772, 6, null, StateNum.S_NULL, 0, 0));  // S_TBALLX3
        _states.Add(new State(SpriteNum.SPR_BAL2, 32768, 4, null, StateNum.S_RBALL2, 0, 0));    // S_RBALL1
        _states.Add(new State(SpriteNum.SPR_BAL2, 32769, 4, null, StateNum.S_RBALL1, 0, 0));    // S_RBALL2
        _states.Add(new State(SpriteNum.SPR_BAL2, 32770, 6, null, StateNum.S_RBALLX2, 0, 0));   // S_RBALLX1
        _states.Add(new State(SpriteNum.SPR_BAL2, 32771, 6, null, StateNum.S_RBALLX3, 0, 0));   // S_RBALLX2
        _states.Add(new State(SpriteNum.SPR_BAL2, 32772, 6, null, StateNum.S_NULL, 0, 0));  // S_RBALLX3
        _states.Add(new State(SpriteNum.SPR_PLSS, 32768, 6, null, StateNum.S_PLASBALL2, 0, 0)); // S_PLASBALL
        _states.Add(new State(SpriteNum.SPR_PLSS, 32769, 6, null, StateNum.S_PLASBALL, 0, 0));  // S_PLASBALL2
        _states.Add(new State(SpriteNum.SPR_PLSE, 32768, 4, null, StateNum.S_PLASEXP2, 0, 0));  // S_PLASEXP
        _states.Add(new State(SpriteNum.SPR_PLSE, 32769, 4, null, StateNum.S_PLASEXP3, 0, 0));  // S_PLASEXP2
        _states.Add(new State(SpriteNum.SPR_PLSE, 32770, 4, null, StateNum.S_PLASEXP4, 0, 0));  // S_PLASEXP3
        _states.Add(new State(SpriteNum.SPR_PLSE, 32771, 4, null, StateNum.S_PLASEXP5, 0, 0));  // S_PLASEXP4
        _states.Add(new State(SpriteNum.SPR_PLSE, 32772, 4, null, StateNum.S_NULL, 0, 0));  // S_PLASEXP5
        _states.Add(new State(SpriteNum.SPR_MISL, 32768, 1, null, StateNum.S_ROCKET, 0, 0));    // S_ROCKET
        _states.Add(new State(SpriteNum.SPR_BFS1, 32768, 4, null, StateNum.S_BFGSHOT2, 0, 0));  // S_BFGSHOT
        _states.Add(new State(SpriteNum.SPR_BFS1, 32769, 4, null, StateNum.S_BFGSHOT, 0, 0));   // S_BFGSHOT2
        _states.Add(new State(SpriteNum.SPR_BFE1, 32768, 8, null, StateNum.S_BFGLAND2, 0, 0));  // S_BFGLAND
        _states.Add(new State(SpriteNum.SPR_BFE1, 32769, 8, null, StateNum.S_BFGLAND3, 0, 0));  // S_BFGLAND2
        _states.Add(new State(SpriteNum.SPR_BFE1, 32770, 8, ActionBFGSpray, StateNum.S_BFGLAND4, 0, 0));    // S_BFGLAND3
        _states.Add(new State(SpriteNum.SPR_BFE1, 32771, 8, null, StateNum.S_BFGLAND5, 0, 0));  // S_BFGLAND4
        _states.Add(new State(SpriteNum.SPR_BFE1, 32772, 8, null, StateNum.S_BFGLAND6, 0, 0));  // S_BFGLAND5
        _states.Add(new State(SpriteNum.SPR_BFE1, 32773, 8, null, StateNum.S_NULL, 0, 0));  // S_BFGLAND6
        _states.Add(new State(SpriteNum.SPR_BFE2, 32768, 8, null, StateNum.S_BFGEXP2, 0, 0));   // S_BFGEXP
        _states.Add(new State(SpriteNum.SPR_BFE2, 32769, 8, null, StateNum.S_BFGEXP3, 0, 0));   // S_BFGEXP2
        _states.Add(new State(SpriteNum.SPR_BFE2, 32770, 8, null, StateNum.S_BFGEXP4, 0, 0));   // S_BFGEXP3
        _states.Add(new State(SpriteNum.SPR_BFE2, 32771, 8, null, StateNum.S_NULL, 0, 0));  // S_BFGEXP4
        _states.Add(new State(SpriteNum.SPR_MISL, 32769, 8, ActionExplode, StateNum.S_EXPLODE2, 0, 0)); // S_EXPLODE1
        _states.Add(new State(SpriteNum.SPR_MISL, 32770, 6, null, StateNum.S_EXPLODE3, 0, 0));  // S_EXPLODE2
        _states.Add(new State(SpriteNum.SPR_MISL, 32771, 4, null, StateNum.S_NULL, 0, 0));  // S_EXPLODE3
        _states.Add(new State(SpriteNum.SPR_TFOG, 32768, 6, null, StateNum.S_TFOG01, 0, 0));    // S_TFOG
        _states.Add(new State(SpriteNum.SPR_TFOG, 32769, 6, null, StateNum.S_TFOG02, 0, 0));    // S_TFOG01
        _states.Add(new State(SpriteNum.SPR_TFOG, 32768, 6, null, StateNum.S_TFOG2, 0, 0)); // S_TFOG02
        _states.Add(new State(SpriteNum.SPR_TFOG, 32769, 6, null, StateNum.S_TFOG3, 0, 0)); // S_TFOG2
        _states.Add(new State(SpriteNum.SPR_TFOG, 32770, 6, null, StateNum.S_TFOG4, 0, 0)); // S_TFOG3
        _states.Add(new State(SpriteNum.SPR_TFOG, 32771, 6, null, StateNum.S_TFOG5, 0, 0)); // S_TFOG4
        _states.Add(new State(SpriteNum.SPR_TFOG, 32772, 6, null, StateNum.S_TFOG6, 0, 0)); // S_TFOG5
        _states.Add(new State(SpriteNum.SPR_TFOG, 32773, 6, null, StateNum.S_TFOG7, 0, 0)); // S_TFOG6
        _states.Add(new State(SpriteNum.SPR_TFOG, 32774, 6, null, StateNum.S_TFOG8, 0, 0)); // S_TFOG7
        _states.Add(new State(SpriteNum.SPR_TFOG, 32775, 6, null, StateNum.S_TFOG9, 0, 0)); // S_TFOG8
        _states.Add(new State(SpriteNum.SPR_TFOG, 32776, 6, null, StateNum.S_TFOG10, 0, 0));    // S_TFOG9
        _states.Add(new State(SpriteNum.SPR_TFOG, 32777, 6, null, StateNum.S_NULL, 0, 0));  // S_TFOG10
        _states.Add(new State(SpriteNum.SPR_IFOG, 32768, 6, null, StateNum.S_IFOG01, 0, 0));    // S_IFOG
        _states.Add(new State(SpriteNum.SPR_IFOG, 32769, 6, null, StateNum.S_IFOG02, 0, 0));    // S_IFOG01
        _states.Add(new State(SpriteNum.SPR_IFOG, 32768, 6, null, StateNum.S_IFOG2, 0, 0)); // S_IFOG02
        _states.Add(new State(SpriteNum.SPR_IFOG, 32769, 6, null, StateNum.S_IFOG3, 0, 0)); // S_IFOG2
        _states.Add(new State(SpriteNum.SPR_IFOG, 32770, 6, null, StateNum.S_IFOG4, 0, 0)); // S_IFOG3
        _states.Add(new State(SpriteNum.SPR_IFOG, 32771, 6, null, StateNum.S_IFOG5, 0, 0)); // S_IFOG4
        _states.Add(new State(SpriteNum.SPR_IFOG, 32772, 6, null, StateNum.S_NULL, 0, 0));  // S_IFOG5
        _states.Add(new State(SpriteNum.SPR_PLAY, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_PLAY
        _states.Add(new State(SpriteNum.SPR_PLAY, 0, 4, null, StateNum.S_PLAY_RUN2, 0, 0)); // S_PLAY_RUN1
        _states.Add(new State(SpriteNum.SPR_PLAY, 1, 4, null, StateNum.S_PLAY_RUN3, 0, 0)); // S_PLAY_RUN2
        _states.Add(new State(SpriteNum.SPR_PLAY, 2, 4, null, StateNum.S_PLAY_RUN4, 0, 0)); // S_PLAY_RUN3
        _states.Add(new State(SpriteNum.SPR_PLAY, 3, 4, null, StateNum.S_PLAY_RUN1, 0, 0)); // S_PLAY_RUN4
        _states.Add(new State(SpriteNum.SPR_PLAY, 4, 12, null, StateNum.S_PLAY, 0, 0)); // S_PLAY_ATK1
        _states.Add(new State(SpriteNum.SPR_PLAY, 32773, 6, null, StateNum.S_PLAY_ATK1, 0, 0)); // S_PLAY_ATK2
        _states.Add(new State(SpriteNum.SPR_PLAY, 6, 4, null, StateNum.S_PLAY_PAIN2, 0, 0));    // S_PLAY_PAIN
        _states.Add(new State(SpriteNum.SPR_PLAY, 6, 4, ActionPain, StateNum.S_PLAY, 0, 0));    // S_PLAY_PAIN2
        _states.Add(new State(SpriteNum.SPR_PLAY, 7, 10, null, StateNum.S_PLAY_DIE2, 0, 0));    // S_PLAY_DIE1
        _states.Add(new State(SpriteNum.SPR_PLAY, 8, 10, ActionPlayerScream, StateNum.S_PLAY_DIE3, 0, 0));  // S_PLAY_DIE2
        _states.Add(new State(SpriteNum.SPR_PLAY, 9, 10, ActionFall, StateNum.S_PLAY_DIE4, 0, 0));  // S_PLAY_DIE3
        _states.Add(new State(SpriteNum.SPR_PLAY, 10, 10, null, StateNum.S_PLAY_DIE5, 0, 0));   // S_PLAY_DIE4
        _states.Add(new State(SpriteNum.SPR_PLAY, 11, 10, null, StateNum.S_PLAY_DIE6, 0, 0));   // S_PLAY_DIE5
        _states.Add(new State(SpriteNum.SPR_PLAY, 12, 10, null, StateNum.S_PLAY_DIE7, 0, 0));   // S_PLAY_DIE6
        _states.Add(new State(SpriteNum.SPR_PLAY, 13, -1, null, StateNum.S_NULL, 0, 0));    // S_PLAY_DIE7
        _states.Add(new State(SpriteNum.SPR_PLAY, 14, 5, null, StateNum.S_PLAY_XDIE2, 0, 0));   // S_PLAY_XDIE1
        _states.Add(new State(SpriteNum.SPR_PLAY, 15, 5, ActionXScream, StateNum.S_PLAY_XDIE3, 0, 0));  // S_PLAY_XDIE2
        _states.Add(new State(SpriteNum.SPR_PLAY, 16, 5, ActionFall, StateNum.S_PLAY_XDIE4, 0, 0)); // S_PLAY_XDIE3
        _states.Add(new State(SpriteNum.SPR_PLAY, 17, 5, null, StateNum.S_PLAY_XDIE5, 0, 0));   // S_PLAY_XDIE4
        _states.Add(new State(SpriteNum.SPR_PLAY, 18, 5, null, StateNum.S_PLAY_XDIE6, 0, 0));   // S_PLAY_XDIE5
        _states.Add(new State(SpriteNum.SPR_PLAY, 19, 5, null, StateNum.S_PLAY_XDIE7, 0, 0));   // S_PLAY_XDIE6
        _states.Add(new State(SpriteNum.SPR_PLAY, 20, 5, null, StateNum.S_PLAY_XDIE8, 0, 0));   // S_PLAY_XDIE7
        _states.Add(new State(SpriteNum.SPR_PLAY, 21, 5, null, StateNum.S_PLAY_XDIE9, 0, 0));   // S_PLAY_XDIE8
        _states.Add(new State(SpriteNum.SPR_PLAY, 22, -1, null, StateNum.S_NULL, 0, 0));    // S_PLAY_XDIE9
        _states.Add(new State(SpriteNum.SPR_POSS, 0, 10, ActionLook, StateNum.S_POSS_STND2, 0, 0)); // S_POSS_STND
        _states.Add(new State(SpriteNum.SPR_POSS, 1, 10, ActionLook, StateNum.S_POSS_STND, 0, 0));  // S_POSS_STND2
        _states.Add(new State(SpriteNum.SPR_POSS, 0, 4, ActionChase, StateNum.S_POSS_RUN2, 0, 0));  // S_POSS_RUN1
        _states.Add(new State(SpriteNum.SPR_POSS, 0, 4, ActionChase, StateNum.S_POSS_RUN3, 0, 0));  // S_POSS_RUN2
        _states.Add(new State(SpriteNum.SPR_POSS, 1, 4, ActionChase, StateNum.S_POSS_RUN4, 0, 0));  // S_POSS_RUN3
        _states.Add(new State(SpriteNum.SPR_POSS, 1, 4, ActionChase, StateNum.S_POSS_RUN5, 0, 0));  // S_POSS_RUN4
        _states.Add(new State(SpriteNum.SPR_POSS, 2, 4, ActionChase, StateNum.S_POSS_RUN6, 0, 0));  // S_POSS_RUN5
        _states.Add(new State(SpriteNum.SPR_POSS, 2, 4, ActionChase, StateNum.S_POSS_RUN7, 0, 0));  // S_POSS_RUN6
        _states.Add(new State(SpriteNum.SPR_POSS, 3, 4, ActionChase, StateNum.S_POSS_RUN8, 0, 0));  // S_POSS_RUN7
        _states.Add(new State(SpriteNum.SPR_POSS, 3, 4, ActionChase, StateNum.S_POSS_RUN1, 0, 0));  // S_POSS_RUN8
        _states.Add(new State(SpriteNum.SPR_POSS, 4, 10, ActionFaceTarget, StateNum.S_POSS_ATK2, 0, 0));    // S_POSS_ATK1
        _states.Add(new State(SpriteNum.SPR_POSS, 5, 8, ActionPosAttack, StateNum.S_POSS_ATK3, 0, 0));  // S_POSS_ATK2
        _states.Add(new State(SpriteNum.SPR_POSS, 4, 8, null, StateNum.S_POSS_RUN1, 0, 0)); // S_POSS_ATK3
        _states.Add(new State(SpriteNum.SPR_POSS, 6, 3, null, StateNum.S_POSS_PAIN2, 0, 0));    // S_POSS_PAIN
        _states.Add(new State(SpriteNum.SPR_POSS, 6, 3, ActionPain, StateNum.S_POSS_RUN1, 0, 0));   // S_POSS_PAIN2
        _states.Add(new State(SpriteNum.SPR_POSS, 7, 5, null, StateNum.S_POSS_DIE2, 0, 0)); // S_POSS_DIE1
        _states.Add(new State(SpriteNum.SPR_POSS, 8, 5, ActionScream, StateNum.S_POSS_DIE3, 0, 0)); // S_POSS_DIE2
        _states.Add(new State(SpriteNum.SPR_POSS, 9, 5, ActionFall, StateNum.S_POSS_DIE4, 0, 0));   // S_POSS_DIE3
        _states.Add(new State(SpriteNum.SPR_POSS, 10, 5, null, StateNum.S_POSS_DIE5, 0, 0));    // S_POSS_DIE4
        _states.Add(new State(SpriteNum.SPR_POSS, 11, -1, null, StateNum.S_NULL, 0, 0));    // S_POSS_DIE5
        _states.Add(new State(SpriteNum.SPR_POSS, 12, 5, null, StateNum.S_POSS_XDIE2, 0, 0));   // S_POSS_XDIE1
        _states.Add(new State(SpriteNum.SPR_POSS, 13, 5, ActionXScream, StateNum.S_POSS_XDIE3, 0, 0));  // S_POSS_XDIE2
        _states.Add(new State(SpriteNum.SPR_POSS, 14, 5, ActionFall, StateNum.S_POSS_XDIE4, 0, 0)); // S_POSS_XDIE3
        _states.Add(new State(SpriteNum.SPR_POSS, 15, 5, null, StateNum.S_POSS_XDIE5, 0, 0));   // S_POSS_XDIE4
        _states.Add(new State(SpriteNum.SPR_POSS, 16, 5, null, StateNum.S_POSS_XDIE6, 0, 0));   // S_POSS_XDIE5
        _states.Add(new State(SpriteNum.SPR_POSS, 17, 5, null, StateNum.S_POSS_XDIE7, 0, 0));   // S_POSS_XDIE6
        _states.Add(new State(SpriteNum.SPR_POSS, 18, 5, null, StateNum.S_POSS_XDIE8, 0, 0));   // S_POSS_XDIE7
        _states.Add(new State(SpriteNum.SPR_POSS, 19, 5, null, StateNum.S_POSS_XDIE9, 0, 0));   // S_POSS_XDIE8
        _states.Add(new State(SpriteNum.SPR_POSS, 20, -1, null, StateNum.S_NULL, 0, 0));    // S_POSS_XDIE9
        _states.Add(new State(SpriteNum.SPR_POSS, 10, 5, null, StateNum.S_POSS_RAISE2, 0, 0));  // S_POSS_RAISE1
        _states.Add(new State(SpriteNum.SPR_POSS, 9, 5, null, StateNum.S_POSS_RAISE3, 0, 0));   // S_POSS_RAISE2
        _states.Add(new State(SpriteNum.SPR_POSS, 8, 5, null, StateNum.S_POSS_RAISE4, 0, 0));   // S_POSS_RAISE3
        _states.Add(new State(SpriteNum.SPR_POSS, 7, 5, null, StateNum.S_POSS_RUN1, 0, 0)); // S_POSS_RAISE4
        _states.Add(new State(SpriteNum.SPR_SPOS, 0, 10, ActionLook, StateNum.S_SPOS_STND2, 0, 0)); // S_SPOS_STND
        _states.Add(new State(SpriteNum.SPR_SPOS, 1, 10, ActionLook, StateNum.S_SPOS_STND, 0, 0));  // S_SPOS_STND2
        _states.Add(new State(SpriteNum.SPR_SPOS, 0, 3, ActionChase, StateNum.S_SPOS_RUN2, 0, 0));  // S_SPOS_RUN1
        _states.Add(new State(SpriteNum.SPR_SPOS, 0, 3, ActionChase, StateNum.S_SPOS_RUN3, 0, 0));  // S_SPOS_RUN2
        _states.Add(new State(SpriteNum.SPR_SPOS, 1, 3, ActionChase, StateNum.S_SPOS_RUN4, 0, 0));  // S_SPOS_RUN3
        _states.Add(new State(SpriteNum.SPR_SPOS, 1, 3, ActionChase, StateNum.S_SPOS_RUN5, 0, 0));  // S_SPOS_RUN4
        _states.Add(new State(SpriteNum.SPR_SPOS, 2, 3, ActionChase, StateNum.S_SPOS_RUN6, 0, 0));  // S_SPOS_RUN5
        _states.Add(new State(SpriteNum.SPR_SPOS, 2, 3, ActionChase, StateNum.S_SPOS_RUN7, 0, 0));  // S_SPOS_RUN6
        _states.Add(new State(SpriteNum.SPR_SPOS, 3, 3, ActionChase, StateNum.S_SPOS_RUN8, 0, 0));  // S_SPOS_RUN7
        _states.Add(new State(SpriteNum.SPR_SPOS, 3, 3, ActionChase, StateNum.S_SPOS_RUN1, 0, 0));  // S_SPOS_RUN8
        _states.Add(new State(SpriteNum.SPR_SPOS, 4, 10, ActionFaceTarget, StateNum.S_SPOS_ATK2, 0, 0));    // S_SPOS_ATK1
        _states.Add(new State(SpriteNum.SPR_SPOS, 32773, 10, ActionSPosAttack, StateNum.S_SPOS_ATK3, 0, 0));    // S_SPOS_ATK2
        _states.Add(new State(SpriteNum.SPR_SPOS, 4, 10, null, StateNum.S_SPOS_RUN1, 0, 0));    // S_SPOS_ATK3
        _states.Add(new State(SpriteNum.SPR_SPOS, 6, 3, null, StateNum.S_SPOS_PAIN2, 0, 0));    // S_SPOS_PAIN
        _states.Add(new State(SpriteNum.SPR_SPOS, 6, 3, ActionPain, StateNum.S_SPOS_RUN1, 0, 0));   // S_SPOS_PAIN2
        _states.Add(new State(SpriteNum.SPR_SPOS, 7, 5, null, StateNum.S_SPOS_DIE2, 0, 0)); // S_SPOS_DIE1
        _states.Add(new State(SpriteNum.SPR_SPOS, 8, 5, ActionScream, StateNum.S_SPOS_DIE3, 0, 0)); // S_SPOS_DIE2
        _states.Add(new State(SpriteNum.SPR_SPOS, 9, 5, ActionFall, StateNum.S_SPOS_DIE4, 0, 0));   // S_SPOS_DIE3
        _states.Add(new State(SpriteNum.SPR_SPOS, 10, 5, null, StateNum.S_SPOS_DIE5, 0, 0));    // S_SPOS_DIE4
        _states.Add(new State(SpriteNum.SPR_SPOS, 11, -1, null, StateNum.S_NULL, 0, 0));    // S_SPOS_DIE5
        _states.Add(new State(SpriteNum.SPR_SPOS, 12, 5, null, StateNum.S_SPOS_XDIE2, 0, 0));   // S_SPOS_XDIE1
        _states.Add(new State(SpriteNum.SPR_SPOS, 13, 5, ActionXScream, StateNum.S_SPOS_XDIE3, 0, 0));  // S_SPOS_XDIE2
        _states.Add(new State(SpriteNum.SPR_SPOS, 14, 5, ActionFall, StateNum.S_SPOS_XDIE4, 0, 0)); // S_SPOS_XDIE3
        _states.Add(new State(SpriteNum.SPR_SPOS, 15, 5, null, StateNum.S_SPOS_XDIE5, 0, 0));   // S_SPOS_XDIE4
        _states.Add(new State(SpriteNum.SPR_SPOS, 16, 5, null, StateNum.S_SPOS_XDIE6, 0, 0));   // S_SPOS_XDIE5
        _states.Add(new State(SpriteNum.SPR_SPOS, 17, 5, null, StateNum.S_SPOS_XDIE7, 0, 0));   // S_SPOS_XDIE6
        _states.Add(new State(SpriteNum.SPR_SPOS, 18, 5, null, StateNum.S_SPOS_XDIE8, 0, 0));   // S_SPOS_XDIE7
        _states.Add(new State(SpriteNum.SPR_SPOS, 19, 5, null, StateNum.S_SPOS_XDIE9, 0, 0));   // S_SPOS_XDIE8
        _states.Add(new State(SpriteNum.SPR_SPOS, 20, -1, null, StateNum.S_NULL, 0, 0));    // S_SPOS_XDIE9
        _states.Add(new State(SpriteNum.SPR_SPOS, 11, 5, null, StateNum.S_SPOS_RAISE2, 0, 0));  // S_SPOS_RAISE1
        _states.Add(new State(SpriteNum.SPR_SPOS, 10, 5, null, StateNum.S_SPOS_RAISE3, 0, 0));  // S_SPOS_RAISE2
        _states.Add(new State(SpriteNum.SPR_SPOS, 9, 5, null, StateNum.S_SPOS_RAISE4, 0, 0));   // S_SPOS_RAISE3
        _states.Add(new State(SpriteNum.SPR_SPOS, 8, 5, null, StateNum.S_SPOS_RAISE5, 0, 0));   // S_SPOS_RAISE4
        _states.Add(new State(SpriteNum.SPR_SPOS, 7, 5, null, StateNum.S_SPOS_RUN1, 0, 0)); // S_SPOS_RAISE5
        _states.Add(new State(SpriteNum.SPR_VILE, 0, 10, ActionLook, StateNum.S_VILE_STND2, 0, 0)); // S_VILE_STND
        _states.Add(new State(SpriteNum.SPR_VILE, 1, 10, ActionLook, StateNum.S_VILE_STND, 0, 0));  // S_VILE_STND2
        _states.Add(new State(SpriteNum.SPR_VILE, 0, 2, ActionVileChase, StateNum.S_VILE_RUN2, 0, 0));  // S_VILE_RUN1
        _states.Add(new State(SpriteNum.SPR_VILE, 0, 2, ActionVileChase, StateNum.S_VILE_RUN3, 0, 0));  // S_VILE_RUN2
        _states.Add(new State(SpriteNum.SPR_VILE, 1, 2, ActionVileChase, StateNum.S_VILE_RUN4, 0, 0));  // S_VILE_RUN3
        _states.Add(new State(SpriteNum.SPR_VILE, 1, 2, ActionVileChase, StateNum.S_VILE_RUN5, 0, 0));  // S_VILE_RUN4
        _states.Add(new State(SpriteNum.SPR_VILE, 2, 2, ActionVileChase, StateNum.S_VILE_RUN6, 0, 0));  // S_VILE_RUN5
        _states.Add(new State(SpriteNum.SPR_VILE, 2, 2, ActionVileChase, StateNum.S_VILE_RUN7, 0, 0));  // S_VILE_RUN6
        _states.Add(new State(SpriteNum.SPR_VILE, 3, 2, ActionVileChase, StateNum.S_VILE_RUN8, 0, 0));  // S_VILE_RUN7
        _states.Add(new State(SpriteNum.SPR_VILE, 3, 2, ActionVileChase, StateNum.S_VILE_RUN9, 0, 0));  // S_VILE_RUN8
        _states.Add(new State(SpriteNum.SPR_VILE, 4, 2, ActionVileChase, StateNum.S_VILE_RUN10, 0, 0)); // S_VILE_RUN9
        _states.Add(new State(SpriteNum.SPR_VILE, 4, 2, ActionVileChase, StateNum.S_VILE_RUN11, 0, 0)); // S_VILE_RUN10
        _states.Add(new State(SpriteNum.SPR_VILE, 5, 2, ActionVileChase, StateNum.S_VILE_RUN12, 0, 0)); // S_VILE_RUN11
        _states.Add(new State(SpriteNum.SPR_VILE, 5, 2, ActionVileChase, StateNum.S_VILE_RUN1, 0, 0));  // S_VILE_RUN12
        _states.Add(new State(SpriteNum.SPR_VILE, 32774, 0, ActionVileStart, StateNum.S_VILE_ATK2, 0, 0));  // S_VILE_ATK1
        _states.Add(new State(SpriteNum.SPR_VILE, 32774, 10, ActionFaceTarget, StateNum.S_VILE_ATK3, 0, 0));    // S_VILE_ATK2
        _states.Add(new State(SpriteNum.SPR_VILE, 32775, 8, ActionVileTarget, StateNum.S_VILE_ATK4, 0, 0)); // S_VILE_ATK3
        _states.Add(new State(SpriteNum.SPR_VILE, 32776, 8, ActionFaceTarget, StateNum.S_VILE_ATK5, 0, 0)); // S_VILE_ATK4
        _states.Add(new State(SpriteNum.SPR_VILE, 32777, 8, ActionFaceTarget, StateNum.S_VILE_ATK6, 0, 0)); // S_VILE_ATK5
        _states.Add(new State(SpriteNum.SPR_VILE, 32778, 8, ActionFaceTarget, StateNum.S_VILE_ATK7, 0, 0)); // S_VILE_ATK6
        _states.Add(new State(SpriteNum.SPR_VILE, 32779, 8, ActionFaceTarget, StateNum.S_VILE_ATK8, 0, 0)); // S_VILE_ATK7
        _states.Add(new State(SpriteNum.SPR_VILE, 32780, 8, ActionFaceTarget, StateNum.S_VILE_ATK9, 0, 0)); // S_VILE_ATK8
        _states.Add(new State(SpriteNum.SPR_VILE, 32781, 8, ActionFaceTarget, StateNum.S_VILE_ATK10, 0, 0));    // S_VILE_ATK9
        _states.Add(new State(SpriteNum.SPR_VILE, 32782, 8, ActionVileAttack, StateNum.S_VILE_ATK11, 0, 0));    // S_VILE_ATK10
        _states.Add(new State(SpriteNum.SPR_VILE, 32783, 20, null, StateNum.S_VILE_RUN1, 0, 0));    // S_VILE_ATK11
        _states.Add(new State(SpriteNum.SPR_VILE, 32794, 10, null, StateNum.S_VILE_HEAL2, 0, 0));   // S_VILE_HEAL1
        _states.Add(new State(SpriteNum.SPR_VILE, 32795, 10, null, StateNum.S_VILE_HEAL3, 0, 0));   // S_VILE_HEAL2
        _states.Add(new State(SpriteNum.SPR_VILE, 32796, 10, null, StateNum.S_VILE_RUN1, 0, 0));    // S_VILE_HEAL3
        _states.Add(new State(SpriteNum.SPR_VILE, 16, 5, null, StateNum.S_VILE_PAIN2, 0, 0));   // S_VILE_PAIN
        _states.Add(new State(SpriteNum.SPR_VILE, 16, 5, ActionPain, StateNum.S_VILE_RUN1, 0, 0));  // S_VILE_PAIN2
        _states.Add(new State(SpriteNum.SPR_VILE, 16, 7, null, StateNum.S_VILE_DIE2, 0, 0));    // S_VILE_DIE1
        _states.Add(new State(SpriteNum.SPR_VILE, 17, 7, ActionScream, StateNum.S_VILE_DIE3, 0, 0));    // S_VILE_DIE2
        _states.Add(new State(SpriteNum.SPR_VILE, 18, 7, ActionFall, StateNum.S_VILE_DIE4, 0, 0));  // S_VILE_DIE3
        _states.Add(new State(SpriteNum.SPR_VILE, 19, 7, null, StateNum.S_VILE_DIE5, 0, 0));    // S_VILE_DIE4
        _states.Add(new State(SpriteNum.SPR_VILE, 20, 7, null, StateNum.S_VILE_DIE6, 0, 0));    // S_VILE_DIE5
        _states.Add(new State(SpriteNum.SPR_VILE, 21, 7, null, StateNum.S_VILE_DIE7, 0, 0));    // S_VILE_DIE6
        _states.Add(new State(SpriteNum.SPR_VILE, 22, 7, null, StateNum.S_VILE_DIE8, 0, 0));    // S_VILE_DIE7
        _states.Add(new State(SpriteNum.SPR_VILE, 23, 5, null, StateNum.S_VILE_DIE9, 0, 0));    // S_VILE_DIE8
        _states.Add(new State(SpriteNum.SPR_VILE, 24, 5, null, StateNum.S_VILE_DIE10, 0, 0));   // S_VILE_DIE9
        _states.Add(new State(SpriteNum.SPR_VILE, 25, -1, null, StateNum.S_NULL, 0, 0));    // S_VILE_DIE10
        _states.Add(new State(SpriteNum.SPR_FIRE, 32768, 2, ActionStartFire, StateNum.S_FIRE2, 0, 0));  // S_FIRE1
        _states.Add(new State(SpriteNum.SPR_FIRE, 32769, 2, ActionFire, StateNum.S_FIRE3, 0, 0));   // S_FIRE2
        _states.Add(new State(SpriteNum.SPR_FIRE, 32768, 2, ActionFire, StateNum.S_FIRE4, 0, 0));   // S_FIRE3
        _states.Add(new State(SpriteNum.SPR_FIRE, 32769, 2, ActionFire, StateNum.S_FIRE5, 0, 0));   // S_FIRE4
        _states.Add(new State(SpriteNum.SPR_FIRE, 32770, 2, ActionFireCrackle, StateNum.S_FIRE6, 0, 0));    // S_FIRE5
        _states.Add(new State(SpriteNum.SPR_FIRE, 32769, 2, ActionFire, StateNum.S_FIRE7, 0, 0));   // S_FIRE6
        _states.Add(new State(SpriteNum.SPR_FIRE, 32770, 2, ActionFire, StateNum.S_FIRE8, 0, 0));   // S_FIRE7
        _states.Add(new State(SpriteNum.SPR_FIRE, 32769, 2, ActionFire, StateNum.S_FIRE9, 0, 0));   // S_FIRE8
        _states.Add(new State(SpriteNum.SPR_FIRE, 32770, 2, ActionFire, StateNum.S_FIRE10, 0, 0));  // S_FIRE9
        _states.Add(new State(SpriteNum.SPR_FIRE, 32771, 2, ActionFire, StateNum.S_FIRE11, 0, 0));  // S_FIRE10
        _states.Add(new State(SpriteNum.SPR_FIRE, 32770, 2, ActionFire, StateNum.S_FIRE12, 0, 0));  // S_FIRE11
        _states.Add(new State(SpriteNum.SPR_FIRE, 32771, 2, ActionFire, StateNum.S_FIRE13, 0, 0));  // S_FIRE12
        _states.Add(new State(SpriteNum.SPR_FIRE, 32770, 2, ActionFire, StateNum.S_FIRE14, 0, 0));  // S_FIRE13
        _states.Add(new State(SpriteNum.SPR_FIRE, 32771, 2, ActionFire, StateNum.S_FIRE15, 0, 0));  // S_FIRE14
        _states.Add(new State(SpriteNum.SPR_FIRE, 32772, 2, ActionFire, StateNum.S_FIRE16, 0, 0));  // S_FIRE15
        _states.Add(new State(SpriteNum.SPR_FIRE, 32771, 2, ActionFire, StateNum.S_FIRE17, 0, 0));  // S_FIRE16
        _states.Add(new State(SpriteNum.SPR_FIRE, 32772, 2, ActionFire, StateNum.S_FIRE18, 0, 0));  // S_FIRE17
        _states.Add(new State(SpriteNum.SPR_FIRE, 32771, 2, ActionFire, StateNum.S_FIRE19, 0, 0));  // S_FIRE18
        _states.Add(new State(SpriteNum.SPR_FIRE, 32772, 2, ActionFireCrackle, StateNum.S_FIRE20, 0, 0));   // S_FIRE19
        _states.Add(new State(SpriteNum.SPR_FIRE, 32773, 2, ActionFire, StateNum.S_FIRE21, 0, 0));  // S_FIRE20
        _states.Add(new State(SpriteNum.SPR_FIRE, 32772, 2, ActionFire, StateNum.S_FIRE22, 0, 0));  // S_FIRE21
        _states.Add(new State(SpriteNum.SPR_FIRE, 32773, 2, ActionFire, StateNum.S_FIRE23, 0, 0));  // S_FIRE22
        _states.Add(new State(SpriteNum.SPR_FIRE, 32772, 2, ActionFire, StateNum.S_FIRE24, 0, 0));  // S_FIRE23
        _states.Add(new State(SpriteNum.SPR_FIRE, 32773, 2, ActionFire, StateNum.S_FIRE25, 0, 0));  // S_FIRE24
        _states.Add(new State(SpriteNum.SPR_FIRE, 32774, 2, ActionFire, StateNum.S_FIRE26, 0, 0));  // S_FIRE25
        _states.Add(new State(SpriteNum.SPR_FIRE, 32775, 2, ActionFire, StateNum.S_FIRE27, 0, 0));  // S_FIRE26
        _states.Add(new State(SpriteNum.SPR_FIRE, 32774, 2, ActionFire, StateNum.S_FIRE28, 0, 0));  // S_FIRE27
        _states.Add(new State(SpriteNum.SPR_FIRE, 32775, 2, ActionFire, StateNum.S_FIRE29, 0, 0));  // S_FIRE28
        _states.Add(new State(SpriteNum.SPR_FIRE, 32774, 2, ActionFire, StateNum.S_FIRE30, 0, 0));  // S_FIRE29
        _states.Add(new State(SpriteNum.SPR_FIRE, 32775, 2, ActionFire, StateNum.S_NULL, 0, 0));    // S_FIRE30
        _states.Add(new State(SpriteNum.SPR_PUFF, 1, 4, null, StateNum.S_SMOKE2, 0, 0));    // S_SMOKE1
        _states.Add(new State(SpriteNum.SPR_PUFF, 2, 4, null, StateNum.S_SMOKE3, 0, 0));    // S_SMOKE2
        _states.Add(new State(SpriteNum.SPR_PUFF, 1, 4, null, StateNum.S_SMOKE4, 0, 0));    // S_SMOKE3
        _states.Add(new State(SpriteNum.SPR_PUFF, 2, 4, null, StateNum.S_SMOKE5, 0, 0));    // S_SMOKE4
        _states.Add(new State(SpriteNum.SPR_PUFF, 3, 4, null, StateNum.S_NULL, 0, 0));  // S_SMOKE5
        _states.Add(new State(SpriteNum.SPR_FATB, 32768, 2, ActionTracer, StateNum.S_TRACER2, 0, 0));   // S_TRACER
        _states.Add(new State(SpriteNum.SPR_FATB, 32769, 2, ActionTracer, StateNum.S_TRACER, 0, 0));    // S_TRACER2
        _states.Add(new State(SpriteNum.SPR_FBXP, 32768, 8, null, StateNum.S_TRACEEXP2, 0, 0)); // S_TRACEEXP1
        _states.Add(new State(SpriteNum.SPR_FBXP, 32769, 6, null, StateNum.S_TRACEEXP3, 0, 0)); // S_TRACEEXP2
        _states.Add(new State(SpriteNum.SPR_FBXP, 32770, 4, null, StateNum.S_NULL, 0, 0));  // S_TRACEEXP3
        _states.Add(new State(SpriteNum.SPR_SKEL, 0, 10, ActionLook, StateNum.S_SKEL_STND2, 0, 0)); // S_SKEL_STND
        _states.Add(new State(SpriteNum.SPR_SKEL, 1, 10, ActionLook, StateNum.S_SKEL_STND, 0, 0));  // S_SKEL_STND2
        _states.Add(new State(SpriteNum.SPR_SKEL, 0, 2, ActionChase, StateNum.S_SKEL_RUN2, 0, 0));  // S_SKEL_RUN1
        _states.Add(new State(SpriteNum.SPR_SKEL, 0, 2, ActionChase, StateNum.S_SKEL_RUN3, 0, 0));  // S_SKEL_RUN2
        _states.Add(new State(SpriteNum.SPR_SKEL, 1, 2, ActionChase, StateNum.S_SKEL_RUN4, 0, 0));  // S_SKEL_RUN3
        _states.Add(new State(SpriteNum.SPR_SKEL, 1, 2, ActionChase, StateNum.S_SKEL_RUN5, 0, 0));  // S_SKEL_RUN4
        _states.Add(new State(SpriteNum.SPR_SKEL, 2, 2, ActionChase, StateNum.S_SKEL_RUN6, 0, 0));  // S_SKEL_RUN5
        _states.Add(new State(SpriteNum.SPR_SKEL, 2, 2, ActionChase, StateNum.S_SKEL_RUN7, 0, 0));  // S_SKEL_RUN6
        _states.Add(new State(SpriteNum.SPR_SKEL, 3, 2, ActionChase, StateNum.S_SKEL_RUN8, 0, 0));  // S_SKEL_RUN7
        _states.Add(new State(SpriteNum.SPR_SKEL, 3, 2, ActionChase, StateNum.S_SKEL_RUN9, 0, 0));  // S_SKEL_RUN8
        _states.Add(new State(SpriteNum.SPR_SKEL, 4, 2, ActionChase, StateNum.S_SKEL_RUN10, 0, 0)); // S_SKEL_RUN9
        _states.Add(new State(SpriteNum.SPR_SKEL, 4, 2, ActionChase, StateNum.S_SKEL_RUN11, 0, 0)); // S_SKEL_RUN10
        _states.Add(new State(SpriteNum.SPR_SKEL, 5, 2, ActionChase, StateNum.S_SKEL_RUN12, 0, 0)); // S_SKEL_RUN11
        _states.Add(new State(SpriteNum.SPR_SKEL, 5, 2, ActionChase, StateNum.S_SKEL_RUN1, 0, 0));  // S_SKEL_RUN12
        _states.Add(new State(SpriteNum.SPR_SKEL, 6, 0, ActionFaceTarget, StateNum.S_SKEL_FIST2, 0, 0));    // S_SKEL_FIST1
        _states.Add(new State(SpriteNum.SPR_SKEL, 6, 6, ActionSkelWhoosh, StateNum.S_SKEL_FIST3, 0, 0));    // S_SKEL_FIST2
        _states.Add(new State(SpriteNum.SPR_SKEL, 7, 6, ActionFaceTarget, StateNum.S_SKEL_FIST4, 0, 0));    // S_SKEL_FIST3
        _states.Add(new State(SpriteNum.SPR_SKEL, 8, 6, ActionSkelFist, StateNum.S_SKEL_RUN1, 0, 0));   // S_SKEL_FIST4
        _states.Add(new State(SpriteNum.SPR_SKEL, 32777, 0, ActionFaceTarget, StateNum.S_SKEL_MISS2, 0, 0));    // S_SKEL_MISS1
        _states.Add(new State(SpriteNum.SPR_SKEL, 32777, 10, ActionFaceTarget, StateNum.S_SKEL_MISS3, 0, 0));   // S_SKEL_MISS2
        _states.Add(new State(SpriteNum.SPR_SKEL, 10, 10, ActionSkelMissile, StateNum.S_SKEL_MISS4, 0, 0)); // S_SKEL_MISS3
        _states.Add(new State(SpriteNum.SPR_SKEL, 10, 10, ActionFaceTarget, StateNum.S_SKEL_RUN1, 0, 0));   // S_SKEL_MISS4
        _states.Add(new State(SpriteNum.SPR_SKEL, 11, 5, null, StateNum.S_SKEL_PAIN2, 0, 0));   // S_SKEL_PAIN
        _states.Add(new State(SpriteNum.SPR_SKEL, 11, 5, ActionPain, StateNum.S_SKEL_RUN1, 0, 0));  // S_SKEL_PAIN2
        _states.Add(new State(SpriteNum.SPR_SKEL, 11, 7, null, StateNum.S_SKEL_DIE2, 0, 0));    // S_SKEL_DIE1
        _states.Add(new State(SpriteNum.SPR_SKEL, 12, 7, null, StateNum.S_SKEL_DIE3, 0, 0));    // S_SKEL_DIE2
        _states.Add(new State(SpriteNum.SPR_SKEL, 13, 7, ActionScream, StateNum.S_SKEL_DIE4, 0, 0));    // S_SKEL_DIE3
        _states.Add(new State(SpriteNum.SPR_SKEL, 14, 7, ActionFall, StateNum.S_SKEL_DIE5, 0, 0));  // S_SKEL_DIE4
        _states.Add(new State(SpriteNum.SPR_SKEL, 15, 7, null, StateNum.S_SKEL_DIE6, 0, 0));    // S_SKEL_DIE5
        _states.Add(new State(SpriteNum.SPR_SKEL, 16, -1, null, StateNum.S_NULL, 0, 0));    // S_SKEL_DIE6
        _states.Add(new State(SpriteNum.SPR_SKEL, 16, 5, null, StateNum.S_SKEL_RAISE2, 0, 0));  // S_SKEL_RAISE1
        _states.Add(new State(SpriteNum.SPR_SKEL, 15, 5, null, StateNum.S_SKEL_RAISE3, 0, 0));  // S_SKEL_RAISE2
        _states.Add(new State(SpriteNum.SPR_SKEL, 14, 5, null, StateNum.S_SKEL_RAISE4, 0, 0));  // S_SKEL_RAISE3
        _states.Add(new State(SpriteNum.SPR_SKEL, 13, 5, null, StateNum.S_SKEL_RAISE5, 0, 0));  // S_SKEL_RAISE4
        _states.Add(new State(SpriteNum.SPR_SKEL, 12, 5, null, StateNum.S_SKEL_RAISE6, 0, 0));  // S_SKEL_RAISE5
        _states.Add(new State(SpriteNum.SPR_SKEL, 11, 5, null, StateNum.S_SKEL_RUN1, 0, 0));    // S_SKEL_RAISE6
        _states.Add(new State(SpriteNum.SPR_MANF, 32768, 4, null, StateNum.S_FATSHOT2, 0, 0));  // S_FATSHOT1
        _states.Add(new State(SpriteNum.SPR_MANF, 32769, 4, null, StateNum.S_FATSHOT1, 0, 0));  // S_FATSHOT2
        _states.Add(new State(SpriteNum.SPR_MISL, 32769, 8, null, StateNum.S_FATSHOTX2, 0, 0)); // S_FATSHOTX1
        _states.Add(new State(SpriteNum.SPR_MISL, 32770, 6, null, StateNum.S_FATSHOTX3, 0, 0)); // S_FATSHOTX2
        _states.Add(new State(SpriteNum.SPR_MISL, 32771, 4, null, StateNum.S_NULL, 0, 0));  // S_FATSHOTX3
        _states.Add(new State(SpriteNum.SPR_FATT, 0, 15, ActionLook, StateNum.S_FATT_STND2, 0, 0)); // S_FATT_STND
        _states.Add(new State(SpriteNum.SPR_FATT, 1, 15, ActionLook, StateNum.S_FATT_STND, 0, 0));  // S_FATT_STND2
        _states.Add(new State(SpriteNum.SPR_FATT, 0, 4, ActionChase, StateNum.S_FATT_RUN2, 0, 0));  // S_FATT_RUN1
        _states.Add(new State(SpriteNum.SPR_FATT, 0, 4, ActionChase, StateNum.S_FATT_RUN3, 0, 0));  // S_FATT_RUN2
        _states.Add(new State(SpriteNum.SPR_FATT, 1, 4, ActionChase, StateNum.S_FATT_RUN4, 0, 0));  // S_FATT_RUN3
        _states.Add(new State(SpriteNum.SPR_FATT, 1, 4, ActionChase, StateNum.S_FATT_RUN5, 0, 0));  // S_FATT_RUN4
        _states.Add(new State(SpriteNum.SPR_FATT, 2, 4, ActionChase, StateNum.S_FATT_RUN6, 0, 0));  // S_FATT_RUN5
        _states.Add(new State(SpriteNum.SPR_FATT, 2, 4, ActionChase, StateNum.S_FATT_RUN7, 0, 0));  // S_FATT_RUN6
        _states.Add(new State(SpriteNum.SPR_FATT, 3, 4, ActionChase, StateNum.S_FATT_RUN8, 0, 0));  // S_FATT_RUN7
        _states.Add(new State(SpriteNum.SPR_FATT, 3, 4, ActionChase, StateNum.S_FATT_RUN9, 0, 0));  // S_FATT_RUN8
        _states.Add(new State(SpriteNum.SPR_FATT, 4, 4, ActionChase, StateNum.S_FATT_RUN10, 0, 0)); // S_FATT_RUN9
        _states.Add(new State(SpriteNum.SPR_FATT, 4, 4, ActionChase, StateNum.S_FATT_RUN11, 0, 0)); // S_FATT_RUN10
        _states.Add(new State(SpriteNum.SPR_FATT, 5, 4, ActionChase, StateNum.S_FATT_RUN12, 0, 0)); // S_FATT_RUN11
        _states.Add(new State(SpriteNum.SPR_FATT, 5, 4, ActionChase, StateNum.S_FATT_RUN1, 0, 0));  // S_FATT_RUN12
        _states.Add(new State(SpriteNum.SPR_FATT, 6, 20, ActionFatRaise, StateNum.S_FATT_ATK2, 0, 0));  // S_FATT_ATK1
        _states.Add(new State(SpriteNum.SPR_FATT, 32775, 10, ActionFatAttack1, StateNum.S_FATT_ATK3, 0, 0));    // S_FATT_ATK2
        _states.Add(new State(SpriteNum.SPR_FATT, 8, 5, ActionFaceTarget, StateNum.S_FATT_ATK4, 0, 0)); // S_FATT_ATK3
        _states.Add(new State(SpriteNum.SPR_FATT, 6, 5, ActionFaceTarget, StateNum.S_FATT_ATK5, 0, 0)); // S_FATT_ATK4
        _states.Add(new State(SpriteNum.SPR_FATT, 32775, 10, ActionFatAttack2, StateNum.S_FATT_ATK6, 0, 0));    // S_FATT_ATK5
        _states.Add(new State(SpriteNum.SPR_FATT, 8, 5, ActionFaceTarget, StateNum.S_FATT_ATK7, 0, 0)); // S_FATT_ATK6
        _states.Add(new State(SpriteNum.SPR_FATT, 6, 5, ActionFaceTarget, StateNum.S_FATT_ATK8, 0, 0)); // S_FATT_ATK7
        _states.Add(new State(SpriteNum.SPR_FATT, 32775, 10, ActionFatAttack3, StateNum.S_FATT_ATK9, 0, 0));    // S_FATT_ATK8
        _states.Add(new State(SpriteNum.SPR_FATT, 8, 5, ActionFaceTarget, StateNum.S_FATT_ATK10, 0, 0));    // S_FATT_ATK9
        _states.Add(new State(SpriteNum.SPR_FATT, 6, 5, ActionFaceTarget, StateNum.S_FATT_RUN1, 0, 0)); // S_FATT_ATK10
        _states.Add(new State(SpriteNum.SPR_FATT, 9, 3, null, StateNum.S_FATT_PAIN2, 0, 0));    // S_FATT_PAIN
        _states.Add(new State(SpriteNum.SPR_FATT, 9, 3, ActionPain, StateNum.S_FATT_RUN1, 0, 0));   // S_FATT_PAIN2
        _states.Add(new State(SpriteNum.SPR_FATT, 10, 6, null, StateNum.S_FATT_DIE2, 0, 0));    // S_FATT_DIE1
        _states.Add(new State(SpriteNum.SPR_FATT, 11, 6, ActionScream, StateNum.S_FATT_DIE3, 0, 0));    // S_FATT_DIE2
        _states.Add(new State(SpriteNum.SPR_FATT, 12, 6, ActionFall, StateNum.S_FATT_DIE4, 0, 0));  // S_FATT_DIE3
        _states.Add(new State(SpriteNum.SPR_FATT, 13, 6, null, StateNum.S_FATT_DIE5, 0, 0));    // S_FATT_DIE4
        _states.Add(new State(SpriteNum.SPR_FATT, 14, 6, null, StateNum.S_FATT_DIE6, 0, 0));    // S_FATT_DIE5
        _states.Add(new State(SpriteNum.SPR_FATT, 15, 6, null, StateNum.S_FATT_DIE7, 0, 0));    // S_FATT_DIE6
        _states.Add(new State(SpriteNum.SPR_FATT, 16, 6, null, StateNum.S_FATT_DIE8, 0, 0));    // S_FATT_DIE7
        _states.Add(new State(SpriteNum.SPR_FATT, 17, 6, null, StateNum.S_FATT_DIE9, 0, 0));    // S_FATT_DIE8
        _states.Add(new State(SpriteNum.SPR_FATT, 18, 6, null, StateNum.S_FATT_DIE10, 0, 0));   // S_FATT_DIE9
        _states.Add(new State(SpriteNum.SPR_FATT, 19, -1, ActionBossDeath, StateNum.S_NULL, 0, 0)); // S_FATT_DIE10
        _states.Add(new State(SpriteNum.SPR_FATT, 17, 5, null, StateNum.S_FATT_RAISE2, 0, 0));  // S_FATT_RAISE1
        _states.Add(new State(SpriteNum.SPR_FATT, 16, 5, null, StateNum.S_FATT_RAISE3, 0, 0));  // S_FATT_RAISE2
        _states.Add(new State(SpriteNum.SPR_FATT, 15, 5, null, StateNum.S_FATT_RAISE4, 0, 0));  // S_FATT_RAISE3
        _states.Add(new State(SpriteNum.SPR_FATT, 14, 5, null, StateNum.S_FATT_RAISE5, 0, 0));  // S_FATT_RAISE4
        _states.Add(new State(SpriteNum.SPR_FATT, 13, 5, null, StateNum.S_FATT_RAISE6, 0, 0));  // S_FATT_RAISE5
        _states.Add(new State(SpriteNum.SPR_FATT, 12, 5, null, StateNum.S_FATT_RAISE7, 0, 0));  // S_FATT_RAISE6
        _states.Add(new State(SpriteNum.SPR_FATT, 11, 5, null, StateNum.S_FATT_RAISE8, 0, 0));  // S_FATT_RAISE7
        _states.Add(new State(SpriteNum.SPR_FATT, 10, 5, null, StateNum.S_FATT_RUN1, 0, 0));    // S_FATT_RAISE8
        _states.Add(new State(SpriteNum.SPR_CPOS, 0, 10, ActionLook, StateNum.S_CPOS_STND2, 0, 0)); // S_CPOS_STND
        _states.Add(new State(SpriteNum.SPR_CPOS, 1, 10, ActionLook, StateNum.S_CPOS_STND, 0, 0));  // S_CPOS_STND2
        _states.Add(new State(SpriteNum.SPR_CPOS, 0, 3, ActionChase, StateNum.S_CPOS_RUN2, 0, 0));  // S_CPOS_RUN1
        _states.Add(new State(SpriteNum.SPR_CPOS, 0, 3, ActionChase, StateNum.S_CPOS_RUN3, 0, 0));  // S_CPOS_RUN2
        _states.Add(new State(SpriteNum.SPR_CPOS, 1, 3, ActionChase, StateNum.S_CPOS_RUN4, 0, 0));  // S_CPOS_RUN3
        _states.Add(new State(SpriteNum.SPR_CPOS, 1, 3, ActionChase, StateNum.S_CPOS_RUN5, 0, 0));  // S_CPOS_RUN4
        _states.Add(new State(SpriteNum.SPR_CPOS, 2, 3, ActionChase, StateNum.S_CPOS_RUN6, 0, 0));  // S_CPOS_RUN5
        _states.Add(new State(SpriteNum.SPR_CPOS, 2, 3, ActionChase, StateNum.S_CPOS_RUN7, 0, 0));  // S_CPOS_RUN6
        _states.Add(new State(SpriteNum.SPR_CPOS, 3, 3, ActionChase, StateNum.S_CPOS_RUN8, 0, 0));  // S_CPOS_RUN7
        _states.Add(new State(SpriteNum.SPR_CPOS, 3, 3, ActionChase, StateNum.S_CPOS_RUN1, 0, 0));  // S_CPOS_RUN8
        _states.Add(new State(SpriteNum.SPR_CPOS, 4, 10, ActionFaceTarget, StateNum.S_CPOS_ATK2, 0, 0));    // S_CPOS_ATK1
        _states.Add(new State(SpriteNum.SPR_CPOS, 32773, 4, ActionCPosAttack, StateNum.S_CPOS_ATK3, 0, 0)); // S_CPOS_ATK2
        _states.Add(new State(SpriteNum.SPR_CPOS, 32772, 4, ActionCPosAttack, StateNum.S_CPOS_ATK4, 0, 0)); // S_CPOS_ATK3
        _states.Add(new State(SpriteNum.SPR_CPOS, 5, 1, ActionCPosRefire, StateNum.S_CPOS_ATK2, 0, 0)); // S_CPOS_ATK4
        _states.Add(new State(SpriteNum.SPR_CPOS, 6, 3, null, StateNum.S_CPOS_PAIN2, 0, 0));    // S_CPOS_PAIN
        _states.Add(new State(SpriteNum.SPR_CPOS, 6, 3, ActionPain, StateNum.S_CPOS_RUN1, 0, 0));   // S_CPOS_PAIN2
        _states.Add(new State(SpriteNum.SPR_CPOS, 7, 5, null, StateNum.S_CPOS_DIE2, 0, 0)); // S_CPOS_DIE1
        _states.Add(new State(SpriteNum.SPR_CPOS, 8, 5, ActionScream, StateNum.S_CPOS_DIE3, 0, 0)); // S_CPOS_DIE2
        _states.Add(new State(SpriteNum.SPR_CPOS, 9, 5, ActionFall, StateNum.S_CPOS_DIE4, 0, 0));   // S_CPOS_DIE3
        _states.Add(new State(SpriteNum.SPR_CPOS, 10, 5, null, StateNum.S_CPOS_DIE5, 0, 0));    // S_CPOS_DIE4
        _states.Add(new State(SpriteNum.SPR_CPOS, 11, 5, null, StateNum.S_CPOS_DIE6, 0, 0));    // S_CPOS_DIE5
        _states.Add(new State(SpriteNum.SPR_CPOS, 12, 5, null, StateNum.S_CPOS_DIE7, 0, 0));    // S_CPOS_DIE6
        _states.Add(new State(SpriteNum.SPR_CPOS, 13, -1, null, StateNum.S_NULL, 0, 0));    // S_CPOS_DIE7
        _states.Add(new State(SpriteNum.SPR_CPOS, 14, 5, null, StateNum.S_CPOS_XDIE2, 0, 0));   // S_CPOS_XDIE1
        _states.Add(new State(SpriteNum.SPR_CPOS, 15, 5, ActionXScream, StateNum.S_CPOS_XDIE3, 0, 0));  // S_CPOS_XDIE2
        _states.Add(new State(SpriteNum.SPR_CPOS, 16, 5, ActionFall, StateNum.S_CPOS_XDIE4, 0, 0)); // S_CPOS_XDIE3
        _states.Add(new State(SpriteNum.SPR_CPOS, 17, 5, null, StateNum.S_CPOS_XDIE5, 0, 0));   // S_CPOS_XDIE4
        _states.Add(new State(SpriteNum.SPR_CPOS, 18, 5, null, StateNum.S_CPOS_XDIE6, 0, 0));   // S_CPOS_XDIE5
        _states.Add(new State(SpriteNum.SPR_CPOS, 19, -1, null, StateNum.S_NULL, 0, 0));    // S_CPOS_XDIE6
        _states.Add(new State(SpriteNum.SPR_CPOS, 13, 5, null, StateNum.S_CPOS_RAISE2, 0, 0));  // S_CPOS_RAISE1
        _states.Add(new State(SpriteNum.SPR_CPOS, 12, 5, null, StateNum.S_CPOS_RAISE3, 0, 0));  // S_CPOS_RAISE2
        _states.Add(new State(SpriteNum.SPR_CPOS, 11, 5, null, StateNum.S_CPOS_RAISE4, 0, 0));  // S_CPOS_RAISE3
        _states.Add(new State(SpriteNum.SPR_CPOS, 10, 5, null, StateNum.S_CPOS_RAISE5, 0, 0));  // S_CPOS_RAISE4
        _states.Add(new State(SpriteNum.SPR_CPOS, 9, 5, null, StateNum.S_CPOS_RAISE6, 0, 0));   // S_CPOS_RAISE5
        _states.Add(new State(SpriteNum.SPR_CPOS, 8, 5, null, StateNum.S_CPOS_RAISE7, 0, 0));   // S_CPOS_RAISE6
        _states.Add(new State(SpriteNum.SPR_CPOS, 7, 5, null, StateNum.S_CPOS_RUN1, 0, 0)); // S_CPOS_RAISE7
        _states.Add(new State(SpriteNum.SPR_TROO, 0, 10, ActionLook, StateNum.S_TROO_STND2, 0, 0)); // S_TROO_STND
        _states.Add(new State(SpriteNum.SPR_TROO, 1, 10, ActionLook, StateNum.S_TROO_STND, 0, 0));  // S_TROO_STND2
        _states.Add(new State(SpriteNum.SPR_TROO, 0, 3, ActionChase, StateNum.S_TROO_RUN2, 0, 0));  // S_TROO_RUN1
        _states.Add(new State(SpriteNum.SPR_TROO, 0, 3, ActionChase, StateNum.S_TROO_RUN3, 0, 0));  // S_TROO_RUN2
        _states.Add(new State(SpriteNum.SPR_TROO, 1, 3, ActionChase, StateNum.S_TROO_RUN4, 0, 0));  // S_TROO_RUN3
        _states.Add(new State(SpriteNum.SPR_TROO, 1, 3, ActionChase, StateNum.S_TROO_RUN5, 0, 0));  // S_TROO_RUN4
        _states.Add(new State(SpriteNum.SPR_TROO, 2, 3, ActionChase, StateNum.S_TROO_RUN6, 0, 0));  // S_TROO_RUN5
        _states.Add(new State(SpriteNum.SPR_TROO, 2, 3, ActionChase, StateNum.S_TROO_RUN7, 0, 0));  // S_TROO_RUN6
        _states.Add(new State(SpriteNum.SPR_TROO, 3, 3, ActionChase, StateNum.S_TROO_RUN8, 0, 0));  // S_TROO_RUN7
        _states.Add(new State(SpriteNum.SPR_TROO, 3, 3, ActionChase, StateNum.S_TROO_RUN1, 0, 0));  // S_TROO_RUN8
        _states.Add(new State(SpriteNum.SPR_TROO, 4, 8, ActionFaceTarget, StateNum.S_TROO_ATK2, 0, 0)); // S_TROO_ATK1
        _states.Add(new State(SpriteNum.SPR_TROO, 5, 8, ActionFaceTarget, StateNum.S_TROO_ATK3, 0, 0)); // S_TROO_ATK2
        _states.Add(new State(SpriteNum.SPR_TROO, 6, 6, ActionTroopAttack, StateNum.S_TROO_RUN1, 0, 0));    // S_TROO_ATK3
        _states.Add(new State(SpriteNum.SPR_TROO, 7, 2, null, StateNum.S_TROO_PAIN2, 0, 0));    // S_TROO_PAIN
        _states.Add(new State(SpriteNum.SPR_TROO, 7, 2, ActionPain, StateNum.S_TROO_RUN1, 0, 0));   // S_TROO_PAIN2
        _states.Add(new State(SpriteNum.SPR_TROO, 8, 8, null, StateNum.S_TROO_DIE2, 0, 0)); // S_TROO_DIE1
        _states.Add(new State(SpriteNum.SPR_TROO, 9, 8, ActionScream, StateNum.S_TROO_DIE3, 0, 0)); // S_TROO_DIE2
        _states.Add(new State(SpriteNum.SPR_TROO, 10, 6, null, StateNum.S_TROO_DIE4, 0, 0));    // S_TROO_DIE3
        _states.Add(new State(SpriteNum.SPR_TROO, 11, 6, ActionFall, StateNum.S_TROO_DIE5, 0, 0));  // S_TROO_DIE4
        _states.Add(new State(SpriteNum.SPR_TROO, 12, -1, null, StateNum.S_NULL, 0, 0));    // S_TROO_DIE5
        _states.Add(new State(SpriteNum.SPR_TROO, 13, 5, null, StateNum.S_TROO_XDIE2, 0, 0));   // S_TROO_XDIE1
        _states.Add(new State(SpriteNum.SPR_TROO, 14, 5, ActionXScream, StateNum.S_TROO_XDIE3, 0, 0));  // S_TROO_XDIE2
        _states.Add(new State(SpriteNum.SPR_TROO, 15, 5, null, StateNum.S_TROO_XDIE4, 0, 0));   // S_TROO_XDIE3
        _states.Add(new State(SpriteNum.SPR_TROO, 16, 5, ActionFall, StateNum.S_TROO_XDIE5, 0, 0)); // S_TROO_XDIE4
        _states.Add(new State(SpriteNum.SPR_TROO, 17, 5, null, StateNum.S_TROO_XDIE6, 0, 0));   // S_TROO_XDIE5
        _states.Add(new State(SpriteNum.SPR_TROO, 18, 5, null, StateNum.S_TROO_XDIE7, 0, 0));   // S_TROO_XDIE6
        _states.Add(new State(SpriteNum.SPR_TROO, 19, 5, null, StateNum.S_TROO_XDIE8, 0, 0));   // S_TROO_XDIE7
        _states.Add(new State(SpriteNum.SPR_TROO, 20, -1, null, StateNum.S_NULL, 0, 0));    // S_TROO_XDIE8
        _states.Add(new State(SpriteNum.SPR_TROO, 12, 8, null, StateNum.S_TROO_RAISE2, 0, 0));  // S_TROO_RAISE1
        _states.Add(new State(SpriteNum.SPR_TROO, 11, 8, null, StateNum.S_TROO_RAISE3, 0, 0));  // S_TROO_RAISE2
        _states.Add(new State(SpriteNum.SPR_TROO, 10, 6, null, StateNum.S_TROO_RAISE4, 0, 0));  // S_TROO_RAISE3
        _states.Add(new State(SpriteNum.SPR_TROO, 9, 6, null, StateNum.S_TROO_RAISE5, 0, 0));   // S_TROO_RAISE4
        _states.Add(new State(SpriteNum.SPR_TROO, 8, 6, null, StateNum.S_TROO_RUN1, 0, 0)); // S_TROO_RAISE5
        _states.Add(new State(SpriteNum.SPR_SARG, 0, 10, ActionLook, StateNum.S_SARG_STND2, 0, 0)); // S_SARG_STND
        _states.Add(new State(SpriteNum.SPR_SARG, 1, 10, ActionLook, StateNum.S_SARG_STND, 0, 0));  // S_SARG_STND2
        _states.Add(new State(SpriteNum.SPR_SARG, 0, 2, ActionChase, StateNum.S_SARG_RUN2, 0, 0));  // S_SARG_RUN1
        _states.Add(new State(SpriteNum.SPR_SARG, 0, 2, ActionChase, StateNum.S_SARG_RUN3, 0, 0));  // S_SARG_RUN2
        _states.Add(new State(SpriteNum.SPR_SARG, 1, 2, ActionChase, StateNum.S_SARG_RUN4, 0, 0));  // S_SARG_RUN3
        _states.Add(new State(SpriteNum.SPR_SARG, 1, 2, ActionChase, StateNum.S_SARG_RUN5, 0, 0));  // S_SARG_RUN4
        _states.Add(new State(SpriteNum.SPR_SARG, 2, 2, ActionChase, StateNum.S_SARG_RUN6, 0, 0));  // S_SARG_RUN5
        _states.Add(new State(SpriteNum.SPR_SARG, 2, 2, ActionChase, StateNum.S_SARG_RUN7, 0, 0));  // S_SARG_RUN6
        _states.Add(new State(SpriteNum.SPR_SARG, 3, 2, ActionChase, StateNum.S_SARG_RUN8, 0, 0));  // S_SARG_RUN7
        _states.Add(new State(SpriteNum.SPR_SARG, 3, 2, ActionChase, StateNum.S_SARG_RUN1, 0, 0));  // S_SARG_RUN8
        _states.Add(new State(SpriteNum.SPR_SARG, 4, 8, ActionFaceTarget, StateNum.S_SARG_ATK2, 0, 0)); // S_SARG_ATK1
        _states.Add(new State(SpriteNum.SPR_SARG, 5, 8, ActionFaceTarget, StateNum.S_SARG_ATK3, 0, 0)); // S_SARG_ATK2
        _states.Add(new State(SpriteNum.SPR_SARG, 6, 8, ActionSargAttack, StateNum.S_SARG_RUN1, 0, 0)); // S_SARG_ATK3
        _states.Add(new State(SpriteNum.SPR_SARG, 7, 2, null, StateNum.S_SARG_PAIN2, 0, 0));    // S_SARG_PAIN
        _states.Add(new State(SpriteNum.SPR_SARG, 7, 2, ActionPain, StateNum.S_SARG_RUN1, 0, 0));   // S_SARG_PAIN2
        _states.Add(new State(SpriteNum.SPR_SARG, 8, 8, null, StateNum.S_SARG_DIE2, 0, 0)); // S_SARG_DIE1
        _states.Add(new State(SpriteNum.SPR_SARG, 9, 8, ActionScream, StateNum.S_SARG_DIE3, 0, 0)); // S_SARG_DIE2
        _states.Add(new State(SpriteNum.SPR_SARG, 10, 4, null, StateNum.S_SARG_DIE4, 0, 0));    // S_SARG_DIE3
        _states.Add(new State(SpriteNum.SPR_SARG, 11, 4, ActionFall, StateNum.S_SARG_DIE5, 0, 0));  // S_SARG_DIE4
        _states.Add(new State(SpriteNum.SPR_SARG, 12, 4, null, StateNum.S_SARG_DIE6, 0, 0));    // S_SARG_DIE5
        _states.Add(new State(SpriteNum.SPR_SARG, 13, -1, null, StateNum.S_NULL, 0, 0));    // S_SARG_DIE6
        _states.Add(new State(SpriteNum.SPR_SARG, 13, 5, null, StateNum.S_SARG_RAISE2, 0, 0));  // S_SARG_RAISE1
        _states.Add(new State(SpriteNum.SPR_SARG, 12, 5, null, StateNum.S_SARG_RAISE3, 0, 0));  // S_SARG_RAISE2
        _states.Add(new State(SpriteNum.SPR_SARG, 11, 5, null, StateNum.S_SARG_RAISE4, 0, 0));  // S_SARG_RAISE3
        _states.Add(new State(SpriteNum.SPR_SARG, 10, 5, null, StateNum.S_SARG_RAISE5, 0, 0));  // S_SARG_RAISE4
        _states.Add(new State(SpriteNum.SPR_SARG, 9, 5, null, StateNum.S_SARG_RAISE6, 0, 0));   // S_SARG_RAISE5
        _states.Add(new State(SpriteNum.SPR_SARG, 8, 5, null, StateNum.S_SARG_RUN1, 0, 0)); // S_SARG_RAISE6
        _states.Add(new State(SpriteNum.SPR_HEAD, 0, 10, ActionLook, StateNum.S_HEAD_STND, 0, 0));  // S_HEAD_STND
        _states.Add(new State(SpriteNum.SPR_HEAD, 0, 3, ActionChase, StateNum.S_HEAD_RUN1, 0, 0));  // S_HEAD_RUN1
        _states.Add(new State(SpriteNum.SPR_HEAD, 1, 5, ActionFaceTarget, StateNum.S_HEAD_ATK2, 0, 0)); // S_HEAD_ATK1
        _states.Add(new State(SpriteNum.SPR_HEAD, 2, 5, ActionFaceTarget, StateNum.S_HEAD_ATK3, 0, 0)); // S_HEAD_ATK2
        _states.Add(new State(SpriteNum.SPR_HEAD, 32771, 5, ActionHeadAttack, StateNum.S_HEAD_RUN1, 0, 0)); // S_HEAD_ATK3
        _states.Add(new State(SpriteNum.SPR_HEAD, 4, 3, null, StateNum.S_HEAD_PAIN2, 0, 0));    // S_HEAD_PAIN
        _states.Add(new State(SpriteNum.SPR_HEAD, 4, 3, ActionPain, StateNum.S_HEAD_PAIN3, 0, 0));  // S_HEAD_PAIN2
        _states.Add(new State(SpriteNum.SPR_HEAD, 5, 6, null, StateNum.S_HEAD_RUN1, 0, 0)); // S_HEAD_PAIN3
        _states.Add(new State(SpriteNum.SPR_HEAD, 6, 8, null, StateNum.S_HEAD_DIE2, 0, 0)); // S_HEAD_DIE1
        _states.Add(new State(SpriteNum.SPR_HEAD, 7, 8, ActionScream, StateNum.S_HEAD_DIE3, 0, 0)); // S_HEAD_DIE2
        _states.Add(new State(SpriteNum.SPR_HEAD, 8, 8, null, StateNum.S_HEAD_DIE4, 0, 0)); // S_HEAD_DIE3
        _states.Add(new State(SpriteNum.SPR_HEAD, 9, 8, null, StateNum.S_HEAD_DIE5, 0, 0)); // S_HEAD_DIE4
        _states.Add(new State(SpriteNum.SPR_HEAD, 10, 8, ActionFall, StateNum.S_HEAD_DIE6, 0, 0));  // S_HEAD_DIE5
        _states.Add(new State(SpriteNum.SPR_HEAD, 11, -1, null, StateNum.S_NULL, 0, 0));    // S_HEAD_DIE6
        _states.Add(new State(SpriteNum.SPR_HEAD, 11, 8, null, StateNum.S_HEAD_RAISE2, 0, 0));  // S_HEAD_RAISE1
        _states.Add(new State(SpriteNum.SPR_HEAD, 10, 8, null, StateNum.S_HEAD_RAISE3, 0, 0));  // S_HEAD_RAISE2
        _states.Add(new State(SpriteNum.SPR_HEAD, 9, 8, null, StateNum.S_HEAD_RAISE4, 0, 0));   // S_HEAD_RAISE3
        _states.Add(new State(SpriteNum.SPR_HEAD, 8, 8, null, StateNum.S_HEAD_RAISE5, 0, 0));   // S_HEAD_RAISE4
        _states.Add(new State(SpriteNum.SPR_HEAD, 7, 8, null, StateNum.S_HEAD_RAISE6, 0, 0));   // S_HEAD_RAISE5
        _states.Add(new State(SpriteNum.SPR_HEAD, 6, 8, null, StateNum.S_HEAD_RUN1, 0, 0)); // S_HEAD_RAISE6
        _states.Add(new State(SpriteNum.SPR_BAL7, 32768, 4, null, StateNum.S_BRBALL2, 0, 0));   // S_BRBALL1
        _states.Add(new State(SpriteNum.SPR_BAL7, 32769, 4, null, StateNum.S_BRBALL1, 0, 0));   // S_BRBALL2
        _states.Add(new State(SpriteNum.SPR_BAL7, 32770, 6, null, StateNum.S_BRBALLX2, 0, 0));  // S_BRBALLX1
        _states.Add(new State(SpriteNum.SPR_BAL7, 32771, 6, null, StateNum.S_BRBALLX3, 0, 0));  // S_BRBALLX2
        _states.Add(new State(SpriteNum.SPR_BAL7, 32772, 6, null, StateNum.S_NULL, 0, 0));  // S_BRBALLX3
        _states.Add(new State(SpriteNum.SPR_BOSS, 0, 10, ActionLook, StateNum.S_BOSS_STND2, 0, 0)); // S_BOSS_STND
        _states.Add(new State(SpriteNum.SPR_BOSS, 1, 10, ActionLook, StateNum.S_BOSS_STND, 0, 0));  // S_BOSS_STND2
        _states.Add(new State(SpriteNum.SPR_BOSS, 0, 3, ActionChase, StateNum.S_BOSS_RUN2, 0, 0));  // S_BOSS_RUN1
        _states.Add(new State(SpriteNum.SPR_BOSS, 0, 3, ActionChase, StateNum.S_BOSS_RUN3, 0, 0));  // S_BOSS_RUN2
        _states.Add(new State(SpriteNum.SPR_BOSS, 1, 3, ActionChase, StateNum.S_BOSS_RUN4, 0, 0));  // S_BOSS_RUN3
        _states.Add(new State(SpriteNum.SPR_BOSS, 1, 3, ActionChase, StateNum.S_BOSS_RUN5, 0, 0));  // S_BOSS_RUN4
        _states.Add(new State(SpriteNum.SPR_BOSS, 2, 3, ActionChase, StateNum.S_BOSS_RUN6, 0, 0));  // S_BOSS_RUN5
        _states.Add(new State(SpriteNum.SPR_BOSS, 2, 3, ActionChase, StateNum.S_BOSS_RUN7, 0, 0));  // S_BOSS_RUN6
        _states.Add(new State(SpriteNum.SPR_BOSS, 3, 3, ActionChase, StateNum.S_BOSS_RUN8, 0, 0));  // S_BOSS_RUN7
        _states.Add(new State(SpriteNum.SPR_BOSS, 3, 3, ActionChase, StateNum.S_BOSS_RUN1, 0, 0));  // S_BOSS_RUN8
        _states.Add(new State(SpriteNum.SPR_BOSS, 4, 8, ActionFaceTarget, StateNum.S_BOSS_ATK2, 0, 0)); // S_BOSS_ATK1
        _states.Add(new State(SpriteNum.SPR_BOSS, 5, 8, ActionFaceTarget, StateNum.S_BOSS_ATK3, 0, 0)); // S_BOSS_ATK2
        _states.Add(new State(SpriteNum.SPR_BOSS, 6, 8, ActionBruisAttack, StateNum.S_BOSS_RUN1, 0, 0));    // S_BOSS_ATK3
        _states.Add(new State(SpriteNum.SPR_BOSS, 7, 2, null, StateNum.S_BOSS_PAIN2, 0, 0));    // S_BOSS_PAIN
        _states.Add(new State(SpriteNum.SPR_BOSS, 7, 2, ActionPain, StateNum.S_BOSS_RUN1, 0, 0));   // S_BOSS_PAIN2
        _states.Add(new State(SpriteNum.SPR_BOSS, 8, 8, null, StateNum.S_BOSS_DIE2, 0, 0)); // S_BOSS_DIE1
        _states.Add(new State(SpriteNum.SPR_BOSS, 9, 8, ActionScream, StateNum.S_BOSS_DIE3, 0, 0)); // S_BOSS_DIE2
        _states.Add(new State(SpriteNum.SPR_BOSS, 10, 8, null, StateNum.S_BOSS_DIE4, 0, 0));    // S_BOSS_DIE3
        _states.Add(new State(SpriteNum.SPR_BOSS, 11, 8, ActionFall, StateNum.S_BOSS_DIE5, 0, 0));  // S_BOSS_DIE4
        _states.Add(new State(SpriteNum.SPR_BOSS, 12, 8, null, StateNum.S_BOSS_DIE6, 0, 0));    // S_BOSS_DIE5
        _states.Add(new State(SpriteNum.SPR_BOSS, 13, 8, null, StateNum.S_BOSS_DIE7, 0, 0));    // S_BOSS_DIE6
        _states.Add(new State(SpriteNum.SPR_BOSS, 14, -1, ActionBossDeath, StateNum.S_NULL, 0, 0)); // S_BOSS_DIE7
        _states.Add(new State(SpriteNum.SPR_BOSS, 14, 8, null, StateNum.S_BOSS_RAISE2, 0, 0));  // S_BOSS_RAISE1
        _states.Add(new State(SpriteNum.SPR_BOSS, 13, 8, null, StateNum.S_BOSS_RAISE3, 0, 0));  // S_BOSS_RAISE2
        _states.Add(new State(SpriteNum.SPR_BOSS, 12, 8, null, StateNum.S_BOSS_RAISE4, 0, 0));  // S_BOSS_RAISE3
        _states.Add(new State(SpriteNum.SPR_BOSS, 11, 8, null, StateNum.S_BOSS_RAISE5, 0, 0));  // S_BOSS_RAISE4
        _states.Add(new State(SpriteNum.SPR_BOSS, 10, 8, null, StateNum.S_BOSS_RAISE6, 0, 0));  // S_BOSS_RAISE5
        _states.Add(new State(SpriteNum.SPR_BOSS, 9, 8, null, StateNum.S_BOSS_RAISE7, 0, 0));   // S_BOSS_RAISE6
        _states.Add(new State(SpriteNum.SPR_BOSS, 8, 8, null, StateNum.S_BOSS_RUN1, 0, 0)); // S_BOSS_RAISE7
        _states.Add(new State(SpriteNum.SPR_BOS2, 0, 10, ActionLook, StateNum.S_BOS2_STND2, 0, 0)); // S_BOS2_STND
        _states.Add(new State(SpriteNum.SPR_BOS2, 1, 10, ActionLook, StateNum.S_BOS2_STND, 0, 0));  // S_BOS2_STND2
        _states.Add(new State(SpriteNum.SPR_BOS2, 0, 3, ActionChase, StateNum.S_BOS2_RUN2, 0, 0));  // S_BOS2_RUN1
        _states.Add(new State(SpriteNum.SPR_BOS2, 0, 3, ActionChase, StateNum.S_BOS2_RUN3, 0, 0));  // S_BOS2_RUN2
        _states.Add(new State(SpriteNum.SPR_BOS2, 1, 3, ActionChase, StateNum.S_BOS2_RUN4, 0, 0));  // S_BOS2_RUN3
        _states.Add(new State(SpriteNum.SPR_BOS2, 1, 3, ActionChase, StateNum.S_BOS2_RUN5, 0, 0));  // S_BOS2_RUN4
        _states.Add(new State(SpriteNum.SPR_BOS2, 2, 3, ActionChase, StateNum.S_BOS2_RUN6, 0, 0));  // S_BOS2_RUN5
        _states.Add(new State(SpriteNum.SPR_BOS2, 2, 3, ActionChase, StateNum.S_BOS2_RUN7, 0, 0));  // S_BOS2_RUN6
        _states.Add(new State(SpriteNum.SPR_BOS2, 3, 3, ActionChase, StateNum.S_BOS2_RUN8, 0, 0));  // S_BOS2_RUN7
        _states.Add(new State(SpriteNum.SPR_BOS2, 3, 3, ActionChase, StateNum.S_BOS2_RUN1, 0, 0));  // S_BOS2_RUN8
        _states.Add(new State(SpriteNum.SPR_BOS2, 4, 8, ActionFaceTarget, StateNum.S_BOS2_ATK2, 0, 0)); // S_BOS2_ATK1
        _states.Add(new State(SpriteNum.SPR_BOS2, 5, 8, ActionFaceTarget, StateNum.S_BOS2_ATK3, 0, 0)); // S_BOS2_ATK2
        _states.Add(new State(SpriteNum.SPR_BOS2, 6, 8, ActionBruisAttack, StateNum.S_BOS2_RUN1, 0, 0));    // S_BOS2_ATK3
        _states.Add(new State(SpriteNum.SPR_BOS2, 7, 2, null, StateNum.S_BOS2_PAIN2, 0, 0));    // S_BOS2_PAIN
        _states.Add(new State(SpriteNum.SPR_BOS2, 7, 2, ActionPain, StateNum.S_BOS2_RUN1, 0, 0));   // S_BOS2_PAIN2
        _states.Add(new State(SpriteNum.SPR_BOS2, 8, 8, null, StateNum.S_BOS2_DIE2, 0, 0)); // S_BOS2_DIE1
        _states.Add(new State(SpriteNum.SPR_BOS2, 9, 8, ActionScream, StateNum.S_BOS2_DIE3, 0, 0)); // S_BOS2_DIE2
        _states.Add(new State(SpriteNum.SPR_BOS2, 10, 8, null, StateNum.S_BOS2_DIE4, 0, 0));    // S_BOS2_DIE3
        _states.Add(new State(SpriteNum.SPR_BOS2, 11, 8, ActionFall, StateNum.S_BOS2_DIE5, 0, 0));  // S_BOS2_DIE4
        _states.Add(new State(SpriteNum.SPR_BOS2, 12, 8, null, StateNum.S_BOS2_DIE6, 0, 0));    // S_BOS2_DIE5
        _states.Add(new State(SpriteNum.SPR_BOS2, 13, 8, null, StateNum.S_BOS2_DIE7, 0, 0));    // S_BOS2_DIE6
        _states.Add(new State(SpriteNum.SPR_BOS2, 14, -1, null, StateNum.S_NULL, 0, 0));    // S_BOS2_DIE7
        _states.Add(new State(SpriteNum.SPR_BOS2, 14, 8, null, StateNum.S_BOS2_RAISE2, 0, 0));  // S_BOS2_RAISE1
        _states.Add(new State(SpriteNum.SPR_BOS2, 13, 8, null, StateNum.S_BOS2_RAISE3, 0, 0));  // S_BOS2_RAISE2
        _states.Add(new State(SpriteNum.SPR_BOS2, 12, 8, null, StateNum.S_BOS2_RAISE4, 0, 0));  // S_BOS2_RAISE3
        _states.Add(new State(SpriteNum.SPR_BOS2, 11, 8, null, StateNum.S_BOS2_RAISE5, 0, 0));  // S_BOS2_RAISE4
        _states.Add(new State(SpriteNum.SPR_BOS2, 10, 8, null, StateNum.S_BOS2_RAISE6, 0, 0));  // S_BOS2_RAISE5
        _states.Add(new State(SpriteNum.SPR_BOS2, 9, 8, null, StateNum.S_BOS2_RAISE7, 0, 0));   // S_BOS2_RAISE6
        _states.Add(new State(SpriteNum.SPR_BOS2, 8, 8, null, StateNum.S_BOS2_RUN1, 0, 0)); // S_BOS2_RAISE7
        _states.Add(new State(SpriteNum.SPR_SKUL, 32768, 10, ActionLook, StateNum.S_SKULL_STND2, 0, 0));    // S_SKULL_STND
        _states.Add(new State(SpriteNum.SPR_SKUL, 32769, 10, ActionLook, StateNum.S_SKULL_STND, 0, 0)); // S_SKULL_STND2
        _states.Add(new State(SpriteNum.SPR_SKUL, 32768, 6, ActionChase, StateNum.S_SKULL_RUN2, 0, 0)); // S_SKULL_RUN1
        _states.Add(new State(SpriteNum.SPR_SKUL, 32769, 6, ActionChase, StateNum.S_SKULL_RUN1, 0, 0)); // S_SKULL_RUN2
        _states.Add(new State(SpriteNum.SPR_SKUL, 32770, 10, ActionFaceTarget, StateNum.S_SKULL_ATK2, 0, 0));   // S_SKULL_ATK1
        _states.Add(new State(SpriteNum.SPR_SKUL, 32771, 4, ActionSkullAttack, StateNum.S_SKULL_ATK3, 0, 0));   // S_SKULL_ATK2
        _states.Add(new State(SpriteNum.SPR_SKUL, 32770, 4, null, StateNum.S_SKULL_ATK4, 0, 0));    // S_SKULL_ATK3
        _states.Add(new State(SpriteNum.SPR_SKUL, 32771, 4, null, StateNum.S_SKULL_ATK3, 0, 0));    // S_SKULL_ATK4
        _states.Add(new State(SpriteNum.SPR_SKUL, 32772, 3, null, StateNum.S_SKULL_PAIN2, 0, 0));   // S_SKULL_PAIN
        _states.Add(new State(SpriteNum.SPR_SKUL, 32772, 3, ActionPain, StateNum.S_SKULL_RUN1, 0, 0));  // S_SKULL_PAIN2
        _states.Add(new State(SpriteNum.SPR_SKUL, 32773, 6, null, StateNum.S_SKULL_DIE2, 0, 0));    // S_SKULL_DIE1
        _states.Add(new State(SpriteNum.SPR_SKUL, 32774, 6, ActionScream, StateNum.S_SKULL_DIE3, 0, 0));    // S_SKULL_DIE2
        _states.Add(new State(SpriteNum.SPR_SKUL, 32775, 6, null, StateNum.S_SKULL_DIE4, 0, 0));    // S_SKULL_DIE3
        _states.Add(new State(SpriteNum.SPR_SKUL, 32776, 6, ActionFall, StateNum.S_SKULL_DIE5, 0, 0));  // S_SKULL_DIE4
        _states.Add(new State(SpriteNum.SPR_SKUL, 9, 6, null, StateNum.S_SKULL_DIE6, 0, 0));    // S_SKULL_DIE5
        _states.Add(new State(SpriteNum.SPR_SKUL, 10, 6, null, StateNum.S_NULL, 0, 0)); // S_SKULL_DIE6
        _states.Add(new State(SpriteNum.SPR_SPID, 0, 10, ActionLook, StateNum.S_SPID_STND2, 0, 0)); // S_SPID_STND
        _states.Add(new State(SpriteNum.SPR_SPID, 1, 10, ActionLook, StateNum.S_SPID_STND, 0, 0));  // S_SPID_STND2
        _states.Add(new State(SpriteNum.SPR_SPID, 0, 3, ActionMetal, StateNum.S_SPID_RUN2, 0, 0));  // S_SPID_RUN1
        _states.Add(new State(SpriteNum.SPR_SPID, 0, 3, ActionChase, StateNum.S_SPID_RUN3, 0, 0));  // S_SPID_RUN2
        _states.Add(new State(SpriteNum.SPR_SPID, 1, 3, ActionChase, StateNum.S_SPID_RUN4, 0, 0));  // S_SPID_RUN3
        _states.Add(new State(SpriteNum.SPR_SPID, 1, 3, ActionChase, StateNum.S_SPID_RUN5, 0, 0));  // S_SPID_RUN4
        _states.Add(new State(SpriteNum.SPR_SPID, 2, 3, ActionMetal, StateNum.S_SPID_RUN6, 0, 0));  // S_SPID_RUN5
        _states.Add(new State(SpriteNum.SPR_SPID, 2, 3, ActionChase, StateNum.S_SPID_RUN7, 0, 0));  // S_SPID_RUN6
        _states.Add(new State(SpriteNum.SPR_SPID, 3, 3, ActionChase, StateNum.S_SPID_RUN8, 0, 0));  // S_SPID_RUN7
        _states.Add(new State(SpriteNum.SPR_SPID, 3, 3, ActionChase, StateNum.S_SPID_RUN9, 0, 0));  // S_SPID_RUN8
        _states.Add(new State(SpriteNum.SPR_SPID, 4, 3, ActionMetal, StateNum.S_SPID_RUN10, 0, 0)); // S_SPID_RUN9
        _states.Add(new State(SpriteNum.SPR_SPID, 4, 3, ActionChase, StateNum.S_SPID_RUN11, 0, 0)); // S_SPID_RUN10
        _states.Add(new State(SpriteNum.SPR_SPID, 5, 3, ActionChase, StateNum.S_SPID_RUN12, 0, 0)); // S_SPID_RUN11
        _states.Add(new State(SpriteNum.SPR_SPID, 5, 3, ActionChase, StateNum.S_SPID_RUN1, 0, 0));  // S_SPID_RUN12
        _states.Add(new State(SpriteNum.SPR_SPID, 32768, 20, ActionFaceTarget, StateNum.S_SPID_ATK2, 0, 0));    // S_SPID_ATK1
        _states.Add(new State(SpriteNum.SPR_SPID, 32774, 4, ActionSPosAttack, StateNum.S_SPID_ATK3, 0, 0)); // S_SPID_ATK2
        _states.Add(new State(SpriteNum.SPR_SPID, 32775, 4, ActionSPosAttack, StateNum.S_SPID_ATK4, 0, 0)); // S_SPID_ATK3
        _states.Add(new State(SpriteNum.SPR_SPID, 32775, 1, ActionSpidRefire, StateNum.S_SPID_ATK2, 0, 0)); // S_SPID_ATK4
        _states.Add(new State(SpriteNum.SPR_SPID, 8, 3, null, StateNum.S_SPID_PAIN2, 0, 0));    // S_SPID_PAIN
        _states.Add(new State(SpriteNum.SPR_SPID, 8, 3, ActionPain, StateNum.S_SPID_RUN1, 0, 0));   // S_SPID_PAIN2
        _states.Add(new State(SpriteNum.SPR_SPID, 9, 20, ActionScream, StateNum.S_SPID_DIE2, 0, 0));    // S_SPID_DIE1
        _states.Add(new State(SpriteNum.SPR_SPID, 10, 10, ActionFall, StateNum.S_SPID_DIE3, 0, 0)); // S_SPID_DIE2
        _states.Add(new State(SpriteNum.SPR_SPID, 11, 10, null, StateNum.S_SPID_DIE4, 0, 0));   // S_SPID_DIE3
        _states.Add(new State(SpriteNum.SPR_SPID, 12, 10, null, StateNum.S_SPID_DIE5, 0, 0));   // S_SPID_DIE4
        _states.Add(new State(SpriteNum.SPR_SPID, 13, 10, null, StateNum.S_SPID_DIE6, 0, 0));   // S_SPID_DIE5
        _states.Add(new State(SpriteNum.SPR_SPID, 14, 10, null, StateNum.S_SPID_DIE7, 0, 0));   // S_SPID_DIE6
        _states.Add(new State(SpriteNum.SPR_SPID, 15, 10, null, StateNum.S_SPID_DIE8, 0, 0));   // S_SPID_DIE7
        _states.Add(new State(SpriteNum.SPR_SPID, 16, 10, null, StateNum.S_SPID_DIE9, 0, 0));   // S_SPID_DIE8
        _states.Add(new State(SpriteNum.SPR_SPID, 17, 10, null, StateNum.S_SPID_DIE10, 0, 0));  // S_SPID_DIE9
        _states.Add(new State(SpriteNum.SPR_SPID, 18, 30, null, StateNum.S_SPID_DIE11, 0, 0));  // S_SPID_DIE10
        _states.Add(new State(SpriteNum.SPR_SPID, 18, -1, ActionBossDeath, StateNum.S_NULL, 0, 0)); // S_SPID_DIE11
        _states.Add(new State(SpriteNum.SPR_BSPI, 0, 10, ActionLook, StateNum.S_BSPI_STND2, 0, 0)); // S_BSPI_STND
        _states.Add(new State(SpriteNum.SPR_BSPI, 1, 10, ActionLook, StateNum.S_BSPI_STND, 0, 0));  // S_BSPI_STND2
        _states.Add(new State(SpriteNum.SPR_BSPI, 0, 20, null, StateNum.S_BSPI_RUN1, 0, 0));    // S_BSPI_SIGHT
        _states.Add(new State(SpriteNum.SPR_BSPI, 0, 3, ActionBabyMetal, StateNum.S_BSPI_RUN2, 0, 0));  // S_BSPI_RUN1
        _states.Add(new State(SpriteNum.SPR_BSPI, 0, 3, ActionChase, StateNum.S_BSPI_RUN3, 0, 0));  // S_BSPI_RUN2
        _states.Add(new State(SpriteNum.SPR_BSPI, 1, 3, ActionChase, StateNum.S_BSPI_RUN4, 0, 0));  // S_BSPI_RUN3
        _states.Add(new State(SpriteNum.SPR_BSPI, 1, 3, ActionChase, StateNum.S_BSPI_RUN5, 0, 0));  // S_BSPI_RUN4
        _states.Add(new State(SpriteNum.SPR_BSPI, 2, 3, ActionChase, StateNum.S_BSPI_RUN6, 0, 0));  // S_BSPI_RUN5
        _states.Add(new State(SpriteNum.SPR_BSPI, 2, 3, ActionChase, StateNum.S_BSPI_RUN7, 0, 0));  // S_BSPI_RUN6
        _states.Add(new State(SpriteNum.SPR_BSPI, 3, 3, ActionBabyMetal, StateNum.S_BSPI_RUN8, 0, 0));  // S_BSPI_RUN7
        _states.Add(new State(SpriteNum.SPR_BSPI, 3, 3, ActionChase, StateNum.S_BSPI_RUN9, 0, 0));  // S_BSPI_RUN8
        _states.Add(new State(SpriteNum.SPR_BSPI, 4, 3, ActionChase, StateNum.S_BSPI_RUN10, 0, 0)); // S_BSPI_RUN9
        _states.Add(new State(SpriteNum.SPR_BSPI, 4, 3, ActionChase, StateNum.S_BSPI_RUN11, 0, 0)); // S_BSPI_RUN10
        _states.Add(new State(SpriteNum.SPR_BSPI, 5, 3, ActionChase, StateNum.S_BSPI_RUN12, 0, 0)); // S_BSPI_RUN11
        _states.Add(new State(SpriteNum.SPR_BSPI, 5, 3, ActionChase, StateNum.S_BSPI_RUN1, 0, 0));  // S_BSPI_RUN12
        _states.Add(new State(SpriteNum.SPR_BSPI, 32768, 20, ActionFaceTarget, StateNum.S_BSPI_ATK2, 0, 0));    // S_BSPI_ATK1
        _states.Add(new State(SpriteNum.SPR_BSPI, 32774, 4, ActionBspiAttack, StateNum.S_BSPI_ATK3, 0, 0)); // S_BSPI_ATK2
        _states.Add(new State(SpriteNum.SPR_BSPI, 32775, 4, null, StateNum.S_BSPI_ATK4, 0, 0)); // S_BSPI_ATK3
        _states.Add(new State(SpriteNum.SPR_BSPI, 32775, 1, ActionSpidRefire, StateNum.S_BSPI_ATK2, 0, 0)); // S_BSPI_ATK4
        _states.Add(new State(SpriteNum.SPR_BSPI, 8, 3, null, StateNum.S_BSPI_PAIN2, 0, 0));    // S_BSPI_PAIN
        _states.Add(new State(SpriteNum.SPR_BSPI, 8, 3, ActionPain, StateNum.S_BSPI_RUN1, 0, 0));   // S_BSPI_PAIN2
        _states.Add(new State(SpriteNum.SPR_BSPI, 9, 20, ActionScream, StateNum.S_BSPI_DIE2, 0, 0));    // S_BSPI_DIE1
        _states.Add(new State(SpriteNum.SPR_BSPI, 10, 7, ActionFall, StateNum.S_BSPI_DIE3, 0, 0));  // S_BSPI_DIE2
        _states.Add(new State(SpriteNum.SPR_BSPI, 11, 7, null, StateNum.S_BSPI_DIE4, 0, 0));    // S_BSPI_DIE3
        _states.Add(new State(SpriteNum.SPR_BSPI, 12, 7, null, StateNum.S_BSPI_DIE5, 0, 0));    // S_BSPI_DIE4
        _states.Add(new State(SpriteNum.SPR_BSPI, 13, 7, null, StateNum.S_BSPI_DIE6, 0, 0));    // S_BSPI_DIE5
        _states.Add(new State(SpriteNum.SPR_BSPI, 14, 7, null, StateNum.S_BSPI_DIE7, 0, 0));    // S_BSPI_DIE6
        _states.Add(new State(SpriteNum.SPR_BSPI, 15, -1, ActionBossDeath, StateNum.S_NULL, 0, 0)); // S_BSPI_DIE7
        _states.Add(new State(SpriteNum.SPR_BSPI, 15, 5, null, StateNum.S_BSPI_RAISE2, 0, 0));  // S_BSPI_RAISE1
        _states.Add(new State(SpriteNum.SPR_BSPI, 14, 5, null, StateNum.S_BSPI_RAISE3, 0, 0));  // S_BSPI_RAISE2
        _states.Add(new State(SpriteNum.SPR_BSPI, 13, 5, null, StateNum.S_BSPI_RAISE4, 0, 0));  // S_BSPI_RAISE3
        _states.Add(new State(SpriteNum.SPR_BSPI, 12, 5, null, StateNum.S_BSPI_RAISE5, 0, 0));  // S_BSPI_RAISE4
        _states.Add(new State(SpriteNum.SPR_BSPI, 11, 5, null, StateNum.S_BSPI_RAISE6, 0, 0));  // S_BSPI_RAISE5
        _states.Add(new State(SpriteNum.SPR_BSPI, 10, 5, null, StateNum.S_BSPI_RAISE7, 0, 0));  // S_BSPI_RAISE6
        _states.Add(new State(SpriteNum.SPR_BSPI, 9, 5, null, StateNum.S_BSPI_RUN1, 0, 0)); // S_BSPI_RAISE7
        _states.Add(new State(SpriteNum.SPR_APLS, 32768, 5, null, StateNum.S_ARACH_PLAZ2, 0, 0));   // S_ARACH_PLAZ
        _states.Add(new State(SpriteNum.SPR_APLS, 32769, 5, null, StateNum.S_ARACH_PLAZ, 0, 0));    // S_ARACH_PLAZ2
        _states.Add(new State(SpriteNum.SPR_APBX, 32768, 5, null, StateNum.S_ARACH_PLEX2, 0, 0));   // S_ARACH_PLEX
        _states.Add(new State(SpriteNum.SPR_APBX, 32769, 5, null, StateNum.S_ARACH_PLEX3, 0, 0));   // S_ARACH_PLEX2
        _states.Add(new State(SpriteNum.SPR_APBX, 32770, 5, null, StateNum.S_ARACH_PLEX4, 0, 0));   // S_ARACH_PLEX3
        _states.Add(new State(SpriteNum.SPR_APBX, 32771, 5, null, StateNum.S_ARACH_PLEX5, 0, 0));   // S_ARACH_PLEX4
        _states.Add(new State(SpriteNum.SPR_APBX, 32772, 5, null, StateNum.S_NULL, 0, 0));  // S_ARACH_PLEX5
        _states.Add(new State(SpriteNum.SPR_CYBR, 0, 10, ActionLook, StateNum.S_CYBER_STND2, 0, 0));    // S_CYBER_STND
        _states.Add(new State(SpriteNum.SPR_CYBR, 1, 10, ActionLook, StateNum.S_CYBER_STND, 0, 0)); // S_CYBER_STND2
        _states.Add(new State(SpriteNum.SPR_CYBR, 0, 3, ActionHoof, StateNum.S_CYBER_RUN2, 0, 0));  // S_CYBER_RUN1
        _states.Add(new State(SpriteNum.SPR_CYBR, 0, 3, ActionChase, StateNum.S_CYBER_RUN3, 0, 0)); // S_CYBER_RUN2
        _states.Add(new State(SpriteNum.SPR_CYBR, 1, 3, ActionChase, StateNum.S_CYBER_RUN4, 0, 0)); // S_CYBER_RUN3
        _states.Add(new State(SpriteNum.SPR_CYBR, 1, 3, ActionChase, StateNum.S_CYBER_RUN5, 0, 0)); // S_CYBER_RUN4
        _states.Add(new State(SpriteNum.SPR_CYBR, 2, 3, ActionChase, StateNum.S_CYBER_RUN6, 0, 0)); // S_CYBER_RUN5
        _states.Add(new State(SpriteNum.SPR_CYBR, 2, 3, ActionChase, StateNum.S_CYBER_RUN7, 0, 0)); // S_CYBER_RUN6
        _states.Add(new State(SpriteNum.SPR_CYBR, 3, 3, ActionMetal, StateNum.S_CYBER_RUN8, 0, 0)); // S_CYBER_RUN7
        _states.Add(new State(SpriteNum.SPR_CYBR, 3, 3, ActionChase, StateNum.S_CYBER_RUN1, 0, 0)); // S_CYBER_RUN8
        _states.Add(new State(SpriteNum.SPR_CYBR, 4, 6, ActionFaceTarget, StateNum.S_CYBER_ATK2, 0, 0));    // S_CYBER_ATK1
        _states.Add(new State(SpriteNum.SPR_CYBR, 5, 12, ActionCyberAttack, StateNum.S_CYBER_ATK3, 0, 0));  // S_CYBER_ATK2
        _states.Add(new State(SpriteNum.SPR_CYBR, 4, 12, ActionFaceTarget, StateNum.S_CYBER_ATK4, 0, 0));   // S_CYBER_ATK3
        _states.Add(new State(SpriteNum.SPR_CYBR, 5, 12, ActionCyberAttack, StateNum.S_CYBER_ATK5, 0, 0));  // S_CYBER_ATK4
        _states.Add(new State(SpriteNum.SPR_CYBR, 4, 12, ActionFaceTarget, StateNum.S_CYBER_ATK6, 0, 0));   // S_CYBER_ATK5
        _states.Add(new State(SpriteNum.SPR_CYBR, 5, 12, ActionCyberAttack, StateNum.S_CYBER_RUN1, 0, 0));  // S_CYBER_ATK6
        _states.Add(new State(SpriteNum.SPR_CYBR, 6, 10, ActionPain, StateNum.S_CYBER_RUN1, 0, 0)); // S_CYBER_PAIN
        _states.Add(new State(SpriteNum.SPR_CYBR, 7, 10, null, StateNum.S_CYBER_DIE2, 0, 0));   // S_CYBER_DIE1
        _states.Add(new State(SpriteNum.SPR_CYBR, 8, 10, ActionScream, StateNum.S_CYBER_DIE3, 0, 0));   // S_CYBER_DIE2
        _states.Add(new State(SpriteNum.SPR_CYBR, 9, 10, null, StateNum.S_CYBER_DIE4, 0, 0));   // S_CYBER_DIE3
        _states.Add(new State(SpriteNum.SPR_CYBR, 10, 10, null, StateNum.S_CYBER_DIE5, 0, 0));  // S_CYBER_DIE4
        _states.Add(new State(SpriteNum.SPR_CYBR, 11, 10, null, StateNum.S_CYBER_DIE6, 0, 0));  // S_CYBER_DIE5
        _states.Add(new State(SpriteNum.SPR_CYBR, 12, 10, ActionFall, StateNum.S_CYBER_DIE7, 0, 0));    // S_CYBER_DIE6
        _states.Add(new State(SpriteNum.SPR_CYBR, 13, 10, null, StateNum.S_CYBER_DIE8, 0, 0));  // S_CYBER_DIE7
        _states.Add(new State(SpriteNum.SPR_CYBR, 14, 10, null, StateNum.S_CYBER_DIE9, 0, 0));  // S_CYBER_DIE8
        _states.Add(new State(SpriteNum.SPR_CYBR, 15, 30, null, StateNum.S_CYBER_DIE10, 0, 0)); // S_CYBER_DIE9
        _states.Add(new State(SpriteNum.SPR_CYBR, 15, -1, ActionBossDeath, StateNum.S_NULL, 0, 0)); // S_CYBER_DIE10
        _states.Add(new State(SpriteNum.SPR_PAIN, 0, 10, ActionLook, StateNum.S_PAIN_STND, 0, 0));  // S_PAIN_STND
        _states.Add(new State(SpriteNum.SPR_PAIN, 0, 3, ActionChase, StateNum.S_PAIN_RUN2, 0, 0));  // S_PAIN_RUN1
        _states.Add(new State(SpriteNum.SPR_PAIN, 0, 3, ActionChase, StateNum.S_PAIN_RUN3, 0, 0));  // S_PAIN_RUN2
        _states.Add(new State(SpriteNum.SPR_PAIN, 1, 3, ActionChase, StateNum.S_PAIN_RUN4, 0, 0));  // S_PAIN_RUN3
        _states.Add(new State(SpriteNum.SPR_PAIN, 1, 3, ActionChase, StateNum.S_PAIN_RUN5, 0, 0));  // S_PAIN_RUN4
        _states.Add(new State(SpriteNum.SPR_PAIN, 2, 3, ActionChase, StateNum.S_PAIN_RUN6, 0, 0));  // S_PAIN_RUN5
        _states.Add(new State(SpriteNum.SPR_PAIN, 2, 3, ActionChase, StateNum.S_PAIN_RUN1, 0, 0));  // S_PAIN_RUN6
        _states.Add(new State(SpriteNum.SPR_PAIN, 3, 5, ActionFaceTarget, StateNum.S_PAIN_ATK2, 0, 0)); // S_PAIN_ATK1
        _states.Add(new State(SpriteNum.SPR_PAIN, 4, 5, ActionFaceTarget, StateNum.S_PAIN_ATK3, 0, 0)); // S_PAIN_ATK2
        _states.Add(new State(SpriteNum.SPR_PAIN, 32773, 5, ActionFaceTarget, StateNum.S_PAIN_ATK4, 0, 0)); // S_PAIN_ATK3
        _states.Add(new State(SpriteNum.SPR_PAIN, 32773, 0, ActionPainAttack, StateNum.S_PAIN_RUN1, 0, 0)); // S_PAIN_ATK4
        _states.Add(new State(SpriteNum.SPR_PAIN, 6, 6, null, StateNum.S_PAIN_PAIN2, 0, 0));    // S_PAIN_PAIN
        _states.Add(new State(SpriteNum.SPR_PAIN, 6, 6, ActionPain, StateNum.S_PAIN_RUN1, 0, 0));   // S_PAIN_PAIN2
        _states.Add(new State(SpriteNum.SPR_PAIN, 32775, 8, null, StateNum.S_PAIN_DIE2, 0, 0)); // S_PAIN_DIE1
        _states.Add(new State(SpriteNum.SPR_PAIN, 32776, 8, ActionScream, StateNum.S_PAIN_DIE3, 0, 0)); // S_PAIN_DIE2
        _states.Add(new State(SpriteNum.SPR_PAIN, 32777, 8, null, StateNum.S_PAIN_DIE4, 0, 0)); // S_PAIN_DIE3
        _states.Add(new State(SpriteNum.SPR_PAIN, 32778, 8, null, StateNum.S_PAIN_DIE5, 0, 0)); // S_PAIN_DIE4
        _states.Add(new State(SpriteNum.SPR_PAIN, 32779, 8, ActionPainDie, StateNum.S_PAIN_DIE6, 0, 0));    // S_PAIN_DIE5
        _states.Add(new State(SpriteNum.SPR_PAIN, 32780, 8, null, StateNum.S_NULL, 0, 0));  // S_PAIN_DIE6
        _states.Add(new State(SpriteNum.SPR_PAIN, 12, 8, null, StateNum.S_PAIN_RAISE2, 0, 0));  // S_PAIN_RAISE1
        _states.Add(new State(SpriteNum.SPR_PAIN, 11, 8, null, StateNum.S_PAIN_RAISE3, 0, 0));  // S_PAIN_RAISE2
        _states.Add(new State(SpriteNum.SPR_PAIN, 10, 8, null, StateNum.S_PAIN_RAISE4, 0, 0));  // S_PAIN_RAISE3
        _states.Add(new State(SpriteNum.SPR_PAIN, 9, 8, null, StateNum.S_PAIN_RAISE5, 0, 0));   // S_PAIN_RAISE4
        _states.Add(new State(SpriteNum.SPR_PAIN, 8, 8, null, StateNum.S_PAIN_RAISE6, 0, 0));   // S_PAIN_RAISE5
        _states.Add(new State(SpriteNum.SPR_PAIN, 7, 8, null, StateNum.S_PAIN_RUN1, 0, 0)); // S_PAIN_RAISE6
        _states.Add(new State(SpriteNum.SPR_SSWV, 0, 10, ActionLook, StateNum.S_SSWV_STND2, 0, 0)); // S_SSWV_STND
        _states.Add(new State(SpriteNum.SPR_SSWV, 1, 10, ActionLook, StateNum.S_SSWV_STND, 0, 0));  // S_SSWV_STND2
        _states.Add(new State(SpriteNum.SPR_SSWV, 0, 3, ActionChase, StateNum.S_SSWV_RUN2, 0, 0));  // S_SSWV_RUN1
        _states.Add(new State(SpriteNum.SPR_SSWV, 0, 3, ActionChase, StateNum.S_SSWV_RUN3, 0, 0));  // S_SSWV_RUN2
        _states.Add(new State(SpriteNum.SPR_SSWV, 1, 3, ActionChase, StateNum.S_SSWV_RUN4, 0, 0));  // S_SSWV_RUN3
        _states.Add(new State(SpriteNum.SPR_SSWV, 1, 3, ActionChase, StateNum.S_SSWV_RUN5, 0, 0));  // S_SSWV_RUN4
        _states.Add(new State(SpriteNum.SPR_SSWV, 2, 3, ActionChase, StateNum.S_SSWV_RUN6, 0, 0));  // S_SSWV_RUN5
        _states.Add(new State(SpriteNum.SPR_SSWV, 2, 3, ActionChase, StateNum.S_SSWV_RUN7, 0, 0));  // S_SSWV_RUN6
        _states.Add(new State(SpriteNum.SPR_SSWV, 3, 3, ActionChase, StateNum.S_SSWV_RUN8, 0, 0));  // S_SSWV_RUN7
        _states.Add(new State(SpriteNum.SPR_SSWV, 3, 3, ActionChase, StateNum.S_SSWV_RUN1, 0, 0));  // S_SSWV_RUN8
        _states.Add(new State(SpriteNum.SPR_SSWV, 4, 10, ActionFaceTarget, StateNum.S_SSWV_ATK2, 0, 0));    // S_SSWV_ATK1
        _states.Add(new State(SpriteNum.SPR_SSWV, 5, 10, ActionFaceTarget, StateNum.S_SSWV_ATK3, 0, 0));    // S_SSWV_ATK2
        _states.Add(new State(SpriteNum.SPR_SSWV, 32774, 4, ActionCPosAttack, StateNum.S_SSWV_ATK4, 0, 0)); // S_SSWV_ATK3
        _states.Add(new State(SpriteNum.SPR_SSWV, 5, 6, ActionFaceTarget, StateNum.S_SSWV_ATK5, 0, 0)); // S_SSWV_ATK4
        _states.Add(new State(SpriteNum.SPR_SSWV, 32774, 4, ActionCPosAttack, StateNum.S_SSWV_ATK6, 0, 0)); // S_SSWV_ATK5
        _states.Add(new State(SpriteNum.SPR_SSWV, 5, 1, ActionCPosRefire, StateNum.S_SSWV_ATK2, 0, 0)); // S_SSWV_ATK6
        _states.Add(new State(SpriteNum.SPR_SSWV, 7, 3, null, StateNum.S_SSWV_PAIN2, 0, 0));    // S_SSWV_PAIN
        _states.Add(new State(SpriteNum.SPR_SSWV, 7, 3, ActionPain, StateNum.S_SSWV_RUN1, 0, 0));   // S_SSWV_PAIN2
        _states.Add(new State(SpriteNum.SPR_SSWV, 8, 5, null, StateNum.S_SSWV_DIE2, 0, 0)); // S_SSWV_DIE1
        _states.Add(new State(SpriteNum.SPR_SSWV, 9, 5, ActionScream, StateNum.S_SSWV_DIE3, 0, 0)); // S_SSWV_DIE2
        _states.Add(new State(SpriteNum.SPR_SSWV, 10, 5, ActionFall, StateNum.S_SSWV_DIE4, 0, 0));  // S_SSWV_DIE3
        _states.Add(new State(SpriteNum.SPR_SSWV, 11, 5, null, StateNum.S_SSWV_DIE5, 0, 0));    // S_SSWV_DIE4
        _states.Add(new State(SpriteNum.SPR_SSWV, 12, -1, null, StateNum.S_NULL, 0, 0));    // S_SSWV_DIE5
        _states.Add(new State(SpriteNum.SPR_SSWV, 13, 5, null, StateNum.S_SSWV_XDIE2, 0, 0));   // S_SSWV_XDIE1
        _states.Add(new State(SpriteNum.SPR_SSWV, 14, 5, ActionXScream, StateNum.S_SSWV_XDIE3, 0, 0));  // S_SSWV_XDIE2
        _states.Add(new State(SpriteNum.SPR_SSWV, 15, 5, ActionFall, StateNum.S_SSWV_XDIE4, 0, 0)); // S_SSWV_XDIE3
        _states.Add(new State(SpriteNum.SPR_SSWV, 16, 5, null, StateNum.S_SSWV_XDIE5, 0, 0));   // S_SSWV_XDIE4
        _states.Add(new State(SpriteNum.SPR_SSWV, 17, 5, null, StateNum.S_SSWV_XDIE6, 0, 0));   // S_SSWV_XDIE5
        _states.Add(new State(SpriteNum.SPR_SSWV, 18, 5, null, StateNum.S_SSWV_XDIE7, 0, 0));   // S_SSWV_XDIE6
        _states.Add(new State(SpriteNum.SPR_SSWV, 19, 5, null, StateNum.S_SSWV_XDIE8, 0, 0));   // S_SSWV_XDIE7
        _states.Add(new State(SpriteNum.SPR_SSWV, 20, 5, null, StateNum.S_SSWV_XDIE9, 0, 0));   // S_SSWV_XDIE8
        _states.Add(new State(SpriteNum.SPR_SSWV, 21, -1, null, StateNum.S_NULL, 0, 0));    // S_SSWV_XDIE9
        _states.Add(new State(SpriteNum.SPR_SSWV, 12, 5, null, StateNum.S_SSWV_RAISE2, 0, 0));  // S_SSWV_RAISE1
        _states.Add(new State(SpriteNum.SPR_SSWV, 11, 5, null, StateNum.S_SSWV_RAISE3, 0, 0));  // S_SSWV_RAISE2
        _states.Add(new State(SpriteNum.SPR_SSWV, 10, 5, null, StateNum.S_SSWV_RAISE4, 0, 0));  // S_SSWV_RAISE3
        _states.Add(new State(SpriteNum.SPR_SSWV, 9, 5, null, StateNum.S_SSWV_RAISE5, 0, 0));   // S_SSWV_RAISE4
        _states.Add(new State(SpriteNum.SPR_SSWV, 8, 5, null, StateNum.S_SSWV_RUN1, 0, 0)); // S_SSWV_RAISE5
        _states.Add(new State(SpriteNum.SPR_KEEN, 0, -1, null, StateNum.S_KEENSTND, 0, 0)); // S_KEENSTND
        _states.Add(new State(SpriteNum.SPR_KEEN, 0, 6, null, StateNum.S_COMMKEEN2, 0, 0)); // S_COMMKEEN
        _states.Add(new State(SpriteNum.SPR_KEEN, 1, 6, null, StateNum.S_COMMKEEN3, 0, 0)); // S_COMMKEEN2
        _states.Add(new State(SpriteNum.SPR_KEEN, 2, 6, ActionScream, StateNum.S_COMMKEEN4, 0, 0)); // S_COMMKEEN3
        _states.Add(new State(SpriteNum.SPR_KEEN, 3, 6, null, StateNum.S_COMMKEEN5, 0, 0)); // S_COMMKEEN4
        _states.Add(new State(SpriteNum.SPR_KEEN, 4, 6, null, StateNum.S_COMMKEEN6, 0, 0)); // S_COMMKEEN5
        _states.Add(new State(SpriteNum.SPR_KEEN, 5, 6, null, StateNum.S_COMMKEEN7, 0, 0)); // S_COMMKEEN6
        _states.Add(new State(SpriteNum.SPR_KEEN, 6, 6, null, StateNum.S_COMMKEEN8, 0, 0)); // S_COMMKEEN7
        _states.Add(new State(SpriteNum.SPR_KEEN, 7, 6, null, StateNum.S_COMMKEEN9, 0, 0)); // S_COMMKEEN8
        _states.Add(new State(SpriteNum.SPR_KEEN, 8, 6, null, StateNum.S_COMMKEEN10, 0, 0));    // S_COMMKEEN9
        _states.Add(new State(SpriteNum.SPR_KEEN, 9, 6, null, StateNum.S_COMMKEEN11, 0, 0));    // S_COMMKEEN10
        _states.Add(new State(SpriteNum.SPR_KEEN, 10, 6, ActionKeenDie, StateNum.S_COMMKEEN12, 0, 0));// S_COMMKEEN11
        _states.Add(new State(SpriteNum.SPR_KEEN, 11, -1, null, StateNum.S_NULL, 0, 0));     // S_COMMKEEN12
        _states.Add(new State(SpriteNum.SPR_KEEN, 12, 4, null, StateNum.S_KEENPAIN2, 0, 0));    // S_KEENPAIN
        _states.Add(new State(SpriteNum.SPR_KEEN, 12, 8, ActionPain, StateNum.S_KEENSTND, 0, 0));   // S_KEENPAIN2
        _states.Add(new State(SpriteNum.SPR_BBRN, 0, -1, null, StateNum.S_NULL, 0, 0));      // S_BRAIN
        _states.Add(new State(SpriteNum.SPR_BBRN, 1, 36, ActionBrainPain, StateNum.S_BRAIN, 0, 0)); // S_BRAIN_PAIN
        _states.Add(new State(SpriteNum.SPR_BBRN, 0, 100, ActionBrainScream, StateNum.S_BRAIN_DIE2, 0, 0)); // S_BRAIN_DIE1
        _states.Add(new State(SpriteNum.SPR_BBRN, 0, 10, null, StateNum.S_BRAIN_DIE3, 0, 0));   // S_BRAIN_DIE2
        _states.Add(new State(SpriteNum.SPR_BBRN, 0, 10, null, StateNum.S_BRAIN_DIE4, 0, 0));   // S_BRAIN_DIE3
        _states.Add(new State(SpriteNum.SPR_BBRN, 0, -1, ActionBrainDie, StateNum.S_NULL, 0, 0));   // S_BRAIN_DIE4
        _states.Add(new State(SpriteNum.SPR_SSWV, 0, 10, ActionLook, StateNum.S_BRAINEYE, 0, 0));   // S_BRAINEYE
        _states.Add(new State(SpriteNum.SPR_SSWV, 0, 181, ActionBrainAwake, StateNum.S_BRAINEYE1, 0, 0));   // S_BRAINEYESEE
        _states.Add(new State(SpriteNum.SPR_SSWV, 0, 150, ActionBrainSpit, StateNum.S_BRAINEYE1, 0, 0));    // S_BRAINEYE1
        _states.Add(new State(SpriteNum.SPR_BOSF, 32768, 3, ActionSpawnSound, StateNum.S_SPAWN2, 0, 0));    // S_SPAWN1
        _states.Add(new State(SpriteNum.SPR_BOSF, 32769, 3, ActionSpawnFly, StateNum.S_SPAWN3, 0, 0));  // S_SPAWN2
        _states.Add(new State(SpriteNum.SPR_BOSF, 32770, 3, ActionSpawnFly, StateNum.S_SPAWN4, 0, 0));  // S_SPAWN3
        _states.Add(new State(SpriteNum.SPR_BOSF, 32771, 3, ActionSpawnFly, StateNum.S_SPAWN1, 0, 0));  // S_SPAWN4
        _states.Add(new State(SpriteNum.SPR_FIRE, 32768, 4, ActionFire, StateNum.S_SPAWNFIRE2, 0, 0));  // S_SPAWNFIRE1
        _states.Add(new State(SpriteNum.SPR_FIRE, 32769, 4, ActionFire, StateNum.S_SPAWNFIRE3, 0, 0));  // S_SPAWNFIRE2
        _states.Add(new State(SpriteNum.SPR_FIRE, 32770, 4, ActionFire, StateNum.S_SPAWNFIRE4, 0, 0));  // S_SPAWNFIRE3
        _states.Add(new State(SpriteNum.SPR_FIRE, 32771, 4, ActionFire, StateNum.S_SPAWNFIRE5, 0, 0));  // S_SPAWNFIRE4
        _states.Add(new State(SpriteNum.SPR_FIRE, 32772, 4, ActionFire, StateNum.S_SPAWNFIRE6, 0, 0));  // S_SPAWNFIRE5
        _states.Add(new State(SpriteNum.SPR_FIRE, 32773, 4, ActionFire, StateNum.S_SPAWNFIRE7, 0, 0));  // S_SPAWNFIRE6
        _states.Add(new State(SpriteNum.SPR_FIRE, 32774, 4, ActionFire, StateNum.S_SPAWNFIRE8, 0, 0));  // S_SPAWNFIRE7
        _states.Add(new State(SpriteNum.SPR_FIRE, 32775, 4, ActionFire, StateNum.S_NULL, 0, 0));        // S_SPAWNFIRE8
        _states.Add(new State(SpriteNum.SPR_MISL, 32769, 10, null, StateNum.S_BRAINEXPLODE2, 0, 0));    // S_BRAINEXPLODE1
        _states.Add(new State(SpriteNum.SPR_MISL, 32770, 10, null, StateNum.S_BRAINEXPLODE3, 0, 0));    // S_BRAINEXPLODE2
        _states.Add(new State(SpriteNum.SPR_MISL, 32771, 10, ActionBrainExplode, StateNum.S_NULL, 0, 0));   // S_BRAINEXPLODE3
        _states.Add(new State(SpriteNum.SPR_ARM1, 0, 6, null, StateNum.S_ARM1A, 0, 0)); // S_ARM1
        _states.Add(new State(SpriteNum.SPR_ARM1, 32769, 7, null, StateNum.S_ARM1, 0, 0));  // S_ARM1A
        _states.Add(new State(SpriteNum.SPR_ARM2, 0, 6, null, StateNum.S_ARM2A, 0, 0)); // S_ARM2
        _states.Add(new State(SpriteNum.SPR_ARM2, 32769, 6, null, StateNum.S_ARM2, 0, 0));  // S_ARM2A
        _states.Add(new State(SpriteNum.SPR_BAR1, 0, 6, null, StateNum.S_BAR2, 0, 0));  // S_BAR1
        _states.Add(new State(SpriteNum.SPR_BAR1, 1, 6, null, StateNum.S_BAR1, 0, 0));  // S_BAR2
        _states.Add(new State(SpriteNum.SPR_BEXP, 32768, 5, null, StateNum.S_BEXP2, 0, 0)); // S_BEXP
        _states.Add(new State(SpriteNum.SPR_BEXP, 32769, 5, ActionScream, StateNum.S_BEXP3, 0, 0)); // S_BEXP2
        _states.Add(new State(SpriteNum.SPR_BEXP, 32770, 5, null, StateNum.S_BEXP4, 0, 0)); // S_BEXP3
        _states.Add(new State(SpriteNum.SPR_BEXP, 32771, 10, ActionExplode, StateNum.S_BEXP5, 0, 0));   // S_BEXP4
        _states.Add(new State(SpriteNum.SPR_BEXP, 32772, 10, null, StateNum.S_NULL, 0, 0)); // S_BEXP5
        _states.Add(new State(SpriteNum.SPR_FCAN, 32768, 4, null, StateNum.S_BBAR2, 0, 0)); // S_BBAR1
        _states.Add(new State(SpriteNum.SPR_FCAN, 32769, 4, null, StateNum.S_BBAR3, 0, 0)); // S_BBAR2
        _states.Add(new State(SpriteNum.SPR_FCAN, 32770, 4, null, StateNum.S_BBAR1, 0, 0)); // S_BBAR3
        _states.Add(new State(SpriteNum.SPR_BON1, 0, 6, null, StateNum.S_BON1A, 0, 0)); // S_BON1
        _states.Add(new State(SpriteNum.SPR_BON1, 1, 6, null, StateNum.S_BON1B, 0, 0)); // S_BON1A
        _states.Add(new State(SpriteNum.SPR_BON1, 2, 6, null, StateNum.S_BON1C, 0, 0)); // S_BON1B
        _states.Add(new State(SpriteNum.SPR_BON1, 3, 6, null, StateNum.S_BON1D, 0, 0)); // S_BON1C
        _states.Add(new State(SpriteNum.SPR_BON1, 2, 6, null, StateNum.S_BON1E, 0, 0)); // S_BON1D
        _states.Add(new State(SpriteNum.SPR_BON1, 1, 6, null, StateNum.S_BON1, 0, 0));  // S_BON1E
        _states.Add(new State(SpriteNum.SPR_BON2, 0, 6, null, StateNum.S_BON2A, 0, 0)); // S_BON2
        _states.Add(new State(SpriteNum.SPR_BON2, 1, 6, null, StateNum.S_BON2B, 0, 0)); // S_BON2A
        _states.Add(new State(SpriteNum.SPR_BON2, 2, 6, null, StateNum.S_BON2C, 0, 0)); // S_BON2B
        _states.Add(new State(SpriteNum.SPR_BON2, 3, 6, null, StateNum.S_BON2D, 0, 0)); // S_BON2C
        _states.Add(new State(SpriteNum.SPR_BON2, 2, 6, null, StateNum.S_BON2E, 0, 0)); // S_BON2D
        _states.Add(new State(SpriteNum.SPR_BON2, 1, 6, null, StateNum.S_BON2, 0, 0));  // S_BON2E
        _states.Add(new State(SpriteNum.SPR_BKEY, 0, 10, null, StateNum.S_BKEY2, 0, 0));    // S_BKEY
        _states.Add(new State(SpriteNum.SPR_BKEY, 32769, 10, null, StateNum.S_BKEY, 0, 0)); // S_BKEY2
        _states.Add(new State(SpriteNum.SPR_RKEY, 0, 10, null, StateNum.S_RKEY2, 0, 0));    // S_RKEY
        _states.Add(new State(SpriteNum.SPR_RKEY, 32769, 10, null, StateNum.S_RKEY, 0, 0)); // S_RKEY2
        _states.Add(new State(SpriteNum.SPR_YKEY, 0, 10, null, StateNum.S_YKEY2, 0, 0));    // S_YKEY
        _states.Add(new State(SpriteNum.SPR_YKEY, 32769, 10, null, StateNum.S_YKEY, 0, 0)); // S_YKEY2
        _states.Add(new State(SpriteNum.SPR_BSKU, 0, 10, null, StateNum.S_BSKULL2, 0, 0));  // S_BSKULL
        _states.Add(new State(SpriteNum.SPR_BSKU, 32769, 10, null, StateNum.S_BSKULL, 0, 0));   // S_BSKULL2
        _states.Add(new State(SpriteNum.SPR_RSKU, 0, 10, null, StateNum.S_RSKULL2, 0, 0));  // S_RSKULL
        _states.Add(new State(SpriteNum.SPR_RSKU, 32769, 10, null, StateNum.S_RSKULL, 0, 0));   // S_RSKULL2
        _states.Add(new State(SpriteNum.SPR_YSKU, 0, 10, null, StateNum.S_YSKULL2, 0, 0));  // S_YSKULL
        _states.Add(new State(SpriteNum.SPR_YSKU, 32769, 10, null, StateNum.S_YSKULL, 0, 0));   // S_YSKULL2
        _states.Add(new State(SpriteNum.SPR_STIM, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_STIM
        _states.Add(new State(SpriteNum.SPR_MEDI, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_MEDI
        _states.Add(new State(SpriteNum.SPR_SOUL, 32768, 6, null, StateNum.S_SOUL2, 0, 0)); // S_SOUL
        _states.Add(new State(SpriteNum.SPR_SOUL, 32769, 6, null, StateNum.S_SOUL3, 0, 0)); // S_SOUL2
        _states.Add(new State(SpriteNum.SPR_SOUL, 32770, 6, null, StateNum.S_SOUL4, 0, 0)); // S_SOUL3
        _states.Add(new State(SpriteNum.SPR_SOUL, 32771, 6, null, StateNum.S_SOUL5, 0, 0)); // S_SOUL4
        _states.Add(new State(SpriteNum.SPR_SOUL, 32770, 6, null, StateNum.S_SOUL6, 0, 0)); // S_SOUL5
        _states.Add(new State(SpriteNum.SPR_SOUL, 32769, 6, null, StateNum.S_SOUL, 0, 0));  // S_SOUL6
        _states.Add(new State(SpriteNum.SPR_PINV, 32768, 6, null, StateNum.S_PINV2, 0, 0)); // S_PINV
        _states.Add(new State(SpriteNum.SPR_PINV, 32769, 6, null, StateNum.S_PINV3, 0, 0)); // S_PINV2
        _states.Add(new State(SpriteNum.SPR_PINV, 32770, 6, null, StateNum.S_PINV4, 0, 0)); // S_PINV3
        _states.Add(new State(SpriteNum.SPR_PINV, 32771, 6, null, StateNum.S_PINV, 0, 0));  // S_PINV4
        _states.Add(new State(SpriteNum.SPR_PSTR, 32768, -1, null, StateNum.S_NULL, 0, 0)); // S_PSTR
        _states.Add(new State(SpriteNum.SPR_PINS, 32768, 6, null, StateNum.S_PINS2, 0, 0)); // S_PINS
        _states.Add(new State(SpriteNum.SPR_PINS, 32769, 6, null, StateNum.S_PINS3, 0, 0)); // S_PINS2
        _states.Add(new State(SpriteNum.SPR_PINS, 32770, 6, null, StateNum.S_PINS4, 0, 0)); // S_PINS3
        _states.Add(new State(SpriteNum.SPR_PINS, 32771, 6, null, StateNum.S_PINS, 0, 0));  // S_PINS4
        _states.Add(new State(SpriteNum.SPR_MEGA, 32768, 6, null, StateNum.S_MEGA2, 0, 0)); // S_MEGA
        _states.Add(new State(SpriteNum.SPR_MEGA, 32769, 6, null, StateNum.S_MEGA3, 0, 0)); // S_MEGA2
        _states.Add(new State(SpriteNum.SPR_MEGA, 32770, 6, null, StateNum.S_MEGA4, 0, 0)); // S_MEGA3
        _states.Add(new State(SpriteNum.SPR_MEGA, 32771, 6, null, StateNum.S_MEGA, 0, 0));  // S_MEGA4
        _states.Add(new State(SpriteNum.SPR_SUIT, 32768, -1, null, StateNum.S_NULL, 0, 0)); // S_SUIT
        _states.Add(new State(SpriteNum.SPR_PMAP, 32768, 6, null, StateNum.S_PMAP2, 0, 0)); // S_PMAP
        _states.Add(new State(SpriteNum.SPR_PMAP, 32769, 6, null, StateNum.S_PMAP3, 0, 0)); // S_PMAP2
        _states.Add(new State(SpriteNum.SPR_PMAP, 32770, 6, null, StateNum.S_PMAP4, 0, 0)); // S_PMAP3
        _states.Add(new State(SpriteNum.SPR_PMAP, 32771, 6, null, StateNum.S_PMAP5, 0, 0)); // S_PMAP4
        _states.Add(new State(SpriteNum.SPR_PMAP, 32770, 6, null, StateNum.S_PMAP6, 0, 0)); // S_PMAP5
        _states.Add(new State(SpriteNum.SPR_PMAP, 32769, 6, null, StateNum.S_PMAP, 0, 0));  // S_PMAP6
        _states.Add(new State(SpriteNum.SPR_PVIS, 32768, 6, null, StateNum.S_PVIS2, 0, 0)); // S_PVIS
        _states.Add(new State(SpriteNum.SPR_PVIS, 1, 6, null, StateNum.S_PVIS, 0, 0));  // S_PVIS2
        _states.Add(new State(SpriteNum.SPR_CLIP, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_CLIP
        _states.Add(new State(SpriteNum.SPR_AMMO, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_AMMO
        _states.Add(new State(SpriteNum.SPR_ROCK, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_ROCK
        _states.Add(new State(SpriteNum.SPR_BROK, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_BROK
        _states.Add(new State(SpriteNum.SPR_CELL, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_CELL
        _states.Add(new State(SpriteNum.SPR_CELP, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_CELP
        _states.Add(new State(SpriteNum.SPR_SHEL, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_SHEL
        _states.Add(new State(SpriteNum.SPR_SBOX, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_SBOX
        _states.Add(new State(SpriteNum.SPR_BPAK, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_BPAK
        _states.Add(new State(SpriteNum.SPR_BFUG, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_BFUG
        _states.Add(new State(SpriteNum.SPR_MGUN, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_MGUN
        _states.Add(new State(SpriteNum.SPR_CSAW, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_CSAW
        _states.Add(new State(SpriteNum.SPR_LAUN, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_LAUN
        _states.Add(new State(SpriteNum.SPR_PLAS, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_PLAS
        _states.Add(new State(SpriteNum.SPR_SHOT, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_SHOT
        _states.Add(new State(SpriteNum.SPR_SGN2, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_SHOT2
        _states.Add(new State(SpriteNum.SPR_COLU, 32768, -1, null, StateNum.S_NULL, 0, 0)); // S_COLU
        _states.Add(new State(SpriteNum.SPR_SMT2, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_STALAG
        _states.Add(new State(SpriteNum.SPR_GOR1, 0, 10, null, StateNum.S_BLOODYTWITCH2, 0, 0));    // S_BLOODYTWITCH
        _states.Add(new State(SpriteNum.SPR_GOR1, 1, 15, null, StateNum.S_BLOODYTWITCH3, 0, 0));    // S_BLOODYTWITCH2
        _states.Add(new State(SpriteNum.SPR_GOR1, 2, 8, null, StateNum.S_BLOODYTWITCH4, 0, 0)); // S_BLOODYTWITCH3
        _states.Add(new State(SpriteNum.SPR_GOR1, 1, 6, null, StateNum.S_BLOODYTWITCH, 0, 0));  // S_BLOODYTWITCH4
        _states.Add(new State(SpriteNum.SPR_PLAY, 13, -1, null, StateNum.S_NULL, 0, 0));    // S_DEADTORSO
        _states.Add(new State(SpriteNum.SPR_PLAY, 18, -1, null, StateNum.S_NULL, 0, 0));    // S_DEADBOTTOM
        _states.Add(new State(SpriteNum.SPR_POL2, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_HEADSONSTICK
        _states.Add(new State(SpriteNum.SPR_POL5, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_GIBS
        _states.Add(new State(SpriteNum.SPR_POL4, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_HEADONASTICK
        _states.Add(new State(SpriteNum.SPR_POL3, 32768, 6, null, StateNum.S_HEADCANDLES2, 0, 0));  // S_HEADCANDLES
        _states.Add(new State(SpriteNum.SPR_POL3, 32769, 6, null, StateNum.S_HEADCANDLES, 0, 0));   // S_HEADCANDLES2
        _states.Add(new State(SpriteNum.SPR_POL1, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_DEADSTICK
        _states.Add(new State(SpriteNum.SPR_POL6, 0, 6, null, StateNum.S_LIVESTICK2, 0, 0));    // S_LIVESTICK
        _states.Add(new State(SpriteNum.SPR_POL6, 1, 8, null, StateNum.S_LIVESTICK, 0, 0)); // S_LIVESTICK2
        _states.Add(new State(SpriteNum.SPR_GOR2, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_MEAT2
        _states.Add(new State(SpriteNum.SPR_GOR3, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_MEAT3
        _states.Add(new State(SpriteNum.SPR_GOR4, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_MEAT4
        _states.Add(new State(SpriteNum.SPR_GOR5, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_MEAT5
        _states.Add(new State(SpriteNum.SPR_SMIT, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_STALAGTITE
        _states.Add(new State(SpriteNum.SPR_COL1, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_TALLGRNCOL
        _states.Add(new State(SpriteNum.SPR_COL2, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_SHRTGRNCOL
        _states.Add(new State(SpriteNum.SPR_COL3, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_TALLREDCOL
        _states.Add(new State(SpriteNum.SPR_COL4, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_SHRTREDCOL
        _states.Add(new State(SpriteNum.SPR_CAND, 32768, -1, null, StateNum.S_NULL, 0, 0)); // S_CANDLESTIK
        _states.Add(new State(SpriteNum.SPR_CBRA, 32768, -1, null, StateNum.S_NULL, 0, 0)); // S_CANDELABRA
        _states.Add(new State(SpriteNum.SPR_COL6, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_SKULLCOL
        _states.Add(new State(SpriteNum.SPR_TRE1, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_TORCHTREE
        _states.Add(new State(SpriteNum.SPR_TRE2, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_BIGTREE
        _states.Add(new State(SpriteNum.SPR_ELEC, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_TECHPILLAR
        _states.Add(new State(SpriteNum.SPR_CEYE, 32768, 6, null, StateNum.S_EVILEYE2, 0, 0));  // S_EVILEYE
        _states.Add(new State(SpriteNum.SPR_CEYE, 32769, 6, null, StateNum.S_EVILEYE3, 0, 0));  // S_EVILEYE2
        _states.Add(new State(SpriteNum.SPR_CEYE, 32770, 6, null, StateNum.S_EVILEYE4, 0, 0));  // S_EVILEYE3
        _states.Add(new State(SpriteNum.SPR_CEYE, 32769, 6, null, StateNum.S_EVILEYE, 0, 0));   // S_EVILEYE4
        _states.Add(new State(SpriteNum.SPR_FSKU, 32768, 6, null, StateNum.S_FLOATSKULL2, 0, 0));   // S_FLOATSKULL
        _states.Add(new State(SpriteNum.SPR_FSKU, 32769, 6, null, StateNum.S_FLOATSKULL3, 0, 0));   // S_FLOATSKULL2
        _states.Add(new State(SpriteNum.SPR_FSKU, 32770, 6, null, StateNum.S_FLOATSKULL, 0, 0));    // S_FLOATSKULL3
        _states.Add(new State(SpriteNum.SPR_COL5, 0, 14, null, StateNum.S_HEARTCOL2, 0, 0));    // S_HEARTCOL
        _states.Add(new State(SpriteNum.SPR_COL5, 1, 14, null, StateNum.S_HEARTCOL, 0, 0)); // S_HEARTCOL2
        _states.Add(new State(SpriteNum.SPR_TBLU, 32768, 4, null, StateNum.S_BLUETORCH2, 0, 0));    // S_BLUETORCH
        _states.Add(new State(SpriteNum.SPR_TBLU, 32769, 4, null, StateNum.S_BLUETORCH3, 0, 0));    // S_BLUETORCH2
        _states.Add(new State(SpriteNum.SPR_TBLU, 32770, 4, null, StateNum.S_BLUETORCH4, 0, 0));    // S_BLUETORCH3
        _states.Add(new State(SpriteNum.SPR_TBLU, 32771, 4, null, StateNum.S_BLUETORCH, 0, 0)); // S_BLUETORCH4
        _states.Add(new State(SpriteNum.SPR_TGRN, 32768, 4, null, StateNum.S_GREENTORCH2, 0, 0));   // S_GREENTORCH
        _states.Add(new State(SpriteNum.SPR_TGRN, 32769, 4, null, StateNum.S_GREENTORCH3, 0, 0));   // S_GREENTORCH2
        _states.Add(new State(SpriteNum.SPR_TGRN, 32770, 4, null, StateNum.S_GREENTORCH4, 0, 0));   // S_GREENTORCH3
        _states.Add(new State(SpriteNum.SPR_TGRN, 32771, 4, null, StateNum.S_GREENTORCH, 0, 0));    // S_GREENTORCH4
        _states.Add(new State(SpriteNum.SPR_TRED, 32768, 4, null, StateNum.S_REDTORCH2, 0, 0)); // S_REDTORCH
        _states.Add(new State(SpriteNum.SPR_TRED, 32769, 4, null, StateNum.S_REDTORCH3, 0, 0)); // S_REDTORCH2
        _states.Add(new State(SpriteNum.SPR_TRED, 32770, 4, null, StateNum.S_REDTORCH4, 0, 0)); // S_REDTORCH3
        _states.Add(new State(SpriteNum.SPR_TRED, 32771, 4, null, StateNum.S_REDTORCH, 0, 0));  // S_REDTORCH4
        _states.Add(new State(SpriteNum.SPR_SMBT, 32768, 4, null, StateNum.S_BTORCHSHRT2, 0, 0));   // S_BTORCHSHRT
        _states.Add(new State(SpriteNum.SPR_SMBT, 32769, 4, null, StateNum.S_BTORCHSHRT3, 0, 0));   // S_BTORCHSHRT2
        _states.Add(new State(SpriteNum.SPR_SMBT, 32770, 4, null, StateNum.S_BTORCHSHRT4, 0, 0));   // S_BTORCHSHRT3
        _states.Add(new State(SpriteNum.SPR_SMBT, 32771, 4, null, StateNum.S_BTORCHSHRT, 0, 0));    // S_BTORCHSHRT4
        _states.Add(new State(SpriteNum.SPR_SMGT, 32768, 4, null, StateNum.S_GTORCHSHRT2, 0, 0));   // S_GTORCHSHRT
        _states.Add(new State(SpriteNum.SPR_SMGT, 32769, 4, null, StateNum.S_GTORCHSHRT3, 0, 0));   // S_GTORCHSHRT2
        _states.Add(new State(SpriteNum.SPR_SMGT, 32770, 4, null, StateNum.S_GTORCHSHRT4, 0, 0));   // S_GTORCHSHRT3
        _states.Add(new State(SpriteNum.SPR_SMGT, 32771, 4, null, StateNum.S_GTORCHSHRT, 0, 0));    // S_GTORCHSHRT4
        _states.Add(new State(SpriteNum.SPR_SMRT, 32768, 4, null, StateNum.S_RTORCHSHRT2, 0, 0));   // S_RTORCHSHRT
        _states.Add(new State(SpriteNum.SPR_SMRT, 32769, 4, null, StateNum.S_RTORCHSHRT3, 0, 0));   // S_RTORCHSHRT2
        _states.Add(new State(SpriteNum.SPR_SMRT, 32770, 4, null, StateNum.S_RTORCHSHRT4, 0, 0));   // S_RTORCHSHRT3
        _states.Add(new State(SpriteNum.SPR_SMRT, 32771, 4, null, StateNum.S_RTORCHSHRT, 0, 0));    // S_RTORCHSHRT4
        _states.Add(new State(SpriteNum.SPR_HDB1, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_HANGNOGUTS
        _states.Add(new State(SpriteNum.SPR_HDB2, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_HANGBNOBRAIN
        _states.Add(new State(SpriteNum.SPR_HDB3, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_HANGTLOOKDN
        _states.Add(new State(SpriteNum.SPR_HDB4, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_HANGTSKULL
        _states.Add(new State(SpriteNum.SPR_HDB5, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_HANGTLOOKUP
        _states.Add(new State(SpriteNum.SPR_HDB6, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_HANGTNOBRAIN
        _states.Add(new State(SpriteNum.SPR_POB1, 0, -1, null, StateNum.S_NULL, 0, 0)); // S_COLONGIBS
        _states.Add(new State(SpriteNum.SPR_POB2, 0, -1, null, StateNum.S_NULL, 0, 0));  // S_SMALLPOOL
        _states.Add(new State(SpriteNum.SPR_BRS1, 0, -1, null, StateNum.S_NULL, 0, 0));     // S_BRAINSTEM
        _states.Add(new State(SpriteNum.SPR_TLMP, 32768, 4, null, StateNum.S_TECHLAMP2, 0, 0)); // S_TECHLAMP
        _states.Add(new State(SpriteNum.SPR_TLMP, 32769, 4, null, StateNum.S_TECHLAMP3, 0, 0)); // S_TECHLAMP2
        _states.Add(new State(SpriteNum.SPR_TLMP, 32770, 4, null, StateNum.S_TECHLAMP4, 0, 0)); // S_TECHLAMP3
        _states.Add(new State(SpriteNum.SPR_TLMP, 32771, 4, null, StateNum.S_TECHLAMP, 0, 0));  // S_TECHLAMP4
        _states.Add(new State(SpriteNum.SPR_TLP2, 32768, 4, null, StateNum.S_TECH2LAMP2, 0, 0));    // S_TECH2LAMP
        _states.Add(new State(SpriteNum.SPR_TLP2, 32769, 4, null, StateNum.S_TECH2LAMP3, 0, 0));    // S_TECH2LAMP2
        _states.Add(new State(SpriteNum.SPR_TLP2, 32770, 4, null, StateNum.S_TECH2LAMP4, 0, 0)); // S_TECH2LAMP3
        _states.Add(new State(SpriteNum.SPR_TLP2, 32771, 4, null, StateNum.S_TECH2LAMP, 0, 0));	// S_TECH2LAMP4
    }
}