﻿using Rpg.Cyborgs.States;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using Newtonsoft.Json;

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

        public bool OnCanAct(Activity activity, Actor owner)
            => (activity.GetActivityProp("damage") ?? Dice.Zero) != Dice.Zero && !owner.IsStateOn(nameof(Parrying));

        public bool OnCost(Activity activity, Actor owner, Actor initiator)
        {
            activity.CostSet
                .Add(new Turn(1, 1), initiator, x => x.CurrentActionPoints, -1);

            return true;
        }

        public bool OnAct(Activity activity, Actor owner, int target, int focusPoints, int? parry)
        {
            if (focusPoints > 0)
                activity.OutcomeSet
                    .Add(owner, x => x.CurrentFocusPoints, -focusPoints);

            activity
                .ActivityMod("diceRoll", "Base", "2d6");

            var bonus = parry != null
                ? parry.Value * (focusPoints + 1)
                : owner.Strength.Value * (focusPoints + 1);

            if (parry != null)
                activity.ActivityMod("diceRoll", "Ability", bonus);
            else
                activity.ActivityMod("diceRoll", "Strength.Value", bonus);

            return true;
        }

        public bool OnOutcome(Activity activity, Actor owner, int diceRoll, int target, int damage)
        {
            if (diceRoll >= target)
            {
                var reduction = owner.Strength.Value > 0
                    ? owner.Strength.Value
                    : 0;

                if (reduction <= 0)
                    reduction = 1;

                if (diceRoll >= target)
                    activity.ActivityMod("damage", "Parry", -reduction);

                var parrying = owner.CreateStateInstance(nameof(Parrying), new SpanOfTime(1, 1));
                activity.OutputSets.Add(parrying);
            }

            return true;
        }
    }
}
