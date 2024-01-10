namespace DoomSharp.Core;

public static class Constants
{
    public const int BaseWidth = 320;

    public const int ScreenWidth = 320;
    public const int ScreenHeight = 200;
    
    public const int ScreenMul = 1;
    public const float InvertedAspectRatio = 0.625f;

    /// <summary>
    /// State updates, number of ticks / second
    /// </summary>
    public const int TicRate = 35;

    public const int DemoMarker = 0x80;

    // Networking and tick handling related
    public const int BackupTics = 12;

    public const int DoomComId = 0x12345678;
    public const int MaxPlayers = 4;
    public const int MaxNetNodes = 8; // 4 players max + drones
    public const int ResendCount = 10;
    public const int PlayerDrone = 0x80;

    public const int MaxEvents = 64;

    // Fixed point, 32 bit as 16.16
    public const int FracBits = 16;
    public const int FracUnit = (1 << FracBits);

    public static readonly Fixed FloatSpeed = Fixed.FromInt(4);
    public const int MaxHealth = 100;
    public static readonly Fixed ViewHeight = Fixed.FromInt(41);

    // LineDef attributes

    public static class Line
    {
        /// <summary>
        /// Solid, is an obstacle.
        /// </summary>
        public const int Blocking = 1;

        /// <summary>
        /// Block monsters only.
        /// </summary>
        public const int BlockMonsters = 2;

        /// <summary>
        /// Backside will not be present at all if not two sided.
        /// </summary>
        public const int TwoSided = 4;

        // If a texture is pegged, the texture will have
        // the end exposed to air held constant at the
        // top or bottom of the texture (stairs or pulled
        // down things) and will move with a height change
        // of one of the neighbor sectors.
        // Unpegged textures allways have the first row of
        // the texture at the top pixel of the line for both
        // top and bottom textures (use next to windows).

        /// <summary>
        /// upper texture unpegged
        /// </summary>
        public const int DontPegTop = 8;

        /// <summary>
        /// lower texture unpegged
        /// </summary>
        public const int DontPegBottom = 16;

        /// <summary>
        /// In AutoMap: don't map as two-sided: IT'S A SECRET!
        /// </summary>
        public const int Secret = 32;

        /// <summary>
        /// Sound rendering: don't let sound cross two of these
        /// </summary>
        public const int SoundBlock = 64;

        /// <summary>
        /// Don't draw on the automap at all
        /// </summary>
        public const int DontDraw = 128;

        /// <summary>
        /// Set if already seen, thus drawn in automap.
        /// </summary>
        public const int Mapped = 256;
    }
    
    // mapblocks are used to check movement
    // against lines and things
    public const int MapBlockunits = 128;
    public static readonly Fixed MapBlockSize = Fixed.FromInt(MapBlockunits);
    public const int MapBlockShift = (FracBits + 7);
    // public const int MapBlockMask = (MapBlockSize - 1);
    public const int MapBlockToFrac = (MapBlockShift - FracBits);

    // MAXRADIUS is for precalculated sector block boxes
    // the spider demon is larger,
    // but we do not have any moving sectors nearby
    public static readonly Fixed MaxRadius = Fixed.FromInt(32);

    public static readonly Fixed Gravity = Fixed.Unit;
    public static readonly Fixed MaxMove = Fixed.FromInt(30);

    public static readonly Fixed UseRange = Fixed.FromInt(64);
    public static readonly Fixed MeleeRange = Fixed.FromInt(64);
    public static readonly Fixed MissileRange = Fixed.FromInt(32 * 64);

    // follow a player exclusively for 3 seconds
    public const int BaseThreshold = 100;

    public static class NetCommands
    {
        public const uint Exit = 0x80000000;
        public const int Retransmit = 0x40000000;
        public const int Setup = 0x20000000;
        public const int Kill = 0x10000000; // kill game
        public const int CheckSum = 0x0fffffff;
    }

    public static readonly string[] SpriteNames =
    {
        "TROO","SHTG","PUNG","PISG","PISF","SHTF","SHT2","CHGG","CHGF","MISG",
        "MISF","SAWG","PLSG","PLSF","BFGG","BFGF","BLUD","PUFF","BAL1","BAL2",
        "PLSS","PLSE","MISL","BFS1","BFE1","BFE2","TFOG","IFOG","PLAY","POSS",
        "SPOS","VILE","FIRE","FATB","FBXP","SKEL","MANF","FATT","CPOS","SARG",
        "HEAD","BAL7","BOSS","BOS2","SKUL","SPID","BSPI","APLS","APBX","CYBR",
        "PAIN","SSWV","KEEN","BBRN","BOSF","ARM1","ARM2","BAR1","BEXP","FCAN",
        "BON1","BON2","BKEY","RKEY","YKEY","BSKU","RSKU","YSKU","STIM","MEDI",
        "SOUL","PINV","PSTR","PINS","MEGA","SUIT","PMAP","PVIS","CLIP","AMMO",
        "ROCK","BROK","CELL","CELP","SHEL","SBOX","BPAK","BFUG","MGUN","CSAW",
        "LAUN","PLAS","SHOT","SGN2","COLU","SMT2","GOR1","POL2","POL5","POL4",
        "POL3","POL1","POL6","GOR2","GOR3","GOR4","GOR5","SMIT","COL1","COL2",
        "COL3","COL4","CAND","CBRA","COL6","TRE1","TRE2","ELEC","CEYE","FSKU",
        "COL5","TBLU","TGRN","TRED","SMBT","SMGT","SMRT","HDB1","HDB2","HDB3",
        "HDB4","HDB5","HDB6","POB1","POB2","BRS1","TLMP","TLP2"
    };

    public const int MaxButtons = MaxPlayers * 4;
    public const int ButtonTime = TicRate; // 1 second in ticks

    /// <summary>
    /// Max number of wall switches in a level
    /// </summary>
    public const int MaxSwitches = 50;

    public const int MaxAnimations = 32;
    public const int MaxLineAnimations = 64;

    public const int NodeLeafSubSector = 0x8000;

    public static readonly Fixed OnFloorZ = Fixed.MinValue;
    public static readonly Fixed OnCeilingZ = Fixed.MaxValue;
}