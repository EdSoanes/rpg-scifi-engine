using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Actions;
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

        public bool OnCanAct(RpgEntity owner, Actor initiator, RpgContainer from, RpgContainer to)
            => initiator.CurrentActionPoints > 0 && from.Contains(owner);

        public ModSet OnCost(int actionNo, RpgEntity owner, Actor initiator, RpgContainer from, RpgContainer to)
        {
            return new ModSet(initiator.Id, new TurnLifecycle(), "Cost")
                .Add(initiator, x => x.CurrentActionPoints, -1);
        }

        public ActionModSet OnAct(ActionInstance actionInstance, RpgEntity owner, Actor initiator, RpgContainer from, RpgContainer to)
            => actionInstance.CreateActionSet();

        public ModSet[] OnOutcome(RpgEntity owner, RpgContainer from, RpgContainer to)
        {
            from.Remove(owner);
            to.Add(owner);

            return Array.Empty<ModSet>();
        }
    }
}
