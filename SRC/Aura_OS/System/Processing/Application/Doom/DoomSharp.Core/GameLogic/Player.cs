using DoomSharp.Core.Networking;
using DoomSharp.Core.Data;
using DoomSharp.Core.Input;

namespace DoomSharp.Core.GameLogic;

public class Player
{
    // player radius for movement checking
    public static readonly Fixed PlayerRadius = Fixed.FromInt(16);

    public Player()
    {
        for (var i = 0; i < PlayerSprites.Length; i++)
        {
            PlayerSprites[i] = new PlayerSprite();
        }
    }

    public PlayerState PlayerState { get; set; } = PlayerState.NotSet;
    public TicCommand Command { get; set; } = new();

    public MapObject? MapObject { get; set; }

    // Determine POV,
    //  including viewpoint bobbing during movement.
    // Focal origin above r.z
    public Fixed ViewZ { get; set; }
    // Base height above floor for viewz.
    public Fixed ViewHeight { get; set; }
    // Bob/squat speed.
    public Fixed DeltaViewHeight { get; set; }
    public bool OnGround { get; set; }
    // bounded/scaled total momentum.
    public Fixed Bob { get; set; }

    // This is only used between levels,
    // mo->health is used during levels.
    public int Health { get; set; }
    public int ArmorPoints { get; set; }
    // Armor type is 0-2.
    public int ArmorType { get; set; }

    // Power ups. invinc and invis are tic counters.
    public int[] Powers { get; } = new int[(int)PowerUpType.NumberOfPowerUps];
    public bool[] Cards { get; } = new bool[(int)KeyCardType.NumberOfKeyCards];
    public bool Backpack { get; set; }

    // Frags, kills of other players.
    public int[] Frags { get; set; } = new int[Constants.MaxPlayers];
    public WeaponType ReadyWeapon { get; set; }

    // Is wp_nochange if not changing.
    public WeaponType PendingWeapon { get; set; }

    public bool[] WeaponOwned { get; } = new bool[(int)WeaponType.NumberOfWeapons];
    public int[] Ammo { get; } = new int[(int)AmmoType.NumAmmo];
    public int[] MaxAmmo { get; } = new int[(int)AmmoType.NumAmmo];

    // True if button down last tic.
    public bool AttackDown { get; set; }
    public bool UseDown { get; set; }

    // Bit flags, for cheats and debug.
    // See cheat_t, above.
    public Cheat Cheats { get; set; }

    // Refired shots are less accurate.
    public int Refire { get; set; }

    // For intermission stats.
    public int KillCount { get; set; }
    public int ItemCount { get; set; }
    public int SecretCount { get; set; }

    // Hint messages.
    public string? Message { get; set; }

    // For screen flashing (red or bright).
    public int DamageCount { get; set; }
    public int BonusCount { get; set; }

    // Who did damage (NULL for floors/ceilings).
    public MapObject? Attacker { get; set; }

    // So gun flashes light up areas.
    public int ExtraLight { get; set; }

    // Current PLAYPAL, ???
    //  can be set to REDCOLORMAP for pain, etc.
    public int FixedColorMap { get; set; }

    // Player skin colorshift,
    //  0-3 for which color to draw player.
    public int ColorMap { get; set; }

    // Overlay view sprites (gun, etc).
    public PlayerSprite[] PlayerSprites { get; } = new PlayerSprite[(int)PlayerSpriteType.NumPlayerSprites];

    // True if secret level has been done.
    public bool DidSecret { get; set; }

