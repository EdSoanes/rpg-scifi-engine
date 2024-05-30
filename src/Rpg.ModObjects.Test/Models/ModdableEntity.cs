using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Cmds;
using Rpg.ModObjects.Modifiers;
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
            Strength = new ScoreBonusValue(Id, nameof(Strength), 14);
            Damage = new DamageValue(Id, nameof(Damage), "d6", 10, 100);
        }

        protected override void OnCreating()
        {
            this.AddMod(new Base(), x => x.Melee, x => x.Strength.Bonus);
            this.AddMod(new Base(), x => x.Damage.Dice, x => x.Strength.Bonus);
        }

        public bool ShouldBuff()
            => Melee.Roll() >= 10;

        [ModState(ShouldActivateMethod = nameof(ShouldBuff))]
        public void Buff(ModSet modSet)
            => modSet.AddMod(new Synced(), this, x => x.Health, 10);

        public bool ShouldNerf()
            => Melee.Roll() < 1;

        [ModState(ShouldActivateMethod = nameof(ShouldNerf))]
        public void Nerf(ModSet modSet)
            => modSet.AddMod(new Synced(), this, x => x.Health, -10);

        [ModCmd()]
        [ModCmdArg("initiator", ModCmdArgType.Actor)]
        public ModSet TestCommand(RpgObject initiator, int target)
        {
            return new ModSet(Id, nameof(TestCommand));
        }
    }
}
