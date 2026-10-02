namespace Rpg.Experimental.Tests
{
    /// <summary>
    /// The dice expression type. It never rolls itself: see Roll_Tests for rolling.
    /// </summary>
    public class Dice_Tests
    {
        [Test]
        public void Dice_Constant_HasANumber()
        {
            var dice = new Dice("7");

            Assert.That(dice.IsConstant, Is.True);
            Assert.That(dice.Number, Is.EqualTo(7));
            Assert.That(dice.Bonus, Is.EqualTo(7));
            Assert.That(dice.DicePart, Is.EqualTo(Dice.Zero));
            Assert.That(dice.TryGetNumber(out var number), Is.True);
            Assert.That(number, Is.EqualTo(7));
        }

        [Test]
        public void Dice_NegativeConstant_HasANumber()
        {
            var dice = new Dice("3 - 5");

            Assert.That(dice.IsConstant, Is.True);
            Assert.That(dice.Number, Is.EqualTo(-2));
        }

        [Test]
        public void Dice_WithDice_HasNoNumber()
        {
            var dice = new Dice("2d6 + 3");

            Assert.That(dice.IsConstant, Is.False);
            Assert.That(dice.TryGetNumber(out _), Is.False);
            Assert.Throws<RpgUnrolledDiceException>(() => { var _ = dice.Number; });
        }

        [Test]
        public void Dice_SplitsIntoDiceAndBonus()
        {
            var dice = new Dice("2d6 + 3 + 1d4 - 1");

            Assert.That(dice.Bonus, Is.EqualTo(2));
            Assert.That(dice.DicePart.ToString(), Is.EqualTo("2d6 + 1d4"));
            Assert.That(dice.DicePart.Bonus, Is.EqualTo(0));
            Assert.That(dice.DicePart.Min(), Is.EqualTo(3));
            Assert.That(dice.DicePart.Max(), Is.EqualTo(16));
        }

        [Test]
        public void Dice_MultipliedByANumber_DoesNotRoll()
        {
            var dice = new Dice("2d6 + 1");

            Assert.That(dice * 2, Is.EqualTo(new Dice("4d6 + 2")));
            Assert.That(2 * dice, Is.EqualTo(new Dice("4d6 + 2")));
            Assert.That(dice * -1, Is.EqualTo(new Dice("-2d6 - 1")));
            Assert.That(dice * 0, Is.EqualTo(Dice.Zero));
            Assert.That(new Dice("5") * 3, Is.EqualTo(new Dice("15")));
        }
    }
}
