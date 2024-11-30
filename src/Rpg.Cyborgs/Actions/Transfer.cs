using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;

namespace Rpg.Cyborgs.Actions
{
    public class Transfer : ActionTemplate<RpgEntity>
    {
        [JsonConstructor] protected Transfer()
            : base() { }

        public Transfer(RpgEntity owner)
            : base(owner) { }

        public bool CanPerform(RpgEntity owner, Actor initiator, RpgContainer from)
            => initiator.CurrentActionPoints > 0 && from.Contains(owner);

        public bool Cost(ModObjects.Activities.Action action, Actor initiator)
        {
            action.CostModSet.Add(new Turn(), initiator, x => x.CurrentActionPoints, -1);
            return true;
        }

        public bool Outcome(RpgEntity owner, RpgContainer from, RpgContainer to)
        {
            from.Remove(owner);
            to.Add(owner);

            return true;
        }
    }
}
