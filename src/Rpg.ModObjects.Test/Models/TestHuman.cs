using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Tests.Models
{
    public class TestHuman : ModObject
    {
        public ScoreBonusValue Strength { get; private set; } = new ScoreBonusValue(nameof(Strength), 14);
        public ScoreBonusValue Intelligence { get; private set; } = new ScoreBonusValue(nameof(Intelligence), 10);
        public DamageValue Damage { get; private set; } = new DamageValue("d6", 10, 100);
        public Dice Melee { get; protected set; } = 2;
        public Dice Missile { get; protected set; }
        public Dice Defense { get; protected set; } = 5;
        public int Health { get; protected set; } = 10;
        public MaxCurrentValue PhysicalActionPoints { get; protected set; } = new MaxCurrentValue(nameof(PhysicalActionPoints), 3, 3);
        public MaxCurrentValue MentalActionPoints { get; protected set; } = new MaxCurrentValue(nameof(MentalActionPoints), 3, 3);

        protected override void OnCreate()
        {
            this
                .AddBaseMod(x => x.Melee, x => x.Strength.Bonus)
                .AddBaseMod(x => x.Damage.Dice, x => x.Strength.Bonus)
                .AddBaseMod(x => x.PhysicalActionPoints.Max, x => x.Strength.Bonus)
                .AddBaseMod(x => x.MentalActionPoints.Max, x => x.Intelligence.Bonus);
        }
    }
}
