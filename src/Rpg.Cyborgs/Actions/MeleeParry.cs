using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.Experimental;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods;

namespace Rpg.Cyborgs.Actions
{
    public class MeleeParry : RpgAction<Actor>
    {
        [JsonConstructor] protected MeleeParry()
            : base() { }

        public MeleeParry(Actor owner)
            : base(owner) { }

        public override void OnCreatingActivityAction(RpgGraph graph, RpgActivityAction activityAction)
        {
            base.OnCreatingActivityAction(graph, activityAction);
            graph.Add(new Initial(activityAction, "diceRoll", "2d6"));
        }

        public bool CanPerform(Actor owner, int damage)
            => damage > 0 && !owner.IsStateOn(nameof(Parrying));

        public bool Cost(RpgGraph graph, RpgActivityAction activityAction, Actor owner, Actor actor, int focusPoints)
        {
            activityAction.Result
                .Add(new Temporal(1, 1), actor, x => x.CurrentActionPoints, -1);

            if (focusPoints > 0)
            {
                graph.Add(new Standard(), activityAction, "focusPoints", focusPoints);
                activityAction.Result
                    .Add(new Temporal(1), actor, x => x.CurrentFocusPoints, -focusPoints);
            }

            return true;
        }

        public bool Perform(RpgGraph graph, RpgActivityAction activityAction, Actor owner, int parryTarget, int? abilityScore)
        {
            var focusPoints = graph.GetPropertyValue<Dice>(activityAction.Id, "focusPoints").Roll();
            var bonus = abilityScore != null
                ? abilityScore.Value * (focusPoints + 1)
                : owner.Strength.Value * (focusPoints + 1);

            graph
                .Reset(activityAction, "diceRoll")
                .Add(new Standard(), activityAction, "diceRoll", bonus);

            return true;
        }

        public bool Outcome(RpgGraph graph, RpgActivityAction activityAction, Actor owner, int diceRoll, int target, int damage)
        {
            activityAction.Result
                .Add(owner.CreateStateActivation(nameof(Parrying), 1, 1, false));

            if (diceRoll >= target)
            {
                var reduction = owner.Strength.Value > 0
                    ? owner.Strength.Value
                    : 0;

                if (reduction <= 0)
                    reduction = 1;

                if (diceRoll >= target)
                    graph.Add(new Standard(), activityAction, "damage", -reduction);
            }
            activityAction
                .SetOutcomeAction(owner, nameof(ArmourCheck), false)
                .SetOutcomeAction(owner, nameof(TakeDamage), true);

            return true;
        }
    }
}
