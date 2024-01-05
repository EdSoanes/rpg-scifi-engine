using Rpg.Sys.Modifiers;

namespace Rpg.Sys
{
    public static class DiceCalculations
    {
        public static Dice Minus(Dice dice) => dice.Negate();

        public static Dice CalculateStatBonus(Dice dice) => (int) Math.Floor((double)(dice.Roll() - 10) / 2);
    }
}