    public void Think()
    {
        // fixme: do this in the cheat code
        if ((Cheats & Cheat.NoClip) != 0)
        {
            MapObject!.Flags |= MapObjectFlag.MF_NOCLIP;
        }
        else
        {
            MapObject!.Flags &= ~MapObjectFlag.MF_NOCLIP;
        }

        // chain saw run forward
        if ((MapObject!.Flags & MapObjectFlag.MF_JUSTATTACKED) != 0)
        {
            Command.AngleTurn = 0;
            Command.ForwardMove = 0xc800 / 512;
            Command.SideMove = 0;
            MapObject.Flags &= ~MapObjectFlag.MF_JUSTATTACKED;
        }

        if (PlayerState == PlayerState.Dead)
        {
            DeathThink();
            return;
        }

        // Move around.
        // Reactiontime is used to prevent movement
        //  for a bit after a teleport.
        if (MapObject.ReactionTime != 0)
        {
            MapObject.ReactionTime--;
        }
        else
        {
            Move();
        }

        CalcHeight();

        if (MapObject?.SubSector?.Sector != null && MapObject.SubSector.Sector.Special != 0)
        {
            PlayerInSpecialSector();
        }

        // Check for weapon change.

        // A special event has no other buttons.
        if ((Command.Buttons & ButtonCode.Special) != 0)
        {
            Command.Buttons = 0;
        }

        if ((Command.Buttons & ButtonCode.Change) != 0)
        {
            // The actual changing of the weapon is done
            //  when the weapon psprite can do it
            //  (read: not in the middle of an attack).
            var newWeapon = (WeaponType)((int)(Command.Buttons & ButtonCode.WeaponMask) >> (int)ButtonCode.WeaponShift);

            if (newWeapon == (int)WeaponType.Fist && 
                WeaponOwned[(int)WeaponType.Chainsaw] &&
                !(ReadyWeapon == WeaponType.Chainsaw &&
                  Powers[(int)PowerUpType.Strength] != 0)
            )
            {
                newWeapon = WeaponType.Chainsaw;
            }

            if (DoomGame.Instance.GameMode == GameMode.Commercial && 
                newWeapon == WeaponType.Shotgun && 
                WeaponOwned[(int)WeaponType.SuperShotgun] &&
                ReadyWeapon != WeaponType.SuperShotgun)
            {
                newWeapon = WeaponType.SuperShotgun;
            }
            
            if (WeaponOwned[(int)newWeapon] && newWeapon != ReadyWeapon)
            {
                // Do not go to plasma or BFG in shareware,
                //  even if cheated.
                if ((newWeapon != WeaponType.Plasma && newWeapon != WeaponType.Bfg) || 
                    DoomGame.Instance.GameMode != GameMode.Shareware)
                {
                    PendingWeapon = newWeapon;
                }
            }
        }

        // check for use
        if ((Command.Buttons & ButtonCode.Use) != 0)
        {
            if (!UseDown)
            {
                UseLines();
                UseDown = true;
            }
        }
        else
        {
            UseDown = false;
        }

        // cycle psprites
        DoomGame.Instance.Game.P_MovePlayerSprites(this);

        // Counters, time dependend power ups.

        // Strength counts up to diminish fade.
        if (Powers[(int)PowerUpType.Strength] > 0)
        {
            Powers[(int)PowerUpType.Strength]++;
        }

        if (Powers[(int)PowerUpType.Invulnerability] > 0)
        {
            Powers[(int)PowerUpType.Invulnerability]--;
        }

        if (Powers[(int)PowerUpType.Invisibility] != 0)
        {
            if (--Powers[(int)PowerUpType.Invisibility] == 0)
            {
                MapObject!.Flags &= ~MapObjectFlag.MF_SHADOW;
            }
        }

        if (Powers[(int)PowerUpType.InfraRed] > 0)
        {
            Powers[(int)PowerUpType.InfraRed]--;
        }

        if (Powers[(int)PowerUpType.IronFeet] > 0)
        {
            Powers[(int)PowerUpType.IronFeet]--;
        }

        if (DamageCount > 0)
        {
            DamageCount--;
        }

        if (BonusCount > 0)
        {
            BonusCount--;
        }

        // Handling colormaps.
        if (Powers[(int)PowerUpType.Invulnerability] != 0)
        {
            if (Powers[(int)PowerUpType.Invulnerability] > 4 * 32
                || (Powers[(int)PowerUpType.Invulnerability] & 8) != 0)
            {
                FixedColorMap = 32; //INVERSECOLORMAP;
            }
            else
            {
                FixedColorMap = 0;
            }
        }
        else if (Powers[(int)PowerUpType.InfraRed] != 0)
        {
            if (Powers[(int)PowerUpType.InfraRed] > 4 * 32
                || (Powers[(int)PowerUpType.InfraRed] & 8) != 0)
            {
                // almost full bright
                FixedColorMap = 1;
            }
            else
            {
                FixedColorMap = 0;
            }
        }
        else
        {
            FixedColorMap = 0;
        }
    }

    /// <summary>
    /// Looks for special lines in front of the player to activate.
    /// </summary>
    private void UseLines()
    {
        var game = DoomGame.Instance.Game;
        game.UseThing = MapObject;

        var angle = MapObject!.Angle;
        var x1 = MapObject.X;
        var y1 = MapObject.Y;
        var x2 = x1 + (Constants.UseRange.Value >> Constants.FracBits) * DoomMath.Cos(angle);
        var y2 = y1 + (Constants.UseRange.Value >> Constants.FracBits) * DoomMath.Sin(angle);

        game.P_PathTraverse(x1, y1, x2, y2, GameController.PT_AddLines, game.PTR_UseTraverse);
    }

