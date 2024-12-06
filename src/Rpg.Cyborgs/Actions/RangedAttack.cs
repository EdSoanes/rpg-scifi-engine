using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.Cyborgs.Actions
{
    public class RangedAttack : ActionTemplate<RangedWeapon>
    {
        [JsonConstructor] protected RangedAttack()
            : base() { }

        public RangedAttack(RangedWeapon owner)
            : base(owner) { }

        public bool CanPerform(RangedWeapon owner, Actor initiator)
            => initiator.Hands.Contains(owner) && initiator.CurrentActionPoints > 0;

        public bool Cost(ModObjects.Activities.Action action, Actor initiator, int focusPoints)
        {
            action.CostModSet.Add(new Turn(), initiator, x => x.CurrentActionPoints, -1);
            if (focusPoints > 0)
                action.CostModSet
                    .Add(new Turn(), initiator, x => x.CurrentFocusPoints, -focusPoints);

            return true;
        }

        public bool Perform(ModObjects.Activities.Action action, RangedWeapon owner, Actor initiator, int targetDefence, int? abilityScore)
        {
            var focusPoints = action.Value("focusPoints")?.Roll();

            var val = abilityScore != null
                ? abilityScore.Value * (focusPoints + 1)
                : initiator.RangedAttack.Value * (focusPoints + 1);

            action
                .SetProp("diceRoll", "2d6")
                .SetProp("diceRoll", owner, x => x.HitBonus)
                .SetProp("diceRoll", val)
                .SetProp("targetDefence", targetDefence);

            return true;
        }

        public bool Outcome(ModObjects.Activities.Action action, RangedWeapon owner, Actor initiator, int diceRoll, int targetDefence)
        {
            action
                .SetProp("damage", owner, x => x.Damage)
                .SetOutcomeState(initiator, nameof(RangedAttacking), new Lifespan(0, 1));

            return true;
        }
    }
}
