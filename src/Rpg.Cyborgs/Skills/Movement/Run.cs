using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.Experimental;
using Rpg.Experimental.Mods;

namespace Rpg.Cyborgs.Skills.Movement
{
    public class Run : Skill
    {
        [JsonConstructor] protected Run() { }

        public Run(Actor owner)
            : base(owner)
        {
            IsIntrinsic = true;
        }

        public bool CanPerform(Actor owner)
            => owner.CurrentActionPoints > 0;

        public bool Cost(RpgActivityAction activityAction, Actor owner)
        {
            activityAction.CostSet
                .Add(new Temporal(1), owner, x => x.CurrentActionPoints, -1);

            return true;
        }

        public bool Outcome(RpgActivityAction activityAction, Actor owner)
        {
            activityAction.Result
                .Add(owner.CreateStateActivation(nameof(Moving), 1, false).NoTurnTracking());

            return true;
        }
    }
}