    private static readonly Angle Angle5 = new(Angle.Angle90.Value / 18);

    /// <summary>
    /// Fall on your face when dying. Decrease POV height to floor height.
    /// </summary>
    public void DeathThink()
    {
        DoomGame.Instance.Game.P_MovePlayerSprites(this);

        // fall to the ground
        if (ViewHeight > Fixed.FromInt(6))
        {
            ViewHeight -= Fixed.Unit;
        }

        if (ViewHeight < Fixed.FromInt(6))
        {
            ViewHeight = Fixed.FromInt(6);
        }

        DeltaViewHeight = Fixed.Zero;
        OnGround = (MapObject!.Z <= MapObject.FloorZ);
        CalcHeight();

        if (Attacker != null && Attacker != MapObject)
        {
            var angle = DoomGame.Instance.Renderer.PointToAngle2(MapObject.X, MapObject.Y, Attacker.X, Attacker.Y);
            var delta = angle - MapObject.Angle;

            var inverseAngle5 = Angle5;
            if (delta < Angle5 || delta > -inverseAngle5)
            {
                // Looking at killer,
                //  so fade damage flash down.
                MapObject.Angle = angle;

                if (DamageCount != 0)
                {
                    DamageCount--;
                }
            }
            else if (delta < Angle.Angle180)
            {
                MapObject.Angle += Angle5;
            }
            else
            {
                MapObject.Angle -= Angle5;
            }
        }
        else if (DamageCount != 0)
        {
            DamageCount--;
        }

        if ((Command.Buttons & ButtonCode.Use) != 0)
        {
            PlayerState = PlayerState.Reborn;
        }
    }

    private static readonly Fixed MaxBob = new(0x100000);

    /// <summary>
    /// Calculate the walking / running height adjustment
    /// </summary>
    public void CalcHeight()
    {
        // Regular movement bobbing
        // (needs to be calculated for gun swing
        // even if not on ground)
        // OPTIMIZE: tablify angle
        // Note: a LUT allows for effects
        //  like a ramp with low health.
        Bob = (MapObject!.MomX * MapObject.MomX) + (MapObject.MomY * MapObject.MomY);

        Bob >>= 2;

        if (Bob > MaxBob)
        {
            Bob = MaxBob;
        }

        if ((Cheats & Cheat.NoMomentum) != 0 || !OnGround)
        {
            ViewZ = MapObject.Z + Constants.ViewHeight;

            if (ViewZ > MapObject.CeilingZ - Fixed.FromInt(4))
            {
                ViewZ = MapObject.CeilingZ - Fixed.FromInt(4);
            }

            ViewZ = MapObject.Z + ViewHeight;
            return;
        }

        var angle = (DoomMath.FineAngleCount / 20 * DoomGame.Instance.Game.LevelTime) & DoomMath.FineMask;
        var bob = Bob / 2 * DoomMath.Sin(angle);

        // move viewheight
        if (PlayerState == PlayerState.Alive)
        {
            ViewHeight += DeltaViewHeight;

            if (ViewHeight > Constants.ViewHeight)
            {
                ViewHeight = Constants.ViewHeight;
                DeltaViewHeight = Fixed.Zero;
            }

            if (ViewHeight < Constants.ViewHeight / 2)
            {
                ViewHeight = Constants.ViewHeight / 2;
                if (DeltaViewHeight <= Fixed.Zero)
                {
                    DeltaViewHeight = new Fixed(1);
                }
            }

            if (DeltaViewHeight != Fixed.Zero)
            {
                DeltaViewHeight += new Fixed(Constants.FracUnit / 4);
                if (DeltaViewHeight == Fixed.Zero)
                {
                    DeltaViewHeight = new Fixed(1);
                }
            }
        }

        ViewZ = MapObject.Z + ViewHeight + bob;

        if (ViewZ > MapObject.CeilingZ - Fixed.FromInt(4))
        {
            ViewZ = MapObject.CeilingZ - Fixed.FromInt(4);
        }
    }

