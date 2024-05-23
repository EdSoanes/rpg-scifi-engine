using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Tests.Models
{
    public class TestGun : ModObject
    {
        public DamageValue Damage { get; private set; } = new DamageValue("d6", 10, 100);
        public MaxCurrentValue Ammo { get; private set; } = new MaxCurrentValue(nameof(Ammo), 10, 10);
        public int HitBonus { get; private set; } = 2;

        [ModCmd(OutcomeMethod = nameof(InflictDamage))]
        public ModSet Shoot(ModSet modSet, TestHuman initiator, int targetDefense, int targetRange)
        {
            modSet
                .Target(initiator, targetDefense)
                .Target(initiator, targetRange, () => DiceCalculations.Range);

            modSet
                .Roll(initiator, "d20")
                .Roll(initiator, x => x.Missile)
                .Roll(initiator, this, x => x.HitBonus);

            modSet
                .AddTurnMod(initiator, x => x.PhysicalActionPoints.Current, -1)
                .AddTurnMod(initiator, x => x.MentalActionPoints.Current, -1)
                .AddSumMod(this, x => x.Ammo.Current, -1);

            return modSet;
        }

        [ModCmd()]
        public ModSet InflictDamage(ModSet modSet, TestHuman initiator, int target, int roll)
        {
            if (roll - target > 0)
                modSet.Roll(initiator, this, x => x.Damage.Dice);

            if (roll - target > 10)
                modSet.Roll(initiator, this, x => x.Damage.Dice);

            return modSet;
        }
    }
}
