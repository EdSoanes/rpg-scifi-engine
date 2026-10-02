namespace Rpg.Experimental
{
    /// <summary>
    /// The saved state of a character sheet at the start of a turn
    /// </summary>
    public class RpgTurnSnapshot
    {
        /// <summary>
        /// The compressed state
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// How far the turns have been renumbered since the snapshot was taken
        /// </summary>
        public int TurnOffset { get; set; }
    }
}
