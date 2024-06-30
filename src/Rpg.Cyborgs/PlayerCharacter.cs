using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Values;

namespace Rpg.Cyborgs
{
    public class PlayerCharacter : Actor
    {
        protected override void OnLifecycleStarting()
        {
            this.BaseMod(x => x.StaminaPoints, x => x.Health, () => CalculateStamina);
            this.BaseMod(x => x.LifePoints, x => x.Strength);
            this.BaseMod(x => x.Defence, x => x.Agility);
            this.BaseMod(x => x.Reactions, x => x.Agility);
            this.BaseMod(x => x.Reactions, x => x.Insight);

            this.BaseMod(x => x.FocusPoints, x => x.Agility);
            this.BaseMod(x => x.FocusPoints, x => x.Brains);
            this.BaseMod(x => x.FocusPoints, x => x.Insight);

            this.BaseMod(x => x.LuckPoints, x => x.Charisma);
            this.BaseMod(x => x.ParryDamageReduction, x => x.Strength);
            this.BaseMod(x => x.RangedAttack, x => x.Agility);
            this.BaseMod(x => x.MeleeAttack, x => x.Strength);
        }

        public Dice CalculateStamina(Dice health)
            => 12 + (health.Roll() * 2);

        public Dice Calculate
    }
}
