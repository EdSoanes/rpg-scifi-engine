using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Tests.Models
{
    public class ModdableEntity : ModObject
    {
        public ScoreBonusValue Strength { get; private set; } = new ScoreBonusValue(nameof(Strength), 14);
        public DamageValue Damage { get; private set; } = new DamageValue("d6", 10, 100);
        public Dice Melee { get; protected set; } = 2;
        public Dice Missile { get; protected set; }
        public int Health { get; protected set; } = 10;

        protected override void OnInitialize()
        {
            this.AddPermanentMod(x => x.Melee, x => x.Strength.Bonus);
            this.AddPermanentMod(x => x.Damage.Dice, x => x.Strength.Bonus);
        }
    }
}
