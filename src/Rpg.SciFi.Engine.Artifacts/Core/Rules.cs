using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Core
{
    public static class Rules
    {
        public static Dice Calc(Dice dice) => dice;

        public static Dice CalculateStatBonus(Dice dice) => (int) Math.Floor((double)(dice.Roll() - 10) / 2);

        public static Modifier[] CalculateToHit(Environment environment, Character character, Artifact weapon, Artifact target, int range)
        {
            var mods = new List<Modifier>();
            return mods.ToArray();
        }
    }
}
