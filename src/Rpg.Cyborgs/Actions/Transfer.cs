using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time.Lifecycles;

namespace Rpg.Cyborgs.Actions
{
    public class Transfer : ModObjects.Actions.Action<RpgEntity>
    {
        [JsonConstructor] private Transfer() { }

        public Transfer(RpgEntity owner)
            : base(owner)
        {
        }

        public bool OnCanAct(Actor initiator)
            => initiator.CurrentActions > 0;

        public ModSet OnCost(int actionNo, RpgEntity owner, Actor initiator, RpgContainer from, RpgContainer to)
        {
            return new ModSet(initiator.Id, new TurnLifecycle())
                .Add(initiator, x => x.CurrentActions, -1);
        }

        public ModSet[] OnAct(int actionNo, RpgEntity owner, Actor initiator, RpgContainer from, RpgContainer to)
        {
            var modSet = new ModSet(initiator.Id, new TurnLifecycle());
            return [modSet];
        }

        public ModSet[] OnOutcome(int actionNo, RpgEntity owner, Actor initiator, RpgContainer from, RpgContainer to)
        {
            from.Remove(owner);
            to.Add(owner);

            return Array.Empty<ModSet>();
        }
    }
}
