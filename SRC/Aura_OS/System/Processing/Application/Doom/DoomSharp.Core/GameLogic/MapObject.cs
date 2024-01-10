using DoomSharp.Core.Data;
using DoomSharp.Core.Graphics;

namespace DoomSharp.Core.GameLogic;

//
// NOTES: mobj_t
//
// mobj_ts are used to tell the refresh where to draw an image,
// tell the world simulation when objects are contacted,
// and tell the sound driver how to position a sound.
//
// The refresh uses the next and prev links to follow
// lists of things in sectors as they are being drawn.
// The sprite, frame, and angle elements determine which patch_t
// is used to draw the sprite if it is visible.
// The sprite and frame values are allmost allways set
// from state_t structures.
// The statescr.exe utility generates the states.h and states.c
// files that contain the sprite/frame numbers from the
// statescr.txt source file.
// The xyz origin point represents a point at the bottom middle
// of the sprite (between the feet of a biped).
// This is the default origin position for patch_ts grabbed
// with lumpy.exe.
// A walking creature will have its z equal to the floor
// it is standing on.
//
// The sound code uses the x,y, and subsector fields
// to do stereo positioning of any sound effited by the mobj_t.
//
// The play simulation uses the blocklinks, x,y,z, radius, height
// to determine when mobj_ts are touching each other,
// touching lines in the map, or hit by trace lines (gunshots,
// lines of sight, etc).
// The mobj_t->flags element has various bit flags
// used by the simulation.
//
// Every mobj_t is linked into a single sector
// based on its origin coordinates.
// The subsector_t is found with R_PointInSubsector(x,y),
// and the sector_t can be found with subsector->sector.
// The sector links are only used by the rendering code,
// the play simulation does not care about them at all.
//
// Any mobj_t that needs to be acted upon by something else
// in the play world (block movement, be shot, etc) will also
// need to be linked into the blockmap.
// If the thing has the MF_NOBLOCK flag set, it will not use
// the block links. It can still interact with other things,
// but only as the instigator (missiles will run into other
// things, but nothing can run into a missile).
// Each block in the grid is 128*128 units, and knows about
// every line_t that it contains a piece of, and every
// interactable mobj_t that has its origin contained.  
//
// A valid mobj_t is a mobj_t that has the proper subsector_t
// filled in for its xy coordinates and is linked into the
// sector from which the subsector was made, or has the
// MF_NOSECTOR flag set (the subsector_t needs to be valid
// even if MF_NOSECTOR is set), and is linked into a blockmap
// block or has the MF_NOBLOCKMAP flag set.
// Links should only be modified by the P_[Un]SetThingPosition()
// functions.
// Do not change the MF_NO? flags while a thing is valid.
//
// Any questions?
//

public class MapObject : Thinker
{
    public MapObject()
    {

    }

    public MapObject(MapObjectInfo info)
    {
        Type = info.Type;
        Info = info;
        Radius = new Fixed(info.Radius);
        Height = new Fixed(info.Height);
        Flags = info.Flags;
        Health = info.SpawnHealth;
    }

    // Info for drawing: position.
    public Fixed X { get; set; } = Fixed.Zero;
    public Fixed Y { get; set; } = Fixed.Zero;
    public Fixed Z { get; set; } = Fixed.Zero;

    // More list: links in sector (if needed)
    public MapObject? SectorNext { get; set; }
    public MapObject? SectorPrev { get; set; }

    // More drawing info: to determine current sprite.
    public Angle Angle { get; set; } // orientation
    public SpriteNum Sprite { get; set; } // used to find patch_t and flip value
    public int Frame { get; set; } // might be ORed with FF_FULLBRIGHT

    // Interaction info, by BLOCKMAP.
    // Links in blocks (if needed).
    public MapObject? BlockNext { get; set; }
    public MapObject? BlockPrev { get; set; }

    public SubSector? SubSector { get; set; }

    // The closest interval over all contacted Sectors.
    public Fixed FloorZ { get; set; } = Fixed.Zero;
    public Fixed CeilingZ { get; set; } = Fixed.Zero;

    // For movement checking.
    public Fixed Radius { get; set; } = Fixed.Zero;
    public Fixed Height { get; set; } = Fixed.Zero;

    // Momentums, used to update position.
    public Fixed MomX { get; set; } = Fixed.Zero;
    public Fixed MomY { get; set; } = Fixed.Zero;
    public Fixed MomZ { get; set; } = Fixed.Zero;

    // If == validcount, already checked.
    public int ValidCount { get; set; }

    public MapObjectType Type { get; set; }
    public MapObjectInfo Info { get; set; }   // &mobjinfo[mobj->type]

    public int Tics { get; set; }	// state tic counter
    public State? State { get; set; }
    public MapObjectFlag Flags { get; set; }
    public int Health { get; set; }

