using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
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

        public override void OnTimeBegins()
        {
            base.OnTimeBegins();
            this
                .AddMod(new Base(), x => x.Melee, x => x.Strength.Bonus)
                .AddMod(new Base(), x => x.Damage.Dice, x => x.Strength.Bonus);

        }
    }
}
