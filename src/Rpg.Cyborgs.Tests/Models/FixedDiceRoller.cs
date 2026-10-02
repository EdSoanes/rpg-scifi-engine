using Rpg.Experimental;

namespace Rpg.Cyborgs.Tests.Models
{
    /// <summary>
    /// A dice roller that returns the numbers it is given, in order, and counts how often it is asked
    /// </summary>
    internal class FixedDiceRoller : IRpgDiceRoller
    {
        private readonly Queue<int> _results;

        public int Rolled { get; private set; }

        public FixedDiceRoller(params int[] results)
            => _results = new Queue<int>(results);

        public int Roll(int sides)
        {
            Rolled++;
            return _results.Count > 0 ? _results.Dequeue() : 1;
        }
    }
}