    // Movement direction, movement generation (zig-zagging).
    public DirectionType MoveDir { get; set; }	// 0-7
    public int MoveCount { get; set; }	// when 0, select a new dir

    // Thing being chased/attacked (or NULL),
    // also the originator for missiles.
    public MapObject? Target { get; set; }

    // Reaction time: if non 0, don't attack yet.
    // Used by player to freeze a bit after teleporting.
    public int ReactionTime { get; set; }

    // If >0, the target will be chased
    // no matter what (even if shot)
    public int Threshold { get; set; }

    // Additional info record for player avatars only.
    // Only valid if type == MT_PLAYER
    public Player? Player { get; set; }

    // Player number last looked for.
    public int LastLook { get; set; }

    // For nightmare respawn.
    public MapThing Spawnpoint { get; set; }

    // Thing being chased/attacked for tracers.
    public MapObject? Tracer { get; set; }

    /// <summary>
    /// Returns true if the mobj is still present
    /// </summary>
    public bool SetState(StateNum state)
    {
        do
        {
            if (state == StateNum.S_NULL)
            {
                State = null;
                // TODO Try to move this method to this class
                DoomGame.Instance.Game.P_RemoveMapObject(this);
                return false;
            }

            var st = State.GetSpawnState(state);
            State = st;
            Tics = st.Tics;
            Sprite = st.Sprite;
            Frame = st.Frame;

            // Modified handling.
            // Call action functions when the state is set
            st.Action?.Invoke(new ActionParams(this));

            state = st.NextState;
        } while (Tics == 0);

        return true;
    }

    /// <summary>
    /// Damages both enemies and players
    /// "inflictor" is the thing that caused the damage
    ///  creature or missile, can be NULL (slime, etc)
    /// "source" is the thing to target after taking damage
    ///  creature or NULL
    /// Source and inflictor are the same for melee attacks.
    /// Source can be NULL for slime, barrel explosions
    /// and other environmental stuff.
    /// </summary>
    public static void DamageMapObject(MapObject target, MapObject? inflictor, MapObject? source, int damage)
    {
        if ((target.Flags & MapObjectFlag.MF_SHOOTABLE) == 0)
        {
            return; // shouldn't happen...
        }

        if (target.Health <= 0)
        {
            return;
        }

        if ((target.Flags & MapObjectFlag.MF_SKULLFLY) != 0)
        {
            target.MomX = target.MomY = target.MomZ = Fixed.Zero;
        }

        var player = target.Player;
        if (player != null && DoomGame.Instance.Game.GameSkill == SkillLevel.Baby)
        {
            damage >>= 1;   // take half damage in trainer mode
        }

        // Some close combat weapons should not
        // inflict thrust and push the victim out of reach,
        // thus kick away unless using the chainsaw.
        if (inflictor != null &&
            (target.Flags & MapObjectFlag.MF_NOCLIP) == 0 &&
            (source?.Player == null || source.Player.ReadyWeapon != WeaponType.Chainsaw))
        {
            var ang = DoomGame.Instance.Renderer.PointToAngle2(inflictor.X, inflictor.Y, target.X, target.Y);
            var thrust = new Fixed(damage * (Constants.FracUnit >> 3) * 100 / target.Info.Mass);

            // make fall forwards sometimes
            if (damage < 40 &&
                damage > target.Health &&
                target.Z - inflictor.Z > Fixed.FromInt(64) &&
                (DoomRandom.P_Random() & 1) != 0)
            {
                ang += Angle.Angle180;
                thrust *= 4;
            }

            target.MomX += (thrust * DoomMath.Cos(ang));
            target.MomY += (thrust * DoomMath.Sin(ang));
        }

        // player specific
        if (player != null)
        {
            // end of game hell hack
            if (target.SubSector?.Sector != null && target.SubSector.Sector.Special == 11 && damage >= target.Health)
            {
                damage = target.Health - 1;
            }

            // Below certain threshold,
            // ignore damage in GOD mode, or with INVUL power.
            if (damage < 1000 &&
                ((player.Cheats & Cheat.GodMode) != 0 ||
                 player.Powers[(int)PowerUpType.Invulnerability] != 0))
            {
                return;
            }

            var saved = 0;
            if (player.ArmorType != 0)
            {
                if (player.ArmorType == 1)
                {
                    saved = damage / 3;
                }
                else
                {
                    saved = damage / 2;
                }

                if (player.ArmorPoints <= saved)
                {
                    // armor is used up
                    saved = player.ArmorPoints;
                    player.ArmorType = 0;
                }
                player.ArmorPoints -= saved;
                damage -= saved;
            }

            player.Health -= damage;   // mirror mobj health here for Dave
            if (player.Health < 0)
            {
                player.Health = 0;
            }

            player.Attacker = source;
            player.DamageCount += damage;  // add damage after armor / invuln

            if (player.DamageCount > 100)
            {
                player.DamageCount = 100;  // teleport stomp does 10k points...
            }

            var temp = damage < 100 ? damage : 100;

            //if (player == DoomGame.Instance.Game.Players[DoomGame.Instance.Game.ConsolePlayer])
            //{
            //    I_Tactile(40, 10, 40 + temp * 2); // unused
            //}
        }

        // do the damage	
        target.Health -= damage;
        if (target.Health <= 0)
        {
            KillMapObject(source, target);
            return;
        }

        if (DoomRandom.P_Random() < target.Info.PainChance &&
            (target.Flags & MapObjectFlag.MF_SKULLFLY) == 0)
        {
            target.Flags |= MapObjectFlag.MF_JUSTHIT;    // fight back!
            target.SetState(target.Info.PainState);
        }

        target.ReactionTime = 0;       // we're awake now...	

        if ((target.Threshold == 0 || target.Type == MapObjectType.MT_VILE) &&
            source != null &&
            source != target &&
            source.Type != MapObjectType.MT_VILE)
        {
            // if not intent on another player,
            // chase after this one
            target.Target = source;
            target.Threshold = Constants.BaseThreshold;
            if (target.State == State.GetSpawnState(target.Info.SpawnState) &&
                target.Info.SeeState != StateNum.S_NULL)
            {
                target.SetState(target.Info.SeeState);
            }
        }
    }