    public void Move()
    {
        MapObject!.Angle += new Angle(Command.AngleTurn << 16);

        // Do not let the player control movement
        //  if not onground.
        OnGround = (MapObject.Z <= MapObject.FloorZ);

        if (Command.ForwardMove != 0 && OnGround)
        {
            Thrust(MapObject.Angle, new Fixed(Command.ForwardMove * 2048));
        }

        if (Command.SideMove != 0 && OnGround)
        {
            Thrust(MapObject.Angle - Angle.Angle90, new Fixed(Command.SideMove * 2048));
        }

        if ((Command.ForwardMove != 0 || Command.SideMove != 0) && 
            MapObject.State == State.GetSpawnState(StateNum.S_PLAY))
        {
            MapObject.SetState(StateNum.S_PLAY_RUN1);
        }
    }

    /// <summary>
    /// Moves the given origin along a given angle.
    /// </summary>
    public void Thrust(Angle angle, Fixed move)
    {
        MapObject!.MomX += (move * DoomMath.Cos(angle));
        MapObject.MomY += (move * DoomMath.Sin(angle));
    }

    /// <summary>
    /// Called every tic frame
    ///  that the player origin is in a special sector
    /// </summary>
    private void PlayerInSpecialSector()
    {
        var sector = MapObject?.SubSector?.Sector;
        if (sector is null)
        {
            return;
        }

        // Falling, not all the way down yet?
        if (MapObject!.Z != sector.FloorHeight)
        {
            return;
        }

        // Has hitten ground.
        switch (sector.Special)
        {
            case 5:
                // HELLSLIME DAMAGE
                if (Powers[(int)PowerUpType.IronFeet] == 0)
                {
                    if ((DoomGame.Instance.Game.LevelTime & 0x1f) == 0)
                    {
                        MapObject.DamageMapObject(MapObject, null, null, 10);
                    }
                }
                break;

            case 7:
                // NUKAGE DAMAGE
                if (Powers[(int)PowerUpType.IronFeet] == 0)
                {
                    if ((DoomGame.Instance.Game.LevelTime & 0x1f) == 0)
                    {
                        MapObject.DamageMapObject(MapObject, null, null, 5);
                    }
                }
                break;

            case 16:
            // SUPER HELLSLIME DAMAGE
            case 4:
                // STROBE HURT
                if (Powers[(int)PowerUpType.IronFeet] == 0 || DoomRandom.P_Random() < 5)
                {
                    if ((DoomGame.Instance.Game.LevelTime & 0x1f) == 0)
                    {
                        MapObject.DamageMapObject(MapObject, null, null, 20);
                    }
                }
                break;

            case 9:
                // SECRET SECTOR
                SecretCount++;
                sector.Special = 0;
                break;

            case 11:
                // EXIT SUPER DAMAGE! (for E1M8 finale)
                Cheats &= ~Cheat.GodMode;

                if ((DoomGame.Instance.Game.LevelTime & 0x1f) == 0)
                {
                    MapObject.DamageMapObject(MapObject, null, null, 20);
                }

                if (Health <= 10)
                {
                    DoomGame.Instance.Game.ExitLevel();
                }
                break;

            default:
                DoomGame.Error($"P_PlayerInSpecialSector: unknown special {sector.Special}");
                break;
        }
    }

    /// <summary>
    /// Num is the number of clip loads,
    /// not the individual count (0= 1/2 clip).
    /// Returns false if the ammo can't be picked up at all
    /// </summary>
    public bool GiveAmmo(AmmoType ammo, int num)
    {
        if (ammo is AmmoType.NoAmmo or AmmoType.NumAmmo)
        {
            return false;
        }

        if (Ammo[(int)ammo] == MaxAmmo[(int)ammo])
        {
            return false;
        }

        if (num != 0)
        {
            num *= GameController.ClipAmmo[(int)ammo];
        }
        else
        {
            num = GameController.ClipAmmo[(int)ammo] / 2;
        }

        if (DoomGame.Instance.Game.GameSkill is SkillLevel.Baby or SkillLevel.Nightmare)
        {
            // give double ammo in trainer mode,
            // you'll need in nightmare
            num <<= 1;
        }

        var oldAmmo = Ammo[(int)ammo];
        Ammo[(int)ammo] += num;

        if (Ammo[(int)ammo] > MaxAmmo[(int)ammo])
        {
            Ammo[(int)ammo] = MaxAmmo[(int)ammo];
        }

        // If non zero ammo, 
        // don't change up weapons,
        // player was lower on purpose.
        if (oldAmmo != 0)
        {
            return true;
        }

        // We were down to zero,
        // so select a new weapon.
        // Preferences are not user selectable.
        switch (ammo)
        {
            case AmmoType.Clip:
                if (ReadyWeapon == WeaponType.Fist)
                {
                    if (WeaponOwned[(int)WeaponType.Chaingun])
                    {
                        PendingWeapon = WeaponType.Chaingun;
                    }
                    else
                    {
                        PendingWeapon = WeaponType.Pistol;
                    }
                }

                break;

            case AmmoType.Shell:
                if (ReadyWeapon is WeaponType.Fist or WeaponType.Pistol)
                {
                    if (WeaponOwned[(int)WeaponType.Shotgun])
                    {
                        PendingWeapon = WeaponType.Shotgun;
                    }
                }

                break;

            case AmmoType.Cell:
                if (ReadyWeapon is WeaponType.Fist or WeaponType.Pistol)
                {
                    if (WeaponOwned[(int)WeaponType.Plasma])
                    {
                        PendingWeapon = WeaponType.Plasma;
                    }
                }

                break;

            case AmmoType.Missile:
                if (ReadyWeapon is WeaponType.Fist)
                {
                    if (WeaponOwned[(int)WeaponType.Missile])
                    {
                        PendingWeapon = WeaponType.Missile;
                    }
                }

                break;
        }

        return true;
    }

