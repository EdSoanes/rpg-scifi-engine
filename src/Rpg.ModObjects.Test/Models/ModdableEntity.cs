using Rpg.ModObjects.Mods;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Tests.States;
using Rpg.ModObjects.Time;
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

        protected override void OnLifecycleStarting()
        {
            this
                .BaseMod(x => x.Melee, x => x.Strength.Bonus)
                .BaseMod(x => x.Damage.Dice, x => x.Strength.Bonus);
        }

        public override void OnBeginningOfTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeginningOfTime(graph, entity);
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            var expiry = base.OnUpdateLifecycle(graph, currentTime, mod);
            StateStore.OnUpdateLifecycle(graph, currentTime);
            return expiry;
        }
    }
}
