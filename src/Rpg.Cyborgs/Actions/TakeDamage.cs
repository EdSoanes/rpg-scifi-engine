using Newtonsoft.Json;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.Cyborgs.Actions
{
    public class TakeDamage : ModObjects.Actions.Action<Actor>
    {
        [JsonConstructor] private TakeDamage() { }

        public TakeDamage(Actor owner)
            : base(owner)
        {
        }

        public override bool IsEnabled<TOwner, TInitiator>(TOwner owner, TInitiator initiator)
            => true;

        public ModSet OnCost(int actionNo, Actor owner)
        {
            return new ModSet(new TimeLifecycle(TimePoints.BeginningOfEncounter));
        }

        public ModSet[] OnAct(int actionNo, Actor owner, int damage)
        {
            var modSet = new ModSet(new TimeLifecycle(TimePoints.Encounter(1)));
            return [modSet];
        }

        public ModSet[] OnOutcome(MeleeWeapon owner, Actor initiator, int damage, int roll1, int roll2)
        {
            return Array.Empty<ModSet>();
        }
    }
}
