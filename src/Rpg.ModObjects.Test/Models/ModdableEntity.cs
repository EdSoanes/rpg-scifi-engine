using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Cmds;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Tests.Models
{
    public class ModdableEntity : ModObject
    {
        public ScoreBonusValue Strength { get; private set; } = new ScoreBonusValue(nameof(Strength), 14);
        public DamageValue Damage { get; private set; } = new DamageValue("d6", 10, 100);
        public Dice Melee { get; protected set; } = 2;
        public Dice Missile { get; protected set; }
        public int Health { get; protected set; } = 10;

        protected override void OnCreate()
        {
            this.AddBaseMod(x => x.Melee, x => x.Strength.Bonus);
            this.AddBaseMod(x => x.Damage.Dice, x => x.Strength.Bonus);
        }

        public bool ShouldBuff()
            => Melee.Roll() >= 10;

        [ModState(ShouldActivateMethod = nameof(ShouldBuff))]
        public void Buff(ModSet modSet)
            => modSet.AddExternalMod(this, x => x.Health, 10);

        public bool ShouldNerf()
            => Melee.Roll() < 1;

        [ModState(ShouldActivateMethod = nameof(ShouldNerf))]
        public void Nerf(ModSet modSet)
            => modSet.AddExternalMod(this, x => x.Health, -10);

        [ModCmd()]
        [ModCmdArg("initiator", ModCmdArgType.Actor)]
        public ModSet TestCommand(ModObject initiator, int target)
        {
            return new ModSet(Id, nameof(TestCommand));
        }
    }
}
