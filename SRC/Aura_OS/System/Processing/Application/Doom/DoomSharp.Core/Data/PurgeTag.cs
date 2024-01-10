namespace DoomSharp.Core.Data
{
    public enum PurgeTag
    {
        /// <summary>
        /// Static entire execution time
        /// </summary>
        Static = 1,
        
        /// <summary>
        /// Static while playing
        /// </summary>
        Sound = 2,

        /// <summary>
        /// Static while playing
        /// </summary>
        Music = 3,

        /// <summary>
        /// Anything else Dave wants static
        /// </summary>
        Dave = 4,

        /// <summary>
        /// Static until level exited
        /// </summary>
        Level = 50,
        
        /// <summary>
        /// A special thinker in a level
        /// </summary>
        LevSpec = 51,
        
        // Tags >= 100 are purgable whenever needed
        PurgeLevel = 100,
        Cache = 101
    }
}