using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Tests.Models
{
    public class TestGun : ModObject
    {
        public DamageValue Damage { get; private set; } = new DamageValue("d6", 10, 100);
        public MaxCurrentValue Ammo { get; private set; } = new MaxCurrentValue(nameof(Ammo), 10, 10);
        public int HitBonus { get; private set; } = 2;

        [ModCmd(OutcomeMethod = nameof(Damages))]
        public ModSet FiresAt(TestHuman initiator, TestHuman target)
        {
            return new ModSet(nameof(FiresAt))
                .AddMod(initiator, nameof(FiresAt), initiator, x => x.Missile)
                .AddMod(initiator, nameof(FiresAt), target, x => x.Defense, () => DiceCalculations.Minus)
                .AddMod(initiator, nameof(FiresAt), this, x => x.HitBonus)
                .AddTurnMod(initiator, x => x.PhysicalActionPoints.Current, -1)
                .AddTurnMod(initiator, x => x.MentalActionPoints.Current, -1)
                .AddSumMod(this, x => x.Ammo.Current, -1);
        }

        [ModCmd()]
        public ModSet Damages(int roll, TestHuman initiator, TestHuman target)
        {
            var res = new ModSet(nameof(Damages));
            if (roll > 10)
                res.AddSumMod(target, x => x.Health, -1);

            return res;
        }
    }
}
