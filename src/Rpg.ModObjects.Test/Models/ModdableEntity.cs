using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Tests.Models
{
    public class ModdableEntity : RpgEntity
    {
        public ScoreBonusValue Strength { get; private set; }
        public DamageValue Damage { get; private set; }
        public Dice Melee { get; protected set; } = 2;
        public Dice Missile { get; protected set; }
        public int Health { get; protected set; } = 10;

        public ModdableEntity()
        {
            Strength = new ScoreBonusValue(nameof(Strength), 14);
            Damage = new DamageValue(nameof(Damage), "d6", 10, 100);
        }

        protected override void OnLifecycleStarting()
        {
            this
                .BaseMod(x => x.Melee, x => x.Strength.Bonus)
                .BaseMod(x => x.Damage.Dice, x => x.Strength.Bonus)
                .InitActionsAndStates(Graph!);
        }
    }
}
