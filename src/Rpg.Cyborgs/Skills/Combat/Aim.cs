using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.Experimental;
using Rpg.Experimental.Mods;

namespace Rpg.Cyborgs.Skills.Combat
{
    public class Aim : Skill
    {
        [JsonConstructor] protected Aim() { }

        public Aim(Actor owner)
            : base(owner)
        {
            IsIntrinsic = true;
            InitialRating = 1;
        }

        public bool CanPerform(Actor owner)
            => !owner.IsStateOn(nameof(Aiming)) || owner.RangedAimBonus < 6;

        public bool Cost(RpgActivityAction activityAction, Actor owner)
        {
            activityAction.CostSet
                .Add(new Temporal(1), owner, x => x.CurrentActionPoints, -1);

            return true;
        }

        public bool Outcome(RpgActivityAction activityAction, Actor owner)
        {
            activityAction.Result
                .Add(new Temporal(1), owner, x => x.RangedAimBonus, 2)
                .Add(owner.CreateStateActivation(nameof(Aiming), 1, false));

            return true;
        }
    }
}