    private static void KillMapObject(MapObject? source, MapObject target)
    {
        target.Flags &= ~(MapObjectFlag.MF_SHOOTABLE | MapObjectFlag.MF_FLOAT | MapObjectFlag.MF_SKULLFLY);

        if (target.Type != MapObjectType.MT_SKULL)
        {
            target.Flags &= ~MapObjectFlag.MF_NOGRAVITY;
        }

        target.Flags |= MapObjectFlag.MF_CORPSE | MapObjectFlag.MF_DROPOFF;
        target.Height >>= 2;

        if (source?.Player != null)
        {
            // count for intermission
            if ((target.Flags & MapObjectFlag.MF_COUNTKILL) != 0)
            {
                source.Player.KillCount++;
            }

            if (target.Player != null)
            {
                var playerIndex = DoomGame.Instance.Game.GetPlayerIndex(target.Player);
                if (playerIndex != -1)
                {
                    source.Player.Frags[playerIndex]++;
                }
            }
        }
        else if (!DoomGame.Instance.Game.NetGame &&
                 (target.Flags & MapObjectFlag.MF_COUNTKILL) != 0)
        {
            // count all monster deaths,
            // even those caused by other monsters
            DoomGame.Instance.Game.Players[0].KillCount++;
        }

        if (target.Player != null)
        {
            // count environment kills against you
            if (source != null)
            {
                var playerIndex = DoomGame.Instance.Game.GetPlayerIndex(target.Player);
                if (playerIndex != -1)
                {
                    target.Player.Frags[playerIndex]++;
                }
            }

            target.Flags &= ~MapObjectFlag.MF_SOLID;
            target.Player.PlayerState = PlayerState.Dead;
            DropWeapon(target.Player);

            if (target.Player == DoomGame.Instance.Game.Players[DoomGame.Instance.Game.ConsolePlayer] &&
                DoomGame.Instance.AutoMapActive)
            {
                // don't die in auto map,
                // switch view prior to dying
                // TODO AM_Stop();
            }
        }

        if (target.Health < -target.Info.SpawnHealth && target.Info.XDeathState != StateNum.S_NULL)
        {
            target.SetState(target.Info.XDeathState);
        }
        else
        {
            target.SetState(target.Info.DeathState);
        }

        target.Tics -= DoomRandom.P_Random() & 3;

        if (target.Tics < 1)
        {
            target.Tics = 1;
        }

        //	I_StartSound (&actor->r, actor->info->deathsound);

        // Drop stuff.
        // This determines the kind of object spawned
        // during the death frame of a thing.
        MapObjectType item;
        switch (target.Type)
        {
            case MapObjectType.MT_WOLFSS:
            case MapObjectType.MT_POSSESSED:
                item = MapObjectType.MT_CLIP;
                break;

            case MapObjectType.MT_SHOTGUY:
                item = MapObjectType.MT_SHOTGUN;
                break;

            case MapObjectType.MT_CHAINGUY:
                item = MapObjectType.MT_CHAINGUN;
                break;

            default:
                return;
        }

        var mo = DoomGame.Instance.Game.P_SpawnMapObject(target.X, target.Y, Constants.OnFloorZ, item);
        mo.Flags |= MapObjectFlag.MF_DROPPED; // special versions of items
    }

    private static void DropWeapon(Player player)
    {
        DoomGame.Instance.Game.P_SetPlayerSprite(
            player,
            PlayerSpriteType.Weapon,
            WeaponInfo.GetByType(player.ReadyWeapon).DownState);
    }
}