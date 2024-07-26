using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time.Lifecycles;
using Rpg.Sys.Archetypes;

namespace Rpg.Sys.Actions
{
    [Action(Required = true)]
    public class Move : ModObjects.Actions.Action<Actor>
    {
        [JsonConstructor] private Move() { }

        public Move(Actor owner)
            : base(owner) { }

        public bool OnCanAct(Actor owner)
            => true;

        public ModSet OnCost(Actor owner, int distance)
        {
            var movementCost = CalculateMoveCost(owner, distance);
            return new ModSet(owner.Id, new TurnLifecycle(), "Cost")
                .Add(owner, x => x.Actions.Action, -movementCost);
        }

        public ActionModSet OnAct(ActionInstance actionInstance)
            => actionInstance.CreateActionSet();

        public ModSet[] OnOutcome(Actor owner, int distance)
        {
            var moveSet = new ModSet(owner.Id, new TurnLifecycle(), "Cost")
                .Add(owner, x => x.Movement.Speed.Current, distance);

            var moving = owner.GetState(nameof(Move))!.CreateInstance(new TurnLifecycle());

            return [moveSet, moving];
        }

        private int CalculateMoveCost(Actor actor, int distance)
        {
            var movementCost = Convert.ToInt32(Math.Ceiling((double)distance / actor.Movement.Speed.Max));
            return movementCost;
        }
    }
}
