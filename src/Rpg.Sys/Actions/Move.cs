using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using Rpg.Sys.Archetypes;
using Rpg.Sys.States;
using Newtonsoft.Json;

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

        public bool OnCost(Activity activity, Actor owner, int distance)
        {
            var movementCost = CalculateMoveCost(owner, distance);

            activity
                .ActivityMod("distance", "distance", distance)
                .ActivityMod("movementCost", "movementCost", movementCost);

            activity.OutcomeSet
                .Add(owner, x => x.Actions.Action, -movementCost);

            return true;
        }

        public bool OnAct(Activity activity)
            => true;

        public bool OnOutcome(Activity activity, Actor owner, int distance)
        {
            activity.OutcomeSet
                .Add(owner, x => x.Movement.Speed.Current, distance);

            var moving = owner.CreateStateInstance(nameof(Moving), new SpanOfTime(0, 1));
            activity.OutputSets.Add(moving);

            return true;
        }

        private int CalculateMoveCost(Actor actor, int distance)
        {
            var movementCost = Convert.ToInt32(Math.Ceiling((double)distance / actor.Movement.Speed.Max));
            return movementCost;
        }
    }
}
