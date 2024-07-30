using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time.Lifecycles;
using Rpg.Sys.Archetypes;
using Rpg.Sys.States;

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

        public bool OnCost(RpgActivity activity, Actor owner, int distance)
        {
            var movementCost = CalculateMoveCost(owner, distance);

            activity
                .AddMod("distance", "distance", distance)
                .AddMod("movementCost", "movementCost", movementCost);

            activity.OutcomeSet
                .Add(owner, x => x.Actions.Action, -movementCost);

            return true;
        }

        public bool OnAct(RpgActivity activity)
            => true;

        public bool OnOutcome(RpgActivity activity, Actor owner, int distance)
        {
            activity.OutcomeSet
                .Add(owner, x => x.Movement.Speed.Current, distance);

            var moving = owner.CreateStateInstance(nameof(Moving));
            activity.OutcomeSets.Add(moving);

            return true;
        }

        private int CalculateMoveCost(Actor actor, int distance)
        {
            var movementCost = Convert.ToInt32(Math.Ceiling((double)distance / actor.Movement.Speed.Max));
            return movementCost;
        }
    }
}
