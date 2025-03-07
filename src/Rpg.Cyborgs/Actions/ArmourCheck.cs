using Newtonsoft.Json;
using Rpg.Experimental;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods;

namespace Rpg.Cyborgs.Actions
{
    public class ArmourCheck : RpgAction<Actor>
    {
        [JsonConstructor] protected ArmourCheck()
            : base() { }

        public ArmourCheck(Actor owner) 
            : base(owner) { }

        public bool CanPerform(RpgActivity activity, Actor owner, int damage)
            => damage > 0 && owner.Wearing.Get<Armour>().Any();

        public override void OnCreatingActivityAction(RpgGraph graph, RpgActivityAction activityAction)
        {
            base.OnCreatingActivityAction(graph, activityAction);
            graph
                .Add(new Initial(activityAction, "diceRoll1", "1d6"))
                .Add(new Initial(activityAction, "diceRoll2", "1d6"));
        }

        public bool Perform(RpgGraph graph, RpgActivityAction activityAction, Actor owner, int luckPoints)
        {
            graph
                .Reset(activityAction, "diceRoll1")
                .Reset(activityAction, "diceRoll2")
                .Reset(activityAction, "armourRating");

            var armourRating = CalculateArmourRating(owner);
            graph
                .Add(new Standard(), activityAction, "armourRating", armourRating);

            if (luckPoints > 0)
                graph
                    .Add(new Override(), activityAction, "diceRoll1", armourRating + 1);

            if (luckPoints > 1)
                graph
                    .Add(new Override(), activityAction, "diceRoll2", armourRating + 1);

            return true;
        }

        public bool Outcome(RpgGraph graph, RpgActivityAction activityAction, Actor owner, int damage, int diceRoll1, int diceRoll2, int armourRating)
        {
            graph
                .Reset(activityAction, "damage");

            var armour = GetArmour(owner);
            if (armour != null)
            {
                var success1 = diceRoll1 > armourRating;
                var success2 = diceRoll2 > armourRating;

                if (success1 && success2)
                {
                    activityAction.Result
                        .Add(new Standard(), armour, x => x.CurrentArmourRating, -2);

                    return true;
                }

                else if (success1)
                {
                    var damageReduction = Convert.ToInt32(Math.Ceiling((double)damage / 2));
                    graph
                        .Add(new Standard(), activityAction, "damage", damage - damageReduction);

                    activityAction.Result
                        .Add(new Standard(), armour, x => x.CurrentArmourRating, -1);

                    return true;
                }
            }

            graph
                .Add(new Standard(), activityAction, "damage", damage);

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
