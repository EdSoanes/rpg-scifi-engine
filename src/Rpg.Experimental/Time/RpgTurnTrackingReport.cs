using Rpg.Experimental.Mods;

namespace Rpg.Experimental.Time
{
    /// <summary>
    /// What currently needs turns to be counted. The engine never ends turn tracking itself. This tells the
    /// players whether anything turn based remains, so they can decide to end it.
    /// </summary>
    public class RpgTurnTrackingReport
    {
        public bool IsTurnTracking { get; init; }
        public int Turn { get; init; }

        /// <summary>
        /// Applied mods that last a number of turns and have not expired
        /// </summary>
        public Mod[] Effects { get; init; } = [];

        /// <summary>
        /// States that are on and need turns to be counted while they are (e.g. bleeding)
        /// </summary>
        public RpgState[] States { get; init; } = [];

        /// <summary>
        /// True if anything turn based is still running
        /// </summary>
        public bool IsNeeded { get => Effects.Any() || States.Any(); }
    }
}
