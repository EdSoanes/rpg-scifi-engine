namespace Rpg.Experimental
{
    /// <summary>
    /// Rolls a single die. A graph owns one, and it can be replaced (e.g. to fix the results in a test).
    /// </summary>
    public interface IRpgDiceRoller
    {
        /// <summary>
        /// A number from 1 to sides
        /// </summary>
        int Roll(int sides);
    }

    public sealed class RpgRandomDiceRoller : IRpgDiceRoller
    {
        private readonly Random _random = new Random();

        public int Roll(int sides)
            => sides > 0 ? _random.Next(1, sides + 1) : 0;
    }

    /// <summary>
    /// Raised when rules code asks for a number from a value that still has unrolled dice in it. Reading a
    /// value never rolls: a roll has to be settled first (see RpgGraph.Roll() and RpgGraph.SetRoll()).
    /// </summary>
    public sealed class RpgUnrolledDiceException : InvalidOperationException
    {
        public RpgUnrolledDiceException(string message)
            : base(message) { }
    }
}