    /// <summary>
    /// The weapon name may have a MF_DROPPED flag ored in.
    /// </summary>
    public bool GiveWeapon(WeaponType weapon, bool dropped)
    {
        var game = DoomGame.Instance.Game;
        if (game.NetGame && game.DeathMatch && !dropped)
        {
            // leave placed weapons forever on net games
            if (WeaponOwned[(int)weapon])
            {
                return false;
            }

            BonusCount += GameController.BonusAdd;
            WeaponOwned[(int)weapon] = true;

            if (game.DeathMatch)
            {
                GiveAmmo(WeaponInfo.GetByType(weapon).Ammo, 5);
            }
            else
            {
                GiveAmmo(WeaponInfo.GetByType(weapon).Ammo, 2);
            }

            PendingWeapon = weapon;
            if (this == DoomGame.Instance.Game.Players[DoomGame.Instance.Game.ConsolePlayer])
            {
                DoomGame.Instance.Sound.StartSound(null, Sound.SoundType.sfx_wpnup);
            }

            return false;
        }

        var gaveAmmo = false;
        var ammo = WeaponInfo.GetByType(weapon).Ammo;
        if (ammo != AmmoType.NoAmmo)
        {
            // give one clip with a dropped weapon,
            // two clips with a found weapon
            gaveAmmo = GiveAmmo(ammo, dropped ? 1 : 2);
        }

        var gaveWeapon = false;
        if (!WeaponOwned[(int)weapon])
        {
            gaveWeapon = true;
            WeaponOwned[(int)weapon] = true;
            PendingWeapon = weapon;
        }

        return gaveWeapon || gaveAmmo;
    }

    /// <summary>
    /// Returns false if the body isn't needed at all
    /// </summary>
    public bool GiveBody(int num)
    {
        if (Health >= Constants.MaxHealth)
        {
            return false;
        }

        Health += num;
        if (Health > Constants.MaxHealth)
        {
            Health = Constants.MaxHealth;
        }

        MapObject!.Health = Health;
        return true;
    }

    /// <summary>
    /// Returns false if the armor is worse
    /// than the current armor.
    /// </summary>
    public bool GiveArmor(int armorType)
    {
        var hits = armorType * 100;
        if (ArmorPoints >= hits)
        {
            return false; // don't pick up
        }

        ArmorType = armorType;
        ArmorPoints = hits;
        return true;
    }

    public void GiveCard(KeyCardType card)
    {
        if (Cards[(int)card])
        {
            return;
        }

        BonusCount = GameController.BonusAdd;
        Cards[(int)card] = true;
    }

    public bool GivePower(PowerUpType power)
    {
        if (power == PowerUpType.Invulnerability)
        {
            Powers[(int)power] = 30 * Constants.TicRate;
            return true;
        }

        if (power == PowerUpType.Invisibility)
        {
            Powers[(int)power] = 60 * Constants.TicRate;
            MapObject!.Flags |= MapObjectFlag.MF_SHADOW;
            return true;
        }

        if (power == PowerUpType.InfraRed)
        {
            Powers[(int)power] = 120 * Constants.TicRate;
            return true;
        }

        if (power == PowerUpType.IronFeet)
        {
            Powers[(int)power] = 60 * Constants.TicRate;
            return true;
        }

        if (power == PowerUpType.Strength)
        {
            GiveBody(100);
            Powers[(int)power] = 1;
            return true;
        }

        if (Powers[(int)power] != 0)
        {
            return false;   // already got it
        }

        Powers[(int)power] = 1;
        return true;
    }
}