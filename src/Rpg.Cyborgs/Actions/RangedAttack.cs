using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.Experimental;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods;

namespace Rpg.Cyborgs.Actions
{
    public class RangedAttack : RpgAction<RangedWeapon>
    {
        [JsonConstructor] protected RangedAttack()
            : base() { }

        public RangedAttack(RangedWeapon owner)
            : base(owner) { }

        public override void OnCreatingActivityAction(RpgGraph graph, RpgActivityAction activityAction)
        {
            base.OnCreatingActivityAction(graph, activityAction);
            graph
                .CreateVirtualProperty(activityAction, "damage")
                .Add(new Initial(activityAction, "diceRoll", "2d6"));
        }

        public bool CanPerform(RangedWeapon owner, Actor actor)
            => actor.Hands.Contains(owner) && actor.CurrentActionPoints > 0;

        public bool Cost(RpgActivityAction activityAction, Actor actor, int focusPoints)
        {
            activityAction.Result
                .Add(new Temporal(1), actor, x => x.CurrentActionPoints, -1);

            if (focusPoints > 0)
                activityAction.Result
                    .Add(new Temporal(1), actor, x => x.CurrentFocusPoints, -focusPoints);

            return true;
        }

        public bool Perform(RpgGraph graph, RpgActivityAction activityAction, RangedWeapon owner, Actor actor, int targetDefence, int focusPoints, int? abilityScore)
        {
            var bonus = abilityScore != null
                ? abilityScore.Value * (focusPoints + 1)
                : actor.RangedAttack * (focusPoints + 1);

            graph
                .Reset(activityAction, "diceRoll")
                .Add(new Standard(), activityAction, "diceRoll", bonus)
                .Add(new Standard(), activityAction, "diceRoll", owner, x => x.HitBonus);

            graph
                .Reset(activityAction, "targetDefence")
                .Add(new Standard(), activityAction, "targetDefence", targetDefence);

            return true;
        }

        public bool Outcome(RpgGraph graph, RpgActivityAction activityAction, RangedWeapon owner, Actor actor, int diceRoll, int targetDefence)
        {
            graph
                .Reset(activityAction, "damage")
                .Add(new Standard(), activityAction, "damage", owner, x => x.Damage);

            activityAction.Result
                .Add(actor.CreateStateActivation(nameof(RangedAttacking), 1, false));

            return true;
        }
    }
}
