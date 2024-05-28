using Rpg.ModObjects.Modifiers;
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
    public class TestHuman : ModObject
    {
        public ScoreBonusValue Strength { get; private set; } = new ScoreBonusValue(nameof(Strength), 14);
        public ScoreBonusValue Intelligence { get; private set; } = new ScoreBonusValue(nameof(Intelligence), 10);
        public ScoreBonusValue Dexterity { get; private set; } = new ScoreBonusValue(nameof(Dexterity), 17);
        public int MeleeAttack { get; protected set; } = 2;
        public int MissileAttack { get; protected set; } = 2;
        public DamageValue MeleeDamage { get; private set; } = new DamageValue("d6", 0, 0);
        public int Defense { get; protected set; } = 5;
        public int Health { get; protected set; } = 10;
        public MaxCurrentValue PhysicalActionPoints { get; protected set; } = new MaxCurrentValue(nameof(PhysicalActionPoints), 3);
        public MaxCurrentValue MentalActionPoints { get; protected set; } = new MaxCurrentValue(nameof(MentalActionPoints), 3);

        protected override void OnCreate()
        {
            this
                .AddMod<BaseBehavior, TestHuman, int, int>(x => x.MeleeAttack, x => x.Strength.Bonus)
                .AddMod<BaseBehavior, TestHuman, Dice, int>(x => x.MeleeDamage.Dice, x => x.Strength.Bonus)
                .AddMod<BaseBehavior, TestHuman, int, int>(x => x.MissileAttack, x => x.Intelligence.Bonus)
                .AddMod<BaseBehavior, TestHuman, int, int>(x => x.PhysicalActionPoints.Max, x => x.Strength.Bonus)
                .AddMod<BaseBehavior, TestHuman, int, int>(x => x.MentalActionPoints.Max, x => x.Intelligence.Bonus);
        }
    }
}
