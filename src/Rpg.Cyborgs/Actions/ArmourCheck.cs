using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Values;
using System.Text.Json.Serialization;

namespace Rpg.Cyborgs.Actions
{
    public class ArmourCheck : ModObjects.Actions.Action<Actor>
    {
        [JsonConstructor] private ArmourCheck() { }

        public ArmourCheck(Actor owner)
            : base(owner)
        {
            CanPerformAfter = [nameof(TakeDamage), nameof(MeleeParry)];
        }

        public bool OnCanAct(Activity activity, Actor owner)
            => owner.Wearing.Get<Armour>().Any() && (activity.GetActivityProp("damage") ?? Dice.Zero) != Dice.Zero;

        public bool OnCost(Activity activity, Actor owner, int luckPoints)
            => true;

        public bool OnAct(Activity activity, Actor owner, int luckPoints)
        {
            var armourRating = CalculateArmourRating(owner);

            activity
                .ActivityMod("diceRoll1", "Base", "1d6")
                .ActivityMod("diceRoll2", "Base", "1d6")
                .ActivityMod("armourRating", "Base", armourRating);

            if (luckPoints > 0)
                activity.ActivityResultMod("diceRoll1", "Result", armourRating + 1);

            if (luckPoints > 1)
                activity.ActivityResultMod("diceRoll2", "Result", armourRating + 1);

            return true;
        }

        public bool OnOutcome(Activity activity, Actor owner, int damage, int diceRoll1, int diceRoll2)
        {
            activity
                .ActivityResultMod("diceRoll1", "Result", diceRoll1)
                .ActivityResultMod("diceRoll2", "Result", diceRoll2);

            var armourRating = activity.GetActivityProp("armourRating")?.Roll() ?? 0;
            var success1 = diceRoll1 > armourRating;
            var success2 = diceRoll2 > armourRating;

            var armour = GetArmour(owner);
            if (armour != null)
            {
                if (success1 && success2)
                    activity
                        .ActivityMod("damage", "ArmourCheck", damage)
                        .OutcomeSet
                            .Add(new Permanent(), armour, x => x.CurrentArmourRating, -2);

                else if (success1)
                    activity
                        .ActivityMod("damage", "ArmourCheck", Convert.ToInt32(Math.Ceiling((double)damage / 2)))
                        .OutcomeSet
                            .Add(new Permanent(), armour, x => x.CurrentArmourRating, -1);
            }

            return true;
        }

        private Armour? GetArmour(Actor owner)
            => owner.Wearing.Get<Armour>()
                .OrderByDescending(x => x.CurrentArmourRating)
                .FirstOrDefault();

        private int CalculateArmourRating(Actor owner)
            => GetArmour(owner)?.CurrentArmourRating ?? 0;
    }
}
