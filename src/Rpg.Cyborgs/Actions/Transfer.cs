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

        public bool OnCost(Activity activity, Actor initiator)
        {
            activity.CostSet
                .Add(initiator, x => x.CurrentActionPoints, -1);

            return true;
        }

        public bool OnAct(Activity activity)
            => true;

        public bool OnOutcome(RpgEntity owner, RpgContainer from, RpgContainer to)
        {
            from.Remove(owner);
            to.Add(owner);

            return true;
        }
    }
}
