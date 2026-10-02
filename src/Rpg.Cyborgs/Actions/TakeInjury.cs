using Newtonsoft.Json;
using Rpg.Experimental;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods;
using Rpg.Experimental.Reflection.Attributes;

namespace Rpg.Cyborgs.Actions
{
    public class TakeInjury : RpgAction<Actor>
    {
        [JsonConstructor] protected TakeInjury()
            : base() { }

        public TakeInjury(Actor owner)
            : base(owner) { }

        public override void OnCreatingActivityAction(RpgGraph graph, RpgActivityAction activityAction)
        {
            base.OnCreatingActivityAction(graph, activityAction);
            graph
                .Add(new Initial(activityAction, "injuryRoll", "2d6"))
                .Add(new Initial(activityAction, "injuryLocationRoll", "1d6"));
        }

        public bool Perform(RpgGraph graph, RpgActivityAction activityAction, Actor owner, int lifeInjury)
        {
            graph
                .Reset(activityAction, "injuryRoll")
                .Reset(activityAction, "injuryLocationRoll");

            if (lifeInjury > 0)
                graph.Add(new Standard(), activityAction, "injuryRoll", -lifeInjury);

            return true;
        }

        [ArgSelect(Arg = "locationType", Enum = typeof(InjuryLocationType))]
        public bool Outcome(RpgActivityAction activityAction, Actor owner, int injuryRoll, int injuryLocationRoll, int locationType)
        {
            var injurySeverity = GetInjurySeverity(injuryRoll);
            var bodyPart = GetLocation(owner, injuryLocationRoll, locationType);

            //Injuries are permanent, they must outlive the activity
            activityAction.Result
                .Add(new Standard()
                    .SetTarget(bodyPart, x => x.InjurySeverity)
                    .SetSource(injurySeverity));

            return true;
        }

        private BodyPart GetLocation(Actor owner, int injuryLocationRoll, int locationType)
        {
            //random
            if (locationType == 0)
                return injuryLocationRoll switch
                {
                    1 => owner.LeftLeg,
                    2 => owner.RightLeg,
                    3 => owner.LeftArm,
                    4 => owner.RightArm,
                    5 => owner.Torso,
                    _ => owner.Head
                };

            //High
            if (locationType == 1)
                return injuryLocationRoll switch
                {
                    1 => owner.LeftArm,
                    2 => owner.RightArm,
                    3 => owner.Torso,
                    4 => owner.Torso,
                    5 => owner.Head,
                    _ => owner.Head
                };

            //Low
            if (locationType == 2)
                return injuryLocationRoll switch
                {
                    1 => owner.LeftLeg,
                    2 => owner.LeftLeg,
                    3 => owner.RightLeg,
                    4 => owner.RightLeg,
                    5 => owner.Torso,
                    _ => owner.Torso
                };

            return owner.Torso;
        }

        private int GetInjurySeverity(int injuryRoll)
            => injuryRoll switch
            {
                <= 2 => 6,
                <= 4 => 5,
                <= 7 => 4,
                <= 10 => 3,
                <= 12 => 2,
                _ => 1
            };
    }
}
