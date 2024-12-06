using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.Cyborgs.Actions
{
    public class MeleeParry : ActionTemplate<Actor>
    {
        [JsonConstructor] protected MeleeParry()
            : base() { }

        public MeleeParry(Actor owner)
            : base(owner) { }

        public bool CanPerform(Actor owner, int damage)
            => damage > 0 && !owner.IsStateOn(nameof(Parrying));

        public bool Cost(ModObjects.Activities.Action action, Actor owner, Actor initiator, int focusPoints)
        {
            action.CostModSet.Add(new Turn(1, 1), initiator, x => x.CurrentActionPoints, -1);
            if (focusPoints > 0)
                action.CostModSet.Add(new Turn(), owner, x => x.CurrentFocusPoints, -focusPoints);

            return true;
        }

        public bool Perform(ModObjects.Activities.Action action, Actor owner, int parryTarget, int? abilityScore)
        {
            var focusPoints = action.Value("focusPoints")?.Roll();
            var bonus = abilityScore != null
                ? abilityScore.Value * (focusPoints + 1)
                : owner.Strength.Value * (focusPoints + 1);

            action
                .SetProp("diceRoll", "2d6")
                .SetProp("diceRoll", bonus)
                .SetProp("parryTarget", parryTarget);

            return true;
        }

        public bool Outcome(ModObjects.Activities.Action action, Actor owner, int diceRoll, int target, int damage)
        {
            action.SetOutcomeState(owner, nameof(Parrying), new Lifespan(1, 1));

            if (diceRoll >= target)
            {
                var reduction = owner.Strength.Value > 0
                    ? owner.Strength.Value
                    : 0;

                if (reduction <= 0)
                    reduction = 1;

                if (diceRoll >= target)
                    action.SetProp("damage", -reduction);
            }
            action
                .SetOutcomeAction(owner, nameof(ArmourCheck), false)
                .SetOutcomeAction(owner, nameof(TakeDamage), true);

            return true;
        }
    }
}
