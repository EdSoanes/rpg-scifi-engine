using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Tests.Models
{
    /// <summary>
    /// Str 14 + 2, Int 10 + 0, Dex 17 + 3
    /// MeleeAttack +4
    /// MeleeDamage d6 + 2
    /// MissileAttack +2
    /// Defense 5
    /// Health 10
    /// PhysicalActionsPoints.Max 5
    /// MentalActionPoints.Max 3
    /// </summary>
    public class TestHuman : RpgEntity
    {
        public ScoreBonusValue Strength { get; private set; }
        public ScoreBonusValue Intelligence { get; private set; }
        public ScoreBonusValue Dexterity { get; private set; }
        public int MeleeAttack { get; protected set; } = 2;
        public int MissileAttack { get; protected set; } = 2;
        public DamageValue MeleeDamage { get; private set; }
        public int Defense { get; protected set; } = 5;
        public int Health { get; protected set; } = 10;
        public MaxCurrentValue PhysicalActionPoints { get; protected set; }
        public MaxCurrentValue MentalActionPoints { get; protected set; }

        public TestHuman()
        {
            Strength = new ScoreBonusValue(nameof(Strength), 14);
            Intelligence = new ScoreBonusValue(nameof(Intelligence), 10);
            Dexterity = new ScoreBonusValue(nameof(Dexterity), 17);

            MeleeDamage = new DamageValue(nameof(MeleeDamage), "d6", 0, 0);

            PhysicalActionPoints = new MaxCurrentValue(nameof(PhysicalActionPoints), 3);
            MentalActionPoints = new MaxCurrentValue(nameof(MentalActionPoints), 3);
        }

        protected override void OnLifecycleStarting()
        {
            this
                .BaseMod(x => x.MeleeAttack, x => x.Strength.Bonus)
                .BaseMod(x => x.MeleeDamage.Dice, x => x.Strength.Bonus)
                .BaseMod(x => x.MissileAttack, x => x.Intelligence.Bonus)
                .BaseMod(x => x.PhysicalActionPoints.Max, x => x.Strength.Bonus)
                .BaseMod(x => x.MentalActionPoints.Max, x => x.Intelligence.Bonus)
                .InitActionsAndStates(Graph!);
        }
    }
}
