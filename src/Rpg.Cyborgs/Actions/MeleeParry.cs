using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time.Lifecycles;
using Rpg.ModObjects.Time.Templates;
using Rpg.ModObjects.Values;

namespace Rpg.Cyborgs.Actions
{
    public class MeleeParry : ModObjects.Actions.Action<Actor>
    {
        [JsonConstructor] private MeleeParry() { }

        public MeleeParry(Actor owner)
            : base(owner)
        {
            CanPerformAfter = [nameof(TakeDamage)];
        }

        public bool OnCanAct(RpgActivity activity, Actor owner)
            => (activity.GetActivityProp("damage") ?? Dice.Zero) != Dice.Zero && !owner.IsStateOn(nameof(Parrying));

        public bool OnCost(RpgActivity activity, Actor owner, Actor initiator)
        {
            activity.CostSet
                .Add(new TurnMod(1, 1), initiator, x => x.CurrentActionPoints, -1);

            return true;
        }

        public bool OnAct(RpgActivity activity, Actor owner, int target, int focusPoints, int? parry)
        {
            if (focusPoints > 0)
                activity.OutcomeSet
                    .Add(owner, x => x.CurrentFocusPoints, -focusPoints);

            activity
                .ActionMod("diceRoll", "Base", "2d6")
                .ActionMod("target", "Base", 11);

            if (parry != null)
                activity.ActionMod("diceRoll", "Ability", parry.Value * (focusPoints + 1));
            else
                activity.ActionMod("diceRoll", owner, x => x.Strength.Value * (focusPoints + 1));

            return true;
        }

        public bool OnOutcome(RpgActivity activity, Actor owner, int diceRoll, int target, int damage)
        {
            activity
                .ActionResultMod("diceRoll", "Result", diceRoll)
                .ActionResultMod("target", "Result", target);

            var reduction = owner.Strength.Value > 0
                ? owner.Strength.Value
                : 0;

            if (diceRoll >= target)
                activity.ActivityMod("damage", "Parry", -reduction);

            var parrying = owner.CreateStateInstance(nameof(Parrying), new TurnLifecycle(1, 1));
            activity.OutputSets.Add(parrying);

            return true;
        }
    }
}
