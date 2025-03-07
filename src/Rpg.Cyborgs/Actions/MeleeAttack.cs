using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.Experimental;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods;

namespace Rpg.Cyborgs.Actions
{
    public class MeleeAttack : RpgAction<MeleeWeapon>
    {
        [JsonConstructor] protected MeleeAttack()
            : base() { }

        public MeleeAttack(MeleeWeapon owner)
            : base(owner) 
        {
        }

        public override void OnCreatingActivityAction(RpgGraph graph, RpgActivityAction activityAction)
        {
            base.OnCreatingActivityAction(graph, activityAction);
            graph
                .Add(new Initial(activityAction, "diceRoll", "2d6"));
        }

        public bool CanPerform(MeleeWeapon owner, Actor initiator)
            => initiator.Hands.Contains(owner) && initiator.CurrentActionPoints > 0;

        public bool Cost(RpgCharacterSheet characterSheet, RpgActivityAction activityAction, Actor actor, int actionPoints, int focusPoints)
        {
            if (actionPoints > 0) 
                activityAction.Result
                    .Add(new Temporal(1), actor, x => x.CurrentActionPoints, -actionPoints);

            if (focusPoints > 0)
                activityAction.Result
                    .Add(new Temporal(1), actor, x => x.CurrentFocusPoints, -focusPoints);

            return true;
        }

        public bool Perform(RpgCharacterSheet characterSheet, RpgActivityAction activityAction, MeleeWeapon owner, Actor actor, int targetDefence, int focusPoints, int? abilityScore)
        {
            var diceRoll = abilityScore != null
                ? abilityScore.Value * (focusPoints + 1)
                : actor.RangedAttack.Value * (focusPoints + 1);

            characterSheet
                .Reset(activityAction, "diceRoll")
                .Add(new Standard(), activityAction, "diceRoll", diceRoll)
                .Add(new Standard(), activityAction, "diceRoll", owner, x => x.HitBonus);

            characterSheet
                .Reset(activityAction, "targetDefence")
                .Add(new Standard(), activityAction, "targetDefence", targetDefence);

            return true;
        }

        public bool Outcome(RpgGraph graph, RpgActivityAction activityAction, MeleeWeapon owner, Actor actor, int diceRoll, int targetDefence)
        {
            graph
                .Reset(activityAction, "damage")
                .Add(new Standard(), activityAction, "damage", owner, x => x.Damage);

            activityAction.Result
                .Add(actor.CreateStateActivation(nameof(MeleeAttacking), 1, false));

            return true;
        }
    }
}
