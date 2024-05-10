namespace Rpg.ModObjects.Values
{
    public static class DiceCalculations
    {
        public static Dice Minus(Dice dice) => dice.Negate();

        public static Dice CalculateStatBonus(Dice dice) => (int) Math.Floor((double)(dice.Roll() - 10) / 2);
        public static Dice WeightSpeedBonus(Dice dice) => -(int)Math.Floor((double)dice.Roll() / 10);
        public static Dice StrengthSpeedBonus(Dice dice) => (int)Math.Floor((double)dice.Roll() / 10);
    }
}
