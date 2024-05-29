using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Cmds;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Tests.Models
{
    /// <summary>
    /// HitBonus +2
    /// Damage.Dice d6
    /// Ammo 10
    /// </summary>

    public class TestGun : RpgObject
    {
        public int HitBonus { get; private set; } = 2;
        public DamageValue Damage { get; private set; } = new DamageValue("d6", 0, 0);
        public MaxCurrentValue Ammo { get; private set; } = new MaxCurrentValue(nameof(Ammo), 10);

        [ModCmd(
            DisabledOnState = nameof(AmmoEmpty), 
            OutcomeMethod = nameof(InflictDamage)
        )]
        public ModSet Shoot(ModSet modSet, TestHuman initiator, int targetDefense, int targetRange)
        {
            modSet
                .Target(initiator, targetDefense)
                .Target(initiator, targetRange, () => DiceCalculations.Range);

            modSet
                .Roll(initiator, "d20")
                .Roll(initiator, x => x.MissileAttack)
                .Roll(initiator, this, x => x.HitBonus);

            modSet
                .AddTurnMod(initiator, x => x.PhysicalActionPoints.Current, -1)
                .AddTurnMod(initiator, x => x.MentalActionPoints.Current, -1)
                .AddSumMod(this, x => x.Ammo.Current, -1);

            return modSet;
        }

        [ModCmd(EnabledOnState = nameof(Shoot))]
        public ModSet InflictDamage(ModSet modSet, TestHuman initiator, TestHuman recipient, int targetRoll, int outcome)
        {
            var success = outcome - targetRoll;
            if (success >= 0)
                modSet.AddSumMod(recipient, x => x.Health, this, x => x.Damage.Dice);

            if (success >= 10)
                modSet.AddSumMod(recipient, x => x.Health, this, x => x.Damage.Dice);

            return modSet;
        }

        public bool IsAmmoEmpty()
            => Ammo.Current <= 0;

        [ModState(ShouldActivateMethod = nameof(IsAmmoEmpty))]
        public void AmmoEmpty(ModSet modSet)
        { }
    }
}
