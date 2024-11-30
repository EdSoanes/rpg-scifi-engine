using Newtonsoft.Json;
using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;

namespace Rpg.Cyborgs.Actions
{
    public class ArmourCheck : ActionTemplate<Actor>
    {
        [JsonConstructor] protected ArmourCheck()
            : base() { }

        public ArmourCheck(Actor owner) 
            : base(owner) { }

        public bool CanPerform(Activity activity, Actor owner, int damage)
            => damage > 0 && owner.Wearing.Get<Armour>().Any();

        public bool Perform(ModObjects.Activities.Action action, Actor owner, int luckPoints)
        {
            var armourRating = CalculateArmourRating(owner);

            action
                .SetProp("diceRoll1", "1d6")
                .SetProp("diceRoll2", "1d6")
                .SetProp("armourRating", armourRating);

            if (luckPoints > 0)
                action.SetProp("diceRoll1", 1);

            if (luckPoints > 1)
                action.SetProp("diceRoll2", 1);

            return true;
        }

        public bool Outcome(ModObjects.Activities.Action action, Actor owner, int damage, int diceRoll1, int diceRoll2, int armourRating)
        {
            var success1 = diceRoll1 > armourRating;
            var success2 = diceRoll2 > armourRating;

            action.ResetProp("damage");

            var armour = GetArmour(owner);
            if (armour != null)
            {
                if (success1 && success2)
                {
                    action.SetProp("damage", damage);
                    action.OutcomeModSet.Add(new Permanent(), armour, x => x.CurrentArmourRating, -2);
                }

                else if (success1)
                {
                    var damageReduction = Convert.ToInt32(Math.Ceiling((double)damage / 2));
                    action.SetProp("damage", damageReduction);
                    action.OutcomeModSet.Add(new Permanent(), armour, x => x.CurrentArmourRating, -1);
                }
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
