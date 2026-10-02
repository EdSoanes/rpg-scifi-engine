using Newtonsoft.Json;
using Rpg.Experimental;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods;

namespace Rpg.Cyborgs.Actions
{
    /// <summary>
    /// Move an item to a child property of another object, e.g. from an actor's hands to a room's contents
    /// </summary>
    public class Transfer : RpgAction<Item>
    {
        [JsonConstructor] protected Transfer()
            : base() { }

        public Transfer(Item owner)
            : base(owner) { }

        public bool CanPerform(Item owner, Actor actor)
            => actor.CurrentActionPoints > 0;

        public bool Cost(RpgActivityAction activityAction, Actor actor)
        {
            activityAction.Result
                .Add(new Temporal(1), actor, x => x.CurrentActionPoints, -1);

            return true;
        }

        public bool Outcome(RpgGraph graph, Item owner, RpgObject to, string toProp)
        {
            if (graph.GetPropertyData<RpgPropertyDataObject>(to.Id, toProp) == null)
                return false;

            graph.Move(to.Id, toProp, owner.Id);
            return true;
        }
    }
}
